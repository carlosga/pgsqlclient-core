using System;
using System.Diagnostics;

namespace PostgreSql.Data.SqlClient.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // for (int i = 0; i < 20; ++i)
            // {
            //     pgsqlclient_test();
            // }
            test();
        }

        static void test()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource      = "localhost";
            csb.InitialCatalog  = "northwind";
            csb.UserID          = "pgsqlclient";
            csb.Password        = "pgsqlclient";
            csb.Pooling         = false;
            csb.PortNumber      = 5432;

            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                connection.Open();

                connection.ChangeDatabase("pubs");
                
                using (var command = new PgCommand("select * from authors", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            
                        }
                    }
                }
            }
        }
        
        public static void AssertException<T>(Action action, string expectedErrorMessage) where T : Exception
        {
            bool throws = false;
            try
            {
                action();
            }
            catch (Exception exception)
            {
                throws = true;
                System.Diagnostics.Debug.Assert(exception.Message == expectedErrorMessage);
            }
            finally 
            {
                if (!throws)
                {
                    throw new Exception();
                }
            }
        }

        static void pgsqlclient_test()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource      = "localhost";
            csb.InitialCatalog  = "northwind";
            csb.UserID          = "pgsqlclient";
            csb.Password        = "pgsqlclient";
            csb.Pooling         = false;
            csb.PortNumber      = 5432;

            int count = 0;

            using (var connection = new PgConnection(csb.ToString()))
            {
                using (var command = new PgCommand("select * from pg_attribute a cross join pg_attribute b limit 200000", connection))
                // using (var command = new PgCommand("SELECT * FROM pg_timezone_names()", connection))
                {
                    connection.Open();

                    // command.FetchSize = 0;

                    Stopwatch stopWatch = new Stopwatch();
                    stopWatch.Start();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read()) 
                        {
                            // for (int i = 0; i < reader.FieldCount; ++i) 
                            // {
                            //     Console.Write($"{reader.GetValue(i)}  |  ");
                            // }
                            // Console.WriteLine(String.Empty);
                            ++count;
                        }
                    }

                    stopWatch.Stop();
                    // Get the elapsed time as a TimeSpan value.
                    TimeSpan ts = stopWatch.Elapsed;

                    // Format and display the TimeSpan value.
                    string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
                    Console.WriteLine("(pgsqlclient) RunTime " + elapsedTime);
                    Console.WriteLine("(pgsqlclient) Row count " + count);
                }
            }
        }

        static void composite_type_test()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "northwind";
            csb.UserID                   = "pgsqlclient";
            csb.Password                 = "pgsqlclient";
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
    }
}
