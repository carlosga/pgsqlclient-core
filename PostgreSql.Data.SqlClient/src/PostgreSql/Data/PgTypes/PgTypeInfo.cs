// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using PostgreSql.Data.SqlClient;

namespace PostgreSql.Data.PgTypes
{
    internal sealed class PgTypeInfo
    {
        private int                   _oid;
        private readonly string       _name;
        private readonly string       _internalName;
        private readonly PgDbType     _providerType;
        private readonly PgTypeFormat _format;
        private readonly Type         _systemType;
        private readonly Type         _pgType;
        private readonly PgTypeInfo   _elementType;
        private readonly int          _size;
        private readonly string       _delimiter;
        private readonly string       _prefix;

        internal int Oid
        {
            get { return _oid; }
            set { _oid = value; }
        }

        internal PgDbType     ProviderType => _providerType;
        internal string       Name         => _name;
        internal string       InternalName => _internalName;
        internal Type         SystemType   => _systemType;
        internal Type         PgType       => _pgType;
        internal PgTypeInfo   ElementType  => _elementType; 
        internal PgTypeFormat Format       => _format;
        internal int          Size         => _size;
        internal bool         IsArray      => (_providerType == PgDbType.Array);
        internal bool         IsBinary     => (_providerType == PgDbType.Bytea);
        internal bool         IsRefCursor  => (_providerType == PgDbType.Refcursor);
        internal string       Delimiter    => _delimiter;
        internal string       Prefix       => _prefix;

        internal bool IsNumeric
        {
            get
            {
                return (_providerType == PgDbType.BigInt
                     || _providerType == PgDbType.Byte
                     || _providerType == PgDbType.Double
                     || _providerType == PgDbType.Integer
                     || _providerType == PgDbType.Money
                     || _providerType == PgDbType.Numeric
                     || _providerType == PgDbType.Real
                     || _providerType == PgDbType.SmallInt);
            }
        }

        internal bool IsPrimitive
        {
            get
            {
                return (_providerType == PgDbType.BigInt
                     || _providerType == PgDbType.Bool
                     || _providerType == PgDbType.Byte
                     || _providerType == PgDbType.Char
                     || _providerType == PgDbType.Double
                     || _providerType == PgDbType.Integer
                     || _providerType == PgDbType.Real
                     || _providerType == PgDbType.SmallInt);
            }
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeFormat format
                          , Type         systemType)
            : this(oid, name, internalName, providerType, format, systemType, -1)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size)
            : this(oid, name, internalName, providerType, null, format, systemType, size)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeInfo   elementType
                          , Type         systemType)
            : this(oid, name, internalName, providerType, elementType, elementType.Format, systemType, -1)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeInfo   elementType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size)
            : this(oid, name, internalName, providerType, elementType, format, systemType, size, String.Empty)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size
                          , string       delimiter)
            : this(oid, name, internalName, providerType, null, format, systemType, size, delimiter, String.Empty)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeInfo   elementType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size
                          , string       delimiter)
            : this(oid, name, internalName, providerType, elementType, format, systemType, size, delimiter, String.Empty)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size
                          , string       delimiter
                          , string       prefix)
            : this(oid, name, internalName, providerType, null, format, systemType, size, delimiter, prefix)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , string       internalName
                          , PgDbType     providerType
                          , PgTypeInfo   elementType
                          , PgTypeFormat format
                          , Type         systemType
                          , int          size
                          , string       delimiter
                          , string       prefix)
        {
            _oid          = oid;
            _name         = name;
            _internalName = internalName;
            _providerType = providerType;
            _elementType  = elementType;
            _format       = format;
            _systemType   = systemType;
            _size         = size;
            _delimiter    = delimiter;
            _prefix       = prefix;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}
