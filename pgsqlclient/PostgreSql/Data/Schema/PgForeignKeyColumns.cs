// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgForeignKeyColumns
        : PgSchema
    {
        public PgForeignKeyColumns(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS CONSTRAINT_CATALOG, " +
                    "pg_namespace.nspname AS CONSTRAINT_SCHEMA, " +
                    "pg_constraint.conname AS CONSTRAINT_NAME, " +
                    "current_database() AS TABLE_CATALOG, " +
                    "constraint_table_namespace.nspname AS TABLE_SCHEMA, " +
                    "constraint_table.relname AS TABLE_NAME, " +
                    "null AS COLUMN_NAME, " +
                    "current_database() AS REFERENCED_TABLE_CATALOG, " +
                    "referenced_table_namespace.nspname AS REFERENCED_TABLE_SCHEMA, " +
                    "referenced_table.relname AS REFERENCED_TABLE_NAME, " +
                    "null AS REFERENCED_COLUMN_NAME, " +
                    "pg_constraint.conkey as CONSTRAINT_TABLE_COLUMNS, " +
                    "pg_constraint.confkey as REFERENCED_TABLE_COLUMNS " +
                "FROM " +
                    "pg_constraint " +
                        "left join pg_namespace ON pg_constraint.connamespace = pg_namespace.oid " +
                        "left join pg_class as constraint_table ON pg_constraint.conrelid = constraint_table.oid " +
                        "left join pg_namespace as constraint_table_namespace ON constraint_table.relnamespace = constraint_table_namespace.oid " +
                        "right join pg_class as referenced_table ON pg_constraint.confrelid = referenced_table.oid " +
                        "left join pg_namespace as referenced_table_namespace ON referenced_table.relnamespace = referenced_table_namespace.oid " +
                        "left join pg_description ON pg_constraint.oid = pg_description.objoid " +
                "WHERE " +
                    "pg_constraint.contype = 'f' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                /* CONSTRAINT_CATALOG	*/
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                /* CONSTRAINT_SCHEMA */
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                /* TABLE_NAME */
                if (restrictions.Length >= 3 && restrictions[2] != null)
                {
                    sql += String.Format(" and constraint_table.relname = '{0}'", restrictions[2]);
                }

                /* CONSTRAINT_NAME */
                if (restrictions.Length >= 4 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_constraint.conname = '{0}'", restrictions[3]);
                }
            }

            return sql + "ORDER BY pg_namespace.nspname, constraint_table.relname, pg_constraint.conname";
        }

        protected override DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            DataTable foreignKeyColumns = schema.Clone();
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

            selectColumn.Prepare();

            foreignKeyColumns.BeginLoadData();

            foreach (DataRow row in schema.Rows)
            {
                Array tableColumns = (Array)row["CONSTRAINT_TABLE_COLUMNS"];
                Array referencedTableColumns = (Array)row["REFERENCED_TABLE_COLUMNS"];

                for (int i = 0; i < tableColumns.Length; i++)
                {
                    DataRow foreignKeyColumn = foreignKeyColumns.NewRow();

                    // Grab the table column name
                    selectColumn.Parameters["@tableSchema"].Value = row["TABLE_SCHEMA"];
                    selectColumn.Parameters["@tableName"].Value = row["TABLE_NAME"];
                    selectColumn.Parameters["@ordinalPosition"].Value = Convert.ToInt16(tableColumns.GetValue(i + 1));

                    string tableColumn = (string)selectColumn.ExecuteScalar();

                    // Grab the referenced table column name
                    selectColumn.Parameters["@tableSchema"].Value = row["REFERENCED_TABLE_SCHEMA"];
                    selectColumn.Parameters["@tableName"].Value = row["REFERENCED_TABLE_NAME"];
                    selectColumn.Parameters["@ordinalPosition"].Value = Convert.ToInt16(referencedTableColumns.GetValue(i + 1));

                    string referencedTableColumn = (string)selectColumn.ExecuteScalar();

                    // Create the new foreign key column info
                    foreignKeyColumn["CONSTRAINT_CATALOG"] = row["CONSTRAINT_CATALOG"];
                    foreignKeyColumn["CONSTRAINT_SCHEMA"] = row["CONSTRAINT_SCHEMA"];
                    foreignKeyColumn["CONSTRAINT_NAME"] = row["CONSTRAINT_NAME"];
                    foreignKeyColumn["TABLE_CATALOG"] = row["TABLE_CATALOG"];
                    foreignKeyColumn["TABLE_SCHEMA"] = row["TABLE_SCHEMA"];
                    foreignKeyColumn["TABLE_NAME"] = row["TABLE_NAME"];
                    foreignKeyColumn["COLUMN_NAME"] = tableColumn;
                    foreignKeyColumn["REFERENCED_TABLE_CATALOG"] = row["REFERENCED_TABLE_CATALOG"];
                    foreignKeyColumn["REFERENCED_TABLE_SCHEMA"] = row["REFERENCED_TABLE_SCHEMA"];
                    foreignKeyColumn["REFERENCED_TABLE_NAME"] = row["REFERENCED_TABLE_NAME"];
                    foreignKeyColumn["REFERENCED_COLUMN_NAME"] = referencedTableColumn;

                    foreignKeyColumns.Rows.Add(foreignKeyColumn);
                }
            }

            foreignKeyColumns.EndLoadData();
            foreignKeyColumns.AcceptChanges();

            foreignKeyColumns.Columns.Remove("CONSTRAINT_TABLE_COLUMNS");
            foreignKeyColumns.Columns.Remove("REFERENCED_TABLE_COLUMNS");

            // CleanUp
            selectColumn.Dispose();

            return foreignKeyColumns;
        }
    }
}