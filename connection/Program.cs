using System;
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
            csb.InitialCatalog  = "chronos";
            csb.UserID          = "chronos";
            csb.Password        = "chronos@2";
            csb.PortNumber      = 5432;
            csb.Ssl             = false;
            csb.Pooling         = false;
                                   
            using (var connection = new PgConnection(csb.ToString()))
            {
                connection.Open();

                System.Console.WriteLine("Connection open");
                    
                using (var transaction = connection.BeginTransaction())
                {
                    System.Console.WriteLine("Transaction Started");
                    
                    using (var command = new PgCommand("SELECT * FROM accounting.accounting_company", connection, transaction))
                    {
                        System.Console.WriteLine("Executing command ");
                        
                        using (var reader = command.ExecuteReader())
                        {
                            System.Console.WriteLine("Fetching rows");
                            
                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    Console.Write(reader.GetValue(i) + "\t");
                                }

                                Console.WriteLine();
                            }
                        }
                    }
                }
            }            
        } 
    }
}
