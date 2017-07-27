// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using PostgreSql.Data.Frontend;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PostgreSql.Data.SqlClient;
using System;

namespace PostgreSql.Data.Schema
{
    internal sealed class EnumTypeInfoProvider
    {
        private static readonly string s_schemaQuery = @"SELECT
      pg_namespace.nspname
    , pg_type.oid
    , pg_type.typarray
    , pg_type.typname
    , pg_enum.enumlabel
    , pg_enum.enumsortorder
    , (SELECT COUNT(*) FROM pg_enum AS pg_enum2 WHERE pg_type.oid = pg_enum2.enumtypid)
FROM  pg_type
 LEFT JOIN pg_namespace ON pg_namespace.oid      = pg_type.typnamespace 
 LEFT JOIN pg_attribute ON pg_attribute.attrelid = pg_type.typrelid
 LEFT JOIN pg_enum      ON pg_type.oid           = pg_enum.enumtypid
WHERE (strpos(pg_namespace.nspname, 'pg_') = 0 AND pg_namespace.nspname <> 'information_schema')
  AND pg_type.typtype IN ('e')
ORDER BY pg_type.oid, pg_enum.enumsortorder;";

        private readonly Connection _connection;

        internal EnumTypeInfoProvider(Connection connection)
        {
            _connection = connection;
        }

        internal void GetTypeInfo(ref Dictionary<int, TypeInfo> types)
        {
            var row = new DataRow();

            using (var command = _connection.CreateStatement(s_schemaQuery))
            {
                command.Prepare();
                command.ExecuteReader();

                while (row.ReadFrom(command))
                {
                    var schema     = row.GetString(0);
                    var typoid     = row.GetInt32(1);
                    var arroid     = row.GetInt32(2);
                    var typname    = row.GetString(3);
                    var attcount   = row.GetInt64(6);
                    var attributes = new TypeAttribute[attcount];

                    attributes[0] = new TypeAttribute(row.GetString(4), (int)row.GetFloat(5));

                    for (int i = 1; i < attributes.Length; ++i)
                    {
                        row.ReadFrom(command);
                        attributes[i] = new TypeAttribute(row.GetString(4), row.GetInt32(1), (int)row.GetFloat(5));
                    }
                    
                    row.Reset();

                    types.Add(typoid, new TypeInfo(typoid, schema, typname, attributes, typeof(Enum), PgDbType.Enum));
                    types.Add(arroid, new TypeInfo(arroid, schema, typname, types[typoid]));                
                }
            }
        }
    }
}
