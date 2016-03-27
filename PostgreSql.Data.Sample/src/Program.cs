using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Sample
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
            
            var query = "SELECT 'Hello World' AS C1, 'Hello World' AS C2";
            
            // , 12, CAST(NULL AS TEXT), 'Hello World', 'Hello World', 'Hello World', CAST(REPEAT('a', 8000) AS TEXT), 'Hello World' COLLATE \"en_GB.utf8\"

            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                connection.Open();
                
                using (PgCommand cmd = new PgCommand(query, connection))
                {
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        char[] buffer = new char[9000];
                        reader.Read();

                        Console.WriteLine("#0");

                        // Basic success paths
                        reader.GetTextReader(0);
                        reader.GetTextReader(1);
                        
                        Console.WriteLine("#1");
                    }
                }
            }
        
            Console.WriteLine("Finished !!");
        } 
    }
}
