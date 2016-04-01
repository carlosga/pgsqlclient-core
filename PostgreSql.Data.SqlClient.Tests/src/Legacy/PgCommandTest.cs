// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using System;
using System.Data;
using System.Diagnostics;

namespace PostgreSql.Data.SqlClient.UnitTests
{
    [TestFixture]
    [Ignore("Needs configuration")]    
    public class PgCommandTest
        : PgBaseTest
    {
        [Test]
        public void ExecuteNonQueryTest()
        {
            Console.WriteLine("\r\nPgCommandTest.ExecuteNonQueryTest");

            string commandText = "update public.test_table set char_field = @char_field, varchar_field = @varchar_field where int4_field = @int4_field";

            using (var transaction = Connection.BeginTransaction())
            {
                using (var command = new PgCommand(commandText, Connection, transaction))
                {
                    // Add command parameters
                    command.Parameters.Add("@char_field"    , PgDbType.Char);
                    command.Parameters.Add("@varchar_field" , PgDbType.VarChar);
                    command.Parameters.Add("@int4_field"    , PgDbType.Integer);

                    for (int i = 0; i < 100; i++)
                    {
                        command.Parameters["@char_field"].Value    = $"Row {i}";
                        command.Parameters["@varchar_field"].Value = $"Row Number {i}";
                        command.Parameters["@int4_field"].Value    = i;

                        Assert.AreEqual(1, command.ExecuteNonQuery());
                    }

                    // Commit transaction
                    transaction.Commit();
                }
            }
        }

        [Test]
        public void ExecuteReaderTest()
        {
            Console.WriteLine("\r\nPgCommandTest.ExecuteReaderTest");

            using (var command = new PgCommand("SELECT * FROM public.test_table ORDER BY date_field", Connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Debug.Write($"{reader.GetName(i)}\t\t");
                    }

                    Debug.Write("\r\n");

                    while (reader.Read())
                    {
                        object[] values = new object[reader.FieldCount];
                        reader.GetValues(values);

                        for (int i = 0; i < values.Length; i++)
                        {
                            Debug.Write($"{Convert.ToString(values[i])}\t\t");
                        }

                        Debug.Write("\r\n");
                    }
                }
            }
        }

        [Test]
        public void ExecuteScalarTest()
        {
            using (PgCommand command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field";

                command.Parameters.Add("@int4_field", 2);

                string charFieldValue = command.ExecuteScalar().ToString();

                Debug.WriteLine($"Scalar value: {charFieldValue}");
            }
        }

        [Test]
        public void PrepareTest()
        {
            using (PgCommand command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field";

                command.Parameters.Add("@int4_field", 2);
                command.Prepare();
            }
        }

        [Test]
        public void NamedParametersTest()
        {
            using (PgCommand command = Connection.CreateCommand())
            {
                command.CommandText = "SELECT char_field FROM public.test_table where int4_field = @int4_field or char_field = @char_field";

                command.Parameters.Add("@int4_field", 2);
                command.Parameters.Add("@char_field", "IRow 20");

                using (PgDataReader reader = command.ExecuteReader())
                {
                    int count = 0;

                    while (reader.Read())
                    {
                        Debug.WriteLine(reader.GetValue(0));
                        count++;
                    }

                    Debug.WriteLine($"\r\n Record fetched {count} \r\n");
                }
            }
        }

        [Test]
        public void ExecuteStoredProcTest()
        {
            using (PgCommand command = new PgCommand("TestCount", Connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@CountResult", PgDbType.BigInt).Direction = ParameterDirection.Output;

                command.ExecuteNonQuery();

                Debug.WriteLine($"ExecuteStoredProcTest - Count result {command.Parameters[0].Value}");
            }
        }

        [Test]
        public void RecordsAffectedTest()
        {
            int recordsAffected = 0;

            // Execute a SELECT command
            using (PgCommand selectCommand = new PgCommand("SELECT * FROM public.test_table WHERE int4_field = 100", Connection))
            {
                recordsAffected = selectCommand.ExecuteNonQuery();
                Assert.AreEqual(-1, recordsAffected, "Invalid records affected value, expected -1");
            }

            // Execute a DELETE command
            using (PgCommand deleteCommand = new PgCommand("DELETE FROM public.test_table WHERE int4_field = 45", Connection))
            {
                recordsAffected = deleteCommand.ExecuteNonQuery();
                Assert.AreEqual(1, recordsAffected, "Invalid records affected value, expected 1");
            }
        }

        [Test]
        public void TestError782096()
        {
            Console.WriteLine("\r\nPgCommandTest.TestError782096");

            using (PgCommand command = new PgCommand("SELECT * FROM public.test_table ORDER BY date_field", Connection))
            {
                using (PgDataReader reader = command.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Debug.Write($"{reader.GetName(i)}\t\t");
                    }

                    Debug.Write("\r\n");

                    while (reader.Read())
                    {
                        object[] values = new object[reader.FieldCount];
                        reader.GetValues(values);

                        for (int i = 0; i < values.Length; i++)
                        {
                            Debug.Write($"{Convert.ToString(values[i])}\t\t");
                        }
                        Debug.Write("\r\n");
                    }
                }
            }
        }

        [Test]
        public void TestCase3()
        {
            using (PgCommand cmd = Connection.CreateCommand())
            {
                cmd.CommandText = @"SELECT int4_field FROM test_table WHERE varchar_field = @parameter";
                cmd.Parameters.Add("@parameter", "IRow Number10");
                int value = (int)cmd.ExecuteScalar();
                
                Assert.AreEqual(10, value, "ExecuteScalar returned an invalid value. Expected 10.");
            }
        }
    }
}
