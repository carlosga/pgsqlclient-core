using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Sample
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
            
            using (PgConnection conn = new PgConnection(csb.ToString()))
            {
                conn.Open();
                PgParameter p = new PgParameter("@p", PgDbType.TimestampTZ);
                p.Value = DBNull.Value;
                p.Size  = 27;
                PgCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT @p";
                cmd.Parameters.Add(p);
                
                cmd.ExecuteScalar();
            }
            
            Console.WriteLine("Finished !!");
        } 
    }
}
