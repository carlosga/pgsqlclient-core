// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;
using PostgreSql.Data.PgTypes;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    [TestFixture]    
    public static class DataStreamTest
    {
        [Test]
        public static void MultipleResults()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string query =
                    "select orderid from orders where orderid < @id order by orderid;" +
                    "select * from shippers order by shipperid;" +
                    "select * from region order by regionid;" +
                    "select lastname from employees order by lastname";

                // Each array in expectedResults is a separate query result
                string[][] expectedResults =
                {
                    new string[] { "10248", "10249", "10250", "10251", "10252", "10253", "10254" }, // All separate rows
                    new string[]
                    {
                        "1", "Speedy Express"   , "(503) 555-9831",  // Query Row 1
                        "2", "United Package"   , "(503) 555-3199",  // Query Row 2
                        "3", "Federal Shipping" , "(503) 555-9931",  // Query Row 3
                        "4", "Alliance Shippers", "1-800-222-0451",  // Query Row 4
                        "5", "UPS"              , "1-800-782-7892",  // Query Row 5
                        "6", "DHL"              , "1-800-225-5345",  // Query Row 6    
                    },
                    new string[]
                    {
                        "1", "Eastern" , // Query Row 1
                        "2", "Western" , // Query Row 2
                        "3", "Northern", // Query Row 3
                        "4", "Southern"  // Query Row 4
                    },
                    new string[] { "Buchanan", "Callahan", "Davolio", "Dodsworth", "Fuller", "King", "Leverling", "Peacock", "Suyama" } // All separate rows
                };

                using (PgCommand cmd = new PgCommand(query, conn))
                {
                    cmd.Parameters.Add(new PgParameter("@id", PgDbType.Int4)).Value = 10255;
                    using (PgDataReader r1 = cmd.ExecuteReader())
                    {
                        int numBatches = 0;
                        do
                        {
                            Assert.True(numBatches < expectedResults.Length, "ERROR: Received more batches than were expected.");
                            object[] values = new object[r1.FieldCount];
                            // Current "column" in expected row is (valuesChecked MOD FieldCount), since 
                            // expected rows for current batch are appended together for easy formatting
                            int valuesChecked = 0;
                            while (r1.Read())
                            {
                                r1.GetValues(values);

                                for (int col = 0; col < values.Length; col++, valuesChecked++)
                                {
                                    Assert.True(valuesChecked < expectedResults[numBatches].Length, "ERROR: Received more results for this batch than was expected");
                                    string expectedVal = expectedResults[numBatches][valuesChecked];
                                    string actualVal = values[col].ToString();

                                    DataTestClass.AssertEqualsWithDescription(expectedVal, actualVal, "FAILED: Received a different value than expected.");
                                }
                            }
                            numBatches++;
                        } while (r1.NextResult());
                    }
                }
            }
        }

        [Test]
        public static void InvalidRead()
        {
            using (PgConnection c = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                c.Open();
                string sqlBatch = "select * from orders where orderid < 10253";
                using (PgCommand cmd = new PgCommand(sqlBatch, c))
                using (PgDataReader reader = cmd.ExecuteReader())
                {
                    string errorMessage = "Invalid attempt to read when no data is present.";
                    DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetInt32(0), errorMessage);
                }
            }
        }

        [Test]
        public static void VariantRead()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string sqlBatch = "select * from orders where orderid < 10253";
                using (PgCommand cmd = new PgCommand(sqlBatch, conn))
                using (PgDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();
                    object v = null;

                    DateTime d;
                    decimal m;
                    string s = null;
                    int i;

                    // read data out of buffer
                    v = rdr.GetValue( 0); i = (int)v;
                    v = rdr.GetValue( 1); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue( 2); i = (int)v;
                    v = rdr.GetValue( 3); d = (DateTime)v;
                    v = rdr.GetValue( 4); d = (DateTime)v;
                    v = rdr.GetValue( 5); d = (DateTime)v;
                    v = rdr.GetValue( 6); i = (int)v;
                    v = rdr.GetValue( 7); m = (decimal)v;
                    v = rdr.GetValue( 8); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue( 9); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue(10); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue(11); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue(12); s = v is DBNull ? null : (string)v;
                    v = rdr.GetValue(13); s = v is DBNull ? null : (string)v;

                    DataTestClass.AssertEqualsWithDescription("France", s.ToString(), "FAILED: Received incorrect last value.");
                }
            }
        }

        [Test]
        public static void TypeRead()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string sqlBatch = "select * from orders where orderid < 10253";
                using (PgCommand cmd = new PgCommand(sqlBatch, conn))
                using (PgDataReader rdr = cmd.ExecuteReader())
                {
                    rdr.Read();

                    DateTime d;
                    decimal  m;
                    string   s = null;
                    int      i;

                    // read data out of buffer
                    i = rdr.GetInt32(0);    // order id
                    s = rdr.GetString(1);   // customer id
                    i = rdr.GetInt32(2);    // employee id
                    d = rdr.GetDateTime(3); // OrderDate
                    d = rdr.GetDateTime(4); // RequiredDate
                    d = rdr.GetDateTime(5); // ShippedDate;
                    i = rdr.GetInt32(6);    // ShipVia
                    m = rdr.GetDecimal(7);  // Freight
                    s = rdr.GetString(8);   // ShipName
                    s = rdr.GetString(9);   // ShipAddres
                    s = rdr.GetString(10);  // ShipCity
    
                    // should get an exception here
                    string errorMessage = "Data is Null. This method or property cannot be called on Null values.";
                    DataTestClass.AssertThrowsWrapper<PgNullValueException>(() => rdr.GetString(11), errorMessage);

                    s = rdr.GetString(12); //ShipPostalCode;
                    s = rdr.GetString(13); //ShipCountry;
                    DataTestClass.AssertEqualsWithDescription("France", s.ToString(), "FAILED: Received incorrect last value.");
                }
            }
        }

        [Test]
        [Ignore("Not ported yet")]
        public static void GetValueOfTRead()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string sqlBatch = "select * from orders where orderid < 10253 and shipregion is null";
                using (PgCommand cmd = new PgCommand(sqlBatch, conn))
                using (PgDataReader rdr = cmd.ExecuteReader())
                {
                    string errorMessage = "Data is Null. This method or property cannot be called on Null values.";

                    rdr.Read();
                    // read data out of buffer
                    rdr.GetFieldValue<int>(0);          // order id
                    rdr.GetFieldValue<string>(1);       // customer id
                    rdr.GetFieldValue<int>(2);          // employee id
                    rdr.GetFieldValue<DateTime>(3);     // OrderDate
                    rdr.GetFieldValue<DateTime>(4);     // RequiredDate
                    rdr.GetFieldValue<DateTime>(5);     // ShippedDate;
                    rdr.GetFieldValue<int>(6);          // ShipVia;
                    rdr.GetFieldValue<decimal>(7);      // Freight;
                    rdr.GetFieldValue<string>(8);       // ShipName;
                    rdr.GetFieldValue<string>(9);       // ShipAddres;
                    rdr.IsDBNull(10);
                    rdr.IsDBNull(10);
                    rdr.GetFieldValue<string>(10);      // ShipCity;
                    // should get an exception here
                    DataTestClass.AssertThrowsWrapper<PgNullValueException>(() => rdr.GetFieldValue<string>(11), errorMessage);
                    rdr.IsDBNull(11);
                    rdr.GetFieldValue<string>(11);
                    rdr.IsDBNull(11);
                    rdr.IsDBNull(12);
                    rdr.GetChars(12, 0, null, 0, 0);
                    rdr.IsDBNull(12);
#warning TODO: Implement INullable                    
                    // rdr.GetFieldValue<INullable>(13);//ShipCountry;

                    rdr.Read();
                    // read data out of buffer
                    rdr.GetFieldValueAsync<int>(0).Wait();          // order id
                    rdr.GetFieldValueAsync<string>(1).Wait();       // customer id
                    rdr.GetFieldValueAsync<int>(2).Wait();          // employee id
                    rdr.GetFieldValueAsync<DateTime>(3).Wait();     // OrderDate
                    rdr.GetFieldValueAsync<DateTime>(4).Wait();     // RequiredDate
                    rdr.GetFieldValueAsync<DateTime>(5).Wait();     // ShippedDate;
                    rdr.GetFieldValueAsync<int>(6).Wait();          // ShipVia;
                    rdr.GetFieldValueAsync<decimal>(7).Wait();      // Freight;
                    rdr.GetFieldValueAsync<string>(8).Wait();       // ShipName;
                    rdr.GetFieldValueAsync<string>(9).Wait();       // ShipAddres;
                    Assert.False(rdr.IsDBNullAsync(10).Result, "FAILED: IsDBNull was true for a non-null value");
                    rdr.GetFieldValueAsync<string>(10).Wait();      // ShipCity;
                    // should get an exception here
                    DataTestClass.AssertThrowsWrapper<AggregateException, PgNullValueException>(() => rdr.GetFieldValueAsync<string>(11).Wait(), innerExceptionMessage: errorMessage);
                    Assert.True(rdr.IsDBNullAsync(11).Result, "FAILED: IsDBNull was false for a null value");

                    rdr.IsDBNullAsync(11).Wait();
                    rdr.GetFieldValueAsync<string>(11).Wait();
                    rdr.IsDBNullAsync(11).Wait();
                    rdr.IsDBNullAsync(12).Wait();
                    rdr.GetChars(12, 0, null, 0, 0);
                    rdr.IsDBNullAsync(12).Wait();
#warning TODO: Implement INullable                    
                    //rdr.GetFieldValueAsync<INullable>(13).Wait(); //ShipCountry;

                    rdr.Read();
                    Assert.True(rdr.IsDBNullAsync(11).Result, "FAILED: IsDBNull was false for a null value");
                }
            }
        }

        [Test]
        public static void OutOfOrderGetChars()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();

                string expectedFirstString  = "Hello, World!";
                string expectedSecondString = "Another string";

                // NOTE: Must be non-Plp types (i.e. not MAX sized columns)
                using (PgCommand cmd = new PgCommand("SELECT @r, @p", conn))
                {
                    cmd.Parameters.AddWithValue("@r", expectedFirstString);
                    cmd.Parameters.AddWithValue("@p", expectedSecondString);
                    
                    // NOTE: Command behavior must NOT be sequential
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        char[] data = new char[20];
                        reader.Read();

                        // Read last column - this will read in all intermediate columns
                        reader.GetValue(1);

                        // Read in first column with GetChars
                        // Since we've haven't called GetChars yet, this caches the value of the column into _columnDataChars
                        long   charsRead         = reader.GetChars(0, 0, data, 0, data.Length);
                        string actualFirstString = new string(data, 0, (int)charsRead);

                        // Now read in the second column
                        charsRead = reader.GetChars(1, 0, data, 0, data.Length);
                        string actualSecondString = new string(data, 0, (int)charsRead);

                        // Validate data
                        DataTestClass.AssertEqualsWithDescription(expectedFirstString, actualFirstString, "FAILED: First string did not match");
                        DataTestClass.AssertEqualsWithDescription(expectedSecondString, actualSecondString, "FAILED: Second string did not match");
                    }
                }
            }
        }

        // private static void SQLTypeRead()
        // {
        //     using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
        //     {
        //         conn.Open();
        //         using (PgCommand cmd = new PgCommand("select * from orders where orderid < 10253", conn))
        //         using (PgDataReader rdr = cmd.ExecuteReader())
        //         {
        //             rdr.Read();

        //             SqlDateTime d;
        //             SqlMoney m;
        //             string s = null;
        //             int i;

        //             // read data out of buffer
        //             i = rdr.Getint(0); //order id
        //             s = rdr.Getstring(1); //customer id
        //             i = rdr.Getint(2); // employee id
        //             d = rdr.GetSqlDateTime(3); //OrderDate
        //             d = rdr.GetSqlDateTime(4); //RequiredDate
        //             d = rdr.GetSqlDateTime(5); //ShippedDate;
        //             i = rdr.Getint(6); //ShipVia;
        //             m = rdr.GetSqlMoney(7); //Freight;
        //             s = rdr.Getstring(8); //ShipName;
        //             s = rdr.Getstring(9); //ShipAddres;
        //             s = rdr.Getstring(10); //ShipCity;
        //             s = rdr.Getstring(11); //ShipRegion;
        //             s = rdr.Getstring(12); //ShipPostalCode;
        //             s = rdr.Getstring(13); //ShipCountry;
        //             DataTestClass.AssertEqualsWithDescription("France", s.ToString(), "FAILED: Received incorrect last value.");
        //         }
        //     }
        // }

        [Test]
        public static void RowBuffer()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                using (PgCommand cmd = new PgCommand("select * from orders where orderid < 10253", conn))
                using (PgDataReader reader = cmd.ExecuteReader())
                {
                    int cRecords = 0;
                    object[] buffer = new object[reader.FieldCount];
                    while (reader.Read())
                    {
                        reader.GetValues(buffer);

                        if (cRecords == 0)
                        {
                            VerifySchema(reader);
                        }

                        VerifyData(reader, buffer);
                        cRecords++;
                    }
                    DataTestClass.AssertEqualsWithDescription(5, cRecords, "FAILED: Received incorrect number of records");
                }
            }
        }

        [Test]
        public static void TimestampRead()
        {
            string tempTable = DataTestClass.GetUniqueName("__", String.Empty, String.Empty);
            tempTable = tempTable.Replace('-', '_');

            try
            {
                using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
                {
                    conn.Open();
                    using (PgCommand cmdDefault = new PgCommand(String.Empty, conn))
                    {
                        cmdDefault.CommandText = $"create table {tempTable} (c1 integer, c2 timestamp)";
                        cmdDefault.ExecuteNonQuery();

                        cmdDefault.CommandText = $"insert into {tempTable} (c1) values (1)";
                        cmdDefault.ExecuteNonQuery();

                        cmdDefault.CommandText = $"select * from {tempTable}";
                        using (PgDataReader reader = cmdDefault.ExecuteReader())
                        {
                            DataTestClass.AssertEqualsWithDescription("timestamp", reader.GetDataTypeName(1), "FAILED: Data value did not have correct type");
                            DataTestClass.AssertEqualsWithDescription(typeof(DateTime), reader.GetFieldType(1), "FAILED: Data value did not have correct type");                        
                        }
                    }
                }
            }
            finally
            {
                using (var conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
                {
                    conn.Open();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"DROP TABLE {tempTable}";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        [Test]
        [Ignore("Not ported yet")]
        public static void BufferSize()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                using (PgCommand cmd = new PgCommand("select * from orders where orderid<@id", conn))
                {
                    cmd.Parameters.Add(new PgParameter("@id", PgDbType.Int4)).Value = 10252;
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        // smaller buffer
                        object[] buf_small = new object[3];
                        // bigger buffer
                        object[] buf_big = new object[reader.FieldCount + 10];
                        object[] buf = buf_small;
                        while (reader.Read())
                        {
                            // alternate buffers
                            reader.GetValues(buf);
                            string bufEntry1 = (string)buf[1];
#warning TODO: Implement GetPgValues ??                            
                            // reader.GetSqlValues(buf);
#warning TODO: Implement PgString ??                            
                            // string bufEntry2 = ((SqlString)buf[1]).Value;
                            string bufEntry2 = (string)buf[1];

                            Assert.True(bufEntry1.Equals(bufEntry2.ToString()),
                                string.Format("FAILED: Should have same value with both buffer entries. Buf2 value: {0}. Buf2 value: {1}", bufEntry1, bufEntry2));

                            buf = (buf == buf_small) ? buf_big : buf_small;
                        }
                    }
                }
            }
        }

        [Test]
        public static void OrphanReader()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                using (PgCommand cmd = new PgCommand("select * from orders where orderid < 10253", conn))
                {
                    object value;
                    PgDataReader reader;
                    using (reader = cmd.ExecuteReader())
                    {
                        conn.Close();
                        Assert.True(reader.IsClosed, "FAILED: Stream was not closed by connection close (Scenario: No Read)");
                    }

                    string errorMessage;
                    conn.Open();
                    using (reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        conn.Close();

                        errorMessage = "Invalid attempt to read when no data is present.";
                        DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => value = reader[0], errorMessage);
                        Assert.True(reader.IsClosed, "FAILED: Stream was not closed by connection close (Scenario: Read)");
                        conn.Open();
                    }

                    using (reader = cmd.ExecuteReader())
                    {
                        reader.Read();
                        value = reader[0];
                        conn.Close();

                        DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => value = reader[0], errorMessage);
                        Assert.True(reader.IsClosed, "FAILED: Stream was not closed by connection close (Scenario: Read Partial Data)");
                        conn.Open();
                    }
                    
                    using (reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    value = reader[i];
                                }
                            }
                        } while (reader.NextResult());

                        conn.Close();
                        DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => value = reader[0], errorMessage);
                        Assert.True(reader.IsClosed, "FAILED: Stream was not closed by connection close (Scenario: Read All Data)");
                    }
                }
            }
        }

        [Test]
        public static void NumericRead()
        {
            string tempTable = DataTestClass.GetUniqueName("TEMP_", "", "");

            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                using (PgCommand cmd = new PgCommand("", conn))
                {
                    // PostgreSQL has no sql_variant type, so the query is modified with both fields as numeric
                    cmd.CommandText = $"create table {tempTable} (c1 numeric(38,23), c2 numeric(38,23))";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = $"insert into {tempTable} values (-123456789012345.67890123456789012345678::numeric(38,23), -123456789012345.67890123456789012345678::numeric(38,23))";
                    cmd.ExecuteNonQuery();
                    
                    cmd.CommandText = $"select * from {tempTable}";
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        reader.Read();

                        object  o = reader.GetValue(0);
                        decimal n = reader.GetDecimal(1);
                        
                        Assert.True(o is decimal, "FAILED: Query result was not a decimal value");
                        DataTestClass.AssertEqualsWithDescription("-123456789012345.67890123456789012345678", ((decimal)o).ToString(), "FAILED: Decimal did not have expected value");
                        DataTestClass.AssertEqualsWithDescription("-123456789012345.67890123456789012345678", n.ToString(), "FAILED: Decimal did not have expected value");

                        // com+ type coercion should fail
                        // Em
                        object value;
                        string errorMessage = SystemDataResourceManager.Instance.SqlMisc_ConversionOverflowMessage;
                        DataTestClass.AssertThrowsWrapper<OverflowException>(() => value = reader[0], errorMessage);
                        DataTestClass.AssertThrowsWrapper<OverflowException>(() => value = reader[1], errorMessage);
                        DataTestClass.AssertThrowsWrapper<OverflowException>(() => value = reader.GetDecimal(0), errorMessage);
                        DataTestClass.AssertThrowsWrapper<OverflowException>(() => value = reader.GetDecimal(1), errorMessage);
                    }
                }
            }
        }

        [Test]
        public static void HasRowsTest()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string sqlBatch =
                    "select * from orders limit 10;" +
                    "select * from orders limit  5;" +
                    "select * from orders limit  0;";
                    
                using (PgCommand cmd = new PgCommand(sqlBatch, conn))
                {
                    PgDataReader reader;
                    using (reader = cmd.ExecuteReader())
                    {
                        Assert.True(reader.HasRows, "FAILED: Failure #1: HasRows");                        
                        while (reader.Read())
                        {
                            Assert.True(reader.HasRows, "FAILED: Failure #2: HasRows");
                        }
                        Assert.True(reader.HasRows, "FAILED: Failure #3: HasRows");

                        Assert.True(reader.NextResult(), "FAILED: Failure #3.5: NextResult");

                        Assert.True(reader.HasRows, "FAILED: Failure #4: HasRows");
                        while (reader.Read())
                        {
                            Assert.True(reader.HasRows, "FAILED: Failure #5: HasRows");
                        }
                        Assert.True(reader.HasRows, "FAILED: Failure #6: HasRows");

                        Assert.True(reader.NextResult(), "FAILED: Failure #6.5: NextResult");

                        Assert.False(reader.HasRows, "FAILED: Failure #7: HasRows");
                        while (reader.Read())
                        {
                            Assert.False(reader.HasRows, "FAILED: Failure #8: HasRows");
                        }
                        Assert.False(reader.HasRows, "FAILED: Failure #9: HasRows");

                        Assert.False(reader.NextResult(), "FAILED: Failure #9.5: NextResult");

                        Assert.False(reader.HasRows, "FAILED: Failure #10: HasRows");
                    }

                    bool result;
                    string errorMessage = "Invalid attempt to read when no data is present.";
                    DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => result = reader.HasRows, errorMessage);
                }
            }
        }

        // private static void SqlCharsBytesTest()
        // {
        //     using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
        //     {
        //         conn.Open();

        //         // select with SqlChars	parameter
        //         PgCommand cmd;
        //         PgDataReader reader;
        //         using (cmd = conn.CreateCommand())
        //         {
        //             cmd.CommandText = "select EmployeeID, FirstName, LastName from Employees where Title = @vm ";

        //             (cmd.Parameters.Add("@vm", SqlDbType.VarChar)).Value = new SqlChars("Vice President, Sales");

        //             using (reader = cmd.ExecuteReader())
        //             {
        //                 Assert.True(reader.Read(), "FAILED: No results were returned from read()");
        //                 DataTestClass.AssertEqualsWithDescription(2, reader.GetInt32(0), "FAILED: GetInt32(0) result did not match expected value");
        //                 DataTestClass.AssertEqualsWithDescription("Andrew", reader.GetString(1), "FAILED: GetString(1) result did not match expected value");
        //                 DataTestClass.AssertEqualsWithDescription("Fuller", reader.GetString(2), "FAILED: GetString(2) result did not match expected value");
        //             }
        //         }

        //         // select with SqlBytes	parameter
        //         using (cmd = conn.CreateCommand())
        //         {
        //             cmd.CommandText = "select EmployeeID, FirstName, LastName from Employees where EmployeeID = 2 and Convert(binary(5), Photo) = @bn ";

        //             byte[] barr = new byte[5] { 0x15, 0x1c, 0x2F, 0x00, 0x02 };
        //             (cmd.Parameters.Add("@bn", SqlDbType.VarBinary)).Value = new SqlBytes(barr);

        //             using (reader = cmd.ExecuteReader())
        //             {
        //                 Assert.True(reader.Read(), "FAILED: No results were returned from read()");
        //                 DataTestClass.AssertEqualsWithDescription(2, reader.GetInt32(0), "FAILED: GetInt32(0) result did not match expected value");
        //                 DataTestClass.AssertEqualsWithDescription("Andrew", reader.GetString(1), "FAILED: GetString(1) result did not match expected value");
        //                 DataTestClass.AssertEqualsWithDescription("Fuller", reader.GetString(2), "FAILED: GetString(2) result did not match expected value");
        //             }
        //         }
        //     }
        // }

        [Test]
        public static void CloseConnection()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                using (PgCommand cmd = new PgCommand("select * from orders where orderid < 10253", conn))
                using (PgDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    DataTestClass.AssertEqualsWithDescription(ConnectionState.Open, conn.State, "FAILED: Connection should be in open state");

                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            reader.GetValue(i);
                        }
                    }
                }

                DataTestClass.AssertEqualsWithDescription(ConnectionState.Closed, conn.State, "FAILED: Connection should be in closed state after reader close");
            }
        }

        [Test]
        public static void OpenConnection()
        {
            // Isolates OpenConnection behavior for sanity testing on x-plat
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                DataTestClass.AssertEqualsWithDescription(ConnectionState.Open, conn.State, "FAILED: Connection should be in open state");
            }
        }

        [Test]
        public static void GetStream()
        {
            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                connection.Open();
                using (PgCommand cmd = new PgCommand("SELECT '\\x12341234'::bytea, '\\x12341234'::bytea, 12, CAST(NULL AS bytea), '\\x12341234'::bytea, '\\x12341234'::bytea, '\\x12341234'::bytea, REPEAT('a', 8000)::bytea, '\\x12341234'::bytea", connection))
                {
                    CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };
                    foreach (CommandBehavior behavior in behaviors)
                    {
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            byte[] buffer = new byte[9000];
                            reader.Read();

                            // Basic success paths
                            reader.GetStream(0);
                            reader.GetStream(1);

                            // Bad values
                            DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => reader.GetStream(2));
                            // Null stream
                            Stream stream = reader.GetStream(3);
                            Assert.False(stream.Read(buffer, 0, buffer.Length) > 0, "FAILED: Read more than 0 bytes from a null stream");

                            // Get column before current column
                            TestDelegate action = (() => reader.GetStream(0));
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                            // Two streams on same column
                            reader.GetStream(4);
                            action = (() => reader.GetStream(4));
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                            // GetStream then GetBytes on same column
                            reader.GetStream(5);
                            action = (() => reader.GetBytes(5, 0, null, 0, 0));
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                            action = (() => reader.GetBytes(5, 0, buffer, 0, buffer.Length));
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                            // GetBytes then GetStream on same column
                            reader.GetBytes(6, 0, null, 0, 0);
                            action = (() => reader.GetStream(6));
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                            reader.GetBytes(6, 0, buffer, 0, buffer.Length);
                            SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);
