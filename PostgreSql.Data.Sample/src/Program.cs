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
            
            var connectionString = csb.ToString();
            var builder          = new PgConnectionStringBuilder(connectionString);

            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { Password = string.Empty };
            using (var connection = new PgConnection(badBuilder.ConnectionString))
            {
                foreach (var key in badBuilder.Keys)
                {
                    Console.WriteLine(key);   
                    Console.WriteLine(badBuilder[key.ToString()]);
                }
                
                connection.Open();
                
                //string errorMessage = string.Format(CultureInfo.InvariantCulture, logonFailedErrorMessage, badBuilder.UserID);
                // VerifyConnectionFailure<PgException>(() => PgConnection.Open(), errorMessage, (ex) => VerifyException(ex, 1, 18456, 1, 14));
            }
            
            Console.WriteLine("Finished !");
        } 
    }
}
