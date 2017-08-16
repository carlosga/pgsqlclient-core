// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;

namespace PostgreSql.Data.Frontend
{
    [DebuggerDisplay("{Name} ({TypeInfo})")]
    internal sealed class FieldDescriptor
    {
        private readonly string   _name;
        private readonly int      _tableOid;
        private readonly short    _columnId;
        private readonly int      _typeOid;
        private readonly short    _typeSize;
        private readonly int      _typeModifier;
        private readonly TypeInfo _typeInfo;
        private readonly int      _numericPrecision;
        private readonly int      _numericScale;
        private readonly bool     _isExpression;

        internal string   Name             => _name;
        internal int      TableOid         => _tableOid;
        internal short    ColumnId         => _columnId;
        internal int      TypeOid          => _typeOid;
        internal short    TypeSize         => _typeSize;
        internal int      TypeModifier     => _typeModifier;
        internal TypeInfo TypeInfo         => _typeInfo;
        internal int      NumericPrecision => _numericPrecision;
        internal int      NumericScale     => _numericScale;
        internal bool     IsExpression     => _isExpression;

        internal FieldDescriptor(string   name
                               , int      tableOid
                               , short    columnId
                               , int      typeOid
                               , short    typeSize
                               , int      typeModifier
                               , TypeInfo typeInfo)
        {
            _name             = name;
            _tableOid         = tableOid;
            _columnId         = columnId;
            _typeOid          = typeOid;
            _typeSize         = typeSize;
            _typeModifier     = typeModifier;
            _typeInfo         = typeInfo;
            _numericPrecision = ((_typeInfo.IsNumeric) ? ((_typeModifier & 0xFFFF) >> 16) : 0);
            _numericScale     = ((_typeInfo.IsNumeric) ? (((ushort)_typeModifier - 4) & 0xFFFF) : 0);
            _isExpression     = (_tableOid == 0 && _columnId == 0);
        }
    }
}
