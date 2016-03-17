// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Reflection;

namespace PostgreSql.Data.Schema
{
    internal static class PgSchemaFactory
    {
        private static readonly string s_resName = "PostgreSql.Data.Schema.MetaData.xml";

        internal static DataTable GetSchema(PgConnection connection, string collectionName, string[] restrictions)
        {
            string filter = $"CollectionName = '{collectionName}'";
            Stream xmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_resName);
            DataSet ds = new DataSet();

            ds.ReadXml(xmlStream);

            DataRow[] collection = ds.Tables[DbMetaDataCollectionNames.MetaDataCollections].Select(filter);

            if (collection.Length != 1)
            {
                throw new NotSupportedException("Unsupported collection name.");
            }

            if (restrictions != null && restrictions.Length > (int)collection[0]["NumberOfRestrictions"])
            {
                throw new InvalidOperationException("The number of specified restrictions is not valid.");
            }

            if (ds.Tables[DbMetaDataCollectionNames.Restrictions].Select(filter).Length != (int)collection[0]["NumberOfRestrictions"])
            {
                throw new InvalidOperationException("Incorrect restriction definition.");
            }

            switch (collection[0]["PopulationMechanism"].ToString())
            {
                case "PrepareCollection":
                    return PrepareCollection(connection, collectionName, restrictions);

                case "DataTable":
                    return ds.Tables[collection[0]["PopulationString"].ToString()].Copy();

                case "SQLCommand":
                    return SqlCommandCollection(connection, collectionName, (string)collection[0]["PopulationString"], restrictions);

                default:
                    throw new NotSupportedException("Unsupported population mechanism");
            }
        }

        private static DataTable PrepareCollection(PgConnection connection, string collectionName, string[] restrictions)
        {
            PgSchema schema = null;

            switch (collectionName.Trim().ToLower())
            {
                case "checkconstraints":
                    schema = new PgCheckConstraints(connection);
                    break;

                case "columns":
                    schema = new PgColumns(connection);
                    break;

                case "indexes":
                    schema = new PgIndexes(connection);
                    break;

                case "indexcolumns":
                    schema = new PgIndexColumns(connection);
                    break;

                case "functions":
                    schema = new PgFunctions(connection);
                    break;

                case "functionparameters":
                    schema = new PgFunctionParameters(connection);
                    break;

                case "foreignkeys":
                    schema = new PgForeignKeys(connection);
                    break;

                case "foreignkeycolumns":
                    schema = new PgForeignKeyColumns(connection);
                    break;

                case "primarykeys":
                    schema = new PgPrimaryKeys(connection);
                    break;

                case "sequences":
                    schema = new PgSequences(connection);
                    break;

                case "tables":
                    schema = new PgTables(connection);
                    break;

                case "triggers":
                    schema = new PgTriggers(connection);
                    break;

                case "uniquekeys":
                    schema = new PgUniqueKeys(connection);
                    break;

                case "views":
                    schema = new PgViews(connection);
                    break;

                case "viewcolumns":
                    schema = new PgViewColumns(connection);
                    break;
            }

            return schema.GetSchema(collectionName, restrictions);
        }

        private static DataTable SqlCommandCollection(PgConnection connection, string collectionName, string sql, string[] restrictions)
        {
            if (restrictions == null)
            {
                restrictions = new string[0];
            }

            DataTable dataTable = null;
            PgDataAdapter adapter = null;
            PgCommand command = new PgCommand(String.Format(sql, restrictions), connection);

            try
            {
                adapter = new PgDataAdapter(command);
                dataTable = new DataTable(collectionName);

                adapter.Fill(dataTable);
            }
            catch (PgException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PgException(ex.Message);
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
            }

            return dataTable;
        }
    }
}