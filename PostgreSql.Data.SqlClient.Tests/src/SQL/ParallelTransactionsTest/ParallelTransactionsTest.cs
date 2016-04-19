// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class ParallelTransactionsTest
    {
        [Fact]
        public void BasicParallelTestShouldThrowsUnsupported()
        {
            string connectionString     = DataTestClass.PostgreSql9_Pubs;
            string expectedErrorMessage = "A transaction is currently active. Parallel transactions are not supported.";
            
            DataTestClass.AssertThrowsWrapper<InvalidOperationException>(
                actionThatFails: () => { BasicParallelTest(connectionString); },
                exceptionMessage: expectedErrorMessage);
        }

        [Fact(Skip="disabled")]
        public void MultipleExecutesInSameTransactionTest_ShouldThrowsUnsupported()
        {
            string connectionString     = DataTestClass.PostgreSql9_Pubs;
            string expectedErrorMessage = "A transaction is currently active. Parallel transactions are not supported.";
            
            DataTestClass.AssertThrowsWrapper<InvalidOperationException>(
                actionThatFails: () => { MultipleExecutesInSameTransactionTest(connectionString); },
                exceptionMessage: expectedErrorMessage);
        }
        
        private void BasicParallelTest(string connectionString)
        {
            using (var connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgTransaction trans1 = connection.BeginTransaction();
                PgTransaction trans2 = connection.BeginTransaction();
                PgTransaction trans3 = connection.BeginTransaction();

                PgCommand com1 = new PgCommand("select au_id from authors limit 1", connection);
                com1.Transaction = trans1;
                com1.ExecuteNonQuery();

                PgCommand com2 = new PgCommand("select au_id from authors limit 1", connection);
                com2.Transaction = trans2;
                com2.ExecuteNonQuery();

                PgCommand com3 = new PgCommand("select au_id from authors limit 1", connection);
                com3.Transaction = trans3;
                com3.ExecuteNonQuery();

                trans1.Rollback();
                trans2.Rollback();
                trans3.Rollback();

                com1.Dispose();
                com2.Dispose();
                com3.Dispose();
            }
        }

        private void MultipleExecutesInSameTransactionTest(string connectionString)
        {
            using (var connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgTransaction trans1 = connection.BeginTransaction();
                PgTransaction trans2 = connection.BeginTransaction();
                PgTransaction trans3 = connection.BeginTransaction();

                PgCommand com1 = new PgCommand("select au_id from authors limit 1", connection);
                com1.Transaction = trans1;
                com1.ExecuteNonQuery();

                PgCommand com2 = new PgCommand("select au_id from authors limit 1", connection);
                com2.Transaction = trans2;
                com2.ExecuteNonQuery();

                PgCommand com3 = new PgCommand("select au_id from authors limit 1", connection);
                com3.Transaction = trans3;
                com3.ExecuteNonQuery();

                trans1.Rollback();
                trans2.Rollback();
                trans3.Rollback();

                com1.Dispose();
                com2.Dispose();
                com3.Dispose();

                PgCommand com4 = new PgCommand($"select au_id from authors limit 1", connection);
                com4.Transaction = trans1;
                PgDataReader reader4 = com4.ExecuteReader();
                reader4.Dispose();
                com4.Dispose();

                trans1.Rollback();
            }
        }
    }
}
