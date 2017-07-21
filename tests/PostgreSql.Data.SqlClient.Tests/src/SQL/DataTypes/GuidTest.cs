// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;
using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class GuidTest
    {      
        [Fact]
        public void ReadWriteLocalGeneratedGuidTest()
        {
            var tableName      = DataTestClass.GetUniqueNameForPostgreSql("UUId_");
            var createTableSql = $"CREATE TABLE {tableName} (Id SERIAL, Guid uuid)";
            var connStr        = DataTestClass.PostgreSql_Northwind;
            var failed         = false;
            var uuid           = Guid.NewGuid();
            var uuidValue      = Guid.Empty;
            var errorMsg       = string.Empty;        

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (var insertCommand = new PgCommand($"INSERT INTO {tableName} (Guid) VALUES (@Guid) RETURNING Guid", connection))
                    {
                        insertCommand.Parameters.Add("@Guid", PgDbType.Uuid).Value = uuid;
                        uuidValue = (Guid)insertCommand.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                failed   = true;
            }
            finally
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($"DROP TABLE {tableName}", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            Assert.False(failed, $"Writing of UUID values has failed. {errorMsg}");
            Assert.Equal(uuid, uuidValue);
        }

        [Fact]
        public void ReadWriteServerGeneratedGuidTest()
        {
            var tableName        = DataTestClass.GetUniqueNameForPostgreSql("UUId_");
            var createSchemaSql  = $@"CREATE EXTENSION IF NOT EXISTS ""uuid-ossp""; CREATE TABLE {tableName} (Id SERIAL, local_uuid uuid, server_uuid uuid DEFAULT uuid_generate_v4());";
            var connStr          = (new PgConnectionStringBuilder(DataTestClass.PostgreSql_Northwind) { MultipleActiveResultSets = true }).ConnectionString;
            var failed           = false;
            var uuid             = Guid.NewGuid();
            var localUuidValue   = Guid.Empty;
            var serverUuidValue  = Guid.Empty;
            var serverUuidString = string.Empty;
            var count            = 0L;            
            var errorMsg         = string.Empty;        

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSchemaSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    using (var insertCommand = new PgCommand($"INSERT INTO {tableName} (local_uuid) VALUES (@local_uuid) RETURNING local_uuid, server_uuid, server_uuid::text", connection))
                    {
                        insertCommand.Parameters.AddWithValue("@local_uuid", uuid);

                        using (var reader = insertCommand.ExecuteReader())
                        {
                            Assert.True(reader.HasRows);
                            Assert.True(reader.Read());

                            localUuidValue   = reader.GetGuid(0);
                            serverUuidValue  = reader.GetGuid(1);
                            serverUuidString = reader.GetString(2);
                        }
                    }
                    using (var selectCommand = new PgCommand($"SELECT COUNT(*) FROM {tableName} WHERE server_uuid = @server_uuid", connection))
                    {
                        selectCommand.Parameters.AddWithValue("@server_uuid", serverUuidValue);

                        count = (long)selectCommand.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                failed   = true;
            }
            finally
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($@"DROP TABLE IF EXISTS {tableName}; DROP EXTENSION IF EXISTS ""uuid-ossp""", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }

            Assert.False(failed, $"Writing of UUID values has failed. {errorMsg}");
            Assert.Equal(1L, count);
            Assert.Equal(uuid, localUuidValue);
            Assert.Equal(serverUuidValue.ToString(), serverUuidString);
            Assert.NotEqual(localUuidValue, serverUuidValue);
            Assert.Equal(uuid, localUuidValue);
        }
    }
}
