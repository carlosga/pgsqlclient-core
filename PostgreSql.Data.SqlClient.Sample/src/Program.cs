using System;

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
             
            using (PgConnection conn = new PgConnection(csb.ToString()))
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
                        // DataTestClass.AssertEqualsWithDescription(expectedFirstString, actualFirstString, "FAILED: First string did not match");
                        // DataTestClass.AssertEqualsWithDescription(expectedSecondString, actualSecondString, "FAILED: Second string did not match");
                    }
                }
            }
             
            Console.WriteLine("Finished !!");
        } 
    }
}
