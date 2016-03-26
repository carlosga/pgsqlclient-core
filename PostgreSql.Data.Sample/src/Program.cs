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
            
            // Cannot open database \"{0}\" requested by the login. The login failed.
            
            // tests incorrect database name
            var badBuilder   = new PgConnectionStringBuilder(csb.ConnectionString) { InitialCatalog = "NotADatabase" };
            var errorMessage = string.Format("Cannot open database \"{0}\" requested by the login. The login failed.", badBuilder.InitialCatalog);
                        
            using (var connection = new PgConnection(badBuilder.ConnectionString))
            {
                connection.Open();
            }
        } 
    }
}
