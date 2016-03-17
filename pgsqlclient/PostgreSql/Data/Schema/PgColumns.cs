// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Linq;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgColumns
        : PgSchema
    {
        public PgColumns(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT  " +
                    "current_database() AS TABLE_CATALOG, " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "pg_attribute.attname AS COLUMN_NAME, " +
                    "CASE " +
                        "WHEN pg_type.typname = 'bpchar' THEN 'char'  " +
                        "WHEN pg_type.typname = '_bpchar' THEN '_char'  " +
                        "ELSE pg_type.typname " +
                    "END AS DATA_TYPE, " +
                    "cast(0 AS Int4) AS COLUMN_SIZE, " +
                    "cast(0 AS Int4) AS CHARACTER_LENGTH, " +
                    "cast (0 AS Int4) AS NUMERIC_PRECISION, " +
                    "cast (0 AS Int4) AS NUMERIC_SCALE, " +
                    "CASE " +
                        "WHEN pg_attribute.attndims > 0 THEN true " +
                        "ELSE false " +
                    "END AS IS_ARRAY, " +
                    "pg_attribute.attndims AS COLUMN_DIMENSIONS, " +
                    "pg_attribute.attnum AS ORDINAL_POSITION, " +
                    "pg_attribute.atthasdef AS HAS_DEFAULT, " +
                    "pg_attrdef.adsrc AS COLUMN_DEFAULT, " +
                    "CASE pg_attribute.attnotnull " +
                        "when true then false " +
                        "when false then true " +
                    "END AS IS_NULLABLE, " +
                    "(pg_depend.objid is not null) AS IS_AUTOINCREMENT, " +
                    "pg_attribute.atttypmod AS TYPE_MODIFIER, " +
                    "pg_description.description AS DESCRIPTION " +
                "FROM " +
                    "pg_attribute " +
                "LEFT JOIN " +
                    "pg_class on pg_attribute.attrelid = pg_class.oid " +
                "LEFT JOIN " +
                    "pg_namespace on pg_class.relnamespace = pg_namespace.oid " +
                "LEFT JOIN " +
                    "pg_attrdef on (pg_class.oid = pg_attrdef.adrelid AND pg_attribute.attnum = pg_attrdef.adnum) " +
                "LEFT JOIN " +
                    "pg_description on (pg_attribute.attrelid = pg_description.objoid AND pg_attribute.attnum = pg_description.objsubid) " +
                "LEFT JOIN " +
                    "pg_depend on (pg_attribute.attrelid = pg_depend.refobjid AND pg_attribute.attnum = pg_depend.refobjsubid  AND pg_depend.deptype = 'i') " +
                "LEFT JOIN " +
                    "pg_type on (pg_type.oid = pg_attribute.atttypid) " +
                "WHERE " +
                    "pg_attribute.attisdropped = false AND pg_attribute.attnum > 0 ";

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

                // COLUMN_NAME
                if (restrictions.Length > 3 && restrictions[3] != null)
                {
                    sql += String.Format(" and pg_attribute.attname = '{0}'", restrictions[3]);
                }
            }

            return sql + " ORDER BY pg_namespace.nspname, pg_class.relname, pg_attribute.attnum";
        }

        protected override DataTable ProcessResult(PgConnection connection, DataTable schema)
        {
            int typeModifier = 0;

            schema.BeginLoadData();

            foreach (DataRow column in schema.Rows)
            {
                typeModifier = column.IsNull("TYPE_MODIFIER") ? -1 : Convert.ToInt32(column["TYPE_MODIFIER"]);

                switch (column["DATA_TYPE"].ToString())
                {
                    case "char":
                    case "varchar":
                        if (typeModifier != -1)
                        {
                            column["COLUMN_SIZE"] = typeModifier - 4;
                            column["CHARACTER_LENGTH"] = typeModifier - 4;
                        }
                        break;

                    case "decimal":
                    case "numeric":
                        if (typeModifier != -1)
                        {
                            column["COLUMN_SIZE"] = ((typeModifier - 4) & 0xFFFF0000) >> 16;
                            column["NUMERIC_PRECISION"] = ((typeModifier - 4) & 0xFFFF0000) >> 16;
                            column["NUMERIC_SCALE"] = (typeModifier - 4) & 0xFFFF;
                        }
                        break;

                    default:
                        var type = Connection.InnerConnection
                                             .Database
                                             .DataTypes
                                             .Where(x => x.Name == column["DATA_TYPE"].ToString())
                                             .SingleOrDefault();

                        if (type != null)
                        {
                            column["COLUMN_SIZE"] = type.Size;
                        }
                        break;
                }
            }

            schema.EndLoadData();
            schema.AcceptChanges();
            schema.Columns.Remove("TYPE_MODIFIER");

            return schema;
        }
    }
}