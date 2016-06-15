// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;

namespace PostgreSql.Data.Frontend
{
    internal static class TypeInfoProviderCache
    {
        private static ConcurrentDictionary<string, Lazy<TypeInfoProvider>> s_providers = new ConcurrentDictionary<string, Lazy<TypeInfoProvider>>();

        internal static TypeInfoProvider GetOrAdd(Connection connection)
        {
            string key = connection.ConnectionOptions.InternalUrl;
            Lazy<TypeInfoProvider> cacheItem;

            if (!s_providers.TryGetValue(key, out cacheItem))
            {
                cacheItem = new Lazy<TypeInfoProvider>(() => new TypeInfoProvider(connection));

                if (!s_providers.TryAdd(key, cacheItem))
                {   
                    cacheItem = s_providers[key];
                }
            }

            var provider = cacheItem.Value;

            provider.AddRef();

            return provider;
        }

        internal static void Release(Connection connection)
        {
            string key = connection.ConnectionOptions.InternalUrl;
            Lazy<TypeInfoProvider> cacheItem;
            if (s_providers.TryGetValue(key, out cacheItem))
            {
                cacheItem.Value.Release();

                if (cacheItem.Value.Count == 0)
                {
                   s_providers.TryRemove(key, out cacheItem);
                }
            }
        }
    }
}
