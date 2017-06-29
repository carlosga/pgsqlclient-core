// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Data;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class TransactionTest
    {
        [Fact]
        public void Run()
        {
            new TransactionTestWorker(DataTestClass.PostgreSql_Northwind).StartTest();
        }
    }

    internal class TransactionTestWorker
    {
        private static string s_tempTableName1 = DataTestClass.GetUniqueName("TEST_", string.Empty, string.Empty);
        private static string s_tempTableName2 = s_tempTableName1 + "_2";
        private string _connectionString;

        public TransactionTestWorker(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void StartTest()
        {
            try
            {
                PrepareTables();

                CommitTransactionTest();
                ResetTables();

                RollbackTransactionTest();
                ResetTables();

                ScopedTransactionTest();
                ResetTables();

                ExceptionTest();
                ResetTables();

                // http://www.postgresql.org/docs/9.1/static/sql-set-transaction.html
                // The SQL standard defines one additional level, READ UNCOMMITTED. In PostgreSQL READ UNCOMMITTED is treated as READ COMMITTED.
                // ReadUncommittedIsolationLevel_ShouldReturnUncommittedData();
                // ResetTables();

                ReadCommitedIsolationLevel_ShouldReceiveTimeoutExceptionBecauseItWaitsForUncommittedTransaction();
                ResetTables();
            }
            finally
            {
                //make sure to clean up
                DropTempTables();
            }
        }

        private void PrepareTables()
        {
            using (var conn = new PgConnection(_connectionString))
            {
                conn.Open();
                PgCommand command = new PgCommand($"CREATE TABLE {s_tempTableName1} (CustomerID char(5) NOT NULL PRIMARY KEY, CompanyName varchar(40) NOT NULL, ContactName varchar(30) NULL); CREATE TABLE {s_tempTableName2} (col1 int, col2 varchar(32))", conn);
                command.ExecuteNonQuery();
            }
        }

        private void DropTempTables()
        {
            using (var conn = new PgConnection(_connectionString))
            {
                var command = new PgCommand($"DROP TABLE {s_tempTableName1}; DROP TABLE {s_tempTableName2}", conn);
                conn.Open();
                command.ExecuteNonQuery();
            }
        }

        public void ResetTables()
        {
            using (var connection = new PgConnection(_connectionString))
            {
                connection.Open();
                using (var command = new PgCommand($"TRUNCATE TABLE {s_tempTableName1}; TRUNCATE TABLE {s_tempTableName2}", connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void CommitTransactionTest()
        {
            using (PgConnection connection = new PgConnection(_connectionString))
            {
                var command = new PgCommand($"SELECT * FROM {s_tempTableName1} WHERE CustomerID='ZYXWV'", connection);

                connection.Open();

                PgTransaction tx = connection.BeginTransaction();
                command.Transaction = tx;

                using (PgDataReader reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows, "Error: table is in incorrect state for test.");
                }

                using (PgCommand command2 = connection.CreateCommand())
                {
                    command2.Transaction = tx;

                    command2.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXWV', 'XYZ', 'John' );";
                    command2.ExecuteNonQuery();
                }

                tx.Commit();

                using (PgDataReader reader = command.ExecuteReader())
                {
                    int count = 0;
                    while (reader.Read()) { count++; }
                    Assert.True(count == 1, "Error: incorrect number of rows in table after update.");
                    Assert.Equal(count, 1);
                }
            }
        }

        private void RollbackTransactionTest()
        {
            using (var connection = new PgConnection(_connectionString))
            {
                var command = new PgCommand($"SELECT * FROM {s_tempTableName1} WHERE CustomerID='ZYXWV'", connection);
                connection.Open();

                PgTransaction tx = connection.BeginTransaction();
                command.Transaction = tx;

                using (PgDataReader reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows, "Error: table is in incorrect state for test.");
                }

                using (PgCommand command2 = connection.CreateCommand())
                {
                    command2.Transaction = tx;

                    command2.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXWV', 'XYZ', 'John' );";
                    command2.ExecuteNonQuery();
                }

                tx.Rollback();

                using (PgDataReader reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows, "Error Rollback Test : incorrect number of rows in table after rollback.");
                    int count = 0;
                    while (reader.Read()) count++;
                    Assert.Equal(count, 0);
                }

                connection.Close();
            }
        }

        private void ScopedTransactionTest()
        {
            using (PgConnection connection = new PgConnection(_connectionString))
            {
                PgCommand command = new PgCommand($"SELECT * FROM {s_tempTableName1} WHERE CustomerID='ZYXWV'",
                    connection);

                connection.Open();

                PgTransaction tx = connection.BeginTransaction("transName");
                command.Transaction = tx;

                using (PgDataReader reader = command.ExecuteReader())
                {
                    Assert.False(reader.HasRows, "Error: table is in incorrect state for test.");
                }
                using (PgCommand command2 = connection.CreateCommand())
                {
                    command2.Transaction = tx;

                    command2.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXWV', 'XYZ', 'John' );";
                    command2.ExecuteNonQuery();
                }
                tx.Save("saveName");

                //insert another one
                using (PgCommand command2 = connection.CreateCommand())
                {
                    command2.Transaction = tx;

                    command2.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXW2', 'XY2', 'KK' );";
                    command2.ExecuteNonQuery();
                }

                tx.Rollback("saveName");

                using (PgDataReader reader = command.ExecuteReader())
                {
                    Assert.True(reader.HasRows, "Error Scoped Transaction Test : incorrect number of rows in table after rollback to save state one.");
                    int count = 0;
                    while (reader.Read()) count++;
                    Assert.Equal(count, 1);
                }

                tx.Rollback();

                connection.Close();
            }
        }

        private void ExceptionTest()
        {
            using (PgConnection connection = new PgConnection(_connectionString))
            {
                connection.Open();

                PgTransaction tx = connection.BeginTransaction();

                string invalidSaveStateMessage = "Invalid transaction or invalid name for a point at which to save within the transaction.";
                string executeCommandWithoutTransactionMessage = "ExecuteNonQuery requires the command to have a transaction when the connection assigned to the command is in a pending local transaction. The Transaction property of the command has not been initialized.";
                string transactionConflictErrorMessage = "The transaction is either not associated with the current connection or has been completed.";
                string parallelTransactionErrorMessage = "A transaction is currently active. Parallel transactions are not supported.";

                AssertException<InvalidOperationException>(() =>
                {
                    var command = new PgCommand("sql", connection);
                    command.ExecuteNonQuery();
                }, executeCommandWithoutTransactionMessage);

                AssertException<InvalidOperationException>(() =>
                {
                    var con1 = new PgConnection(_connectionString);
                    con1.Open();

                    var command = new PgCommand("sql", con1);
                    command.Transaction = tx;
                    command.ExecuteNonQuery();
                }, transactionConflictErrorMessage);

                AssertException<InvalidOperationException>(() =>
                {
                    connection.BeginTransaction(null);
                }, parallelTransactionErrorMessage);

                AssertException<InvalidOperationException>(() =>
                {
                    connection.BeginTransaction(string.Empty);
                }, parallelTransactionErrorMessage);

                AssertException<ArgumentException>(() =>
                {
                    tx.Rollback(null);
                }, invalidSaveStateMessage);

                AssertException<ArgumentException>(() =>
                {
                    tx.Rollback(string.Empty);
                }, invalidSaveStateMessage);

                AssertException<ArgumentException>(() =>
                {
                    tx.Save(null);
                }, invalidSaveStateMessage);

                AssertException<ArgumentException>(() =>
                {
                    tx.Save(string.Empty);
                }, invalidSaveStateMessage);
            }
        }

        public static void AssertException<T>(Action action, string expectedErrorMessage) where T : Exception
        {
            var exception = Assert.Throws<T>(action);
            Assert.Equal(exception.Message, expectedErrorMessage);
        }

        private void ReadUncommittedIsolationLevel_ShouldReturnUncommittedData()
        {
            using (PgConnection connection1 = new PgConnection(_connectionString))
            {
                connection1.Open();
                PgTransaction tx1 = connection1.BeginTransaction();

                using (PgCommand command1 = connection1.CreateCommand())
                {
                    command1.Transaction = tx1;

                    command1.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXWV', 'XYZ', 'John' );";
                    command1.ExecuteNonQuery();
                }
                using (PgConnection connection2 = new PgConnection(_connectionString))
                {
                    var command2 =
                        new PgCommand($"SELECT * FROM {s_tempTableName1} WHERE CustomerID='ZYXWV'",
                            connection2);
                    connection2.Open();
                    PgTransaction tx2 = connection2.BeginTransaction(IsolationLevel.ReadUncommitted);
                    command2.Transaction = tx2;

                    using (PgDataReader reader = command2.ExecuteReader())
                    {
                        int count = 0;
                        while (reader.Read()) count++;
                        Assert.True(count == 1, "Should Expected 1 row because Isolation Level is read uncommitted which should return uncommitted data.");
                    }

                    tx2.Rollback();
                    connection2.Close();
                }

                tx1.Rollback();
                connection1.Close();
            }
        }

        private void ReadCommitedIsolationLevel_ShouldReceiveTimeoutExceptionBecauseItWaitsForUncommittedTransaction()
        {
            using (PgConnection connection1 = new PgConnection(_connectionString))
            {
                connection1.Open();
                PgTransaction tx1 = connection1.BeginTransaction();

                using (PgCommand command1 = connection1.CreateCommand())
                {
                    command1.Transaction = tx1;
                    command1.CommandText = $"INSERT INTO {s_tempTableName1} VALUES ( 'ZYXWV', 'XYZ', 'John' );";
                    command1.ExecuteNonQuery();
                }

                using (PgConnection connection2 = new PgConnection(_connectionString))
                {
                    PgCommand command2 =
                        new PgCommand($"SELECT * FROM {s_tempTableName1} WHERE CustomerID='ZYXWV'",
                            connection2);

                    connection2.Open();
                    PgTransaction tx2 = connection2.BeginTransaction(IsolationLevel.ReadCommitted);
                    command2.Transaction = tx2;

                    int count = 0;

                    using (var reader = command2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count++;
                        }
                    }

                    Assert.True(count == 0, "Should Expected 0 rows because Isolation Level is read committed which should not return uncommitted data.");

                    tx2.Rollback();
                    connection2.Close();
                }

                tx1.Rollback();
                connection1.Close();
            }
        }
    }
}
