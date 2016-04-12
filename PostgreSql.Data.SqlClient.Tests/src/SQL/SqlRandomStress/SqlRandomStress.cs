// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class SqlRandomStress
    {
        private static readonly TimeSpan TimeLimitDefault = new TimeSpan(0, 0, 10);
        private const int ThreadCountDefault = 4;
        private const int IterationsPerTableDefault = 50;

        private const int MaxColumns = 1600;
        private const int MaxRows    = 100;
        private const int MaxTotal   = MaxColumns * 10;

        private static string[] _connectionStrings;
        private static string   _operationCanceledErrorMessage;
        private static string   _severeErrorMessage;

        private static SqlRandomTypeInfoCollection _sqlTypes;
        private static ManualResetEvent            _endEvent;
        private static int                         _runningThreads;

        private static long _totalValues;
        private static long _totalTables;
        private static long _totalIterations;
        private static long _totalTicks;
        
        private static RandomizerPool _randPool;

        [Fact]
        public static void TestMain()
        {
            _operationCanceledErrorMessage = "Operation cancelled by user.";
            _severeErrorMessage            = "A severe error occurred on the current command. The results, if any, should be discarded.";

            // pure random
            _randPool = new RandomizerPool();

            var regularConnectionString = new PgConnectionStringBuilder();

            regularConnectionString.ConnectionString         = DataTestClass.PostgreSql9_Northwind;
            regularConnectionString.MultipleActiveResultSets = false;

            List<string> connStrings = new List<string>();
            connStrings.Add(regularConnectionString.ToString());

            connStrings.Add(regularConnectionString.ToString());

            regularConnectionString.MultipleActiveResultSets = true;
            connStrings.Add(regularConnectionString.ToString());

            _connectionStrings = connStrings.ToArray();

            _sqlTypes = SqlRandomTypeInfoCollection.CreateSqlTypesCollection();
            _endEvent = new ManualResetEvent(false);

            if (_randPool.ReproMode)
            {
                _runningThreads = 1;
                TestThread();
            }
            else
            {
                for (int tcount = 0; tcount < ThreadCountDefault; tcount++)
                {
                    Thread t = new Thread(TestThread);
                    t.Start();
                }
            }
        }

        private static void NextConnection(ref PgConnection con, Randomizer rand)
        {
            if (con != null)
            {
                con.Close();
            }

            string connString = _connectionStrings[rand.Next(_connectionStrings.Length)];

            con = new PgConnection(connString);
            con.Open();
        }

        private static void TestThread()
        {
            try
            {
                using (var rootScope = _randPool.RootScope<SqlRandomizer>())
                {
                    Stopwatch    watch = new Stopwatch();
                    PgConnection con   = null;
                    try
                    {
                        NextConnection(ref con, rootScope.Current);

                        if (_randPool.ReproMode)
                        {
                            using (var testScope = rootScope.NewScope<SqlRandomizer>())
                            {
                                // run only once if repro file is provided
                                RunTest(con, testScope, _sqlTypes, watch);
                            }
                        }
                        else
                        {
                            while (watch.Elapsed < TimeLimitDefault)
                            {
                                using (var testScope = rootScope.NewScope<SqlRandomizer>())
                                {
                                    RunTest(con, testScope, _sqlTypes, watch);
                                }

                                if (rootScope.Current.Next(100) == 0)
                                {
                                    // replace the connection
                                    NextConnection(ref con, rootScope.Current);
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (con != null)
                        {
                            con.Close();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                StringBuilder output = new StringBuilder();
                output.Append(e.ToString());
                output.AppendLine();

                if (!_randPool.ReproMode)
                {
                    // add .repro extension to enable easy delete on repro files
                    string reproFile = Path.GetRandomFileName() + ".repro";
                    _randPool.SaveLastThreadScopeRepro(reproFile);
                    output.AppendFormat("ReproFile (use with /repro:reproFilePath):{0}{1}{0}",
                        Environment.NewLine,
                        reproFile);
                }

                Console.WriteLine(output);
            }
            finally
            {
                if (Interlocked.Decrement(ref _runningThreads) == 0)
                {
                    _endEvent.Set();
                }
            }
        }

        private static void RunTest(PgConnection                        con
                                  , RandomizerPool.Scope<SqlRandomizer> testScope
                                  , SqlRandomTypeInfoCollection         types
                                  , Stopwatch                           watch)
        {
            Exception pendingException = null;
            string    tempTableName    = null;

            try
            {
                // select number of columns to use and null bitmap to test
                int columnsCount, rowsCount;
                testScope.Current.NextTableDimensions(MaxRows, MaxColumns, MaxTotal, out rowsCount, out columnsCount);
                SqlRandomTable table = SqlRandomTable.Create(testScope.Current, types, columnsCount, rowsCount, createPrimaryKeyColumn: true);

                long total = (long)rowsCount * columnsCount;
                Interlocked.Add(ref _totalValues, total);
                Interlocked.Increment(ref _totalTables);

                tempTableName = SqlRandomizer.GenerateUniqueTempTableName();
                table.GenerateTableOnServer(con, tempTableName);

                long prevTicks = watch.ElapsedTicks;
                watch.Start();

                if (_randPool.ReproMode)
                {
                    // perform one iteration only
                    using (var iterationScope = testScope.NewScope<SqlRandomizer>())
                    {
                        RunTestIteration(con, iterationScope.Current, table, tempTableName);
                        Interlocked.Increment(ref _totalIterations);
                    }
                }
                else
                {
                    // continue with normal loop
                    for (int i = 0; i < IterationsPerTableDefault && watch.Elapsed < TimeLimitDefault; i++)
                    {
                        using (var iterationScope = testScope.NewScope<SqlRandomizer>())
                        {
                            RunTestIteration(con, iterationScope.Current, table, tempTableName);
                            Interlocked.Increment(ref _totalIterations);
                        }
                    }
                }

                watch.Stop();
                Interlocked.Add(ref _totalTicks, watch.ElapsedTicks - prevTicks);
            }
            catch (Exception e)
            {
                pendingException = e;
                throw;
            }
            finally
            {
                // keep the temp table for troubleshooting if debugger is attached
                // the thread is going down anyway and connection will be closed
                if (pendingException == null && tempTableName != null)
                {
                    // destroy the temp table to free resources on the server
                    PgCommand cmd = con.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = $"DROP TABLE {tempTableName}";
                    try
                    {
                        cmd.ExecuteNonQuery();
                    }
                    catch
                    {
                    }
                }
            }
        }

        private static void RunTestIteration(PgConnection con, SqlRandomizer rand, SqlRandomTable table, string tableName)
        {
            // random list of columns
            int   columnCount    = table.Columns.Count;
            int[] columnIndicies = rand.NextIndices(columnCount);
            int   selectedCount  = rand.NextIntInclusive(1, maxValueInclusive: columnCount);

            StringBuilder selectBuilder = new StringBuilder();
            table.GenerateSelectFromTableSql(tableName, selectBuilder, columnIndicies, 0, selectedCount);
            PgCommand cmd = con.CreateCommand();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = selectBuilder.ToString();

            bool cancel = rand.Next(100) == 0; // in 1% of the cases, call Cancel

            if (cancel)
            {
                int cancelAfterMilliseconds = rand.Next(5);
                int cancelAfterSpinCount    = rand.Next(1000);

                ThreadPool.QueueUserWorkItem((object state) =>
                {
                    for (int i = 0; cancel && i < cancelAfterMilliseconds; i++)
                    {
                        Thread.Sleep(1);
                    }
                    if (cancel && cancelAfterSpinCount > 0)
                    {
                        SpinWait.SpinUntil(() => false, new TimeSpan(cancelAfterSpinCount));
                    }
                    if (cancel)
                    {
                        cmd.Cancel();
                    }
                });
            }

            int readerRand = rand.NextIntInclusive(0, maxValueInclusive: 256);
            CommandBehavior readerBehavior = CommandBehavior.Default;
            try
            {
                using (PgDataReader reader = cmd.ExecuteReader(readerBehavior))
                {
                    int row = 0;
                    while (reader.Read())
                    {
                        int rowRand = rand.NextIntInclusive();
                        if (rowRand % 1000 == 0)
                        {
                            // abandon this reader
                            break;
                        }
                        else if (rowRand % 25 == 0)
                        {
                            // skip the row
                            row++;
                            continue;
                        }

                        IList<object> expectedRow = table[row];
                        for (int c = 0; c < reader.FieldCount; c++)
                        {
                            if (rand.NextIntInclusive(0, maxValueInclusive: 10) == 0)
                            {
                                // skip the column
                                continue;
                            }

                            int expectedTableColumn = columnIndicies[c];
                            object expectedValue = expectedRow[expectedTableColumn];
                            if (table.Columns[expectedTableColumn].CanCompareValues)
                            {
                                Assert.True(expectedValue != null, "FAILED: Null is expected with CanCompareValues");

                                // read the value same way it was written
                                object actualValue = table.Columns[expectedTableColumn].Read(reader, c, expectedValue.GetType());
                                Assert.True(table.Columns[expectedTableColumn].CompareValues(expectedValue, actualValue),
                                    string.Format("FAILED: Data Comparison Failure:\n{0}", table.Columns[expectedTableColumn].BuildErrorMessage(expectedValue, actualValue)));
                            }
                        }

                        row++;
                    }
                }

                // keep last - this will stop the cancel task, if it is still active
                cancel = false;
            }
            catch (PgException e)
            {
                if (!cancel)
                {
                    throw;
                }

                bool expected = false;

                foreach (PgError error in e.Errors)
                {
                    if (error.Message == _operationCanceledErrorMessage)
                    {
                        // ignore this one - expected if canceled
                        expected = true;
                        break;
                    }
                    else if (error.Message == _severeErrorMessage)
                    {
                        // A severe error occurred on the current command.  The results, if any, should be discarded.
                        expected = true;
                        break;
                    }
                }

                if (!expected)
                {
                    // rethrow to the user
                    foreach (PgError error in e.Errors)
                    {
                        Console.WriteLine("{0} {1}", error.Code, error.Message);
                    }
                    throw;
                }
            }
            catch (InvalidOperationException e)
            {
                bool expected = false;

                if (e.Message == _operationCanceledErrorMessage)
                {
                    // "Operation canceled" exception is raised as a PgException (as one of PgError objects) and as InvalidOperationException
                    expected = true;
                }

                if (!expected)
                {
                    throw;
                }
            }
        }
    }
}