// #if DEBUG
//                             // GetStream while async is pending
//                             Task t = null;
//                             using (PendAsyncReadsScope pendScope = new PendAsyncReadsScope(reader))
//                             {
//                                 t = reader.ReadAsync();
//                                 Assert.False(t.Wait(1), "FAILED: Read completed immediately");
//                                 DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetStream(8));
//                             }
//                             t.Wait();

//                             // GetStream after Read 
//                             DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetStream(0));
// #endif
                        }

                        // IsDBNull + GetStream
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            reader.GetStream(8);
                            // Past column
                            reader.IsDBNull(8);
                        }

                        // IsDBNullAsync + GetStream
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            reader.GetStream(8);
                            // Past column
                            reader.IsDBNullAsync(8).Wait();
                        }
                    }
                }
            }
        }

        [Test]
        public static void GetTextReader()
        {
            string[] queryStrings =
            {
                "SELECT 'Hello World', 'Hello World', 12, CAST(NULL AS TEXT), 'Hello World', 'Hello World', 'Hello World', CAST(REPEAT('a', 8000) AS TEXT), 'Hello World' COLLATE \"en_GB.utf8\"",
                string.Format("SELECT {0} {1}, {0} {1}, 12, CAST(NULL AS TEXT), {0} {1}, {0} {1}, {0} {1}, CAST(REPEAT((e'\uFF8A' {1}), 8000) AS TEXT), {0} {1}", "e'\uFF8A\uFF9B\uFF70\uFF9C\uFF70\uFF99\uFF84\uFF9E'", "COLLATE \"C.UTF-8\"")
            };

            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                connection.Open();
                foreach (string query in queryStrings)
                {
                    using (PgCommand cmd = new PgCommand(query, connection))
                    {
                        CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };
                        foreach (CommandBehavior behavior in behaviors)
                        {
                            using (PgDataReader reader = cmd.ExecuteReader(behavior))
                            {
                                char[] buffer = new char[9000];
                                reader.Read();

                                // Basic success paths
                                reader.GetTextReader(0);
                                reader.GetTextReader(1);
                                
                                // Bad values
                                DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => reader.GetTextReader(2));
                                // Null stream
                                TextReader textReader = reader.GetTextReader(3);
                                Assert.False(textReader.Read(buffer, 0, buffer.Length) > 0, "FAILED: Read more than 0 chars from a null TextReader");

                                // Get column before current column
                                TestDelegate action = (() => reader.GetTextReader(0));
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                                // Two TextReaders on same column
                                reader.GetTextReader(4);
                                action = (() => reader.GetTextReader(4));
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                                // GetTextReader then GetBytes on same column
                                reader.GetTextReader(5);
                                action = (() => reader.GetChars(0, 0, null, 0, 0));
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                                action = (() => reader.GetChars(5, 0, buffer, 0, buffer.Length));
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                                // GetBytes then GetTextReader on same column
                                reader.GetChars(6, 0, null, 0, 0);
                                action = (() => reader.GetTextReader(6));
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

                                reader.GetChars(6, 0, buffer, 0, buffer.Length);
                                SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);
                                
// #if DEBUG
//                                 // GetTextReader while async is pending
//                                 Task t = null;
//                                 using (PendAsyncReadsScope pendScope = new PendAsyncReadsScope(reader))
//                                 {
//                                     t = reader.ReadAsync();
//                                     Assert.False(t.IsCompleted, "FAILED: Read completed immediately");
//                                     DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetTextReader(8));
//                                 }
//                                 t.Wait();

//                                 // GetTextReader after Read 
//                                 DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetTextReader(0));
// #endif
                            }
                            
                            // IsDBNull + GetTextReader
                            using (PgDataReader reader = cmd.ExecuteReader(behavior))
                            {
                                reader.Read();
                                reader.GetTextReader(8);
                                // Past column
                                reader.IsDBNull(8);
                            }
                            
                            // IsDBNullAsync + GetTextReader
                            using (PgDataReader reader = cmd.ExecuteReader(behavior))
                            {
                                reader.Read();
                                reader.GetTextReader(8);
                                // Past column
                                reader.IsDBNullAsync(8).Wait();
                            }
                        }
                    }
                }
            }
        }

