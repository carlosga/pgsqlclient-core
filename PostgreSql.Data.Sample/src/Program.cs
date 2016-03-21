using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PostgreSql.Data.PostgreSqlClient;

namespace ConsoleApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource      = "localhost";
            csb.InitialCatalog  = "northwind";
            csb.UserID          = "northwind";
            csb.Password        = "northwind@2";
            csb.PortNumber      = 5432;
            csb.Ssl             = false;
            csb.Pooling         = false;

            // Ported from the Microsoft System.Data.SqlClient test suite.
            // ---------------------------------------------------------------------
            // Licensed to the .NET Foundation under one or more agreements.
            // The .NET Foundation licenses this file to you under the MIT license.
            // See the LICENSE file in the project root for more information.
            using (PgConnection conn = new PgConnection(csb.ToString()))
            {
                conn.Open();
                string query =
                    "select \"OrderID\" from orders where \"OrderID\" < @id order by \"OrderID\";"; 
                //   + "select * from shippers order by shipperid;" 
                //   + "select * from shippers order by shipperid;" 
                //   + "select * from region order by regionid;" 
                //   + "select lastname from employees order by lastname";

                // Each array in the expectedResults is a separate query result
                string[][] expectedResults =
                {
                    new string[] { "10248", "10249", "10250", "10251", "10252", "10253", "10254" }, // All separate rows
                    new string[] { "" }, // Empty query result
                    new string[]
                    {
                        "1", "Speedy Express"  , "(503) 555-9831",  // Query Row 1
                        "2", "United Package"  , "(503) 555-3199",  // Query Row 2
                        "3", "Federal Shipping", "(503) 555-9931"   // Query Row 3
                    },
                    new string[]
                    {
                        "1", "Eastern                                           ", // Query Row 1
                        "2", "Western                                           ", // Query Row 2
                        "3", "Northern                                          ", // Query Row 3
                        "4", "Southern                                          "  // Query Row 4
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
                            // Assert.True(numBatches < expectedResults.Length, "ERROR: Received more batches than were expected.");
                            object[] values = new object[r1.FieldCount];
                            // Current "column" in expected row is (valuesChecked MOD FieldCount), since 
                            // expected rows for current batch are appended together for easy formatting
                            int valuesChecked = 0;
                            while (r1.Read())
                            {
                                r1.GetValues(values);

                                for (int col = 0; col < values.Length; col++, valuesChecked++)
                                {
                                    // Assert.True(valuesChecked < expectedResults[numBatches].Length, "ERROR: Received more results for this batch than was expected");
                                    string expectedVal = expectedResults[numBatches][valuesChecked];
                                    string actualVal = values[col].ToString();

                                    // DataTestClass.AssertEqualsWithDescription(expectedVal, actualVal, "FAILED: Received a different value than expected.");
                                }
                            }
                            numBatches++;
                        } while (r1.NextResult());
                    }
                }
            }                                   
        } 
    }
}
