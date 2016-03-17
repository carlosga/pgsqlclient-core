// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgIndexColumns
        : PgSchema
    {
        public PgIndexColumns(PgConnection connection)
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
                    "pg_attribute.attname AS COLUMN_NAME, " +
                    "pg_attribute.attnum AS ORDINAL_POSITION " +
                "FROM pg_index " +
                    "left join pg_class ON pg_index.indrelid = pg_class.oid " +
                    "left join pg_class as pg_classidx ON pg_index.indexrelid = pg_classidx.oid " +
                    "left join pg_namespace ON pg_classidx.relnamespace = pg_namespace.oid " +
                    "left join pg_namespace as pg_namespidx ON pg_classidx.relnamespace = pg_namespidx.oid " +
                    "left join pg_attribute ON pg_index.indexrelid = pg_attribute.attrelid ";

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

                // COLUMN_NAME
                if (restrictions.Length > 4 && restrictions[4] != null)
                {
                    sql += String.Format(" and pg_attribute.attname = '{0}'", restrictions[4]);
                }
            }

            return sql + "ORDER BY pg_namespace.nspname, pg_class.relname, pg_classidx.relname, pg_attribute.attnum";
        }
    }
}