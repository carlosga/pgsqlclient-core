using System;
using System.Data;
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

            // MultiThreadedCancel(csb.ToString(), false);
            
            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                // MultipleErrorHandling(connection);   
            }

            using (var conn = new PgConnection(csb.ToString()))
            {                                     
                var command = conn.CreateCommand();

                command.Parameters.AddWithValue("@p1", 1);
                command.Parameters.AddWithValue("@p2", 2);
                command.Parameters.AddWithValue("@p3", 3);
                
                command.CommandText = "SELECT @p1;SELECT @p2;SELECT @p3";
               
                conn.Open();
                
                try
                {
                   
                    int v1 = command.ExecuteNonQuery();                    
                }
                catch (System.Exception)
                {                    
                }
            }
            
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

        private static void MultipleErrorHandling(PgConnection connection)
        {
            try
            {
                Console.WriteLine("MultipleErrorHandling {0}", connection.GetType().Name);
                Type expectedException = typeof(PgException);

                connection.InfoMessage += delegate (object sender, PgInfoMessageEventArgs args)
                {
                    Console.WriteLine($"*** SQL CONNECTION INFO MESSAGE : {args.Message} ****");
                };

                connection.Open();

                using (PgCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT raise_notice('0');"
                      + "SELECT 1 as num, 'ABC' as str;"
                      + "SELECT raise_notice('1');"
                      + "SELECT raise_error('Error 1');"
                      + "SELECT raise_notice('3');"
                      + "SELECT 2 as num, 'ABC' as str;"
                      + "SELECT raise_notice('4');"
                      + "SELECT raise_error('Error 2');"
                      + "SELECT raise_notice('5');"
                      + "SELECT 3 as num, 'ABC' as str;"
                      + "SELECT raise_notice('6');"
                      + "SELECT raise_error('Error 3');"
                      + "SELECT raise_notice('7');"
                      + "SELECT 4 as num, 'ABC' as str;"
                      + "SELECT raise_notice('8');"
                      + "SELECT raise_error('Error 4');" 
                      + "SELECT raise_notice('9');"
                      + "SELECT 5 as num, 'ABC' as str;"
                      + "SELECT raise_notice('10');"
                      + "SELECT raise_error('Error 5');"
                      + "SELECT raise_notice('11');";

                    try
                    {
                        Console.WriteLine("**** ExecuteNonQuery *****");
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        // PrintException(expectedException, e);
                    }

                    try
                    {
                        Console.WriteLine("**** ExecuteScalar ****");
                        command.ExecuteScalar();
                    }
                    catch (Exception e)
                    {
                        // PrintException(expectedException, e);
                    }

                    try
                    {
                        Console.WriteLine("**** ExecuteReader ****");
                        using (PgDataReader reader = command.ExecuteReader())
                        {
                            bool moreResults = true;
                            do
                            {
                                try
                                {
                                    Console.WriteLine("NextResult");
                                    moreResults = reader.NextResult();
                                }
                                catch (Exception e)
                                {
                                    // PrintException(expectedException, e);
                                }
                            } while (moreResults);
                        }
                    }
                    catch (Exception e)
                    {
                        // PrintException(null, e);
                    }
                }
            }
            catch (Exception e)
            {
                // PrintException(null, e);
            }
            try
            {
                connection.Dispose();
            }
            catch (Exception e)
            {
                // PrintException(null, e);
            }
        }        
    }
}
