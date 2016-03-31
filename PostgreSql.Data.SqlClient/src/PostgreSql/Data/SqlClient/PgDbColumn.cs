// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;
using System;
using PostgreSql.Data.Frontend;

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

        internal void Populate(object[] row)
        {
            if (row[0] != null && row[0] != DBNull.Value)
            {
                BaseSchemaName = (string)row[0];
            }
            if (row[1] != null && row[1] != DBNull.Value)
            {
                BaseTableName = (string)row[1];
            }
            if (row[2] != null && row[2] != DBNull.Value)
            {
                BaseColumnName = (string)row[2];
            }
            if (row[3] != null && row[3] != DBNull.Value)
            {
                AllowDBNull = !(bool)row[3];
            }
            if (row[4] != null && row[4] != DBNull.Value)
            {
                IsAutoIncrement = (bool)row[4];
                IsReadOnly	    = IsAutoIncrement;
            }
            if (row[5] != null && row[5] != DBNull.Value)
            {
                IsKey = (bool)row[5];
            }
            if (row[6] != null && row[6] != DBNull.Value)
            {
                IsUnique = (bool)row[6];
            }

            IsAliased = !ColumnName.CaseInsensitiveCompare(BaseColumnName);
        }
    }
}
