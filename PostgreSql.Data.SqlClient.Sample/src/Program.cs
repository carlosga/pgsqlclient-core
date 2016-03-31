using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "pubs";
            csb.UserID                   = "pubs";
            csb.Password                 = "pubs";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;
            
            var count = 0;
            
            using (PgConnection c = new PgConnection(csb.ToString()))
            {
                c.Open();
                string sqlBatch = "select * from titles";
                using (PgCommand cmd = new PgCommand(sqlBatch, c))
                using (PgDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read()) 
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            Console.Write($"{reader.GetValue(i)} | ");
                        }
                        
                        Console.WriteLine(String.Empty);
                        
                        ++count; 
                    }
                }
            }
        
            Console.WriteLine($"Finished ({count}) !!");
        } 
    }
}
