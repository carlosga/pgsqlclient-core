// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class ConnectionTest
    {
        [Fact(Skip = "Needs PostgreSQL v10")]
        public static void SaslScramAuthenticationTest()
        {
            string connString = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { Pooling = false }).ConnectionString;

            try
            {
                using (PgConnection connection = new PgConnection(connString))
                {
                    connection.Open();

                    string createRole = "DROP ROLE IF EXISTS foorole;SET password_encryption = 'SCRAM-SHA-256';CREATE ROLE foorole WITH LOGIN PASSWORD 'foo';GRANT ALL ON SCHEMA public TO foorole";

                    using (var command = new PgCommand(createRole, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                var fooConnStr = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { Pooling = false, UserID = "foorole", Password="foo" }).ConnectionString;

                using (PgConnection connection = new PgConnection(fooConnStr))
                {
                    connection.Open();
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                using (PgConnection connection = new PgConnection(connString))
                {
                    connection.Open();
                    string createRole = "ALTER ROLE foorole WITH NOLOGIN;REVOKE ALL PRIVILEGES ON SCHEMA public FROM foorole; DROP ROLE IF EXISTS foorole;";

                    using (var command = new PgCommand(createRole, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
