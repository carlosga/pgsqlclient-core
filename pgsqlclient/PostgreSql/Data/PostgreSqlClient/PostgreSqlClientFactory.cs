// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PostgreSqlClientFactory
        : DbProviderFactory
    {
        public static readonly PostgreSqlClientFactory Instance = new PostgreSqlClientFactory();

        private PostgreSqlClientFactory()
            : base()
        {
        }

        public override DbCommand CreateCommand()
        {
            return new PgCommand();
        }

        public override DbConnection CreateConnection()
        {
            return new PgConnection();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new PgConnectionStringBuilder();
        }

        public override DbParameter CreateParameter()
        {
            return new PgParameter();
        }
    }
}