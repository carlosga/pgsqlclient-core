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
            get { return _parameterName; }
            set { _parameterName = value; }
        }

        public override byte Precision
        {
            get { return _precision; }
            set { _precision = value; }
        }

        public override byte Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }

        public override int Size
        {
            get { return _size; }
            set { _size = value; }
        }

        public override DbType DbType
        {
            get { return TypeInfoProvider.GetDbType(_pgDbType); }
            set { PgDbType = TypeInfoProvider.GetProviderType(value); }
        }

        public PgDbType PgDbType
        {
            get { return _pgDbType; }
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
            get { return _direction; }
            set { _direction = value; }
        }

        public override bool IsNullable
        {
            get { return _isNullable; }
            set { _isNullable = value; }
        }

        public override string SourceColumn
        {
            get { return _sourceColumn; }
            set { _sourceColumn = value; }
        }

        public override object Value
        {
            get { return _value; }
            set { PgValue = value; }
        }

        public object PgValue
        {
            get { return _pgvalue; }
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
            get { return _sourceColumnNullMapping; }
            set { _sourceColumnNullMapping = value; }
        }

        internal PgParameterCollection Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        internal TypeInfo TypeInfo
        {
            get { return _typeInfo; }
            set { _typeInfo = value; }
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

        public PgParameter(string   parameterName
                         , PgDbType pgDbType
                         , int      size
                         , string   sourceColumn)
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
            if (_typeInfo == null && _pgDbType != PgDbType.Composite)
            {
                throw ADP.InvalidOperation("Unknown value type, set the parameter provider type before assigning its value.");
            }
            if (!_isTypeSet)
            {
                _pgDbType = TypeInfo.PgDbType;
            }
        }
    }
}
