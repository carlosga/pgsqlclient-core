// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgForeignKeys
        : PgSchema
    {
        public PgForeignKeys(PgConnection connection)
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
                    "current_database() AS REFERENCED_TABLE_CATALOG, " +
                    "referenced_table_namespace.nspname AS REFERENCED_TABLE_SCHEMA, " +
                    "referenced_table.relname AS REFERENCED_TABLE_NAME, " +
                    "case pg_constraint.confupdtype " +
                        "when 'a' then 'NO ACTION' " +
                        "when 'r' then 'RESTRICT' " +
                        "when 'c' then 'CASCADE' " +
                        "when 'd' then 'SET DEFAULT' " +
                        "when 'n' then 'SET NULL'  " +
                    "END AS UPDATE_RULE, " +
                    "case pg_constraint.confdeltype " +
                        "when 'a' then 'NO ACTION' " +
                        "when 'r' then 'RESTRICT' " +
                        "when 'c' then 'CASCADE' " +
                        "when 'd' then 'SET DEFAULT' " +
                        "when 'n' then 'SET NULL' " +
                    "end AS DELETE_RULE, " +
                    "pg_constraint.condeferrable AS DEFERRABILITY, " +
                    "pg_constraint.condeferred AS IS_DEFERRED, " +
                    "pg_description.description AS DESCRIPTION " +
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
    }
}