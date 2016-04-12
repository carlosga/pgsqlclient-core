using System;
using System.Data;
using System.Diagnostics;
using System.Threading;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.SqlClient.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            pgsqlclient_test();
        }
        
        static void pgsqlclient_test()
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
            csb.PacketSize               = Int16.MaxValue;
            csb.FetchSize                = 1000;

            int count = 0;

            using (var conn = new PgConnection(csb.ToString()))
            {  
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select * from pg_type a cross join pg_type b";
                    
                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) { ++count; }
                    }

                    stopWatch.Stop();

                    TimeSpan ts = stopWatch.Elapsed;

                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                    Console.WriteLine("RunTime " + elapsedTime);
                }
            }

            Console.WriteLine($"Finished {count}");
        }
    }
}
