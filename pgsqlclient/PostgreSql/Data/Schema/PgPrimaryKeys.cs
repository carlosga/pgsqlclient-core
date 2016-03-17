// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgPrimaryKeys
        : PgSchema
    {
        public PgPrimaryKeys(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "null AS COLUMN_NAME, " +
                    "pg_constraint.conname AS PK_NAME, " +
                    "pg_constraint.conkey AS PK_COLUMNS, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_constraint " +
                    "left join pg_class ON pg_constraint.conrelid = pg_class.oid " +
                    "left join pg_namespace ON pg_constraint.connamespace = pg_namespace.oid " +
                    "left join pg_description ON pg_constraint.oid = pg_description.objoid " +
                "WHERE " +
                    "pg_constraint.contype = 'p' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // TABLE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // TABLE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // TABLE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname, pg_constraint.conname";
        }

        protected override System.Data.DataTable ProcessResult(PostgreSql.Data.PostgreSqlClient.PgConnection connection, System.Data.DataTable schema)
        {
            DataTable primaryKeyColumns = schema.Clone();
            string sql =
                "SELECT " +
                    "column_name " +
                "FROM information_schema.columns " +
                "WHERE " +
                    "table_catalog=current_database() AND " +
                    "table_schema=@tableSchema AND " +
                    "table_name=@tableName AND " +
                    "ordinal_position=@ordinalPosition";

            PgCommand selectColumn = new PgCommand(sql, connection);
            selectColumn.Parameters.Add("@tableSchema", PgDbType.Text);
            selectColumn.Parameters.Add("@tableName", PgDbType.Text);
            selectColumn.Parameters.Add("@ordinalPosition", PgDbType.Text);

            try
            {
                primaryKeyColumns.BeginLoadData();

                selectColumn.Prepare();

                foreach (DataRow row in schema.Rows)
                {
                    Array pkColumns = (Array)row["PK_COLUMNS"];

                    for (int i = 0; i < pkColumns.Length; i++)
                    {
                        DataRow primaryKeyColumn = primaryKeyColumns.NewRow();

                        // Grab the table column name
                        selectColumn.Parameters["@tableSchema"].Value = row["TABLE_SCHEMA"];
                        selectColumn.Parameters["@tableName"].Value = row["TABLE_NAME"];
                        selectColumn.Parameters["@ordinalPosition"].Value = Convert.ToInt16(pkColumns.GetValue(i + 1));

                        string pkColumnName = (string)selectColumn.ExecuteScalar();

                        // Create the new primary key column info
                        primaryKeyColumn["TABLE_CATALOG"] = row["TABLE_CATALOG"];
                        primaryKeyColumn["TABLE_SCHEMA"] = row["TABLE_SCHEMA"];
                        primaryKeyColumn["TABLE_NAME"] = row["TABLE_NAME"];
                        primaryKeyColumn["COLUMN_NAME"] = pkColumnName;
                        primaryKeyColumn["DESCRIPTION"] = row["DESCRIPTION"];

                        primaryKeyColumns.Rows.Add(primaryKeyColumn);
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                // CleanUp
                selectColumn.Dispose();

                primaryKeyColumns.EndLoadData();
                primaryKeyColumns.AcceptChanges();

                primaryKeyColumns.Columns.Remove("PK_COLUMNS");
            }

            return primaryKeyColumns;
        }
    }
}