using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public sealed class TypeBindingProvider
    {
#warning TODO: Use ConcurrentDictionary
        private readonly Dictionary<string, ITypeBinding> _bindings;
        private readonly string                           _connectionString;

        public string ConnectionString 
        {
            get { return _connectionString; }
        }

        public TypeBindingProvider(string connectionString)
        {
            _connectionString = connectionString;
            _bindings         = new Dictionary<string, ITypeBinding>(10);
        }

        public bool IsRegistered(string typeName)
        {
            return _bindings.ContainsKey(typeName);
        }

        public ITypeBinding GetBinding(string typeName)
        {
            return _bindings[typeName];
        }

        public TypeBindingProvider RegisterBinding<T>()
            where T: ITypeBinding, new()
        {
            var binding = new T();

            _bindings[binding.Name] = binding;

            return this;
        }

        public void Clear()
        {
            _bindings.Clear();
        }
    }
}
