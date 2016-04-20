// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    // Implementation of a key to connection pool groups for specifically to be used for PgConnection
    internal sealed class PgConnectionPoolKey 
        : DbConnectionPoolKey
    {
        private int _hashValue;

        internal override string ConnectionString
        {
            get { return base.ConnectionString; }
            set
            {
                base.ConnectionString = value;
                CalculateHashCode();
            }
        }

        internal PgConnectionPoolKey(string connectionString) 
            : base(connectionString)
        {
            CalculateHashCode();
        }

        private PgConnectionPoolKey(PgConnectionPoolKey key) 
            : base(key)
        {
            CalculateHashCode();
        }

        public override bool Equals(object obj)
        {
            PgConnectionPoolKey key = obj as PgConnectionPoolKey;
            return (key != null && ConnectionString == key.ConnectionString);
        }

        public override int GetHashCode()
        {
            return _hashValue;
        }

        internal override DbConnectionPoolKey Clone()
        {
            return new PgConnectionPoolKey(this);
        }

        private void CalculateHashCode()
        {
            _hashValue = base.GetHashCode();
        }
    }
}
