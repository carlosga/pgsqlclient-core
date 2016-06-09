// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Text;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class ReaderTest
    {
        [Fact]
        public static void TestMain()
        {
            string connectionString = DataTestClass.PostgreSql9_Pubs;
            string tempTable        = DataTestClass.GetUniqueName("T", "", "");
            string tempKey          = DataTestClass.GetUniqueName("K", "", "");

            var provider = PgSqlClientFactory.Instance;
            try
            {
                using (var con = provider.CreateConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    using (var cmd = provider.CreateCommand())
                    {
                        cmd.Connection = con;

                        #region <<Create temp table>>
                        cmd.CommandText = $"SELECT au_id, au_lname, au_fname, phone, address, city, state, zip, contract into {tempTable} from authors where au_id='UNKNOWN-ID'";
                        cmd.ExecuteNonQuery();

                        cmd.CommandText = $"alter table {tempTable} add constraint {tempKey} primary key (au_id)";
                        cmd.ExecuteNonQuery();

                        #endregion

                        var tx = con.BeginTransaction();
                        cmd.Transaction = tx;

                        cmd.CommandText = $"insert into {tempTable} (au_id, au_lname, au_fname, phone, address, city, state, zip, contract) values ('876-54-3210', 'Doe', 'Jane' , '882-8080', 'One Microsoft Way', 'Redmond', 'WA', '98052', false)";
                        cmd.ExecuteNonQuery();
                        
                        cmd.CommandText = $"insert into {tempTable} (au_id, au_lname, au_fname, phone, address, city, state, zip, contract) values ('876-54-3211', 'Doe', 'John' , '882-8181', NULL, NULL, NULL, NULL, false)";
                        cmd.ExecuteNonQuery();

                        tx.Commit();

                        cmd.Transaction = null;
                        
                        string parameterName = "@p1";
                        var p1 = cmd.CreateParameter();
                        p1.ParameterName = parameterName;
                        p1.Value = "876-54-3210";
                        cmd.Parameters.Add(p1);

                        cmd.CommandText = $"select * from {tempTable} where au_id >= {parameterName}";

                        // Test GetValue + IsDBNull
                        using (var rdr = cmd.ExecuteReader())
                        {
                            var actualResult = new StringBuilder();
                            int currentValue = 0;
                            string[] expectedValues =
                            {
                                "876-54-3210,Doe,Jane,882-8080,One Microsoft Way,Redmond,WA,98052,False",
                                "876-54-3211,Doe,John,882-8181,(NULL),(NULL),(NULL),(NULL),False"
                            };

                            while (rdr.Read())
                            {
                                Assert.True(currentValue < expectedValues.Length, "ERROR: Received more values than expected");

                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    if (i > 0)
                                    {
                                        actualResult.Append(",");
                                    }
                                    if (rdr.IsDBNull(i))
                                    {
                                        actualResult.Append("(NULL)");
                                    }
                                    else
                                    {
                                        actualResult.Append(rdr.GetValue(i));
                                    }
                                }

                                DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue++], actualResult.ToString(), "FAILED: Did not receive expected data");
                                actualResult.Clear();
                            }
                        }

                        // Test GetFieldValue<T> + IsDBNull
                        using (var rdr = cmd.ExecuteReader())
                        {
                            var      actualResult   = new StringBuilder();
                            int      currentValue   = 0;
                            string[] expectedValues =
                            {
                                "876-54-3210,Doe,Jane,882-8080,One Microsoft Way,Redmond,WA,98052,False",
                                "876-54-3211,Doe,John,882-8181,(NULL),(NULL),(NULL),(NULL),False"
                            };

                            while (rdr.Read())
                            {
                                Assert.True(currentValue < expectedValues.Length, "ERROR: Received more values than expected");

                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    if (i > 0)
                                    {
                                        actualResult.Append(",");
                                    }
                                    if (rdr.IsDBNull(i))
                                    {
                                        actualResult.Append("(NULL)");
                                    }
                                    else
                                    {
                                        if (rdr.GetFieldType(i) == typeof(bool))
                                        {
                                            actualResult.Append(rdr.GetFieldValue<bool>(i));
                                        }
                                        else if (rdr.GetFieldType(i) == typeof(decimal))
                                        {
                                            actualResult.Append(rdr.GetFieldValue<decimal>(i));
                                        }
                                        else
                                        {
                                            actualResult.Append(rdr.GetFieldValue<string>(i));
                                        }
                                    }
                                }

                                DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue++], actualResult.ToString(), "FAILED: Did not receive expected data");
                                actualResult.Clear();
                            }
                        }

                        // Test GetFieldValueAsync<T> + IsDBNullAsync
                        using (var rdr = cmd.ExecuteReaderAsync().Result)
                        {
                            var      actualResult   = new StringBuilder();
                            int      currentValue   = 0;
                            string[] expectedValues =
                            {
                                "876-54-3210,Doe,Jane,882-8080,One Microsoft Way,Redmond,WA,98052,False",
                                "876-54-3211,Doe,John,882-8181,(NULL),(NULL),(NULL),(NULL),False"
                            };

                            while (rdr.ReadAsync().Result)
                            {
                                Assert.True(currentValue < expectedValues.Length, "ERROR: Received more values than expected");

                                for (int i = 0; i < rdr.FieldCount; i++)
                                {
                                    if (i > 0)
                                    {
                                        actualResult.Append(",");
                                    }
                                    if (rdr.IsDBNullAsync(i).Result)
                                    {
                                        actualResult.Append("(NULL)");
                                    }
                                    else
                                    {
                                        if (rdr.GetFieldType(i) == typeof(bool))
                                        {
                                            actualResult.Append(rdr.GetFieldValueAsync<bool>(i).Result);
                                        }
                                        else if (rdr.GetFieldType(i) == typeof(decimal))
                                        {
                                            actualResult.Append(rdr.GetFieldValueAsync<decimal>(i).Result);
                                        }
                                        else
                                        {
                                            actualResult.Append(rdr.GetFieldValueAsync<string>(i).Result);
                                        }
                                    }
                                }

                                DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue++], actualResult.ToString(), "FAILED: Did not receive expected data");
                                actualResult.Clear();
                            }
                        }
                    }

                    // GetStream
                    byte[] correctBytes = { 0x12, 0x34, 0x56, 0x78 };
                    string queryString;
                    string correctBytesAsString = "'\\x12345678'";
                    queryString = string.Format("SELECT {0}::bytea", correctBytesAsString);
                    using (var command = provider.CreateCommand())
                    {
                        command.CommandText = queryString;
                        command.Connection  = con;
                        
                        using (var reader = command.ExecuteReader())
                        {
                            reader.Read();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                var buffer    = new byte[256];
                                var stream    = reader.GetStream(i);
                                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                                for (int j = 0; j < correctBytes.Length; j++)
                                {
                                    Assert.True(correctBytes[j] == buffer[j], "ERROR: Bytes do not match");
                                }
                            }
                        }
                    }

                    // GetTextReader
                    string[] correctStrings = { "'Hello World'", "e'\uFF8A\uFF9B\uFF70\uFF9C\uFF70\uFF99\uFF84\uFF9E'" };
                    string[] collations     = { "C.UTF-8", "C.UTF-8" };

                    for (int j = 0; j < collations.Length; j++)
                    {
                        string substring = string.Format("({0} COLLATE \"{1}\")", correctStrings[j], collations[j]);
                        queryString = string.Format("SELECT CAST({0} AS CHAR(20)), CAST({0} AS VARCHAR(20)), CAST({0} AS TEXT)", substring);
                        using (var command = provider.CreateCommand())
                        {
                            command.CommandText = queryString;
                            command.Connection  = con;
                            using (var reader = command.ExecuteReader())
                            {
                                reader.Read();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    char[] buffer     = new char[256];
                                    var    textReader = reader.GetTextReader(i);
                                    int    charsRead  = textReader.Read(buffer, 0, buffer.Length);
                                    string stringRead = new string(buffer, 0, charsRead);

                                    Assert.True(stringRead == (string)reader.GetValue(i), "ERROR: Strings to not match");
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                using (var con = provider.CreateConnection())
                {
                    con.ConnectionString = connectionString;
                    con.Open();

                    using (var cmd = provider.CreateCommand())
                    {
                        cmd.Connection  = con;
                        cmd.CommandText = $"DROP TABLE {tempTable}";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
