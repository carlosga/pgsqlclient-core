// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using System.Xml;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class XmlTest
    {      
        [Fact]
        public void ReadXmlSimple()
        {
            var query       = @"SELECT '<IndividualSurvey xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey""><TotalPurchaseYTD>0</TotalPurchaseYTD></IndividualSurvey>'::xml";
            var expectedXml = @"<IndividualSurvey xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey""><TotalPurchaseYTD>0</TotalPurchaseYTD></IndividualSurvey>";
            var connStr     = DataTestClass.PostgreSql_Northwind;
         
            using (var connection = new PgConnection(connStr)) 
            {
                connection.Open();
                using (var command = new PgCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var xmlString = reader.GetFieldValue<string>(0);

                            Assert.Equal(expectedXml, xmlString);
                        }
                    }
                }
            }
        }

        [Fact]
        public void InsertXmlFromString()
        {
            var expectedXml = @"<IndividualSurvey xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey""><TotalPurchaseYTD>0</TotalPurchaseYTD></IndividualSurvey>";
            var connStr     = DataTestClass.PostgreSql_Northwind;
            var tableName   = string.Empty;
            var xmlString   = string.Empty;
            var id          = 0;
         
            try
            {
                tableName = CreateTable();

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($"INSERT INTO {tableName} (xmlfield) VALUES (@XmlString) RETURNING id, xmlfield", connection))
                    {
                        command.Parameters.AddWithValue("@XmlString", expectedXml).PgDbType = PgDbType.Xml;

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Assert.False(reader.IsDBNull(0));
                                Assert.False(reader.IsDBNull(1));

                                id        = reader.GetFieldValue<int>(0);
                                xmlString = reader.GetFieldValue<string>(1);

                                Assert.Equal(expectedXml, xmlString);
                            }
                        }
                    }

                    using (var command = new PgCommand($"SELECT xmlfield FROM {tableName} WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        xmlString = (string)command.ExecuteScalar();

                        Assert.Equal(expectedXml, xmlString);                        
                    }                    
                }
            }
            finally
            {
                DropTable(tableName);
            }
        }

        [Fact]
        public void InsertXmlFromXmlDocument()
        {
            var expectedXml = @"<IndividualSurvey xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey""><TotalPurchaseYTD>0</TotalPurchaseYTD></IndividualSurvey>";
            var connStr     = DataTestClass.PostgreSql_Northwind;
            var tableName   = string.Empty;
            var xmlString   = string.Empty;
            var id          = 0;
         
            try
            {
                tableName = CreateTable();

                var document = new XmlDocument();

                document.LoadXml(expectedXml);

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($"INSERT INTO {tableName} (xmlfield) VALUES (@XmlDocument) RETURNING id, xmlfield", connection))
                    {
                        command.Parameters.AddWithValue("@XmlDocument", document);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Assert.False(reader.IsDBNull(0));
                                Assert.False(reader.IsDBNull(1));

                                id        = reader.GetFieldValue<int>(0);
                                xmlString = reader.GetFieldValue<string>(1);

                                Assert.Equal(expectedXml, xmlString);
                            }
                        }                        
                    }

                    using (var command = new PgCommand($"SELECT xmlfield FROM {tableName} WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        xmlString = (string)command.ExecuteScalar();

                        Assert.Equal(expectedXml, xmlString);                        
                    }
                }
            }
            finally
            {
                DropTable(tableName);
            }
        }

        [Fact]
        public void InsertXmlFromXmlElement()
        {
            var expectedXml = @"<IndividualSurvey xmlns=""http://schemas.microsoft.com/sqlserver/2004/07/adventure-works/IndividualSurvey""><TotalPurchaseYTD>0</TotalPurchaseYTD></IndividualSurvey>";
            var connStr     = DataTestClass.PostgreSql_Northwind;
            var tableName   = string.Empty;
            var xmlString   = string.Empty;
            var id          = 0;
         
            try
            {
                tableName = CreateTable();

                var document = new XmlDocument();

                document.LoadXml(expectedXml);

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($"INSERT INTO {tableName} (xmlfield) VALUES (@XmlElement) RETURNING id, xmlfield", connection))
                    {
                        command.Parameters.AddWithValue("@XmlElement", document.DocumentElement);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Assert.False(reader.IsDBNull(0));
                                Assert.False(reader.IsDBNull(1));

                                id        = reader.GetFieldValue<int>(0);
                                xmlString = reader.GetFieldValue<string>(1);

                                Assert.Equal(expectedXml, xmlString);
                            }
                        }                        
                    }

                    using (var command = new PgCommand($"SELECT xmlfield FROM {tableName} WHERE id = @Id", connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        xmlString = (string)command.ExecuteScalar();

                        Assert.Equal(expectedXml, xmlString);                        
                    }
                }
            }
            finally
            {
                DropTable(tableName);
            }
        }

        [Fact]
        public void InsertXmlFromInvalidValue()
        {
            var connStr         = DataTestClass.PostgreSql_Northwind;
            var tableName       = string.Empty;
            var expectedMessage = "The parameter data type of Object is invalid.";
         
            try
            {
                tableName = CreateTable();

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand($"INSERT INTO {tableName} (xmlfield) VALUES (@XmlElement)", connection))
                    {
                        command.Parameters.AddWithValue("@XmlElement", new object()).PgDbType = PgDbType.Xml;

                        var exception = Assert.Throws<ArgumentException>(() => command.ExecuteNonQuery());

                        Assert.Equal(expectedMessage, exception.Message);
                    }
                }
            }
            finally
            {
                DropTable(tableName);
            }
        }

        private static string CreateTable()
        {
            var tableName = DataTestClass.GetUniqueNameForPostgreSql("XML_");
            var sql       = $"CREATE TABLE {tableName} (id SERIAL, xmlfield XML);";
            var connStr   = DataTestClass.PostgreSql_Northwind;
         
            using (var connection = new PgConnection(connStr)) 
            {
                connection.Open();
                using (var command = new PgCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }

            return tableName;
        }

        private static void DropTable(string tableName)
        {
            var sql       = $"DROP TABLE IF EXISTS {tableName};";
            var connStr   = DataTestClass.PostgreSql_Northwind;
         
            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(sql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch
            {                
            }
        }
    }
}
