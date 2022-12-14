// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class MARSTest
    {
        private static readonly string s_ConnectionString = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { MultipleActiveResultSets = true }).ConnectionString;

#if DEBUG
        [Fact]
        public static void MARSAsyncTimeoutTest()
        {
            var connectionString = (new PgConnectionStringBuilder(s_ConnectionString) { CommandTimeout = 1}).ConnectionString;

            using (PgConnection connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgCommand command = new PgCommand("SELECT pg_sleep(1);SELECT 1", connection);
                command.CommandTimeout = 1;
                Task<object> result = command.ExecuteScalarAsync();

                Assert.True(((IAsyncResult)result).AsyncWaitHandle.WaitOne(30 * 1000), "Expected timeout after one second, but no results after 30 seconds");
                Assert.True(result.IsFaulted, string.Format("Expected task result to be faulted, but instead it was {0}", result.Status));
                Assert.True(connection.State == ConnectionState.Open, string.Format("Expected connection to be open after soft timeout, but it was {0}", connection.State));

                PgCommand command2 = new PgCommand("SELECT pg_sleep(1);SELECT 1", connection);
                command2.CommandTimeout = 1;
                result = command2.ExecuteScalarAsync();

                Assert.True(((IAsyncResult)result).AsyncWaitHandle.WaitOne(30 * 1000), "Expected timeout after six or so seconds, but no results after 30 seconds");
                Assert.True(result.IsFaulted, string.Format("Expected task result to be faulted, but instead it was {0}", result.Status));

                // Pause here to ensure that the async closing is completed
                Thread.Sleep(200);
#warning Original test expects the connection to be closed
                Assert.True(connection.State == ConnectionState.Open, string.Format("Expected connection to be open after hard timeout, but it was {0}", connection.State));
            }
        }

        [Fact]
        public static void MARSSyncTimeoutTest()
        {
            var connectionString = (new PgConnectionStringBuilder(s_ConnectionString) { CommandTimeout = 1}).ConnectionString;
            
            using (PgConnection connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgCommand command = new PgCommand("SELECT pg_sleep(1);SELECT 1", connection);
                command.CommandTimeout = 1;
                bool hitException = false;
                try
                {
                    object result = command.ExecuteScalar();
                }
                catch (Exception e)
                {
                    Assert.True(e is PgException, "Expected PgException but found " + e);
                    hitException = true;
                }
                Assert.True(hitException, "Expected a timeout exception but ExecutScalar succeeded");

                Assert.True(connection.State == ConnectionState.Open, string.Format("Expected connection to be open after soft timeout, but it was {0}", connection.State));

                hitException = false;

                PgCommand command2 = new PgCommand("SELECT pg_sleep(1);SELECT 1", connection);
                command2.CommandTimeout = 1;
                try
                {
                    object result = command2.ExecuteScalar();
                }
                catch (Exception e)
                {
                    Assert.True(e is PgException, "Expected PgException but found " + e);
                    hitException = true;
                }
                Assert.True(hitException, "Expected a timeout exception but ExecutScalar succeeded");

#warning Original test expects the connection to be closed
                Assert.True(connection.State == ConnectionState.Open, string.Format("Expected connection to be open after hard timeout, but it was {0}", connection.State));
            }
        }
