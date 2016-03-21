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
                    
                    using (var command = new PgCommand("SELECT * FROM accounting.accounting_plan_totals", connection, transaction))
                    {
                        System.Console.WriteLine("Executing command ");
                        
                        int count = 0;
                        
                        Stopwatch stopWatch = new Stopwatch();
                        stopWatch.Start();
                        
                        using (var reader = command.ExecuteReader())
                        {
                            System.Console.WriteLine("Fetching rows");
                                                                
                            while (reader.Read())
                            {
                                // for (int i = 0; i < reader.FieldCount; i++)
                                // {
                                //     Console.Write(reader.GetValue(i) + "\t");
                                // }

                                // Console.WriteLine();
                                
                                count++;
                            }
                        }
                        
                        stopWatch.Stop();
                        // Get the elapsed time as a TimeSpan value.
                        TimeSpan ts = stopWatch.Elapsed;

                        // Format and display the TimeSpan value.
                        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                            ts.Hours, ts.Minutes, ts.Seconds,
                            ts.Milliseconds / 10);
                        Console.WriteLine("RunTime " + elapsedTime);
                        Console.WriteLine("RowCount " + count);
                    }
                }
            }            
        } 
    }
}
