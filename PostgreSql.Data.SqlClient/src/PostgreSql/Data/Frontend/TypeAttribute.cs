// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Frontend
{
    internal sealed class TypeAttribute
    {
        private readonly string _name;
        private readonly int    _oid;

        internal string Name => _name;
        internal int    Oid  => _oid;

        internal TypeAttribute(string name, int oid)
        {
            _name = name;
            _oid  = oid;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