//         private static void GetXmlReader()
//         {
//             using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
//             {
//                 connection.Open();
//                 string xml = "CAST('<test><subtest /><subtest>asdfasdfasdf</subtest></test>' AS XML)";
//                 string queryString = string.Format("SELECT {0}, {0}, 12, CAST(NULL AS XML), {0}, CAST(('<test>' + REPLICATE(CAST('a' AS VARCHAR(MAX)), 10000) + '</test>') AS XML), {0}", xml);
//                 using (PgCommand cmd = new PgCommand(queryString, connection))
//                 {
//                     CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };
//                     foreach (CommandBehavior behavior in behaviors)
//                     {
//                         using (PgDataReader reader = cmd.ExecuteReader(behavior))
//                         {
//                             reader.Read();

//                             // Basic success paths
//                             reader.GetXmlReader(0);
//                             reader.GetXmlReader(1);

//                             // Bad values
//                             DataTestClass.AssertThrowsWrapper<InvalidCastException>(() => reader.GetXmlReader(2));
//                             // Null stream
//                             XmlReader xmlReader = reader.GetXmlReader(3);
//                             Assert.False(xmlReader.Read(), "FAILED: Successfully read on a null XmlReader");

//                             // Get column before current column
//                             Action action = (() => reader.GetXmlReader(0));
//                             SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);

