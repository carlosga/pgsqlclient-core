// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace PostgreSql.Data.SqlClient.Tests
{
    public enum Mood
    {
        Sad   = 1,
        Ok    = 2,
        Happy = 3
    }

    public class EnumsTest
    {
        [Fact]
        public static void InsertEnumTest()
        {
            var dropSql   = @"DROP TABLE IF EXISTS Person; DROP TYPE IF EXISTS Mood;";
            var createSql = $@"{dropSql} CREATE TYPE Mood AS ENUM ('Sad', 'Ok', 'Happy');
CREATE TABLE Person (
    name text,
    current_mood mood
);";

            try
            {
                using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
                {
                    connection.Open();

                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
                {
                    connection.Open();

                    using (var command = new PgCommand("INSERT INTO person VALUES (@name, @mood);", connection))
                    {
                        command.Parameters.AddWithValue("@name", "Moe");
                        command.Parameters.AddWithValue("@mood", Mood.Happy);
                        
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
                {
                    connection.Open();

                    using (var command = new PgCommand("SELECT current_mood FROM person LIMIT 1;", connection))
                    {                       
                        var enumValue = (Mood)command.ExecuteScalar();
                        Assert.Equal(Mood.Happy, enumValue);
                    }
                }

                using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
                {
                    connection.Open();

                    using (var command = new PgCommand("SELECT current_mood FROM person LIMIT 1;", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            Assert.True(reader.HasRows);
                            reader.Read();
                            var enumValue = (Mood)reader.GetValue(0);
                            Assert.Equal(Mood.Happy, enumValue);
                            enumValue = reader.GetFieldValue<Mood>(0);
                            Assert.Equal(Mood.Happy, enumValue);
                        }
                    }
                }
            }
            finally
            {
                using (var connection = new PgConnection(DataTestClass.PostgreSql_Northwind))
                {
                    connection.Open();

                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }            
        }
    }
}
