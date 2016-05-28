// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgSqlClientFactory
        : DbProviderFactory
    {
        public static readonly PgSqlClientFactory Instance = new PgSqlClientFactory();

        private PgSqlClientFactory()
            : base()
        {
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new PgConnectionStringBuilder();

        public override DbConnection CreateConnection() => new PgConnection();
        public override DbCommand    CreateCommand()    => new PgCommand();
        public override DbParameter  CreateParameter()  => new PgParameter();
    }
}
