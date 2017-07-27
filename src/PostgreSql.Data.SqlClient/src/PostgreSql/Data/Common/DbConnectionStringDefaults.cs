// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Data.Common
{
    internal static class DbConnectionStringDefaults
    {
        internal const string ApplicationName            = "pgsqlclient";
        internal const int    CommandTimeout             = 0;
        internal const int    ConnectRetryCount          = 1;
        internal const int    ConnectRetryInterval       = 10;
        internal const int    ConnectTimeout             = 15;
        internal const string DataSource                 = "localhost";
        internal const bool   DefaultTransactionReadOnly = false;
        internal const string DefaultTablespace          = null;
        internal const bool   Encrypt                    = false;
        internal const string InitialCatalog             = "";
        internal const int    LoadBalanceTimeout         = 0;
        internal const int    LockTimeout                = 0;
        internal const int    MaxPoolSize                = 100;
        internal const int    MinPoolSize                = 0;
        internal const bool   MultipleActiveResultSets   = false;
        internal const int    PacketSize                 = 4096;
        internal const int    PortNumber                 = 5432;
        internal const string Password                   = null;
        internal const bool   Pooling                    = true;
        internal const string SearchPath                 = null;
        internal const string UserID                     = "postgres";
    }
}
