// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgFieldDescriptor
    {
        private readonly string       _name;
        private readonly int          _tableOid;
        private readonly short        _columnAttributeNumber;
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

        internal short ColumnAttributeNumber
        {
            get { return _columnAttributeNumber; }
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

        internal PgFieldDescriptor(string       name
                                 , int          tableOid
                                 , short        columnAttributeNumber
                                 , int          typeOid
                                 , short        typeSize
                                 , int          typeModifier
                                 , PgTypeFormat format
                                 , PgType       type)
        {
            _name                  = name;
            _tableOid              = tableOid;
            _columnAttributeNumber = columnAttributeNumber;
            _typeOid               = typeOid;
            _typeSize              = typeSize;
            _typeModifier          = typeModifier;
            _format                = format;
            _type                  = type;
        }
    }
}