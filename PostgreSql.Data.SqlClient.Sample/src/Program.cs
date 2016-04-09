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
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;

            var box1 = new PgBox(6.18805847773352E+305, 9.48221676957806E+307
                               , -6.4471844263193E+307, 1.17666759338731E+308);

            var box2 = new PgBox(-6.4471844263193E+307, 1.17666759338731E+308
                               , 6.18805847773352E+305, 9.48221676957806E+307);
            
            Console.WriteLine("Finished !!");
        } 
    }
}
