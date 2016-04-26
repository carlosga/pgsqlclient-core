// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
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

        internal PgDbColumn(FieldDescriptor descriptor)
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

        internal void Populate(DataRecord row)
        {
            if (!row.IsDBNull(0))
            {
                BaseSchemaName = row.GetString(0);
            }
            if (!row.IsDBNull(1))
            {
                BaseTableName = row.GetString(1);
            }
            if (!row.IsDBNull(2))
            {
                BaseColumnName = row.GetString(2);
            }
            if (!row.IsDBNull(3))
            {
                AllowDBNull = !row.GetBoolean(3);
            }
            if (!row.IsDBNull(4))
            {
                IsAutoIncrement = row.GetBoolean(4);
                IsReadOnly	    = IsAutoIncrement;
            }
            if (!row.IsDBNull(5))
            {
                IsKey = row.GetBoolean(5);
            }
            if (!row.IsDBNull(6))
            {
                IsUnique = row.GetBoolean(6);
            }

            IsAliased = !ColumnName.CaseInsensitiveCompare(BaseColumnName);
        }
    }
}
