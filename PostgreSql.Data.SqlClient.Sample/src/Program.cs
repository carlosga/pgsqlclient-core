using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.SqlClient.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;
            csb.PacketSize               = Int16.MaxValue;
            csb.FetchSize                = 200;

            using (PgConnection conn = new PgConnection(csb.ToString()))
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
                    cmd.Parameters.Add(new PgParameter("@id", PgDbType.Integer)).Value = 10255;
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
        
        static void pgsqlclient_test()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;
            csb.PacketSize               = Int16.MaxValue;
            csb.FetchSize                = 200;

            int count = 0;

            using (var conn = new PgConnection(csb.ToString()))
            {  
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select * from pg_type a cross join pg_type b";
                    
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) { ++count; }
                    }

                    stopWatch.Stop();

                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;

                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                    Console.WriteLine("RunTime " + elapsedTime);
                }
            }

            Console.WriteLine($"Finished {count}");
        }
    }
}
