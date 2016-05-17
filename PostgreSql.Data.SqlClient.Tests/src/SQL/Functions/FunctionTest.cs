// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.Data;
using System.Text;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class FunctionTest
    {
        [Fact]
        public static void CallFunctionWithDefaultSyntaxTest()
        {
            string[] expectedResults = { "213-46-8915", "267-41-2394", "672-71-3249" };
            int      rowCount        = 0;

            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Pubs))
            {
                connection.Open();
                
                using (PgCommand command = new PgCommand("byroyalty", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@percentage", 40);

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Assert.True(rowCount <= expectedResults.Length, "ERROR: Received more results for this batch than was expected");
                            string expectedVal = expectedResults[rowCount];
                            string actualVal   = reader.GetString(0);
                            DataTestClass.AssertEqualsWithDescription(expectedVal, actualVal, "FAILED: Received a different value than expected.");

                            rowCount++;
                        }
                    }
                }
            }
        }

        [Fact]
        public static void CallFunctionWithAlternativeSyntaxTest()
        {
            string[] expectedResults = { "213-46-8915", "267-41-2394", "672-71-3249" };
            int      rowCount        = 0;

            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Pubs))
            {
                connection.Open();
                
                using (PgCommand command = new PgCommand("select * from byroyalty(@percentage)", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@percentage", 40);

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Assert.True(rowCount <= expectedResults.Length, "ERROR: Received more results for this batch than was expected");
                            string expectedVal = expectedResults[rowCount];
                            string actualVal   = reader.GetString(0);
                            DataTestClass.AssertEqualsWithDescription(expectedVal, actualVal, "FAILED: Received a different value than expected.");

                            rowCount++;
                        }
                    }
                }
            }
        }
        
        [Fact]
        public static void CallFunctionWithOutputParameter()
        {
            var functionName = DataTestClass.GetUniqueName("F", String.Empty, String.Empty);
            var createFuncSql  = new StringBuilder();

            createFuncSql.AppendFormat("CREATE OR REPLACE FUNCTION {0}() ", functionName);
            createFuncSql.Append("RETURNS int8 AS");
            createFuncSql.Append("'");
            createFuncSql.Append("SELECT COUNT(*) FROM authors;");
            createFuncSql.Append("'");
            createFuncSql.Append("LANGUAGE 'sql' VOLATILE;");

            try
            {
                long expectedResult = 23;

                using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Pubs))
                {
                    connection.Open();

                    using (PgCommand createFunc = new PgCommand(createFuncSql.ToString(), connection))
                    {
                        createFunc.ExecuteNonQuery();
                    }
                    using (PgCommand command = new PgCommand(functionName, connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add("@CountResult", PgDbType.BigInt).Direction = ParameterDirection.Output;
                        command.ExecuteNonQuery();
                        DataTestClass.AssertEqualsWithDescription(expectedResult, command.Parameters[0].Value, "FAILED: Received a different value than expected.");
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Pubs))
                {
                    connection.Open();

                    using (PgCommand command = new PgCommand($"DROP FUNCTION IF EXISTS {functionName}()", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
