// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgViews
        : PgSchema
    {
        public PgViews(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS VIEW_CATALOG, " +
                    "pg_namespace.nspname AS VIEW_SCHEMA, " +
                    "pg_class.relname AS VIEW_NAME, " +
                    "pg_get_ruledef(pg_rewrite.oid) AS DEFINITION, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM " +
                    "pg_class " +
                        "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " +
                        "left join pg_rewrite ON pg_class.oid = pg_rewrite.ev_class " +
                        "left join pg_description ON pg_class.oid = pg_description.objoid " +
                "WHERE " +
                    "pg_class.relkind = 'v' ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // VIEW_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // VIEW_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // VIEW_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname";
        }
    }
}