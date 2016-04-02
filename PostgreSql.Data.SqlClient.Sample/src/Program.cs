using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PostgreSql.Data.PgTypes;

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
            
            
            DateTime d  = DateTime.Today;
            object   o  = d;
            PgDate   p1 = (PgDate)d;
            PgDate   p2 = (PgDate)o;
                                
            Console.WriteLine($"Finished!!");
        } 
    }
}
