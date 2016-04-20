// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Security.Principal;

namespace System.Data.ProviderBase
{
    internal sealed partial class DbConnectionPoolIdentity
    {
        public static readonly DbConnectionPoolIdentity NoIdentity = new DbConnectionPoolIdentity(String.Empty, false, true);

        private readonly string _sidString;
        private readonly bool   _isRestricted;
        private readonly bool   _isNetwork;
        private readonly int    _hashCode;

        private DbConnectionPoolIdentity(string sidString, bool isRestricted, bool isNetwork)
        {
            _sidString    = sidString;
            _isRestricted = isRestricted;
            _isNetwork    = isNetwork;
            _hashCode     = ((sidString == null) ? 0 : sidString.GetHashCode());
        }

        internal bool IsRestricted => _isRestricted;

        public override bool Equals(object value)
        {
            bool result = ((this == NoIdentity) || (this == value));
            if (!result && (value != null))
            {
                DbConnectionPoolIdentity that = ((DbConnectionPoolIdentity)value);
                result = ((_sidString == that._sidString) && (_isRestricted == that._isRestricted) && (_isNetwork == that._isNetwork));
            }
            return result;
        }

        public override int GetHashCode() => _hashCode;
    }
}

