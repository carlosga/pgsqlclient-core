// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data.ProviderBase;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class SqlConnectionPoolProviderInfo
        : DbConnectionPoolProviderInfo
    {
        private string _instanceName;

        internal string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }
    }
}
