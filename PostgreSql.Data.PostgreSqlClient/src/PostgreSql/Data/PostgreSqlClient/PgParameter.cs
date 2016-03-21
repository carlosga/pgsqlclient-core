// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using System.Data.Common;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgParameter
        : DbParameter
    {
        private ParameterDirection    _direction;
        private bool                  _isNullable;
        private bool                  _sourceColumnNullMapping;
        private string                _parameterName;
        private string                _sourceColumn;
        private object                _value;
        private byte                  _precision;
        private byte                  _scale;
        private int                   _size;
        private PgDbType              _providerType;
        private bool                  _isTypeSet;
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
            get { return TypeHelper.ProviderDbTypeToDbType(_providerType); }
            set { PgDbType = TypeHelper.DbTypeToProviderType(value); }
        }

        public PgDbType PgDbType
        {
            get { return _providerType; }
            set
            {
                _providerType = value;
                _isTypeSet    = true;
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
            set
            {
                if (value == null)
                {
                    value = DBNull.Value;
                }

                _value = value;

                if (!_isTypeSet)
                {          
                    _providerType = TypeHelper.GetDbProviderType(value);
                    _isTypeSet    = true;
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

        public PgParameter()
        {
            _direction     = ParameterDirection.Input;
            _isNullable    = false;
            _providerType  = PgDbType.VarChar;
        }

        public PgParameter(string parameterName, object value) : this()
        {
            _parameterName = parameterName;
            _value         = value;
        }

        public PgParameter(string parameterName, PgDbType dbType) : this()
        {
            _parameterName = parameterName;
            _providerType  = dbType;
        }

        public PgParameter(string parameterName, PgDbType dbType, int size) : this()
        {
            _parameterName = parameterName;
            _size          = size;
            PgDbType       = dbType;
        }

        public PgParameter(string   parameterName
                         , PgDbType dbType
                         , int      size
                         , string   sourceColumn)
            : this()
        {
            _parameterName = parameterName;
            _size          = size;
            _sourceColumn  = sourceColumn;
            PgDbType       = dbType;
        }

        public PgParameter(string             parameterName
                         , PgDbType           dbType
                         , int                size
                         , ParameterDirection direction
                         , bool               isNullable
                         , byte               precision
                         , byte               scale
                         , string             sourceColumn
                         , DataRowVersion     sourceVersion
                         , object             value)
        {
            _parameterName  = parameterName;
            _size           = size;
            _direction      = direction;
            _isNullable     = isNullable;
            _precision      = precision;
            _scale          = scale;
            _sourceColumn   = sourceColumn;
            _value          = value;
            PgDbType        = dbType;
        }

        public override string ToString() => _parameterName;

        public override void ResetDbType()
        {
            throw new Exception("The method or operation is not implemented.");
        }
    }
}