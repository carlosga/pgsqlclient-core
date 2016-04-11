// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class SchemaProvider
        : IDbColumnSchemaGenerator
    {
        private static readonly string ColumnSchemaQuery = 
                "SELECT" 
                 + "  pg_namespace.nspname        AS TABLE_SCHEMA"
                 + ", pg_class.relname            AS TABLE_NAME"
                 + ", pg_attribute.attname        AS COLUMN_NAME"
                 + ", pg_attribute.attnotnull     AS IS_NOT_NULL"
                 + ", pg_depend.objid is not null AS IS_AUTOINCREMENT "
                 + ", CASE WHEN pg_constraint.contype = 'p' AND pg_attribute.attnum = ANY (pg_constraint.conkey) THEN true ELSE false END AS IS_PRIMARYKEY "
                 + ", CASE WHEN pg_constraint.contype = 'u' AND pg_attribute.attnum = ANY (pg_constraint.conkey) THEN true ELSE false END AS IS_UNIQUEKEY "
              + "FROM pg_attribute " 
                 + "LEFT JOIN pg_constraint ON pg_attribute.attrelid  = pg_constraint.conrelid "
                 + "LEFT JOIN pg_class      ON pg_attribute.attrelid  = pg_class.oid "
                 + "LEFT JOIN pg_namespace  ON pg_class.relnamespace  = pg_namespace.oid "
                 + "LEFT JOIN pg_attrdef    ON (pg_class.oid          = pg_attrdef.adrelid AND pg_attribute.attnum = pg_attrdef.adnum) "
                 + "LEFT JOIN pg_depend     ON (pg_attribute.attrelid = pg_depend.refobjid AND pg_attribute.attnum = pg_depend.refobjsubid AND pg_depend.deptype = 'i') "
              + "WHERE " 
                 + "    pg_attribute.attrelid     = $1 "
                 + "AND pg_attribute.attnum       = $2 "
                 + "AND pg_attribute.attisdropped = false";

        private readonly PgConnection  _connection;
        private readonly RowDescriptor _descriptor;

        internal SchemaProvider(PgConnection connection, RowDescriptor descriptor)
        {
            _connection = connection;
            _descriptor = descriptor;
        }

        public ReadOnlyCollection<DbColumn> GetColumnSchema()
        {
            var columns = new DbColumn[_descriptor.Count];

            using (var command = _connection.InnerConnection.CreateStatement(ColumnSchemaQuery))
            {
                command.Parameters = new PgParameterCollection();

                command.Parameters.Add(new PgParameter("@TableOid", PgDbType.Integer));
                command.Parameters.Add(new PgParameter("@ColumnId", PgDbType.Integer));

                command.Prepare();

                for (int i = 0; i < columns.Length; i++)
                {
                    var schema = new PgDbColumn(_descriptor[i]);

                    if (!_descriptor[i].IsExpression)
                    {
                        command.Parameters[0].Value = _descriptor[i].TableOid;
                        command.Parameters[1].Value = _descriptor[i].ColumnId;

                        command.ExecuteReader();

                        schema.Populate(command.FetchRow());
                    }

                    columns[i] = schema;
                }
            }

            return new ReadOnlyCollection<DbColumn>(columns);
        }
    }
}
