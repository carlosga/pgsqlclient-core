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
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "northwind";
            csb.Password                 = "northwind";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;

            using (var connection = new PgConnection(csb.ToString()))
            {
                MultipleErrorHandling(connection);
            }

            // composite_type_test();
            // pgsqlclient_test();
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

                using (var command = connection.CreateCommand())
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
                        using (var reader = command.ExecuteReader())
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

        static void composite_type_test()
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

            try
            {
                // try
                // {
                //     var dropSql = "DROP TABLE address; DROP TYPE address_type";
                //     using (PgConnection connection = new PgConnection(csb.ToString()))
                //     {
                //         connection.Open();
                //         using (PgCommand command = new PgCommand(dropSql, connection))
                //         {
                //             command.ExecuteNonQuery();
                //         }
                //     }
                // }
                // catch
                // {
                // }

                var createSql = @"CREATE TYPE address_type AS
                                  ( street_address  VARCHAR
                                  , city            VARCHAR
                                  , state           VARCHAR
                                  , zip_code        VARCHAR );
                                  CREATE TABLE address
                                  ( address_id      SERIAL
                                  , address_struct  ADDRESS_TYPE );";

                var insertSql = @"-- Insert the first row.
                                  INSERT INTO address ( address_struct ) VALUES (('#1 52 Hubble Street','Lexington','KY','40511-1225'));
                                  -- Insert the second row.
                                  INSERT INTO address ( address_struct ) VALUES (('#2 54 Hubble Street','Lexington','KY','40511-1225'));";

                var selectAll = "SELECT * FROM address;";
                var selectRaw = @"SELECT address_id
                                  ,      (address_struct).street_address
                                  ,      (address_struct).city
                                  ,      (address_struct).state
                                  ,      (address_struct).zip_code
                                  FROM   address;";

                using (PgConnection connection = new PgConnection(csb.ToString()))
                {
                    connection.Open();

                    // Console.WriteLine("Creating composite type and table");
                    // using (PgCommand createCommand = new PgCommand(createSql, connection))
                    // {
                    //     createCommand.ExecuteNonQuery();
                    // }

                    // Console.WriteLine("Inserting data");
                    // using (PgCommand insertCommand = new PgCommand(insertSql, connection))
                    // {
                    //     insertCommand.ExecuteNonQuery();
                    // }

                    // var pinsertSql = "INSERT INTO address ( address_struct ) VALUES ((@p1,@p2,@p3,@p4))";
                    // using (PgCommand pinsertCommand = new PgCommand(pinsertSql, connection))
                    // {
                    //     pinsertCommand.Parameters.AddWithValue("@p1", "#3 52 Hubble Street");
                    //     pinsertCommand.Parameters.AddWithValue("@p2", "#3 Lexington");
                    //     pinsertCommand.Parameters.AddWithValue("@p3", "#3 KY");
                    //     pinsertCommand.Parameters.AddWithValue("@p4", "#3 40511-1225");

                    //     pinsertCommand.ExecuteNonQuery();
                    // }

                    // Console.WriteLine("Issuing raw select");
                    // using (PgCommand selectRawCommand = new PgCommand(selectRaw, connection))
                    // {
                    //     using (PgDataReader readerRaw = selectRawCommand.ExecuteReader())
                    //     {
                    //         while (readerRaw.Read()) { }
                    //     }
                    // }
                    // Console.WriteLine("Issuing select *");
                    // using (PgCommand selectAllCommand = new PgCommand(selectAll, connection))
                    // {
                    //     using (PgDataReader readerAll = selectAllCommand.ExecuteReader())
                    //     {
                    //         while (readerAll.Read()) { }
                    //     }
                    // }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                // var dropSql = "DROP TABLE address; DROP TYPE address_type";
                // using (PgConnection connection = new PgConnection(csb.ToString()))
                // {
                //     connection.Open();
                //     using (PgCommand command = new PgCommand(dropSql, connection))
                //     {
                //         command.ExecuteNonQuery();
                //     }
                // }
            }
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
            //csb.CommandTimeout           = 10000;

            int count = 0;

            using (var conn = new PgConnection(csb.ToString()))
            {  
                conn.Open();

                using (var command = conn.CreateCommand())
                {
                    command.FetchSize   = 2000;
                    command.CommandText = "select * from pg_attribute a cross join pg_attribute b limit 2000";

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    Console.WriteLine("-->");

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
