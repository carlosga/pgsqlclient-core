// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System.Data.Common;
using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgDbColumn
        : DbColumn
    {
        public bool IsArray
        {
            get;
            private set;
        }

        public bool IsRefCursor
        {
            get;
            private set;
        }

        public bool IsRowVersion
        {
            get;
            private set;
        }

        public override object this[string property]
        {
            get
            {
                switch (property)
                {
                case nameof(IsArray):
                    return IsArray;

                case nameof(IsRefCursor):
                    return IsRefCursor;

                case nameof(IsRowVersion):
                    return IsRowVersion;

                default:
                    return base[property];
                }
            }
        }

        internal PgDbColumn(PgFieldDescriptor descriptor)
        {
            ColumnName       = descriptor.Name;
            ColumnOrdinal    = descriptor.ColumnId;
            ColumnSize       = descriptor.TypeInfo.Size;
            DataType         = descriptor.TypeInfo.SystemType;
            DataTypeName     = descriptor.TypeInfo.Name;
            IsArray          = descriptor.TypeInfo.IsArray;
            IsExpression     = descriptor.IsExpression;
            IsLong           = descriptor.TypeInfo.IsBinary;
            IsRefCursor      = descriptor.TypeInfo.IsRefCursor;
            NumericPrecision = descriptor.NumericPrecision;
            NumericScale     = descriptor.NumericScale;
        }

        internal void Populate(PgDataReader reader)
        {
            if (!reader.IsDBNull(0))
            {
                BaseSchemaName = reader.GetString(0);   
            }
            if (!reader.IsDBNull(1))
            {
                BaseTableName = reader.GetString(1);
            }
            if (!reader.IsDBNull(2))
            {
                BaseColumnName = reader.GetString(2);
            }
            if (!reader.IsDBNull(3))
            {
                AllowDBNull = !reader.GetBoolean(3);
            }            
            if (!reader.IsDBNull(4))
            {
                IsAutoIncrement = reader.GetBoolean(4);
                IsReadOnly	    = reader.GetBoolean(4);
            }
            if (!reader.IsDBNull(5))
            {            
                IsKey = reader.GetBoolean(5);
            }
            if (!reader.IsDBNull(6))
            {            
                IsUnique = reader.GetBoolean(6);
            }

            IsAliased = !ColumnName.CaseInsensitiveCompare(BaseColumnName);
        }
    }
}
