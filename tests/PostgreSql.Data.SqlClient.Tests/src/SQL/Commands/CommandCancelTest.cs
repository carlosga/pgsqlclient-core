// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Threading;
using System.Data;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class CommandCancelTest
    {
        // Shrink the packet size - this should make timeouts more likely
        private static string s_constr = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { PacketSize = 512 }).ConnectionString;

        [Fact]
        public void MultiThreadedCancel_NonAsync()
        {
            MultiThreadedCancel(s_constr, false);
        }

        [Fact]
        public void TimeoutCancel()
        {
            InternalTimeoutCancel(s_constr);
        }

        [Fact]
        public void CancelAndDisposePreparedCommand()
        {
            InternalCancelAndDisposePreparedCommand(s_constr);
        }

        private void MultiThreadedCancel(string constr, bool async)
        {
            using (var con = new PgConnection(constr))
            {
                con.Open();
                var command = con.CreateCommand();
                command.CommandText = "SELECT * FROM orders; SELECT pg_sleep(8); SELECT * FROM customers";

                Thread rThread1 = new Thread(ExecuteCommandCancelExpected);
                Thread rThread2 = new Thread(CancelSharedCommand);
                Barrier threadsReady = new Barrier(2);
                object state = new Tuple<bool, PgCommand, Barrier>(async, command, threadsReady);

                rThread1.Start(state);
                rThread2.Start(state);
                rThread1.Join();
                rThread2.Join();

                CommandCancelTest.VerifyConnection(command);
            }
        }

        private void InternalTimeoutCancel(string constr)
        {
            var connectionString = (new PgConnectionStringBuilder(constr) { CommandTimeout = 1 }).ConnectionString;
            using (var con = new PgConnection(connectionString))
            {
                con.Open();
                PgCommand cmd = con.CreateCommand();

                cmd.CommandTimeout = 1;
                cmd.CommandText    = "SELECT pg_sleep(30);SELECT * FROM customers";

                // string errorMessage = "Timeout expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.";
                string errorMessage = "canceling statement due to statement timeout";
                DataTestClass.ExpectFailure<PgException>(() => cmd.ExecuteReader(), errorMessage);

                VerifyConnection(cmd);
            }
        }

        private static void InternalCancelAndDisposePreparedCommand(string constr)
        {
            int expectedValue = 1;
            using (var connection = new PgConnection(constr))
            {
                try
                {
                    // Generate a query with a large number of results.
                    using (var command = new PgCommand("select @P from pg_type a cross join pg_type b", connection))
                    {
                        command.Parameters.Add(new PgParameter("@P", PgDbType.Integer) { Value = expectedValue });
                        connection.Open();
                        command.Prepare();
                        using (var reader = command.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            if (reader.Read())
                            {
                                int actualValue = reader.GetInt32(0);
                                Assert.True(actualValue == expectedValue, string.Format("Got incorrect value. Expected: {0}, Actual: {1}", expectedValue, actualValue));
                            }
                            // Abandon reading the results.
                            command.Cancel();
                        }
                    }
                }
                finally
                {
                    connection.Dispose();
                }
            }
        }

        private static void VerifyConnection(PgCommand cmd)
        {
            Assert.True(cmd.Connection.State == ConnectionState.Open, "FAILURE: - unexpected non-open state after Execute!");

            cmd.CommandText = "select 'ABC'"; // Verify Connection
            string value = (string)cmd.ExecuteScalar();
            Assert.True(value == "ABC", "FAILURE: upon validation execute on connection: '" + value + "'");
        }

        private void ExecuteCommandCancelExpected(object state)
        {
            var       stateTuple   = (Tuple<bool, PgCommand, Barrier>)state;
            bool      async        = stateTuple.Item1;
            PgCommand command      = stateTuple.Item2;
            Barrier   threadsReady = stateTuple.Item3;

            string errorMessage = "Operation cancelled by user.";
            DataTestClass.ExpectFailure<PgException>(() =>
            {
                threadsReady.SignalAndWait();
                using (PgDataReader r = command.ExecuteReader())
                {
                    do
                    {
                        while (r.Read())
                        {
                        }
                    } while (r.NextResult());
                }
            }, errorMessage);
        }

        private static void CancelSharedCommand(object state)
        {
            var stateTuple = (Tuple<bool, PgCommand, Barrier>)state;

            // sleep 1 seconds before cancel to ensure ExecuteReader starts and 
            // ensure it does not end before Cancel is called (command is running WAITFOR 8 seconds)
            stateTuple.Item3.SignalAndWait();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            stateTuple.Item2.Cancel();
        }
    }
}
