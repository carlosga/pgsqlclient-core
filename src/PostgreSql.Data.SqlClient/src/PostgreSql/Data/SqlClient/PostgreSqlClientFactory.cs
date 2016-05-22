// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PostgreSqlClientFactory
        : DbProviderFactory
    {
        public static readonly PostgreSqlClientFactory Instance = new PostgreSqlClientFactory();

        private PostgreSqlClientFactory()
            : base()
        {
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder() => new PgConnectionStringBuilder();

        public override DbConnection CreateConnection() => new PgConnection();
        public override DbCommand    CreateCommand()    => new PgCommand();
        public override DbParameter  CreateParameter()  => new PgParameter();
    }
}
