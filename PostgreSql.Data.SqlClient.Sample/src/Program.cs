using System;
using System.Threading;
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

            MultiThreadedCancel(csb.ToString(), false);

            Console.WriteLine("Finished !!");
        }

        public static void MultiThreadedCancel(string constr, bool async)
        {
            using (PgConnection con = new PgConnection(constr))
            {
                con.Open();
                var command = con.CreateCommand();
                command.CommandText = "SELECT * FROM orders; SELECT pg_sleep(8); SELECT * FROM customers";

                Thread rThread1 = new Thread(ExecuteCommandCancelExpected);
                Thread rThread2 = new Thread(CancelSharedCommand);
                Barrier threadsReady = new Barrier(2);
                object state = new Tuple<bool, PgCommand, Barrier>(async, command, threadsReady);

                rThread1.Start(state);
                rThread2.Start(state);
                rThread1.Join();
                rThread2.Join();
                
                Console.WriteLine("Threads finished");

                //CommandCancelTest.VerifyConnection(command);
            }
        }

        public static void ExecuteCommandCancelExpected(object state)
        {
            var       stateTuple   = (Tuple<bool, PgCommand, Barrier>)state;
            bool      async        = stateTuple.Item1;
            PgCommand command      = stateTuple.Item2;
            Barrier   threadsReady = stateTuple.Item3;

            threadsReady.SignalAndWait();
            using (PgDataReader r = command.ExecuteReader())
            {
                do
                {
                    while (r.Read())
                    {
                    }
                } while (r.NextResult());
            }
        }

        public static void CancelSharedCommand(object state)
        {
            var stateTuple = (Tuple<bool, PgCommand, Barrier>)state;

            stateTuple.Item3.SignalAndWait();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            stateTuple.Item2.Cancel();
        }
    }
}
