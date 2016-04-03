using System;
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
                        
            object o  = PgDate.MaxValue;
            var    p2 = (DateTime)o;
                                
            Console.WriteLine($"Finished!!");
        } 
    }
}
