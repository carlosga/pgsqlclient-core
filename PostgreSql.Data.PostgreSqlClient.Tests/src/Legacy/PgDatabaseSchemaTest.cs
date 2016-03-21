// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    // public class PgDatabaseSchemaTest
    //     : PgBaseTest
    // {
    //     [TestMethod]
    //     public void Aggregates()
    //     {
    //         DataTable aggregates = Connection.GetSchema("Aggregates", null);
    //     }

    //     [TestMethod]
    //     public void Casts()
    //     {
    //         DataTable casts = Connection.GetSchema("Casts", null);
    //     }

    //     [TestMethod]
    //     public void CheckConstraints()
    //     {
    //         DataTable checkConstraints = Connection.GetSchema("CheckConstraints", null);
    //     }

    //     [TestMethod]
    //     public void Columns()
    //     {
    //         DataTable columns = Connection.GetSchema("Columns", null);
    //     }

    //     [TestMethod]
    //     public void Databases()
    //     {
    //         DataTable databases = Connection.GetSchema("Databases", null);
    //     }

    //     [TestMethod]
    //     public void DataSourceInformation()
    //     {
    //         DataTable dataSourceInformation = Connection.GetSchema("DataSourceInformation", null);
    //     }

    //     [TestMethod]
    //     public void DataTypes()
    //     {
    //         DataTable providerTypes = Connection.GetSchema("DataTypes", null);
    //     }

    //     [TestMethod]
    //     public void ForeignKeys()
    //     {
    //         DataTable foreignKeys = Connection.GetSchema("ForeignKeys", null);
    //     }

    //     [TestMethod]
    //     public void ForeignKeyColumns()
    //     {
    //         DataTable foreignKeys = Connection.GetSchema("ForeignKeyColumns", null);
    //     }

    //     [TestMethod]
    //     public void Functions()
    //     {
    //         DataTable functions = Connection.GetSchema("Functions", null);
    //     }

    //     [TestMethod]
    //     public void Groups()
    //     {
    //         DataTable groups = Connection.GetSchema("Groups", null);
    //     }

    //     [TestMethod]
    //     public void Indexes()
    //     {
    //         DataTable indexes = Connection.GetSchema("Indexes", null);
    //     }

    //     [TestMethod]
    //     public void IndexColumns()
    //     {
    //         DataTable indexes = Connection.GetSchema("Indexes", null);

    //         foreach (DataRow index in indexes.Rows)
    //         {
    //             string catalog = !index.IsNull("TABLE_CATALOG") ? (string)index["TABLE_CATALOG"] : null;
    //             string schema = !index.IsNull("TABLE_SCHEMA") ? (string)index["TABLE_SCHEMA"] : null;
    //             string tableName = !index.IsNull("TABLE_NAME") ? (string)index["TABLE_NAME"] : null;
    //             string indexName = !index.IsNull("INDEX_NAME") ? (string)index["INDEX_NAME"] : null;

    //             DataTable indexColumns = Connection.GetSchema("IndexColumns", new string[] { catalog, schema, tableName, indexName });
    //         }
    //     }

    //     [TestMethod]
    //     public void PrimaryKeys()
    //     {
    //         DataTable primaryKeys = Connection.GetSchema("PrimaryKeys", null);
    //     }

    //     [TestMethod]
    //     public void ReservedWords()
    //     {
    //         DataTable reservedWords = Connection.GetSchema("ReservedWords", null);
    //     }

    //     [TestMethod]
    //     public void Restrictions()
    //     {
    //         DataTable restrictions = Connection.GetSchema("Restrictions", null);
    //     }

    //     [TestMethod]
    //     public void Schemas()
    //     {
    //         DataTable schemas = Connection.GetSchema("Schemas");
    //     }

    //     [TestMethod]
    //     public void Sequences()
    //     {
    //         DataTable sequences = Connection.GetSchema("Sequences");
    //     }

    //     [TestMethod]
    //     public void SqlLanguages()
    //     {
    //         DataTable sqlLanguages = Connection.GetSchema("SqlLanguages");
    //     }

    //     [TestMethod]
    //     public void Tables()
    //     {
    //         DataTable tables = Connection.GetSchema("Tables", null);
    //     }

    //     [TestMethod]
    //     public void Triggers()
    //     {
    //         DataTable triggers = Connection.GetSchema("Triggers", null);
    //     }

    //     [TestMethod]
    //     public void ViewColumns()
    //     {
    //         DataTable viewColumns = Connection.GetSchema("ViewColumns", null);
    //     }

    //     [TestMethod]
    //     public void Views()
    //     {
    //         DataTable views = Connection.GetSchema("Views", null);
    //     }
    // }
}