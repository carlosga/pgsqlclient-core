// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace PostgreSql.Data.Frontend
{
    internal static class TypeInfoProviderCache
    {
        private static readonly List<TypeInfoProvider> s_providers = new List<TypeInfoProvider>();
        private static readonly object                 s_sync      = new object();

        internal static TypeInfoProvider GetOrAdd(Connection connection)
        {
            TypeInfoProvider provider = null;

            lock (s_sync)
            {
                provider = s_providers.SingleOrDefault(p => p.Address == connection.InternalUrl);
                if (provider == null)
                {
                    provider = new TypeInfoProvider(connection.InternalUrl);
                    provider.DiscoverTypes(connection);

                    s_providers.Add(provider);
                }
            }

            provider.AddRef();

            return provider;
        }

        internal static void Release(Connection connection)
        {
            lock (s_sync)
            {
                var provider = s_providers.SingleOrDefault(p => p.Address == connection.InternalUrl);
                if (provider != null)
                {
                    provider.Release();

                    if (provider.Count == 0)
                    {
                        s_providers.Remove(provider);
                    }
                }
            }
        }
    }
}
