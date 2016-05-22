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
        private readonly string          _internalName;
        private readonly PgDbType        _pgDbType;
        private readonly TypeFormat      _format;
        private readonly short           _formatCode;
        private readonly Type            _systemType;
        private readonly Type            _pgType;
        private readonly TypeInfo        _elementType;
        private readonly int             _size;
        private readonly TypeAttribute[] _attributes;

        internal string          Schema       => _schema; 
        internal int             Oid          => _oid;
        internal string          Name         => _name;
        internal string          InternalName => _internalName;
        internal PgDbType        PgDbType     => _pgDbType;
        internal Type            SystemType   => _systemType;
        internal Type            PgType       => _pgType;
        internal TypeInfo        ElementType  => _elementType; 
        internal TypeFormat      Format       => _format;
        internal short           FormatCode   => _formatCode;
        internal int             Size         => _size;
        internal TypeAttribute[] Attributes   => _attributes;
        internal bool            IsArray      => (_pgDbType == PgDbType.Array);
        internal bool            IsBinary     => (_pgDbType == PgDbType.Bytea);
        internal bool            IsRefCursor  => (_oid      == (int)Frontend.Oid.RefCursor);

        internal bool IsNumeric
        {
            get
            {
                return (_pgDbType == PgDbType.BigInt
                     || _pgDbType == PgDbType.Byte
                     || _pgDbType == PgDbType.Double
                     || _pgDbType == PgDbType.Integer
                     || _pgDbType == PgDbType.Money
                     || _pgDbType == PgDbType.Numeric
                     || _pgDbType == PgDbType.Real
                     || _pgDbType == PgDbType.SmallInt);
            }
        }

        internal bool IsPrimitive
        {
            get
            {
                return (_pgDbType == PgDbType.BigInt
                     || _pgDbType == PgDbType.Boolean
                     || _pgDbType == PgDbType.Byte
                     || _pgDbType == PgDbType.Char
                     || _pgDbType == PgDbType.Double
                     || _pgDbType == PgDbType.Integer
                     || _pgDbType == PgDbType.Real
                     || _pgDbType == PgDbType.SmallInt);
            }
        }

        internal TypeInfo(int        oid
                        , string     name
                        , string     internalName
                        , PgDbType   pgDbType
                        , TypeFormat format
                        , Type       systemType
                        , Type       pgType)
            : this(oid, name, internalName, pgDbType, null, format, systemType, pgType, -1)
        {
        }

        internal TypeInfo(int        oid
                        , string     name
                        , string     internalName
                        , PgDbType   pgDbType
                        , TypeFormat format
                        , Type       systemType
                        , Type       pgType
                        , int        size)
            : this(oid, name, internalName, pgDbType, null, format, systemType, pgType, size)
        {
        }

        internal TypeInfo(int      oid
                        , string   name
                        , string   internalName
                        , PgDbType pgDbType
                        , TypeInfo elementType
                        , Type     systemType
                        , Type     pgType)
            : this(oid, name, internalName, pgDbType, elementType, elementType.Format, systemType, pgType, -1)
        {
        }

        internal TypeInfo(int        oid
                        , string     name
                        , string     internalName
                        , PgDbType   pgDbType
                        , TypeInfo   elementType
                        , TypeFormat format
                        , Type       systemType
                        , Type       pgType
                        , int        size)
        {
            _schema       = null;
            _oid          = oid;
            _name         = name;
            _internalName = internalName;
            _pgDbType     = pgDbType;
            _elementType  = elementType;
            _format       = format;
            _formatCode   = (short)format;
            _systemType   = systemType;
            _pgType       = pgType;
            _size         = size;
            _attributes   = null;
        }

        internal TypeInfo(string          schema
                        , int             oid
                        , string          name
                        , TypeAttribute[] attributes)
        {
            _schema       = schema;
            _oid          = oid;
            _name         = name;
            _internalName = name;
            _pgDbType     = PgDbType.Composite;
            _format       = TypeFormat.Binary;
            _formatCode   = (short)_format;
            _systemType   = typeof(object);
            _pgType       = typeof(object);
            _size         = -1;
            _attributes   = attributes;
        }

        public override string ToString() => _name;
    }
}
