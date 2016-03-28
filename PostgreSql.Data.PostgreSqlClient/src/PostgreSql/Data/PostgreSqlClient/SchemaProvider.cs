// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
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
                 + "    pg_attribute.attrelid     = @TableOid "
                 + "AND pg_attribute.attnum       = @ColumnId "
                 + "AND pg_attribute.attisdropped = false ";

        private readonly PgConnection    _connection;
        private readonly PgRowDescriptor _descriptor;
        
        internal SchemaProvider(PgConnection connection, PgRowDescriptor descriptor)
        {
            _connection = connection;
            _descriptor = descriptor;
        }
        
        public ReadOnlyCollection<DbColumn> GetColumnSchema()
        {
            var columns = new DbColumn[_descriptor.Count];
            
            using (var command = new PgCommand(ColumnSchemaQuery, _connection))
            {
                command.Parameters.Add("@TableOid", PgDbType.Int4);
                command.Parameters.Add("@ColumnId", PgDbType.Int4);

                command.Prepare();
                
                for (int i = 0; i < columns.Length; i++)
                {
                    var schema = new PgDbColumn(_descriptor[i]);

                    if (!_descriptor[i].IsExpression)
                    {
                        // Execute commands
                        command.Parameters[0].Value = _descriptor[i].TableOid;
                        command.Parameters[1].Value = _descriptor[i].ColumnId;

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                schema.Populate(reader);
                            }
                        }
                    }                    
                    
                    columns[i] = schema;
                }
            } 
            
            return new ReadOnlyCollection<DbColumn>(columns);
        }
    }
}
