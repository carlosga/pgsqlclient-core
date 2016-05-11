using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
            TestReaderNonMars();
        }

        private const string COMMAND_TEXT_1 = "SELECT au_id, au_lname, au_fname, phone, address, city, state, zip, contract from authors";
        private const string COMMAND_TEXT_2 = "SELECT au_lname from authors";
        private const string COLUMN_NAME_2  = "au_lname";
        private const string DATABASE_NAME  = "northwind";

        private enum ReaderTestType
        {
            ReaderClose,
            ReaderDispose,
            ReaderGC,
            ConnectionClose,
            ReaderGCConnectionClose,
        }

        private enum ReaderVerificationType
        {
            ExecuteReader,
            ChangeDatabase,
            BeginTransaction,
            EnlistDistributedTransaction,
        }

        private enum TransactionTestType
        {
            TransactionRollback,
            TransactionDispose,
            TransactionGC,
            ConnectionClose,
            TransactionGCConnectionClose,
        }

        public static void TestReaderNonMars()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource      = "localhost";
            csb.InitialCatalog  = "pubs";
            csb.UserID          = "pgsqlclient";
            csb.Password        = "pgsqlclient";
            csb.Pooling         = true;
            csb.PortNumber      = 5432;
            csb.MaxPoolSize     = 1;
            
            string connString = csb.ToString();

            TestReaderNonMarsCase("Case  1: ExecuteReader, Close, ExecuteReader.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case  2: ExecuteReader, Dispose, ExecuteReader.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case  3: ExecuteReader, GC, ExecuteReader.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case  4: ExecuteReader, Connection Close, ExecuteReader.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case  5: ExecuteReader, GC, Connection Close, ExecuteReader.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ExecuteReader);

            TestReaderNonMarsCase("Case  6: ExecuteReader, Close, ChangeDatabase.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case  7: ExecuteReader, Dispose, ChangeDatabase.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case  8: ExecuteReader, GC, ChangeDatabase.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case  9: ExecuteReader, Connection Close, ChangeDatabase.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case 10: ExecuteReader, GC, Connection Close, ChangeDatabase.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ChangeDatabase);

            TestReaderNonMarsCase("Case 11: ExecuteReader, Close, BeginTransaction.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 12: ExecuteReader, Dispose, BeginTransaction.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 13: ExecuteReader, GC, BeginTransaction.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 14: ExecuteReader, Connection Close, BeginTransaction.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 15: ExecuteReader, GC, Connection Close, BeginTransaction.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.BeginTransaction);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestReaderNonMarsCase(string caseName, string connectionString, ReaderTestType testType, ReaderVerificationType verificationType)
        {
            WeakReference weak = null;

            using (PgConnection con = new PgConnection(connectionString))
            {
                con.Open();

                using (PgCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = COMMAND_TEXT_1;

                    PgDataReader gch = null;
                    if ((testType != ReaderTestType.ReaderGC) && (testType != ReaderTestType.ReaderGCConnectionClose))
                    {
                        gch = cmd.ExecuteReader();
                    }

                    switch (testType)
                    {
                        case ReaderTestType.ReaderClose:
                            gch.Dispose();
                            break;

                        case ReaderTestType.ReaderDispose:
                            gch.Dispose();
                            break;

                        case ReaderTestType.ReaderGC:
                            weak = OpenNullifyReader(cmd);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Debug.Assert(!weak.IsAlive, "Reader is still alive!");
                            break;

                        case ReaderTestType.ConnectionClose:
                            GC.SuppressFinalize(gch);
                            con.Close();
                            con.Open();
                            break;

                        case ReaderTestType.ReaderGCConnectionClose:
                            weak = OpenNullifyReader(cmd);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Debug.Assert(!weak.IsAlive, "Reader is still alive!");
                            con.Close();
                            con.Open();
                            break;
                    }

                    switch (verificationType)
                    {
                        case ReaderVerificationType.ExecuteReader:
                            cmd.CommandText = COMMAND_TEXT_2;
                            using (PgDataReader rdr = cmd.ExecuteReader())
                            {
                                rdr.Read();
                                Debug.Assert(rdr.FieldCount == 1);
                                Debug.Assert(rdr.GetName(0) == COLUMN_NAME_2);
                            }
                            break;

                        case ReaderVerificationType.ChangeDatabase:
                            con.ChangeDatabase(DATABASE_NAME);
                            Debug.Assert(con.Database == DATABASE_NAME);
                            break;

                        case ReaderVerificationType.BeginTransaction:
#warning TODO: See how to port to postgresql
                            // cmd.Transaction = con.BeginTransaction();
                            // cmd.CommandText = "select @@trancount";
                            // int tranCount = (int)cmd.ExecuteScalar();
                            // Assert.AreEqual(tranCount, 1);
                            break;
                    }
                }
            }
        }

        private static WeakReference OpenNullifyReader(PgCommand cmd)
        {
            PgDataReader reader = cmd.ExecuteReader();
            WeakReference weak = new WeakReference(reader);
            reader = null;
            return weak;
        }

        private static WeakReference OpenNullifyTransaction(PgConnection connection)
        {
            PgTransaction transaction = connection.BeginTransaction();
            WeakReference weak = new WeakReference(transaction);
            transaction = null;
            return weak;
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
