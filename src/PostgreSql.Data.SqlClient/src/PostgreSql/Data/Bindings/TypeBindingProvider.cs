// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace PostgreSql.Data.Bindings
{
    public sealed class TypeBindingProvider
    {
        private readonly ConcurrentDictionary<string, ITypeBinding> _bindings;
        private readonly string                                     _connectionString;

        public string ConnectionString 
        {
            get { return _connectionString; }
        }

        public TypeBindingProvider(string connectionString)
        {
            _connectionString = connectionString;
            _bindings         = new ConcurrentDictionary<string, ITypeBinding>();
        }

        public bool IsRegistered(string schema, string typeName)
        {
            if (_bindings.Count == 0)
            {
                return false;
            }
            return _bindings.ContainsKey($"{schema}.{typeName}");
        }

        public ITypeBinding GetBinding(string schema, string typeName)
        {
            if (_bindings.Count == 0)
            {
                return null;
            }
            ITypeBinding binding;
            if (_bindings.TryGetValue($"{schema}.{typeName}", out binding))
            {
                return binding;
            }
            return null;
        }

        public TypeBindingProvider RegisterBinding<T>()
            where T: ITypeBinding, new()
        {
            ITypeBinding binding = new T();
            ITypeBinding current;
            string       key     = $"{binding.Schema}.{binding.Name}";

            if (!_bindings.TryGetValue(key, out current))
            {
                if (!_bindings.TryAdd(key, binding))
                {
                    throw ADP.InvalidOperation("An error has occurred while trying to register the binding.");
                }
            }

            return this;
        }

        public void Clear()
        {
            _bindings.Clear();
        }
    }
}
