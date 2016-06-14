using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public sealed class TypeBindingContext
    {
#warning TODO: Use ConcurrentDictionary
        private static readonly Dictionary<string, TypeBindingProvider> s_providers = new Dictionary<string, TypeBindingProvider>(10);

        private TypeBindingContext()
        {
        }

        public static TypeBindingProvider GetProvider(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            return s_providers[connectionString];
        }

        public static TypeBindingProvider Register(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            if (!s_providers.ContainsKey(connectionString))
            {
                s_providers[connectionString] = new TypeBindingProvider(connectionString); 
            }

            return s_providers[connectionString];
        }

        public static void UnRegister(string connectionstring)
        {
            s_providers.Remove(connectionstring);
        }

        public static void Clear()
        {
            s_providers.Clear();
        }
    }
}
