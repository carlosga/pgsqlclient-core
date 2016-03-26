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

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind@2";
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
                        reader.Read();
                    }
                }
            }
        } 
    }
}
