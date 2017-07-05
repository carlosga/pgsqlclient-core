// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;
using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class DecimalTest
    {
        public static IEnumerable<object[]> InsertDecimalValues_TestData()
        {
            yield return new object[] { Decimal.MinusOne };
            yield return new object[] { Decimal.MinValue };
            yield return new object[] { Decimal.Zero     };
            yield return new object[] { Decimal.MaxValue };
            yield return new object[] { 1500000M };
            yield return new object[] { -1500000M };
            yield return new object[] { -0.5M };
            yield return new object[] { 0.5M };
            yield return new object[] { 0.001M };
            yield return new object[] { -0.001M };
        }
        
        [Theory]
        [MemberData(nameof(InsertDecimalValues_TestData))]
        public void InsertTest(decimal price)
        {
            var tableName      = DataTestClass.GetUniqueNameForPostgreSql("DecimalValues_");
            var createTableSql = $"CREATE TABLE {tableName} (Id SERIAL, Price numeric)";
            var connStr        = DataTestClass.PostgreSql_Northwind;
            var failed         = false;
            var decValue       = 0.0M;

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createTableSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var insertCommand = new PgCommand($"INSERT INTO {tableName} (Price) VALUES (@Price)", connection))
                    {
                        insertCommand.Parameters.Add("@Price", PgDbType.Numeric).Value = price;
                        insertCommand.ExecuteNonQuery();
                    }

                    using (var selectCommand = new PgCommand($"SELECT Price FROM {tableName} WHERE Id = 1", connection))
                    {
                        decValue = (decimal)selectCommand.ExecuteScalar();
                    }
                }
            }
            catch
            {
                failed = true;
            }

            Assert.Equal(price, decValue);
            Assert.False(failed, "Writing of decimal values has failed");
        }
    }
}
