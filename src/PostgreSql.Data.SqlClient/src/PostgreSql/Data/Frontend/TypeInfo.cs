// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed class TypeInfo
    {
        private readonly string          _schema;
        private readonly int             _oid;
        private readonly string          _name;
        private readonly PgDbType        _pgDbType;
        private readonly Type            _systemType;
        private readonly Type            _pgType;
        private readonly TypeInfo        _elementType;
        private readonly int             _size;
        private readonly bool            _isArray;
        private readonly bool            _isBinary;
        private readonly bool            _isRefCursor;
        private readonly bool            _isNumeric;
        private readonly bool            _isComposite;
        private readonly TypeAttribute[] _attributes;

        internal string          Schema       => _schema; 
        internal int             Oid          => _oid;
        internal string          Name         => _name;
        internal PgDbType        PgDbType     => _pgDbType;
        internal Type            SystemType   => _systemType;
        internal Type            PgType       => _pgType;
        internal TypeInfo        ElementType  => _elementType; 
        internal int             Size         => _size;
        internal bool            IsArray      => _isArray;
        internal bool            IsBinary     => _isBinary;
        internal bool            IsRefCursor  => _isRefCursor;
        internal bool            IsNumeric    => _isNumeric;
        internal bool            IsComposite  => _isComposite; 
        internal TypeAttribute[] Attributes   => _attributes;

        internal TypeInfo(int oid, string name, PgDbType pgDbType, Type systemType, Type pgType)
            : this(oid, name, pgDbType, null, systemType, pgType, -1)
        {
        }

        internal TypeInfo(int oid, string name, PgDbType pgDbType, Type systemType, Type pgType, int size)
            : this(oid, name, pgDbType, null, systemType, pgType, size)
        {
        }

        internal TypeInfo(int oid, string name, PgDbType pgDbType, TypeInfo elementType, Type systemType, Type  pgType)
            : this(oid, name, pgDbType, elementType, systemType, pgType, -1)
        {
        }

        internal TypeInfo(int oid, string name, PgDbType pgDbType, TypeInfo elementType, Type systemType, Type pgType, int size)
        {
            _oid          = oid;
            _name         = name;
            _pgDbType     = pgDbType;
            _elementType  = elementType;
            _systemType   = systemType;
            _pgType       = pgType;
            _size         = size;
            _isArray      = (_pgDbType == PgDbType.Array);
            _isBinary     = (_pgDbType == PgDbType.Bytea);
            _isRefCursor  = (_oid      == Frontend.Oid.RefCursor);
            _isNumeric    = (_pgDbType == PgDbType.BigInt
                          || _pgDbType == PgDbType.Byte
                          || _pgDbType == PgDbType.Double
                          || _pgDbType == PgDbType.Integer
                          || _pgDbType == PgDbType.Money
                          || _pgDbType == PgDbType.Numeric
                          || _pgDbType == PgDbType.Real
                          || _pgDbType == PgDbType.SmallInt);
        }

        internal TypeInfo(int oid, string schema, string name, TypeAttribute[] attributes, Type systemType)
        {
            _schema      = schema;
            _oid         = oid;
            _name        = name;
            _pgDbType    = PgDbType.Composite;
            _size        = -1;
            _attributes  = attributes;
            _systemType  = systemType;
            _pgType      = systemType;
            _isComposite = true;
        }

        internal TypeInfo(int oid, string schema, string name, TypeInfo elementType)
        {
            _schema      = schema;
            _oid         = oid;
            _name        = name;
            _pgDbType    = PgDbType.Array;
            _elementType = elementType;
            _systemType  = typeof(object);
            _pgType      = typeof(object);
            _size        = -1;
            _isComposite = true;
        }

        public override string ToString() => _name;
    }
}
