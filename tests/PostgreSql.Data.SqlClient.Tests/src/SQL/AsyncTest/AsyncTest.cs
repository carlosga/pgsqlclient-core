// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using System.Data.Common;
using System;
using Xunit;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class AsyncTest
    {
        private const int TaskTimeout = 5000;

        [Fact]
        public static void ExecuteTest()
        {
            var connStr = DataTestClass.PostgreSql_Northwind;
            var com     = new PgCommand("select * from orders");
            var con     = new PgConnection(connStr);

            com.Connection = con;

            con.Open();

            var  readerTask    = com.ExecuteReaderAsync();
            bool taskCompleted = readerTask.Wait(TaskTimeout);
            Assert.True(taskCompleted, "FAILED: ExecuteReaderAsync Task did not complete successfully.");

            DbDataReader reader = readerTask.Result;

            int rows;
            for (rows = 0; reader.Read(); rows++) ;

            Assert.True(rows == 830, string.Format("FAILED: ExecuteTest reader had wrong number of rows. Expected: {0}. Actual: {1}", 830, rows));

            reader.Dispose();
            con.Close();
        }

        [Fact(Skip = "disabled")]
        public static void FailureTest()
        {
            var  connStr       = DataTestClass.PostgreSql_Northwind;
            bool failure       = false;
            bool taskCompleted = false;

            var com = new PgCommand("select * from orders");
            var con = new PgConnection(connStr + "pooling=false");
            
            com.Connection = con;
            con.Open();

            Task<int> nonQueryTask = com.ExecuteNonQueryAsync();
            
            try
            {
                com.ExecuteNonQueryAsync().Wait(TaskTimeout);
            }
            catch (AggregateException agrEx)
            {
                agrEx.Handle(
                    (ex) =>
                    {
                        Assert.True(ex is InvalidOperationException, "FAILED: Thrown exception for ExecuteNonQueryAsync was not an InvalidOperationException: " + ex.Message);
                        failure = true;
                        return true;
                    });
            }
            
            Assert.True(failure, "FAILED: No exception thrown after trying second ExecuteNonQueryAsync.");
            failure = false;

            taskCompleted = nonQueryTask.Wait(TaskTimeout);
            Assert.True(taskCompleted, "FAILED: ExecuteNonQueryAsync Task did not complete successfully.");

            var readerTask = com.ExecuteReaderAsync();
            try
            {
                com.ExecuteReaderAsync().Wait(TaskTimeout);
            }
            catch (AggregateException agrEx)
            {
                agrEx.Handle(
                    (ex) =>
                    {
                        Assert.True(ex is InvalidOperationException, "FAILED: Thrown exception for ExecuteReaderAsync was not an InvalidOperationException: " + ex.Message);
                        failure = true;
                        return true;
                    });
            }
            Assert.True(failure, "FAILED: No exception thrown after trying second ExecuteReaderAsync.");

            taskCompleted = readerTask.Wait(TaskTimeout);
            Assert.True(taskCompleted, "FAILED: ExecuteReaderAsync Task did not complete successfully.");

            readerTask.Result.Dispose();
            con.Close();
        }
    }
}
