// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace PostgreSql.Data.Frontend
{
    internal static class TypeInfoProviderCache
    {
        private static readonly ConcurrentDictionary<string, TypeInfoProvider> s_providers = new ConcurrentDictionary<string, TypeInfoProvider>();

        internal static TypeInfoProvider GetOrAdd(Connection connection)
        {
            string           key = connection.ConnectionOptions.InternalUrl;
            TypeInfoProvider provider;

            if (!s_providers.TryGetValue(key, out provider))
            {
                provider = new TypeInfoProvider(connection.ConnectionOptions);
                s_providers.TryAdd(key, provider);
            }

            provider.AddRef();

            return provider;
        }

        internal static void Release(Connection connection)
        {
            string           key = connection.ConnectionOptions.InternalUrl;
            TypeInfoProvider provider;
            if (s_providers.TryGetValue(key, out provider))
            {
                provider.Release();

                if (provider.Count == 0)
                {
                   s_providers.TryRemove(key, out provider);
                }
            }
        }
    }
}