#endif

        [Fact]
        public static void MARSSyncBusyReaderTest()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    int rows1 = 0;
                    while (reader1.Read())
                    {
                        rows1++;
                        if (rows1 == 415)
                            break;
                    }
                    Assert.True(rows1 == 415, "MARSSyncBusyReaderTest Failure, #1");

                    using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                    {
                        int rows2 = 0;
                        while (reader2.Read())
                        {
                            rows2++;
                            if (rows2 == 415)
                                break;
                        }
                        Assert.True(rows2 == 415, "MARSSyncBusyReaderTest Failure, #2");

                        for (int i = 415; i < 830; i++)
                        {
                            Assert.True(reader1.Read() && reader2.Read(), "MARSSyncBusyReaderTest Failure #3");
                            Assert.True(reader1.GetInt32(0) == reader2.GetInt32(0),
                                        "MARSSyncBusyReaderTest, Failure #4" + "\n" +
                                        "reader1.GetInt32(0): " + reader1.GetInt32(0) + "\n" +
                                        "reader2.GetInt32(0): " + reader2.GetInt32(0));
                        }

                        Assert.False(reader1.Read() || reader2.Read(), "MARSSyncBusyReaderTest, Failure #5");
                    }
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteNonQueryTest()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgCommand comm1 = new PgCommand("select * from Orders", conn))
                using (PgCommand comm2 = new PgCommand("select * from Orders", conn))
                using (PgCommand comm3 = new PgCommand("select * from Orders", conn))
                using (PgCommand comm4 = new PgCommand("select * from Orders", conn))
                using (PgCommand comm5 = new PgCommand("select * from Orders", conn))
                {
                    comm1.ExecuteNonQuery();
                    comm2.ExecuteNonQuery();
                    comm3.ExecuteNonQuery();
                    comm4.ExecuteNonQuery();
                    comm5.ExecuteNonQuery();
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteReaderTest1()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader4 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader5 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    int rows = 0;
                    while (reader1.Read())
                    {
                        rows++;
                    }
                    Assert.True(rows == 830, "MARSSyncExecuteReaderTest1 failure, #1");

                    rows = 0;
                    while (reader2.Read())
                    {
                        rows++;
                    }
                    Assert.True(rows == 830, "MARSSyncExecuteReaderTest1 failure, #2");

                    rows = 0;
                    while (reader3.Read())
                    {
                        rows++;
                    }
                    Assert.True(rows == 830, "MARSSyncExecuteReaderTest1 failure, #3");

                    rows = 0;
                    while (reader4.Read())
                    {
                        rows++;
                    }
                    Assert.True(rows == 830, "MARSSyncExecuteReaderTest1 failure, #4");

                    rows = 0;
                    while (reader5.Read())
                    {
                        rows++;
                    }
                    Assert.True(rows == 830, "MARSSyncExecuteReaderTest1 failure, #5");
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteReaderTest2()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader4 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader5 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    for (int i = 0; i < 830; i++)
                    {
                        Assert.True(reader1.Read() && reader2.Read() && reader3.Read() && reader4.Read() && reader5.Read(), "MARSSyncExecuteReaderTest2 Failure #1");
                    }

                    Assert.False(reader1.Read() || reader2.Read() || reader3.Read() || reader4.Read() || reader5.Read(), "MARSSyncExecuteReaderTest2 Failure #2");
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteReaderTest3()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader4 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader5 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    for (int i = 0; i < 830; i++)
                    {
                        Assert.True(reader1.Read() && reader2.Read() && reader3.Read() && reader4.Read() && reader5.Read(), "MARSSyncExecuteReaderTest3 Failure #1");

                        // All reads succeeded - check values.
                        Assert.True(reader1.GetInt32(0) == reader2.GetInt32(0) &&
                                    reader2.GetInt32(0) == reader3.GetInt32(0) &&
                                    reader3.GetInt32(0) == reader4.GetInt32(0) &&
                                    reader4.GetInt32(0) == reader5.GetInt32(0),
                                    "MARSSyncExecuteReaderTest3, Failure #2" + "\n" +
                                    "reader1.GetInt32(0): " + reader1.GetInt32(0) + "\n" +
                                    "reader2.GetInt32(0): " + reader2.GetInt32(0) + "\n" +
                                    "reader3.GetInt32(0): " + reader3.GetInt32(0) + "\n" +
                                    "reader4.GetInt32(0): " + reader4.GetInt32(0) + "\n" +
                                    "reader5.GetInt32(0): " + reader5.GetInt32(0));
                    }

                    Assert.False(reader1.Read() || reader2.Read() || reader3.Read() || reader4.Read() || reader5.Read(), "MARSSyncExecuteReaderTest3 Failure #3");
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteReaderTest4()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders where OrderID = 10248", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders where OrderID = 10249", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders where OrderID = 10250", conn)).ExecuteReader())
                {
                    Assert.True(reader1.Read() && reader2.Read() && reader3.Read(), "MARSSyncExecuteReaderTest4 failure #1");

                    Assert.True(reader1.GetInt32(0) == 10248 &&
                                reader2.GetInt32(0) == 10249 &&
                                reader3.GetInt32(0) == 10250,
                                "MARSSyncExecuteReaderTest4 failure #2");

                    Assert.False(reader1.Read() || reader2.Read() || reader3.Read(), "MARSSyncExecuteReaderTest4 failure #3");
                }
            }
        }

        [Fact]
        public static void MARSSyncExecuteReaderWithExplicitTransactionTest()
        {
            using (PgConnection conn = new PgConnection(s_ConnectionString))
            {
                conn.Open();

                using (var tx = conn.BeginTransaction())
                {
                    using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn, tx)).ExecuteReader())
                    using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn, tx)).ExecuteReader())
                    using (PgDataReader reader3 = (new PgCommand("select * from Orders", conn, tx)).ExecuteReader())
                    using (PgDataReader reader4 = (new PgCommand("select * from Orders", conn, tx)).ExecuteReader())
                    using (PgDataReader reader5 = (new PgCommand("select * from Orders", conn, tx)).ExecuteReader())
                    {
                        for (int i = 0; i < 830; i++)
                        {
                            Assert.True(reader1.Read() && reader2.Read() && reader3.Read() && reader4.Read() && reader5.Read(), "MARSSyncExecuteReaderTest2 Failure #1");
                        }

                        Assert.False(reader1.Read() || reader2.Read() || reader3.Read() || reader4.Read() || reader5.Read(), "MARSSyncExecuteReaderTest2 Failure #2");
                    }

                    tx.Commit();
                }
            }
        }

        [Fact]
        public static void ThrowsWhenMARSSupportIsDisabled()
        {
            var connectionString = (new PgConnectionStringBuilder(s_ConnectionString) { MultipleActiveResultSets = false}).ConnectionString;
            using (PgConnection conn = new PgConnection(connectionString))
            {
                conn.Open();

                bool yes = conn is System.ComponentModel.Component;

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    using (var command2 = new PgCommand("select * from Orders", conn))
                    {
                        Assert.Throws<InvalidOperationException>(() => command2.ExecuteReader());
                    }
                }
            }
        }
    }
}


