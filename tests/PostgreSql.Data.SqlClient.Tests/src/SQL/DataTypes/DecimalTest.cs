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
        public static IEnumerable<object[]> ReadWriteDecimalValues_TestData()
        {
            yield return new object[] { Decimal.MinValue };
            yield return new object[] { Decimal.MaxValue };
            yield return new object[] { Decimal.Zero     };
            yield return new object[] { Decimal.MinusOne };
            yield return new object[] { 1500000M };
            yield return new object[] { -1500000M };
            yield return new object[] { -0.5M };
            yield return new object[] { 0.5M };
            yield return new object[] { 0.001M };
            yield return new object[] { -0.001M };
            yield return new object[] { 1E-28M };
            yield return new object[] { 1E-24M };
            yield return new object[] { 1E-20M };
            yield return new object[] { 1E-16M };
            yield return new object[] { 1E-12M };
            yield return new object[] { 1E-8M };
            yield return new object[] { 1E-4M };
            yield return new object[] { 1M };
            yield return new object[] { 1E+4M };
            yield return new object[] { 1E+8M };
            yield return new object[] { 1E+12M };
            yield return new object[] { 1E+16M };
            yield return new object[] { 1E+20M };
            yield return new object[] { 1E+24M };
            yield return new object[] { 1E+28M };   
        }
        
        [Theory]
        [MemberData(nameof(ReadWriteDecimalValues_TestData))]
        public void ReadWriteDecimalValuesTest(decimal price)
        {
            var tableName      = DataTestClass.GetUniqueNameForPostgreSql("DecimalValues_");
            var createTableSql = $"CREATE TABLE {tableName} (Id SERIAL, Price numeric)";
            var connStr        = DataTestClass.PostgreSql_Northwind;
            var failed         = false;
            var decValue       = 0.0M;
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
                    using (var insertCommand = new PgCommand($"INSERT INTO {tableName} (Price) VALUES (@Price) RETURNING Price", connection))
                    {
                        insertCommand.Parameters.Add("@Price", PgDbType.Numeric).Value = price;
                        decValue = (decimal)insertCommand.ExecuteScalar();
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

            Assert.False(failed, $"Writing of decimal values has failed. {errorMsg}");
            Assert.Equal(price, decValue);
        }
    }
}
