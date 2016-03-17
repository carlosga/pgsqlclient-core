// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgTables
        : PgSchema
    {
        public PgTables(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string where = "";
            string sql =
                "SELECT " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "case pg_class.relkind  " +
                        "when 'r' THEN 'TABLE' " +
                        "when 'v' THEN 'VIEW' " +
                    "END  AS TABLE_TYPE, " +
                    "pg_tablespace.spcname AS TABLESPACE, " +
                    "pg_class.relhasindex AS HAS_INDEXES, " +
                    "pg_class.relisshared AS IS_SHARED, " +
                    "pg_class.relchecks AS CONSTRAINT_COUNT, " +
                    "pg_class.relhastriggers AS HAS_TRIGGERS, " +
                    "pg_class.relhaspkey AS HAS_PRIMARY_KEY, " +
                    "pg_class.relhasrules AS HAS_RULES, " +
                    "pg_class.relhassubclass AS HAS_SUBCLASS, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM pg_class " +
                    "LEFT JOIN pg_namespace ON pg_class.relnamespace = pg_namespace.oid " +
                    "LEFT JOIN pg_tablespace ON pg_class.reltablespace = pg_tablespace.oid " +
                    "LEFT JOIN pg_description ON pg_class.oid = pg_description.objoid ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // TABLE_CATALOG
                if (restrictions.Length > 0 && restrictions[0] != null)
                {
                }

                // TABLE_SCHEMA
                if (restrictions.Length > 1 && restrictions[1] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and  ";
                    }
                    where += String.Format("pg_namespace.nspname = '{0}'", restrictions[1]);
                }

                // TABLE_NAME
                if (restrictions.Length > 2 && restrictions[2] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and  ";
                    }
                    where += String.Format("pg_class.relname = '{0}'", restrictions[2]);
                }

                // TABLE_TYPE
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and  ";
                    }
                    where += String.Format("pg_class.relkind = '{0}'", restrictions[3]);
                }
                else
                {
                    if (where.Length > 0)
                    {
                        where += " and  ";
                    }
                    where += "pg_class.relkind = 'r'";
                }

                // TABLESPACE
                if (restrictions.Length > 4 && restrictions[4] != null)
                {
                    if (where.Length > 0)
                    {
                        where += " and  ";
                    }
                    where += String.Format("pg_tablespace.spcname = '{0}'", restrictions[4]);
                }
            }
            else
            {
                where += " pg_class.relkind = 'r'";
            }

            if (where.Length > 0)
            {
                sql += " WHERE " + where;
            }

            return sql + " ORDER BY pg_class.relkind, pg_namespace.nspname, pg_class.relname";
        }

        protected override string[] ParseRestrictions(string[] restrictions)
        {
            string[] parsed = restrictions;

            if (parsed != null)
            {
                if (parsed.Length == 4 && parsed[3] != null)
                {
                    switch (parsed[3].ToString().ToUpper())
                    {
                        case "TABLE":
                            parsed[3] = "r";
                            break;

                        case "VIEW":
                            parsed[3] = "v";
                            break;
                    }
                }
            }

            return parsed;
        }
    }
}