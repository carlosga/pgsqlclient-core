// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgFieldDescriptor
    {
        private readonly string       _name;
        private readonly int          _tableOid;
        private readonly short        _columnId;
        private readonly int          _typeOid;
        private readonly short        _typeSize;
        private readonly int          _typeModifier;
        private readonly PgTypeFormat _format;
        private readonly PgType       _type;

        internal string Name
        {
            get { return _name; }
        }

        internal int TableOid
        {
            get { return _tableOid; }
        }

        internal short ColumnId
        {
            get { return _columnId; }
        }

        internal int TypeOid
        {
            get { return _typeOid; }
        }

        internal short TypeSize
        {
            get { return _typeSize; }
        }

        internal int TypeModifier
        {
            get { return _typeModifier; }
        }

        internal PgTypeFormat Format
        {
            get { return _format; }
        }

        internal PgType Type
        {
            get { return _type; }
        }
        
        internal int NumericPrecision
        {
            get
            {
                if (!_type.IsNumeric)
                {
                    return 0;
                }

                return (_typeModifier >> 16);
            }
        }

        internal int NumericScale
        {
            get
            {
                if (!_type.IsNumeric)
                {
                    return 0;
                }

                return ((ushort)_typeModifier - 4);
            }
        }        

        internal PgFieldDescriptor(string       name
                                 , int          tableOid
                                 , short        columnId
                                 , int          typeOid
                                 , short        typeSize
                                 , int          typeModifier
                                 , PgTypeFormat format
                                 , PgType       type)
        {
            _name         = name;
            _tableOid     = tableOid;
            _columnId     = columnId;
            _typeOid      = typeOid;
            _typeSize     = typeSize;
            _typeModifier = typeModifier;
            _format       = format;
            _type         = type;
        }
    }
}