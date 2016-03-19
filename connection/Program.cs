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
            Run().Wait();
                        
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
            
            var reader = await command.ExecuteReaderAsync();

            System.Console.WriteLine("Fetching rows");
            
            while (await reader.ReadAsync())
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    Console.Write(reader.GetValue(i) + "\t");
                }

                Console.WriteLine();
            }
            
            reader.Dispose();
            command.Dispose();
            transaction.Dispose();
            connection.Dispose();
        } 
    }
}
