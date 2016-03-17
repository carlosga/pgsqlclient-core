// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgFunctionParameters
        : PgSchema
    {
        public PgFunctionParameters(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string where = String.Empty;
            string sql =
                "SELECT " +
                    "current_database()     AS FUNCTION_CATALOG, " +
                    "pg_namespace.nspname   AS FUNCTION_SCHEMA, " +
                    "pg_proc.proname        AS FUNCTION_NAME, " +
                    "null                   AS PARAMETER_NAME, " +
                    "null                   AS DATA_TYPE, " +
                    "0                      AS PARAMETER_DIRECTION, " +
                    "pg_proc.pronargs       AS ARGUMENT_NUMBER, " +
                    "pg_proc.proallargtypes AS ARGUMENT_TYPES, " +
                    "pg_proc.proargmodes    AS ARGUMENT_MODES, " +
                    "pg_proc.proargnames    AS ARGUMENT_NAMES " +
                "FROM " +
                    "pg_proc " +
                    "LEFT JOIN pg_namespace ON pg_proc.pronamespace = pg_namespace.oid ";

            if (restrictions != null && restrictions.Length > 0)
            {
                // FUNCTION_CATALOG
                if (restrictions.Length > 0 && !String.IsNullOrWhiteSpace(restrictions[0]))
                {
                }

                // FUNCTION_SCHEMA
                if (restrictions.Length > 1 && !String.IsNullOrWhiteSpace(restrictions[1]))
                {
                    if (where.Length > 0)
                    {
                        where += " AND ";
                    }
                    where += $"pg_namespace.nspname = '{restrictions[1]}'";
                }

                // FUNCTION_NAME
                if (restrictions.Length > 2 && !String.IsNullOrWhiteSpace(restrictions[2]))
                {
                    if (where.Length > 0)
                    {
                        where += " AND ";
                    }
                    where += $" pg_proc.proname = '{restrictions[2]}'";
                }
            }

            if (where.Length > 0)
            {
                sql += " WHERE " + where;
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_proc.proname";
        }

        protected override DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            DataTable functionParameters = schema.Clone();

            if (schema.Rows.Count > 0)
            {
                functionParameters.BeginLoadData();

                foreach (DataRow row in schema.Rows)
                {
                    int argNumber = Convert.ToInt32(row["ARGUMENT_NUMBER"]);
                    Array argTypes = (Array)row["ARGUMENT_TYPES"];
                    Array argNames = (Array)row["ARGUMENT_NAMES"];

                    if (!Convert.ToBoolean(row["RETURNS_SET"]))
                    {
                        DataRow functionParameter = functionParameters.NewRow();

                        // Create the new foreign key column info
                        functionParameter["FUNCTION_CATALOG"] = row["FUNCTION_CATALOG"];
                        functionParameter["FUNCTION_SCHEMA"] = row["FUNCTION_SCHEMA"];
                        functionParameter["FUNCTION_NAME"] = row["FUNCTION_NAME"];
                        functionParameter["PARAMETER_NAME"] = "result";
                        functionParameter["DATA_TYPE"] = String.Empty;
                        functionParameter["PARAMETER_DIRECTION"] = (Int32)ParameterDirection.Output;

                        functionParameters.Rows.Add(functionParameter);
                    }

                    for (int i = 0; i < argNumber; i++)
                    {
                        DataRow functionParameter = functionParameters.NewRow();

                        // Create the new foreign key column info
                        functionParameter["FUNCTION_CATALOG"] = row["FUNCTION_CATALOG"];
                        functionParameter["FUNCTION_SCHEMA"] = row["FUNCTION_SCHEMA"];
                        functionParameter["FUNCTION_NAME"] = row["FUNCTION_NAME"];
                        functionParameter["PARAMETER_NAME"] = (string)argNames.GetValue(i + 1);
                        functionParameter["DATA_TYPE"] = String.Empty;
                        functionParameter["PARAMETER_DIRECTION"] = (Int32)ParameterDirection.Input;

                        functionParameters.Rows.Add(functionParameter);
                    }
                }

                functionParameters.EndLoadData();
                functionParameters.AcceptChanges();

                functionParameters.Columns.Remove("RETURNS_SET");
                functionParameters.Columns.Remove("ARGUMENT_NUMBER");
                functionParameters.Columns.Remove("ARGUMENT_TYPES");
                functionParameters.Columns.Remove("ARGUMENT_MODES");
                functionParameters.Columns.Remove("ARGUMENT_NAMES");
            }

            return functionParameters;
        }
    }
}