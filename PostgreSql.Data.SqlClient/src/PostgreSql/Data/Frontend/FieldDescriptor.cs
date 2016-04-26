// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal sealed class FieldDescriptor
    {
        private readonly string   _name;
        private readonly int      _tableOid;
        private readonly short    _columnId;
        private readonly int      _typeOid;
        private readonly short    _typeSize;
        private readonly int      _typeModifier;
        private readonly TypeInfo _typeInfo;

        internal string   Name             => _name;
        internal int      TableOid         => _tableOid;
        internal short    ColumnId         => _columnId;
        internal int      TypeOid          => _typeOid;
        internal short    TypeSize         => _typeSize;
        internal int      TypeModifier     => _typeModifier;
        internal TypeInfo TypeInfo         => _typeInfo;
        internal int      NumericPrecision => ((_typeInfo.IsNumeric) ? ((_typeModifier & 0xFFFF) >> 16) : 0);
        internal int      NumericScale     => ((_typeInfo.IsNumeric) ? (((ushort)_typeModifier - 4) & 0xFFFF) : 0);
        internal bool     IsExpression     => (_tableOid == 0 && _columnId == 0);

        internal FieldDescriptor(string   name
                               , int      tableOid
                               , short    columnId
                               , int      typeOid
                               , short    typeSize
                               , int      typeModifier
                               , TypeInfo typeInfo)
        {
            _name         = name;
            _tableOid     = tableOid;
            _columnId     = columnId;
            _typeOid      = typeOid;
            _typeSize     = typeSize;
            _typeModifier = typeModifier;
            _typeInfo     = typeInfo;
        }
    }
}
