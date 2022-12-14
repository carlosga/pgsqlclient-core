// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using PostgreSql.Data.PgTypes;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgParameter
        : DbParameter
    {
        private ParameterDirection    _direction;
        private bool                  _isNullable;
        private bool                  _isTypeSet;
        private bool                  _sourceColumnNullMapping;
        private string                _parameterName;
        private string                _sourceColumn;
        private byte                  _precision;
        private byte                  _scale;
        private int                   _size;
        private object                _value;
        private object                _pgvalue;
        private PgDbType              _pgDbType;
        private TypeInfo              _typeInfo;
        private PgParameterCollection _parent;

        public override string ParameterName
        {
            get => _parameterName;
            set => _parameterName = value;
        }

        public override byte Precision
        {
            get =>_precision;
            set => _precision = value;
        }

        public override byte Scale
        {
            get => _scale;
            set => _scale = value;
        }

        public override int Size
        {
            get => ((_size == 0) ? ValueSize() : _size);
            set => _size = value;
        }

        public override DbType DbType
        {
            get => TypeInfoProvider.GetDbType(_pgDbType);
            set => PgDbType = TypeInfoProvider.GetProviderType(value);
        }

        public PgDbType PgDbType
        {
            get => _pgDbType;
            set
            {
                _pgDbType  = value;
                _isTypeSet = true;
                if (_pgDbType != PgDbType.Composite)
                {
                    // Composite type info will be determined before parsing the query
                    _typeInfo  = TypeInfoProvider.GetTypeInfo(value);
                }
            }
        }

        public override ParameterDirection Direction
        {
            get => _direction;
            set => _direction = value;
        }

        public override bool IsNullable
        {
            get => _isNullable;
            set => _isNullable = value;
        }

        public override string SourceColumn
        {
            get => _sourceColumn;
            set => _sourceColumn = value;
        }

        public override object Value
        {
            get => _value;
            set => PgValue = value;
        }

        public object PgValue
        {
            get => _pgvalue;
            set
            {
                _value   = value;
                _pgvalue = ((value is INullable) ? value : null);

                if (!_isTypeSet || _pgDbType == PgDbType.Array)
                {
                    UpdateTypeInfo(value);
                }
            }
        }

        public override bool SourceColumnNullMapping
        {
            get => _sourceColumnNullMapping;
            set => _sourceColumnNullMapping = value;
        }

        internal PgParameterCollection Parent
        {
            get => _parent;
            set => _parent = value;
        }

        internal bool IsTypeSet => _isTypeSet;

        internal TypeInfo TypeInfo
        {
            get => _typeInfo;
            set => _typeInfo = value;
        }

        public PgParameter()
        {
            _direction  = ParameterDirection.Input;
            _isNullable = false;
            _pgDbType   = PgDbType.VarChar;
        }

        public PgParameter(string parameterName, object value) 
            : this()
        {
            _parameterName = parameterName;
            PgValue        = value;
        }

        public PgParameter(string parameterName, PgDbType pgDbType) 
            : this()
        {
            _parameterName = parameterName;
            PgDbType       = pgDbType;
        }

        public PgParameter(string parameterName, PgDbType pgDbType, int size) 
            : this()
        {
            _parameterName = parameterName;
            _size          = size;
            PgDbType       = pgDbType;
        }

        public PgParameter(string parameterName, PgDbType pgDbType, int size, string sourceColumn)
            : this()
        {
            _parameterName = parameterName;
            _size          = size;
            _sourceColumn  = sourceColumn;
            PgDbType       = pgDbType;
        }

        public PgParameter(string             parameterName
                         , PgDbType           pgDbType
                         , int                size
                         , ParameterDirection direction
                         , bool               isNullable
                         , byte               precision
                         , byte               scale
                         , string             sourceColumn
                         , DataRowVersion     sourceVersion
                         , object             value)
        {
            _parameterName = parameterName;
            _size          = size;
            _direction     = direction;
            _isNullable    = isNullable;
            _precision     = precision;
            _scale         = scale;
            _sourceColumn  = sourceColumn;
            PgDbType       = pgDbType;
            PgValue        = value;
        }

        public override string ToString() => _parameterName;

        public override void ResetDbType()
        {
            throw ADP.NotSupported("The method or operation is not implemented.");
        }

        private void UpdateTypeInfo(object value)
        {
            if (_pgDbType == PgDbType.Array)
            {
                _typeInfo = TypeInfoProvider.GetArrayTypeInfo(value);
            }
            else if (_pgDbType == PgDbType.Vector)
            {
                _typeInfo = TypeInfoProvider.GetVectorTypeInfo(value);
            }
            else
            {
                _typeInfo = TypeInfoProvider.GetTypeInfo(value);
            }            
            if (_typeInfo != null && !_isTypeSet)
            {
                _pgDbType = TypeInfo.PgDbType;
            }
        }

        private int ValueSize()
        {
            if (!ADP.IsNull(_value))
            {
                switch (_value)
                {
                case string stringValue:
                    return stringValue.Length;
                case byte[] byteArrayValue:
                    return byteArrayValue.Length;
                case char[] charArrayValue:
                    return charArrayValue.Length;
                }
            }
            
            return 0;            
        }        
    }
}
