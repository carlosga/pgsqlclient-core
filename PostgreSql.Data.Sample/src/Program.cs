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
            
            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                connection.Open();
                PgCommand command = new PgCommand("pg_sleep(1);SELECT 1", connection);
                command.CommandTimeout = 1;
                Task<object> result = command.ExecuteScalarAsync();

                //Assert.True(((IAsyncResult)result).AsyncWaitHandle.WaitOne(30 * 1000), "Expected timeout after one second, but no results after 30 seconds");
                //Assert.True(result.IsFaulted, string.Format("Expected task result to be faulted, but instead it was {0}", result.Status));
                //Assert.True(connection.State == ConnectionState.Open, string.Format("Expected connection to be open after soft timeout, but it was {0}", connection.State));

                PgCommand command2 = new PgCommand("pg_sleep(1);SELECT 1", connection);
                command2.CommandTimeout = 1;
                result = command2.ExecuteScalarAsync();

                //Assert.True(((IAsyncResult)result).AsyncWaitHandle.WaitOne(30 * 1000), "Expected timeout after six or so seconds, but no results after 30 seconds");
                //Assert.True(result.IsFaulted, string.Format("Expected task result to be faulted, but instead it was {0}", result.Status));

                // Pause here to ensure that the async closing is completed
                // Thread.Sleep(200);
                // Assert.True(connection.State == ConnectionState.Closed, string.Format("Expected connection to be closed after hard timeout, but it was {0}", connection.State));
            }
            
            Console.WriteLine("Finished !");
        } 
    }
}
