// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal static class ConnectionStringKeywords
    {
        internal const string DataSource               = "data source";
        internal const string InitialCatalog           = "initial catalog";
        internal const string UserId                   = "user id";
        internal const string Password                 = "password";
        internal const string PortNumber               = "port number";
        internal const string PacketSize               = "packet size";
        internal const string ConnectionTimeout        = "connection timeout";
        internal const string Pooling                  = "pooling";
        internal const string ConnectionLifetime       = "connection lifetime";
        internal const string MinPoolSize              = "min pool size";
        internal const string MaxPoolSize              = "max pool size";
        internal const string Encrypt                  = "encrypt";
        internal const string MultipleActiveResultSets = "multipleactiveresultsets";
        internal const string SearchPath               = "search path";
        internal const string FetchSize                = "fetch size";
    }
}
