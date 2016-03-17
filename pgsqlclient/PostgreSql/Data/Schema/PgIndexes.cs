// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgIndexes
        : PgSchema
    {
        public PgIndexes(PgConnection connection)
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
                    "current_database() AS INDEX_CATALOG, " +
                    "pg_namespidx.nspname AS INDEX_SCHEMA, " +
                    "pg_classidx.relname AS INDEX_NAME, " +
                    "pg_am.amname AS TYPE, " +
                    "pg_index.indisclustered AS IS_CLUSTERED, " +
                    "pg_index.indisunique AS IS_UNIQUE, " +
                    "pg_index.indisprimary AS IS_PRIMARY, " +
                    "pg_am.amsearchnulls AS ALLOW_NULLS, " +
                    "pg_am.amcanmulticol AS IS_MULTICOLUMN, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_index " +
                    "LEFT JOIN pg_class ON pg_index.indrelid = pg_class.oid " +
                    "LEFT JOIN pg_class AS pg_classidx ON pg_index.indexrelid = pg_classidx.oid " +
                    "LEFT JOIN pg_namespace ON pg_classidx.relnamespace = pg_namespace.oid " +
                    "LEFT JOIN pg_namespace AS pg_namespidx ON pg_classidx.relnamespace = pg_namespidx.oid " +
                    "LEFT JOIN pg_am ON pg_classidx.relam = pg_am.oid " +
                    "LEFT JOIN pg_description ON pg_index.indexrelid = pg_description.objoid ";

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

                // INDEX_NAME
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_classidx.relname = '{0}'", restrictions[3]);
                }
            }

            return sql + "ORDER BY pg_namespace.nspname, pg_class.relname, pg_classidx.relname";
        }
    }
}