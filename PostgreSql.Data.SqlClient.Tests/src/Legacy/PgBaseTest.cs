// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using NUnit.Framework;
using System;
using System.Data;
using System.Diagnostics;
using System.Net.Security;
using System.Text;

namespace PostgreSql.Data.SqlClient.UnitTests
{
    public abstract class PgBaseTest
    {
        private PgConnection _connection;

        public PgConnection Connection
        {
            get { return _connection; }
        }

        public PgBaseTest()
        {
        }

        public void SetUp()
        {
            // DropDatabase();
            // CreateDatabase();

            // // Build the connection string
            // var csb = new PgConnectionStringBuilder();

            // csb.DataSource      = ConfigurationManager.AppSettings["Data Source"];
            // csb.InitialCatalog  = ConfigurationManager.AppSettings["Initial Catalog"];
            // csb.UserID          = ConfigurationManager.AppSettings["User ID"];
            // csb.Password        = ConfigurationManager.AppSettings["Password"];
            // csb.PortNumber      = Convert.ToInt32(ConfigurationManager.AppSettings["Port Number"]);
            // csb.Ssl             = Convert.ToBoolean(ConfigurationManager.AppSettings["SSL"]);
            // csb.Pooling         = false;

            // _connection = new PgConnection(csb.ToString());

            // _connection.StateChange               += new StateChangeEventHandler(StateChange);
            // _connection.UserCertificateValidation += new RemoteCertificateValidationCallback(connection_UserCertificateValidation);

            // _connection.Open();

            // CreateTables();
            // CreateFunctions();
        }

        public void TearDown()
        {
            _connection.Close();
        }

        private void CreateDatabase()
        {
            // var csb = new PgConnectionStringBuilder();

            // csb.DataSource     = ConfigurationManager.AppSettings["Data Source"];
            // csb.UserID         = ConfigurationManager.AppSettings["User ID"];
            // csb.Password       = ConfigurationManager.AppSettings["Password"];
            // csb.PortNumber     = Convert.ToInt32(ConfigurationManager.AppSettings["Port Number"]);
            // csb.Ssl            = Convert.ToBoolean(ConfigurationManager.AppSettings["SSL"]);
            // csb.InitialCatalog = String.Empty;
            // csb.Pooling        = false;

            // using (var connection = new PgConnection(csb.ToString()))
            // {
            //     connection.StateChange               += new StateChangeEventHandler(StateChange);
            //     connection.UserCertificateValidation += new RemoteCertificateValidationCallback(connection_UserCertificateValidation);
            //     connection.Open();

            //     using (var createDatabase = new PgCommand($"CREATE DATABASE {ConfigurationManager.AppSettings["Initial Catalog"]} WITH ENCODING='UTF8'", connection))
            //     {
            //         createDatabase.ExecuteNonQuery();
            //     }
            // }
        }

        private void DropDatabase()
        {
            // var csb = new PgConnectionStringBuilder();

            // csb.DataSource     = ConfigurationManager.AppSettings["Data Source"];
            // csb.UserID         = ConfigurationManager.AppSettings["User ID"];
            // csb.Password       = ConfigurationManager.AppSettings["Password"];
            // csb.PortNumber     = Convert.ToInt32(ConfigurationManager.AppSettings["Port Number"]);
            // csb.Ssl            = Convert.ToBoolean(ConfigurationManager.AppSettings["SSL"]);
            // csb.InitialCatalog = String.Empty;
            // csb.Pooling        = false;

            // try
            // {
            //     using (var connection = new PgConnection(csb.ToString()))
            //     {
            //         connection.StateChange += new StateChangeEventHandler(StateChange);
            //         connection.UserCertificateValidation += new RemoteCertificateValidationCallback(connection_UserCertificateValidation);
            //         connection.Open();

            //         using (var dropDatabase = new PgCommand($"drop database {ConfigurationManager.AppSettings["Initial Catalog"]}", connection))
            //         {
            //             dropDatabase.ExecuteNonQuery();
            //         }
            //     }
            // }
            // catch
            // {
            // }
        }

