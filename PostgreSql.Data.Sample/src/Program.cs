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
            
            using (PgConnection conn = new PgConnection(csb.ToString()))
            {
                conn.Open();

                using (PgDataReader reader1 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader4 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                using (PgDataReader reader5 = (new PgCommand("select * from Orders", conn)).ExecuteReader())
                {
                    int rows = 0;
                    while (reader1.Read())
                    {
                        rows++;
                    }
                    Console.WriteLine($"#1 {rows}");

                    // rows = 0;
                    // while (reader2.Read())
                    // {
                    //     rows++;
                    // }
                    // Console.WriteLine($"#2 {rows}");

                    // rows = 0;
                    // while (reader3.Read())
                    // {
                    //     rows++;
                    // }
                    // Console.WriteLine($"#3 {rows}");

                    // rows = 0;
                    // while (reader4.Read())
                    // {
                    //     rows++;
                    // }
                    // Console.WriteLine($"#4 {rows}");

                    // rows = 0;
                    // while (reader5.Read())
                    // {
                    //     rows++;
                    // }
                    // Console.WriteLine($"#5 {rows}");
                }
            }
            
            Console.WriteLine("Finished !");
        } 
    }
}
