// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;
using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class CharacterTest
    {      
        [Fact]
        public void ReadWriteCharacterTest()
        {
            var tableName      = DataTestClass.GetUniqueNameForPostgreSql("UUId_");
            var createTableSql = $"CREATE TABLE {tableName} (Id SERIAL, CharField character(3))";
            var connStr        = DataTestClass.PostgreSql_Northwind;
            var failed         = false;
            var charInput      = "Lon";
            var charValue      = String.Empty;;
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
                    using (var insertCommand = new PgCommand($"INSERT INTO {tableName} (CharField) VALUES (@Char) RETURNING CharField", connection))
                    {
                        insertCommand.Parameters.Add("@Char", PgDbType.Char).Value = charInput;
                        charValue = (string)insertCommand.ExecuteScalar();
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

            Assert.False(failed, $"Writing of CHARACTER values has failed. {errorMsg}");
            Assert.Equal(charInput, charValue);
        }
    }
}