        private void CreateTables()
        {
            StringBuilder commandText = new StringBuilder();

            // Table for general purpouse tests
            commandText.Append("CREATE TABLE public.test_table(");
            commandText.Append("int4_field int4 NOT NULL,");
            commandText.Append("char_field char(10),");
            commandText.Append("varchar_field varchar(30),");
            commandText.Append("single_field float4,");
            commandText.Append("double_field float8,");
            commandText.Append("date_field date,");
            commandText.Append("time_field time,");
            commandText.Append("timestamp_field timestamp,");
            commandText.Append("blob_field bytea,");
            commandText.Append("bool_field bool,");
            commandText.Append("int2_field int2,");
            commandText.Append("int8_field int8,");
            commandText.Append("money_field money,");
            commandText.Append("numeric_field numeric(8,2),");
            commandText.Append("bool_array bool[],");
            commandText.Append("int2_array int2[],");
            commandText.Append("int4_array int4[],");
            commandText.Append("int8_array int8[],");
            commandText.Append("mint2_array int2[][],");
            commandText.Append("serial_field serial NOT NULL,");
            commandText.Append("macaddr_field macaddr,");
            commandText.Append("inet_field inet,");
            commandText.Append("name_field name,");
            commandText.Append("CONSTRAINT test_table_pkey PRIMARY KEY (int4_field)");
            commandText.Append(") WITH OIDS;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            commandText = new StringBuilder();

            // Table for Geometric types tests
            commandText.Append("CREATE TABLE public.geometric_table(");
            commandText.Append("pk int4 NOT NULL,");
            commandText.Append("point_field point,");
            commandText.Append("box_field box,");
            commandText.Append("circle_field circle,");
            commandText.Append("lseg_field lseg,");
            commandText.Append("path_field path,");
            commandText.Append("polygon_field polygon,");
            commandText.Append("point_array point[],");
            commandText.Append("box_array box[],");
            commandText.Append("circle_array circle[],");
            commandText.Append("lseg_array lseg[],");
            commandText.Append("path_array path[],");
            commandText.Append("polygon_array polygon[],");
            commandText.Append("line_field line,");
            commandText.Append("line_array line[],");
            commandText.Append("CONSTRAINT geometric_test_pkey PRIMARY KEY (pk)");
            commandText.Append(") WITH OIDS;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            InsertTestData();
            InsertGeometricTestData();
        }

        private void CreateFunctions()
        {
            // Create language functions
            StringBuilder commandText = new StringBuilder();

            commandText.Append("CREATE OR REPLACE FUNCTION public.plpgsql_call_handler()");
            commandText.Append("RETURNS language_handler AS");
            commandText.Append("'$libdir/plpgsql', 'plpgsql_call_handler'");
            commandText.Append("LANGUAGE 'c' VOLATILE;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            // Create languages
            commandText = new StringBuilder();

            //try
            //{
            //    commandText.Append("CREATE TRUSTED PROCEDURAL LANGUAGE 'plpgsql' HANDLER plpgsql_call_handler;");

            //    command = new PgCommand(commandText.ToString(), _connection);
            //    command.ExecuteNonQuery();
            //}
            //catch
            //{
            //}

            // Create test function public.TestCount()
            commandText = new StringBuilder();

            commandText.Append("CREATE OR REPLACE FUNCTION public.TestCount()");
            commandText.Append("RETURNS int8 AS");
            commandText.Append("'");
            commandText.Append("select count(*) from test_table;");
            commandText.Append("'");
            commandText.Append("LANGUAGE 'sql' VOLATILE;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            // Create test function public.DeriveCount()
            commandText = new StringBuilder();

            commandText.Append("CREATE OR REPLACE FUNCTION public.DeriveCount(int4)");
            commandText.Append("RETURNS int8 AS");
            commandText.Append("'");
            commandText.Append("select count(*) from test_table where int4_field < $1;");
            commandText.Append("'");
            commandText.Append("LANGUAGE 'sql' VOLATILE;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }

            // Create test function public.DeleteRows()
            commandText = new StringBuilder();

            commandText.Append("CREATE OR REPLACE FUNCTION public.DeleteRows(int4)\r\n");
            commandText.Append("RETURNS BOOLEAN AS '\r\n");
            commandText.Append("DECLARE\r\n");
            commandText.Append("\t\trows INTEGER;\r\n");
            commandText.Append("BEGIN\r\n");
            commandText.Append("DELETE FROM public.test_table WHERE int4_field > $1;\r\n");
            commandText.Append("GET DIAGNOSTICS rows = ROW_COUNT;\r\n");
            commandText.Append("IF rows > 0 THEN\r\n");
            commandText.Append("\t\tRETURN TRUE;\r\n");
            commandText.Append("ELSE\r\n");
            commandText.Append("\t\tRETURN FALSE;\r\n");
            commandText.Append("END IF;\r\n");
            commandText.Append("END;\r\n");
            commandText.Append("'\r\n");
            commandText.Append("LANGUAGE 'plpgsql' VOLATILE;");

            using (var command = new PgCommand(commandText.ToString(), _connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private void InsertTestData()
        {
            string commandText = "insert into public.test_table values(@int4_field, @char_field, @varchar_field, @single_field, @double_field, @date_field, @time_field, @timestamp_field, @blob_field, @bool_field)";

            using (var transaction = _connection.BeginTransaction())
            {
                using (var command = new PgCommand(commandText, _connection, transaction))
                {
                    command.Parameters.Add("@int4_field", PgDbType.Integer);
                    command.Parameters.Add("@char_field", PgDbType.Char);
                    command.Parameters.Add("@varchar_field", PgDbType.VarChar);
                    command.Parameters.Add("@single_field", PgDbType.Real);
                    command.Parameters.Add("@double_field", PgDbType.Double);
                    command.Parameters.Add("@date_field", PgDbType.Date);
                    command.Parameters.Add("@time_field", PgDbType.Time);
                    command.Parameters.Add("@timestamp_field", PgDbType.Timestamp);
                    command.Parameters.Add("@blob_field", PgDbType.Bytea);
                    command.Parameters.Add("@bool_field", PgDbType.Bool);

                    for (int i = 0; i < 100; i++)
                    {
                        command.Parameters["@int4_field"].Value = i;
                        command.Parameters["@char_field"].Value = "IRow " + i.ToString();
                        command.Parameters["@varchar_field"].Value = "IRow Number" + i.ToString();
                        command.Parameters["@single_field"].Value = (float)(i + 10) / 5;
                        command.Parameters["@double_field"].Value = Math.Log(i, 10);
                        command.Parameters["@date_field"].Value = DateTime.Now;
                        command.Parameters["@time_field"].Value = DateTime.Now;
                        command.Parameters["@timestamp_field"].Value = DateTime.Now;
                        command.Parameters["@blob_field"].Value = Encoding.UTF8.GetBytes("IRow " + i.ToString());
                        command.Parameters["@bool_field"].Value = true;

                        command.ExecuteNonQuery();
                    }
                }

                // Commit transaction
                transaction.Commit();
            }
        }

        private void InsertGeometricTestData()
        {
            string commandText = "insert into public.geometric_table values(@pk, @point, @box, @circle, @lseg, @path, @polygon)";

            using (var transaction = _connection.BeginTransaction())
            {
                using (var command = new PgCommand(commandText, _connection, transaction))
                {
                    // Add command parameters
                    command.Parameters.Add("@pk", PgDbType.Integer);
                    command.Parameters.Add("@point", PgDbType.Point);
                    command.Parameters.Add("@box", PgDbType.Box);
                    command.Parameters.Add("@circle", PgDbType.Circle);
                    command.Parameters.Add("@lseg", PgDbType.LSeg);
                    command.Parameters.Add("@path", PgDbType.Path);
                    command.Parameters.Add("@polygon", PgDbType.Polygon);

                    for (int i = 0; i < 100; i++)
                    {
                        command.Parameters["@pk"].Value = i;
                        command.Parameters["@point"].Value = new PgPoint(i, i + 10);
                        command.Parameters["@box"].Value = new PgBox(new PgPoint(0, i), new PgPoint(i, i));
                        command.Parameters["@circle"].Value = new PgCircle(new PgPoint(i, 0), i);
                        command.Parameters["@lseg"].Value = new PgLSeg(new PgPoint(-1, 0), new PgPoint(1, 0));
                        command.Parameters["@path"].Value = new PgPath(new PgPoint[] { new PgPoint(0, 0), new PgPoint(1, 0) });
                        command.Parameters["@polygon"].Value = new PgPolygon(new PgPoint[] { new PgPoint(1, 1), new PgPoint(0, 0) });

                        command.ExecuteNonQuery();
                    }
                }

                // Commit transaction
                transaction.Commit();
            }
        }

        private void StateChange(object sender, StateChangeEventArgs e)
        {
            Debug.WriteLine($"Connection state changed from {e.OriginalState} to {e.CurrentState}");
        }

        private bool connection_UserCertificateValidation(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
