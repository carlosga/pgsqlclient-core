// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgSequences
        : PgSchema
    {
        public PgSequences(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS SEQUENCE_CATALOG, " +
                    "pg_namespace.nspname AS SEQUENCE_SCHEMA, " +
                    "pg_class.relname AS SEQUENCE_NAME, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_class " +
                    "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " +
                    "left join pg_description ON pg_class.oid = pg_description.objoid " +
                "WHERE pg_class.relkind = 'S' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // SEQUENCE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // SEQUENCE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // SEQUENCE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname";
        }
    }
}