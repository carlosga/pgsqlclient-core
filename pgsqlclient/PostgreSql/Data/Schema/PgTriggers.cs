// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgTriggers
        : PgSchema
    {
        public PgTriggers(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_class.relnamespace AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "current_database() AS TRIGGER_CATALOG, " +
                    "pg_namespace.nspname AS TRIGGER_SCHEMA, " +
                    "pg_proc.proname AS TRIGGER_NAME, " +
                    "pg_language.lanname AS PROCEDURE_LANGUAGE, " +
                    "pg_proc.proisagg AS IS_AGGREGATE, " +
                    "pg_proc.prosecdef AS IS_SECURITY_DEFINER, " +
                    "pg_proc.proisstrict AS IS_STRICT, " +
                    "pg_proc.proretset AS RETURNS_SET " +
                "FROM " +
                    "pg_trigger " +
                    "left join pg_class ON pg_trigger.tgconstrrelid = pg_class.oid " +
                    "left join pg_proc ON pg_trigger.tgfoid = pg_proc.oid " +
                    "left join pg_namespace ON pg_proc.pronamespace = pg_namespace.oid " +
                    "left join pg_language ON pg_proc.prolang = pg_language.oid ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // TABLE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // TABLE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    sql += String.Format(" and pg_class.relnamespace = '{0}'", restrictions[1]);
                }

                // TABLE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    sql += String.Format(" and pg_class.relname = '{0}'", restrictions[2]);
                }

                // TRIGGER_NAME
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_proc.proname = '{0}'", restrictions[3]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_proc.proname";
        }
    }
}