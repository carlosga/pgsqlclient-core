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
            csb.PacketSize               = Int16.MaxValue;

            string sql    = "SELECT @1; SELECT @2; SELECT @3;";
            int[]  values = new int[] { 1, 2, 3 };
            int    index  = 0;
            
            using (PgConnection connection = new PgConnection(csb.ToString()))
            {
                connection.Open();

                using (PgCommand command = new PgCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@1", values[0]);
                    command.Parameters.AddWithValue("@2", values[1]);
                    command.Parameters.AddWithValue("@3", values[2]);

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        do 
                        {
                            var readed = reader.Read();
                            var value  = reader.GetInt32(0);

                            Console.WriteLine($"expected: {values[index]} actual: {value}");

                            ++index;

                        } while (reader.NextResult());
                    }
                }
            }

            // composite_type_test();
            // pgsqlclient_test();
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
                var createSql = @"CREATE TYPE address_type AS
                                  ( street_address  VARCHAR
                                  , city            VARCHAR
                                  , state           VARCHAR
                                  , zip_code        VARCHAR );
                                  CREATE TABLE address
                                  ( address_id      SERIAL
                                  , address_struct  ADDRESS_TYPE );";

                var insertSql = @"-- Insert the first row.
                                  INSERT INTO address ( address_struct ) VALUES (('52 Hubble Street','Lexington','KY','40511-1225'));
                                  -- Insert the second row.
                                  INSERT INTO address ( address_struct ) VALUES (('54 Hubble Street','Lexington','KY','40511-1225'));";

                var selectAll = "SELECT * FROM address;";
                var selectRaw = @"SELECT address_id
                                  ,      (address_struct).street_address
                                  ,      (address_struct).city
                                  ,      (address_struct).state
                                  ,      (address_struct).zip_code
                                  FROM   address;";

                using (PgConnection connection = new PgConnection(csb.ToString()))
                {
                    Console.WriteLine("Creating composite type and table");
                    connection.Open();
                    using (PgCommand createCommand = new PgCommand(createSql, connection))
                    {
                        createCommand.ExecuteNonQuery();
                    }

                    Console.WriteLine("Inserting data");
                    using (PgCommand insertCommand = new PgCommand(insertSql, connection))
                    {
                        insertCommand.ExecuteNonQuery();
                    }
                    Console.WriteLine("Issuing raw select");
                    using (PgCommand selectRawCommand = new PgCommand(selectRaw, connection))
                    {
                        using (PgDataReader readerRaw = selectRawCommand.ExecuteReader())
                        {
                            while (readerRaw.Read()) { }
                        }
                    }
                    Console.WriteLine("Issuing select *");
                    using (PgCommand selectAllCommand = new PgCommand(selectAll, connection))
                    {
                        using (PgDataReader readerAll = selectAllCommand.ExecuteReader())
                        {
                            while (readerAll.Read()) { }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                throw;
            }
            finally
            {
                var dropSql = "DROP TABLE address; DROP TYPE address_type";
                using (PgConnection connection = new PgConnection(csb.ToString()))
                {
                    connection.Open();
                    using (PgCommand command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
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
                    command.CommandText = "select * from pg_attribute a cross join pg_attribute b";

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
