// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Data.ProviderBase
{
    internal sealed class DbConnectionPoolIdentity
        : IEquatable<DbConnectionPoolIdentity>
    {
        internal static readonly DbConnectionPoolIdentity NoIdentity = new DbConnectionPoolIdentity("(NoIdentity)", false, true);

        private readonly string _sidString;
        private readonly bool   _isRestricted;
        private readonly bool   _isNetwork;
        private readonly int    _hashCode;

        internal bool IsRestricted => _isRestricted;

        private DbConnectionPoolIdentity(string sidString, bool isRestricted, bool isNetwork)
        {
            _sidString    = sidString;
            _isRestricted = isRestricted;
            _isNetwork    = isNetwork;
            _hashCode     = ((sidString == null) ? 0 : sidString.GetHashCode());
        }

        public bool Equals(DbConnectionPoolIdentity other)
        {
            return (other         != null 
                 && _sidString    == other._sidString 
                 && _isRestricted == other._isRestricted 
                 && _isNetwork    == other._isNetwork);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            DbConnectionPoolIdentity that = obj as DbConnectionPoolIdentity;
            if (that == null)
            {
                return false;
            }
            return Equals(obj);
        }

        public override int GetHashCode() => _hashCode;

        public static bool operator ==(DbConnectionPoolIdentity lhs, DbConnectionPoolIdentity rhs)
        {
            if ((lhs as object) == null || (rhs as object) == null)
            {
                return Object.Equals(lhs, rhs);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(DbConnectionPoolIdentity lhs, DbConnectionPoolIdentity rhs)
        {
            if ((lhs as object) == null || (rhs as object) == null)
            {
                return !Object.Equals(lhs, rhs);
            }

            return !lhs.Equals(rhs);
        }
    }
}
