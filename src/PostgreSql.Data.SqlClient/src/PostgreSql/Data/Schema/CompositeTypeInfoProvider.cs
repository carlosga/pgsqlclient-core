// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using PostgreSql.Data.Frontend;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PostgreSql.Data.Schema
{
    internal sealed class CompositeTypeInfoProvider
    {
        private static readonly string s_schemaQuery = @"SELECT
      pg_namespace.nspname
    , pg_type.oid
    , pg_type.typarray
    , (select count(*) from pg_attribute a where a.attrelid = pg_type.typrelid)
    , pg_type.typname
    , pg_attribute.atttypid
    , pg_attribute.attname
    , pg_attribute.attnum
FROM  pg_type
LEFT  JOIN pg_namespace ON pg_namespace.oid = pg_type.typnamespace 
LEFT  JOIN pg_attribute ON pg_attribute.attrelid = pg_type.typrelid
WHERE (pg_type.typrelid = 0 OR (SELECT c.relkind = 'c' FROM pg_class c WHERE c.oid = pg_type.typrelid)) 
  AND NOT EXISTS(SELECT 1 FROM pg_type el WHERE el.oid = pg_type.typelem AND el.typarray = pg_type.oid)
  AND pg_namespace.nspname NOT IN ('pg_catalog', 'information_schema')
  AND pg_attribute.attisdropped = false
ORDER BY pg_type.oid, pg_attribute.attnum";

        private readonly Connection _connection;

        internal CompositeTypeInfoProvider(Connection connection)
        {
            _connection = connection;
        }

        internal void GetTypeInfo(ref Dictionary<int, TypeInfo> types)
        {
            var row         = new DataRow();
            var defaultType = typeof(object);
            var provider    = TypeBindingContext.GetProvider(_connection.ConnectionOptions.ConnectionString);

            ITypeBinding binding = null;

            using (var command = _connection.CreateStatement(s_schemaQuery))
            {
                command.Prepare();
                command.ExecuteReader();

                while (row.ReadFrom(command))
                {
                    var schema     = row.GetString(0);
                    var typoid     = row.GetInt32(1);
                    var arroid     = row.GetInt32(2);
                    var attcount   = row.GetInt64(3);
                    var typname    = row.GetString(4);
                    var attributes = new TypeAttribute[attcount];

                    attributes[0] = new TypeAttribute(row.GetString(6), row.GetInt32(5));

                    for (int i = 1; i < attributes.Length; ++i)
                    {
                        row.ReadFrom(command);
                        attributes[i] = new TypeAttribute(row.GetString(6), row.GetInt32(5));
                    }

                    row.Reset();

                    if (provider != null)
                    {
                        binding = provider.GetBinding(schema, typname);
                    }

                    types.Add(typoid, new TypeInfo(typoid, schema, typname, attributes, binding?.Type ?? defaultType));
                    types.Add(arroid, new TypeInfo(arroid, schema, typname, types[typoid]));
                }
            }
        }
    }
}
