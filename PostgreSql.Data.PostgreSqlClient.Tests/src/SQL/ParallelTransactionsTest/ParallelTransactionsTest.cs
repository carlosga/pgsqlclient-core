// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    public class ParallelTransactionsTest
    {
        #region <<Basic Parallel Test>>
        [Fact]
        public void BasicParallelTest_ShouldThrowsUnsupported_Yukon()
        {
            BasicParallelTest_shouldThrowsUnsupported(DataTestClass.SQL2005_Pubs);
        }

        [Fact]
        public void BasicParallelTest_ShouldThrowsUnsupported_Katmai()
        {
            BasicParallelTest_shouldThrowsUnsupported(DataTestClass.SQL2008_Pubs);
        }

        private void BasicParallelTest_shouldThrowsUnsupported(string connectionString)
        {
            string expectedErrorMessage = SystemDataResourceManager.Instance.ADP_ParallelTransactionsNotSupported(typeof(PgConnection).Name);
            string tempTableName = "";
            try
            {
                tempTableName = CreateTempTable(connectionString);
                DataTestClass.AssertThrowsWrapper<InvalidOperationException>(
                    actionThatFails: () => { BasicParallelTest(connectionString, tempTableName); },
                    exceptionMessage: expectedErrorMessage);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempTableName))
                {
                    DropTempTable(connectionString, tempTableName);
                }
            }
        }

        private void BasicParallelTest(string connectionString, string tempTableName)
        {
            using (var connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgTransaction trans1 = connection.BeginTransaction();
                PgTransaction trans2 = connection.BeginTransaction();
                PgTransaction trans3 = connection.BeginTransaction();

                PgCommand com1 = new PgCommand("select top 1 au_id from " + tempTableName, connection);
                com1.Transaction = trans1;
                com1.ExecuteNonQuery();

                PgCommand com2 = new PgCommand("select top 1 au_id from " + tempTableName, connection);
                com2.Transaction = trans2;
                com2.ExecuteNonQuery();

                PgCommand com3 = new PgCommand("select top 1 au_id from " + tempTableName, connection);
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

        #endregion

        #region <<MultipleExecutesInSameTransactionTest>>
        [Fact]
        public void MultipleExecutesInSameTransactionTest_ShouldThrowsUnsupported_Yukon()
        {
            MultipleExecutesInSameTransactionTest_shouldThrowsUnsupported(DataTestClass.SQL2005_Pubs);
        }

        [Fact]
        public void MultipleExecutesInSameTransactionTest_ShouldThrowsUnsupported_Katmai()
        {
            MultipleExecutesInSameTransactionTest_shouldThrowsUnsupported(DataTestClass.SQL2008_Northwind);
        }

        private void MultipleExecutesInSameTransactionTest_shouldThrowsUnsupported(string connectionString)
        {
            string expectedErrorMessage = SystemDataResourceManager.Instance.ADP_ParallelTransactionsNotSupported(typeof(PgConnection).Name);
            string tempTableName = "";
            try
            {
                tempTableName = CreateTempTable(connectionString);
                DataTestClass.AssertThrowsWrapper<InvalidOperationException>(
                    actionThatFails: () => { MultipleExecutesInSameTransactionTest(connectionString, tempTableName); },
                    exceptionMessage: expectedErrorMessage);
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempTableName))
                {
                    DropTempTable(connectionString, tempTableName);
                }
            }
        }

        private void MultipleExecutesInSameTransactionTest(string connectionString, string tempTableName)
        {
            using (var connection = new PgConnection(connectionString))
            {
                connection.Open();
                PgTransaction trans1 = connection.BeginTransaction();
                PgTransaction trans2 = connection.BeginTransaction();
                PgTransaction trans3 = connection.BeginTransaction();

                PgCommand com1 = new PgCommand($"select au_id from {tempTableName} limit 1", connection);
                com1.Transaction = trans1;
                com1.ExecuteNonQuery();

                PgCommand com2 = new PgCommand($"select au_id from {tempTableName} limit 1", connection);
                com2.Transaction = trans2;
                com2.ExecuteNonQuery();

                PgCommand com3 = new PgCommand($"select au_id from {tempTableName} limit 1", connection);
                com3.Transaction = trans3;
                com3.ExecuteNonQuery();

                trans1.Rollback();
                trans2.Rollback();
                trans3.Rollback();

                com1.Dispose();
                com2.Dispose();
                com3.Dispose();

                PgCommand com4 = new PgCommand($"select au_id from {tempTableName} limit 1", connection);
                com4.Transaction = trans1;
                SqlDataReader reader4 = com4.ExecuteReader();
                reader4.Dispose();
                com4.Dispose();

                trans1.Rollback();
            }
        }
        #endregion

        private string CreateTempTable(string connectionString)
        {
            var uniqueKey = string.Format("{0}_{1}_{2}", Environment.GetEnvironmentVariable("ComputerName"), Environment.TickCount, Guid.NewGuid()).Replace("-", "_");
            var tempTableName = "TEMP_" + uniqueKey;
            using (var conn = new PgConnection(connectionString))
            {
                conn.Open();
                PgCommand cmd = new PgCommand(string.Format("SELECT au_id, au_lname, au_fname, phone, address, city, state, zip, contract into {0} from pubs.dbo.authors", tempTableName), conn);
                cmd.ExecuteNonQuery();
                cmd.CommandText = string.Format("alter table {0} add constraint au_id_{1} primary key (au_id)", tempTableName, uniqueKey);
                cmd.ExecuteNonQuery();
            }

            return tempTableName;
        }

        private void DropTempTable(string connectionString, string tempTableName)
        {
            using (PgConnection con1 = new PgConnection(connectionString))
            {
                con1.Open();
                PgCommand cmd = new PgCommand("Drop table " + tempTableName, con1);
                cmd.ExecuteNonQuery();
            }
        }
    }
}


