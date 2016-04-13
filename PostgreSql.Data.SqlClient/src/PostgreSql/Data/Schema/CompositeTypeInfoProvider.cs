// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using PostgreSql.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace PostgreSql.Data.Schema
{
    internal sealed class CompositeTypeInfoProvider
    {
        private static readonly string SchemaQuery = ""; 

        private readonly Connection _connection;

        internal CompositeTypeInfoProvider(Connection connection)
        {
            _connection = connection;
        }

        public ReadOnlyCollection<TypeInfo> GetTypeInfo()
        {
            return null;

            // var columns = new DbColumn[_descriptor.Count];

            // using (var command = _connection.InnerConnection.CreateStatement(ColumnSchemaQuery))
            // {
            //     command.Parameters = new PgParameterCollection();

            //     command.Parameters.Add(new PgParameter("@TableOid", PgDbType.Integer));
            //     command.Parameters.Add(new PgParameter("@ColumnId", PgDbType.Integer));

            //     command.Prepare();

            //     for (int i = 0; i < columns.Length; i++)
            //     {
            //         var schema = new PgDbColumn(_descriptor[i]);

            //         if (!_descriptor[i].IsExpression)
            //         {
            //             command.Parameters[0].Value = _descriptor[i].TableOid;
            //             command.Parameters[1].Value = _descriptor[i].ColumnId;

            //             command.ExecuteReader();

            //             schema.Populate(command.FetchRow());
            //         }

            //         columns[i] = schema;
            //     }
            // }

            // return new ReadOnlyCollection<DbColumn>(columns);
        }
    }
}
