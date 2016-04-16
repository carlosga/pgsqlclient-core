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
            // composite_type_test();
            pgsqlclient_test();
        }

        static void pgsqlclient_test()
        {
            var csb = new PgConnectionStringBuilder();

            csb.DataSource               = "localhost";
            csb.InitialCatalog           = "pubs";
            csb.UserID                   = "pubs";
            csb.Password                 = "pubs";
            csb.PortNumber               = 5432;
            csb.Encrypt                  = false;
            csb.Pooling                  = false;
            csb.MultipleActiveResultSets = true;
            csb.PacketSize               = Int16.MaxValue;
            //csb.CommandTimeout           = 10000;

        [Fact]
        public static void HasRowsTest()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                string sqlBatch =
                    "select * from orders limit 10;" +
                    "select * from orders limit  5;" +
                    "select * from orders limit  0;";

                using (PgCommand cmd = new PgCommand(sqlBatch, conn))
                {
                    PgDataReader reader;
                    using (reader = cmd.ExecuteReader())
                    {
                        Assert.True(reader.HasRows, "FAILED: Failure #1: HasRows");
                        while (reader.Read())
                        {
                            Assert.True(reader.HasRows, "FAILED: Failure #2: HasRows");
                        }
                        Assert.True(reader.HasRows, "FAILED: Failure #3: HasRows");

                        Assert.True(reader.NextResult(), "FAILED: Failure #3.5: NextResult");

                        Assert.True(reader.HasRows, "FAILED: Failure #4: HasRows");
                        while (reader.Read())
                        {
                            Assert.True(reader.HasRows, "FAILED: Failure #5: HasRows");
                        }
                        Assert.True(reader.HasRows, "FAILED: Failure #6: HasRows");

                        Assert.True(reader.NextResult(), "FAILED: Failure #6.5: NextResult");

                        Assert.False(reader.HasRows, "FAILED: Failure #7: HasRows");
                        while (reader.Read())
                        {
                            Assert.False(reader.HasRows, "FAILED: Failure #8: HasRows");
                        }
                        Assert.False(reader.HasRows, "FAILED: Failure #9: HasRows");

                        Assert.False(reader.NextResult(), "FAILED: Failure #9.5: NextResult");

                        Assert.False(reader.HasRows, "FAILED: Failure #10: HasRows");
                    }

                    bool result;
                    string errorMessage = "Invalid attempt to read when no data is present.";
                    DataTestClass.AssertThrowsWrapper<InvalidOperationException>(() => result = reader.HasRows, errorMessage);
                }
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
    }
}
