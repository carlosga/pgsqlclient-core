// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Data.Common
{
    // Immutable key for connection pool groups
    internal sealed class DbConnectionPoolKey
        : IEquatable<DbConnectionPoolKey>
    {
        private readonly string _connectionString;
        private readonly int    _hashValue;

        internal string ConnectionString => _connectionString;

        internal DbConnectionPoolKey(string connectionString)
        {
            _connectionString = connectionString;
            _hashValue        = ((_connectionString == null) ? 0 : _connectionString.GetHashCode());
        }

        private DbConnectionPoolKey(DbConnectionPoolKey key)
            : this(key.ConnectionString)
        {
        }

        public bool Equals(DbConnectionPoolKey other) => (other != null && _connectionString == other._connectionString);

        public override int GetHashCode() => _hashValue;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            var key = obj as DbConnectionPoolKey;
            if (key == null)
            {
                throw ADP.WrongType(obj.GetType(), typeof(DbConnectionPoolKey));
            }
            return Equals(key);
        }

        public static bool operator ==(DbConnectionPoolKey lhs, DbConnectionPoolKey rhs)
        {
            if ((lhs as object) == null || (rhs as object) == null)
            {
                return Object.Equals(lhs, rhs);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(DbConnectionPoolKey lhs, DbConnectionPoolKey rhs)
        {
            if ((lhs as object) == null || (rhs as object) == null)
            {
                return !Object.Equals(lhs, rhs);
            }

            return !lhs.Equals(rhs);
        }

        internal DbConnectionPoolKey Clone()  =>  new DbConnectionPoolKey(this);        
    }
}
