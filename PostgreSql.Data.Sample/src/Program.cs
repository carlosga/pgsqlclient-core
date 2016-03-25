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

                using (PgDataReader reader1 = (new PgCommand("select * from Orders where OrderID = 10248", conn)).ExecuteReader())
                using (PgDataReader reader2 = (new PgCommand("select * from Orders where OrderID = 10249", conn)).ExecuteReader())
                using (PgDataReader reader3 = (new PgCommand("select * from Orders where OrderID = 10250", conn)).ExecuteReader())
                {
                    if (!(reader1.Read() && reader2.Read() && reader3.Read()))
                    {
                        throw new Exception("MARSSyncExecuteReaderTest4 failure #1");
                    }

                    if (!(reader1.GetInt32(0) == 10248 
                       && reader2.GetInt32(0) == 10249 
                       && reader3.GetInt32(0) == 10250))
                    {
                        throw new Exception("MARSSyncExecuteReaderTest4 failure #2");                        
                    }
                                                       
                    if (reader1.Read() || reader2.Read() || reader3.Read())
                    {
                        throw new Exception("MARSSyncExecuteReaderTest4 failure #3");
                    }
                }
            }
            
            Console.WriteLine("Finished !");
        } 
    }
}
