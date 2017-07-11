// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal sealed class TypeAttribute
    {
        private readonly string _name;
        private readonly int    _oid;
        private readonly int    _index;

        internal string Name  => _name;
        internal int    Oid   => _oid;
        internal int    Index => _index;

        internal TypeAttribute(string name, int oid, int index = -1)
        {
            _name  = name;
            _oid   = oid;
            _index = index;
        }
    }
}
