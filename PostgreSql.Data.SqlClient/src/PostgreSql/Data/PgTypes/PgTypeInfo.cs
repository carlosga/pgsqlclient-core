// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using PostgreSql.Data.SqlClient;

namespace PostgreSql.Data.PgTypes
{
    internal sealed class PgTypeInfo
    {
        private readonly int          _oid;
        private readonly string       _name;
        private readonly string       _internalName;
        private readonly PgDbType     _pgDbType;
        private readonly PgTypeFormat _format;
        private readonly Type         _systemType;
        private readonly Type         _pgType;
        private readonly PgTypeInfo   _elementType;
        private readonly int          _size;

        internal int          Oid          => _oid;
        internal string       Name         => _name;
        internal string       InternalName => _internalName;
        internal PgDbType     PgDbType     => _pgDbType;
        internal Type         SystemType   => _systemType;
        internal Type         PgType       => _pgType;
        internal PgTypeInfo   ElementType  => _elementType; 
        internal PgTypeFormat Format       => _format;
        internal int          Size         => _size;
        internal bool         IsArray      => (_pgDbType == PgDbType.Array);
        internal bool         IsBinary     => (_pgDbType == PgDbType.Bytea);
        internal bool         IsRefCursor  => (_oid      == (int)PostgreSql.Data.PgTypes.Oid.RefCursor);

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
                     || _pgDbType == PgDbType.Bool
                     || _pgDbType == PgDbType.Byte
                     || _pgDbType == PgDbType.Char
                     || _pgDbType == PgDbType.Double
                     || _pgDbType == PgDbType.Integer
                     || _pgDbType == PgDbType.Real
                     || _pgDbType == PgDbType.SmallInt);
            }
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     pgDbType
                          , PgTypeFormat format
                          , Type         systemType
                          , Type         pgType)
            : this(oid, name, internalName, pgDbType, null, format, systemType, pgType, -1)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     pgDbType
                          , PgTypeFormat format
                          , Type         systemType
                          , Type         pgType
                          , int          size)
            : this(oid, name, internalName, pgDbType, null, format, systemType, pgType, size)
        {
        }

        internal PgTypeInfo(int        oid
                          , string     name
                          , string     internalName
                          , PgDbType   pgDbType
                          , PgTypeInfo elementType
                          , Type       systemType
                          , Type       pgType)
            : this(oid, name, internalName, pgDbType, elementType, elementType.Format, systemType, pgType, -1)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     pgDbType
                          , PgTypeInfo   elementType
                          , PgTypeFormat format
                          , Type         systemType
                          , Type         pgType
                          , int          size)
        {
            _oid          = oid;
            _name         = name;
            _internalName = internalName;
            _pgDbType     = pgDbType;
            _elementType  = elementType;
            _format       = format;
            _systemType   = systemType;
            _pgType       = pgType;
            _size         = size;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
