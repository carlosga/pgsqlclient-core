// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Data.Common;

namespace PostgreSql.Data.Frontend
{
    internal static class TypeInfoProviderCache
    {
        private static readonly ConcurrentDictionary<string, TypeInfoProvider> s_providers = new ConcurrentDictionary<string, TypeInfoProvider>();

        internal static TypeInfoProvider GetOrAdd(DbConnectionOptions options)
        {
            TypeInfoProvider provider;

            if (!s_providers.TryGetValue(options.InternalUrl, out provider))
            {
                provider = new TypeInfoProvider(options);
                s_providers.TryAdd(options.InternalUrl, provider);
            }

            provider.AddRef();

            return provider;
        }

        internal static void Release(DbConnectionOptions options)
        {
            TypeInfoProvider provider;
            if (s_providers.TryGetValue(options.InternalUrl, out provider))
            {
                if (provider.Release() == 0)
                {
                   s_providers.TryRemove(options.InternalUrl, out provider);
                }
            }
        }
    }
}
