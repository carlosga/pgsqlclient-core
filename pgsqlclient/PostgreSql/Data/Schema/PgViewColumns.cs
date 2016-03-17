// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PostgreSqlClient;
using System;
using System.Data;

namespace PostgreSql.Data.Schema
{
    internal sealed class PgViewColumns
        : PgSchema
    {
        public PgViewColumns(PgConnection connection)
            : base(connection)
        {
        }

        protected override string BuildSql(string[] restrictions)
        {
            string sql =
                "SELECT  " +
                    "current_database() AS VIEW_CATALOG, " +
                    "pg_namespace.nspname AS VIEW_SCHEMA, " +
                    "pg_class.relname AS VIEW_NAME, " +
                    "pg_attribute.attname AS COLUMN_NAME, " +
                    "CASE " +
                        "WHEN pg_type.typname = 'bpchar' THEN 'char'  " +
                        "WHEN pg_type.typname = '_bpchar' THEN '_char'  " +
                        "ELSE pg_type.typname " +
                    "END AS DATA_TYPE, " +
                    "cast(pg_attribute.attlen AS Int4) AS COLUMN_SIZE, " +
                    "information_schema.columns.character_octet_length AS CHARACTER_OCTET_LENGTH, " +
                    "information_schema.columns.character_maximum_length AS CHARACTER_LENGTH, " +
                    "cast(0 AS Int4) AS NUMERIC_PRECISION, " +
                    "cast(0 AS Int4) AS NUMERIC_SCALE, " +
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
                    "pg_description.description AS DESCRIPTION " +
                "FROM " +
                    "pg_attribute " +
                "LEFT JOIN " +
                    "pg_class on (pg_attribute.attrelid = pg_class.oid)" +
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
                "LEFT JOIN " +
                    "information_schema.columns on (table_catalog=current_database() and table_schema=pg_namespace.nspname and table_name=pg_class.relname and ordinal_position=pg_attribute.attnum) " +
                "WHERE " +
                    "pg_class.relkind = 'v' AND " +
                    "pg_attribute.attisdropped = false AND pg_attribute.attnum > 0 ";

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
            DataTable dataTypes = connection.GetSchema("DataTypes");

            schema.BeginLoadData();

            foreach (DataRow column in schema.Rows)
            {
                switch (column["DATA_TYPE"].ToString())
                {
                    case "text":
                        column["COLUMN_SIZE"] = column["CHARACTER_OCTET_LENGTH"];
                        break;

                    case "char":
                    case "varchar":
                        column["COLUMN_SIZE"] = column["CHARACTER_LENGTH"];
                        break;

                    default:
                        dataTypes.DefaultView.RowFilter = String.Format("TypeName='{0}'", column["COLUMN_NAME"]);

                        if (dataTypes.DefaultView.Count == 1)
                        {
                            column["COLUMN_SIZE"] = dataTypes.DefaultView[0]["ColumnSize"];
                        }
                        break;
                }
            }

            schema.EndLoadData();
            schema.AcceptChanges();

            return schema;
        }
    }
}