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
            Task t = Task.Run(async () => await Run());
            
            t.Wait(-1);
            
            Console.WriteLine("finishing program");   
        }
        
        async static Task Run()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource      = "localhost";
            csb.InitialCatalog  = "chronos";
            csb.UserID          = "chronos";
            csb.Password        = "::pelanas.2016::";
            csb.PortNumber      = 5432;
            csb.Ssl             = false;
            csb.Pooling         = false;
                                   
            var connection = new PgConnection(csb.ToString());

            await connection.OpenAsync();

            System.Console.WriteLine("Connection open");
                
            var transaction = await connection.BeginTransactionAsync();

            System.Console.WriteLine("Transaction Started");
            
            var command = new PgCommand("SELECT * FROM accounting.accounting_company", connection, transaction);

            System.Console.WriteLine("Executing command");
            
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
            
            transaction.Rollback();                    
        } 
    }
}
