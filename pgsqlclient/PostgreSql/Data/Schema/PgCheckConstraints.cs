// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.


using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgCheckConstraints
        : PgSchema
    {
        public PgCheckConstraints(PgConnection connection)
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
                    "tbn.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "pg_get_constraintdef(pg_constraint.oid) AS CHECK_CLAUSULE, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM " +
                    "pg_constraint " +
                    "left join pg_class ON pg_class.oid = pg_constraint.conrelid " +
                    "left join pg_namespace tbn ON pg_class.relnamespace = tbn.oid " +
                    "left join pg_namespace ON pg_constraint.connamespace = pg_namespace.oid " +
                    "left join pg_description ON pg_constraint.oid = pg_description.objoid " +
                "WHERE " +
                    "pg_constraint.contype = 'c' AND " +
                    "pg_class.relkind = 'r' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // CONSTRAINT_CATALOG
                if (restrictions.Length > 0)
                {
                }

                // CONSTRAINT_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // CONSTRAINT_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_constraint.conname = '{0}'", restrictions[2]);
                }
            }

            return sql + "ORDER BY pg_namespace.nspname, pg_class.relname, pg_constraint.conname";
        }
    }
}