//                             // Two XmlReaders on same column
//                             reader.GetXmlReader(4);
//                             action = (() => reader.GetXmlReader(4));
//                             SeqAccessFailureWrapper<InvalidOperationException>(action, behavior);
// #if DEBUG
//                             // GetXmlReader while async is pending
//                             Task t = null;
//                             using (PendAsyncReadsScope pendScope = new PendAsyncReadsScope(reader))
//                             {
//                                 t = reader.ReadAsync();
//                                 Assert.False(t.IsCompleted, "FAILED: Read completed immediately");
//                                 DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetXmlReader(6));
//                             }
//                             t.Wait();

//                             // GetXmlReader after Read 
//                             DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => reader.GetXmlReader(0));
// #endif
//                         }
//                     }
//                 }
//             }
//         }

        [Test]
        public static void ReadStream()
        {
            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                connection.Open();
                CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };
                foreach (CommandBehavior behavior in behaviors)
                {
                    byte[]       smallBuffer  = new byte[2];
                    byte[]       buffer       = new byte[16];
                    byte[]       largeBuffer  = new byte[9000];
                    Stream       stream       = null;
                    TestDelegate action       = null;
                    using (PgCommand cmd = new PgCommand("SELECT '\\x12341234'::bytea", connection))
                    {
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            // Basic case
                            using (stream = reader.GetStream(0))
                            {
                                stream.Read(smallBuffer, 0, smallBuffer.Length);
                                stream.Read(buffer, 2, 2);

                                // Testing stream properties
                                stream.Flush();
                                DataTestClass.AssertThrowsWrapper<NotSupportedException>(() => stream.SetLength(1));
                                Action<Stream> performOnStream = ((s) => { int i = s.WriteTimeout; });
                                DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => performOnStream(stream));

                                stream.Seek(0, SeekOrigin.Begin);
                                long position = stream.Position;
                                long length = stream.Length;
                            }

                            // Once Stream is closed
                            DataTestClass.AssertThrowsWrapper<ObjectDisposedException>(() => stream.Read(buffer, 0, buffer.Length));
                        }

                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            // Reading more than is there, and when there is nothing there
                            stream = reader.GetStream(0);
                            stream.Read(buffer, 0, buffer.Length);
                            stream.Read(buffer, 0, buffer.Length);

                            // Argument exceptions
                            DataTestClass.AssertThrowsWrapper<ArgumentNullException>(() => stream.Read(null, 0, 1));
                            DataTestClass.AssertThrowsWrapper<ArgumentOutOfRangeException>(() => stream.Read(buffer, -1, 2));
                            DataTestClass.AssertThrowsWrapper<ArgumentOutOfRangeException>(() => stream.Read(buffer, 2, -1));
                            DataTestClass.AssertThrowsWrapper<ArgumentException>(() => stream.Read(buffer, buffer.Length, buffer.Length));
                            DataTestClass.AssertThrowsWrapper<ArgumentException>(() => stream.Read(buffer, int.MaxValue, int.MaxValue));
                        }

                        // Once Reader is closed
                        action = (() => stream.Read(buffer, 0, buffer.Length));
                        SeqAccessFailureWrapper<ObjectDisposedException>(action, behavior);
                    }

                    using (PgCommand cmd = new PgCommand("SELECT '\\x12341234'::bytea, 12", connection))
                    {
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            // Read after advancing reader
                            stream = reader.GetStream(0);
                            reader.GetInt32(1);

                            action = (() => stream.Read(buffer, 0, buffer.Length));
                            SeqAccessFailureWrapper<ObjectDisposedException>(action, behavior);
                        }
                    }
                }
            }
        }

        [Test]
        public static void ReadTextReader()
        {
            CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };

            // Some more complex unicode characters, including surrogate pairs
            byte[] unicodeArray = { 0xFF, 0xDB, 0xFD, 0xDF, 0x34, 0xD8, 0xDD, 0xD8, 0x1E, 0xDC, 0x00, 0x6C, 0x00, 0x34 };
            string unicodeString = System.Text.Encoding.Unicode.GetString(unicodeArray, 0, unicodeArray.Length);

            foreach (CommandBehavior behavior in behaviors)
            {
                string[] correctStrings = {
                    "CAST(('Hello world' COLLATE \"en_GB.utf8\") AS TEXT)",
                    string.Format("CAST('{0}Hello world' AS TEXT)", unicodeString),
                    "CAST(('\uFF8A\uFF9B\uFF70\uFF9C\uFF70\uFF99\uFF84\uFF9E' COLLATE \"C.UTF-8\") AS TEXT)" };

                foreach (string correctString in correctStrings)
                {
                    using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
                    {
                        connection.Open();
                        
                        char[]       smallBuffer = new char[2];
                        char[]       buffer      = new char[16];
                        char[]       largeBuffer = new char[9000];
                        TextReader   textReader  = null;
                        TestDelegate action      = null;
                        
                        using (PgCommand cmd = new PgCommand(string.Format("SELECT {0}", correctString), connection))
                        {
                            using (PgDataReader reader = cmd.ExecuteReader(behavior))
                            {
                                reader.Read();
                                // Basic case
                                using (textReader = reader.GetTextReader(0))
                                {
                                    textReader.Read();
                                    textReader.Read(smallBuffer, 0, smallBuffer.Length);
                                    textReader.Read(buffer, 2, 2);
                                    textReader.Peek();
                                    textReader.Read(buffer, 3, 2);
                                    textReader.Peek();
                                    textReader.Read();
                                }
                                
                                // Once Reader is closed
                                DataTestClass.AssertThrowsWrapper<ObjectDisposedException>(() => textReader.Read(buffer, 0, buffer.Length));
                            }

                            using (PgDataReader reader = cmd.ExecuteReader(behavior))
                            {
                                reader.Read();
                                // Reading more than is there, and when there is nothing there
                                textReader = reader.GetTextReader(0);
                                textReader.Read(buffer, 0, buffer.Length);
                                textReader.Read(buffer, 0, buffer.Length);
                                textReader.Read();
                                textReader.Peek();

                                // Argument exceptions
                                DataTestClass.AssertThrowsWrapper<ArgumentNullException>(() => textReader.Read(null, 0, 1));
                                DataTestClass.AssertThrowsWrapper<ArgumentOutOfRangeException>(() => textReader.Read(buffer, -1, 2));
                                DataTestClass.AssertThrowsWrapper<ArgumentOutOfRangeException>(() => textReader.Read(buffer, 2, -1));
                                DataTestClass.AssertThrowsWrapper<ArgumentException>(() => textReader.Read(buffer, buffer.Length, buffer.Length));
                                DataTestClass.AssertThrowsWrapper<ArgumentException>(() => textReader.Read(buffer, int.MaxValue, int.MaxValue));
                            }

                            // Once Reader is closed
                            action = (() => textReader.Read(buffer, 0, buffer.Length));
                            SeqAccessFailureWrapper<ObjectDisposedException>(action, behavior);
                        }

                        using (PgCommand cmd = new PgCommand(string.Format("SELECT {0}, 12", correctString), connection))
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            // Read after advancing reader
                            textReader = reader.GetTextReader(0);
                            reader.GetInt32(1);

                            action = (() => textReader.Read(buffer, 0, buffer.Length));
                            SeqAccessFailureWrapper<ObjectDisposedException>(action, behavior);
                        }

                    }
                }
            }
        }

        [Test]
        public static void StreamingBlobDataTypes()
        {
            using (PgConnection connection = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                connection.Open();
                CommandBehavior[] behaviors = new CommandBehavior[] { CommandBehavior.Default };
                foreach (CommandBehavior behavior in behaviors)
                {
                    // GetStream
                    byte[] correctBytes         = { 0x12, 0x34, 0x56, 0x78 };
                    string correctBytesAsString = "12345678";
                    string queryString          = $"SELECT '\\x{correctBytesAsString}'::bytea";
                                       
                    using (PgCommand cmd = new PgCommand(queryString, connection))
                    using (PgDataReader reader = cmd.ExecuteReader(behavior))
                    {
                        reader.Read();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            byte[] buffer = new byte[256];
                            Stream stream = reader.GetStream(i);
                            int bytesRead = stream.Read(buffer, 0, buffer.Length);
                            for (int j = 0; j < correctBytes.Length; j++)
                            {
                                DataTestClass.AssertEqualsWithDescription(correctBytes[j], buffer[j], "FAILED: Bytes do not match");
                            }
                        }
                    }

                    // GetTextReader
                    string[] correctStrings = { "Hello World", "\uFF8A\uFF9B\uFF70\uFF9C\uFF70\uFF99\uFF84\uFF9E" };
                    string[] collations     = { "en_GB.utf8", "C.UTF-8" };

                    for (int j = 0; j < collations.Length; j++)
                    {
                        string substring = string.Format("('{0}' COLLATE \"{1}\")", correctStrings[j], collations[j]);
                        queryString = string.Format("SELECT CAST({0} AS CHAR(20)), CAST({0} AS CHAR(20)), CAST({0} AS TEXT), CAST({0} AS VARCHAR(20)), CAST({0} AS TEXT), CAST({0} AS TEXT), CAST({0} AS VARCHAR(20)), CAST({0} AS TEXT), CAST({0} AS TEXT)", substring);
                        
                        using (PgCommand cmd = new PgCommand(queryString, connection))
                        using (PgDataReader reader = cmd.ExecuteReader(behavior))
                        {
                            reader.Read();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                char[] buffer = new char[256];
                                TextReader textReader = reader.GetTextReader(i);
                                int charsRead = textReader.Read(buffer, 0, buffer.Length);
                                string stringRead = new string(buffer, 0, charsRead);
                                DataTestClass.AssertEqualsWithDescription(correctStrings[j], stringRead.TrimEnd(), "FAILED: Strings to not match");
                            }
                        }
                    }
                }
            }
        }

        [Test]
        [Ignore("Not ported yet")]
        public static void TimeoutDuringReadAsyncWithClosedReaderTest()
        {
            // Create the proxy
            string connectionString = DataTestClass.PostgreSql9_Northwind;
            
            ProxyServer proxy = ProxyServer.CreateAndStartProxy(connectionString, out connectionString);
            proxy.SimulatedPacketDelay = 100;
            proxy.SimulatedOutDelay = true;
            try
            {
                using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
                {
                    // Start the command
                    conn.Open();
                    using (PgCommand cmd = new PgCommand("SELECT @p, @p, @p, @p, @p", conn))
                    {
                        cmd.CommandTimeout = 1;
                        cmd.Parameters.AddWithValue("@p", new string('a', 3000));
                        using (PgDataReader reader = cmd.ExecuteReader())
                        {
                            // Start reading, and then force a timeout
                            Task<bool> task = reader.ReadAsync();
                            proxy.PauseCopying();
                            // Before the timeout occurs, but after ReadAsync has started waiting for a packet, close the reader
                            Thread.Sleep(200);
                            Task closeTask = Task.Run(() => reader.Dispose());

                            // Wait for the task to see the timeout
                            string errorMessage = "Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.";
                            DataTestClass.AssertThrowsWrapper<AggregateException, PgException>(() => task.Wait(), innerExceptionMessage: errorMessage);
                        }
                    }
                }
                proxy.Stop();
            }
            catch (PgException ex)
            {
                // In case of error, stop the proxy and dump its logs (hopefully this will help with debugging
                proxy.Stop();
                throw ex;
            }
        }

        [Test]
        [Ignore("Not ported yet")]
        public static void NonFatalTimeoutDuringRead()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind;
            
            // Create the proxy
            ProxyServer proxy = ProxyServer.CreateAndStartProxy(connectionString, out connectionString);
            proxy.SimulatedPacketDelay = 100;
            proxy.SimulatedOutDelay = true;
            try
            {
                using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
                {
                    // Start the command
                    conn.Open();
                    using (PgCommand cmd = new PgCommand("SELECT @p, @p, @p, @p, @p", conn))
                    {
                        cmd.CommandTimeout = 1;
                        cmd.Parameters.AddWithValue("@p", new string('a', 3000));
                        using (PgDataReader reader = cmd.ExecuteReader())
                        {
                            // Slow down packets and wait on ReadAsync
                            proxy.SimulatedPacketDelay = 1500;
                            reader.ReadAsync().Wait();

                            // Allow proxy to copy at full speed again
                            proxy.SimulatedOutDelay = false;
                            reader.SetDefaultTimeout(30000);

                            // Close will now observe the stored timeout error
                            string errorMessage = "Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.";
                            DataTestClass.AssertThrowsWrapper<PgException>(reader.Dispose, errorMessage);
                        }
                    }
                }
                proxy.Stop();
            }
            catch
            {
                // In case of error, stop the proxy and dump its logs (hopefully this will help with debugging
                proxy.Stop();
                throw;
            }
        }

        private static void SeqAccessFailureWrapper<TException>(TestDelegate action, CommandBehavior behavior) where TException : Exception
        {
            if (behavior == CommandBehavior.SequentialAccess)
            {
                DataTestClass.AssertThrowsWrapper<TException>(action);   
            }
            else
            {
                action();   
            }
        }

        internal static void VerifySchema(PgDataReader reader)
        {
            string[] expectedColNames =
            {
                "orderid", "customerid", "employeeid", "orderdate", "requireddate", "shippeddate", "shipvia",
                "freight", "shipname", "shipaddress", "shipcity", "shipregion", "shippostalcode", "shipcountry"
            };
            string[] expectedColTypeNames =
            {
                "int4"   , "bpchar" , "int4"   , "date"    , "date"    , "date"    , "int4",
                "numeric", "varchar", "varchar", "varchar" , "varchar" , "varchar" , "varchar"
            };

            for (int i = 0; i < reader.FieldCount; i++)
            {
                DataTestClass.AssertEqualsWithDescription(expectedColNames[i], reader.GetName(i), "FAILED: Received incorrect column name in VerifySchema.");
                DataTestClass.AssertEqualsWithDescription(expectedColTypeNames[i], reader.GetDataTypeName(i), "FAILED: Received incorrect column type name in VerifySchema.");
            }
        }

        private static bool Equal(object v1, object v2)
        {
            if (v1 is DBNull && v2 is DBNull)
            {
                return true;
            }
            return (v1.Equals(v2));
        }

        internal static void VerifyData(PgDataReader reader, object[] buffer)
        {
            object value = null;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                value = reader.GetValue(i);
                Assert.True(Equal(buffer[i], value),
                    string.Format("FAILED: Buffer value and stream.GetValue(i, v) don't match. Buffer value: {0}. GetValue value: {1}.", buffer[i], value));

                value = reader[i];
                Assert.True(Equal(buffer[i], value),
                    string.Format("FAILED: Buffer value and stream.this[i] don't match. Buffer value: {0}. this[i] value: {1}.", buffer[i], value));

                value = reader[reader.GetName(i)];
                Assert.True(Equal(buffer[i], value),
                    string.Format("FAILED: Buffer value and stream.this[name] don't match. Buffer value: {0}. this[name] value: {1}.", buffer[i], value));
            }
        }
    }
}

