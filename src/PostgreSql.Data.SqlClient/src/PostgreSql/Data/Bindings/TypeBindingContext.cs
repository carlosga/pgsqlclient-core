﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace PostgreSql.Data.Bindings
{
    public static class TypeBindingContext
    {
        private static readonly ConcurrentDictionary<string, TypeBindingProvider> s_providers = new ConcurrentDictionary<string, TypeBindingProvider>();

        public static TypeBindingProvider GetProvider(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (s_providers.Count == 0)
            {
                return null;
            }

            DbConnectionOptions options = new DbConnectionOptions(connectionString);
            TypeBindingProvider provider;
            s_providers.TryGetValue(options.InternalUrl, out provider);
            return provider;
        }

        public static TypeBindingProvider Register(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            DbConnectionOptions options = new DbConnectionOptions(connectionString);
            TypeBindingProvider provider;

            if (!s_providers.TryGetValue(options.InternalUrl, out provider))
            {
                provider = new TypeBindingProvider(options.InternalUrl);

                if (!s_providers.TryAdd(options.InternalUrl, provider))
                {
                    throw ADP.InvalidOperation("An error has occurred while try to register the provider.");
                }
            }

            return provider;
        }

        public static void UnRegister(string connectionString)
        {
            if (s_providers.Count == 0)
            {
                return;
            }
            
            DbConnectionOptions options = new DbConnectionOptions(connectionString);
            TypeBindingProvider provider;
            s_providers.TryRemove(options.InternalUrl, out provider);
        }

        public static void Clear()
        {
            s_providers.Clear();
        }
    }
}
