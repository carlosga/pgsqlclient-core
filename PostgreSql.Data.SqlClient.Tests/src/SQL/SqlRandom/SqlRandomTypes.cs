// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Data.Common;
using System.Diagnostics;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    internal sealed class SqlVarCharTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypePrefix      = "varchar";
        private const int    DefaultCharSize = 1;

        internal SqlVarCharTypeInfo()
            : base(PgDbType.VarChar)
        {
        }

        private int GetCharSize(SqlRandomTableColumn columnInfo)
        {
            ValidateColumnInfo(columnInfo);

            return columnInfo.StorageSize.HasValue ? columnInfo.StorageSize.Value : DefaultCharSize;
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return GetCharSize(columnInfo);
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return string.Format("{0}({1})", TypePrefix, GetCharSize(columnInfo));
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            int size = rand.NextAllocationSizeBytes(1);
            return new SqlRandomTableColumn(this, options, size);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            int size = columnInfo.StorageSize.HasValue ? columnInfo.StorageSize.Value : DefaultCharSize;
            return rand.NextAnsiArray(0, size);
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: false);
        }
    }

    internal sealed class SqlBigIntTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "bigint";
        private const int    StorageSize  = 8;

        public SqlBigIntTypeInfo()
            : base(PgDbType.Int8)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return StorageSize;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextBigInt();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(Int64), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetInt64(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Int64>(expected, actual);
        }
    }

    internal sealed class SqlIntTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "int";
        private const int    StorageSize  = 4;

        public SqlIntTypeInfo()
            : base(PgDbType.Int4)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return StorageSize;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextIntInclusive();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(int), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetInt32(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Int32>(expected, actual);
        }
    }

    internal sealed class SqlSmallIntTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "smallint";
        private const int    StorageSize  = 2;

        public SqlSmallIntTypeInfo()
            : base(PgDbType.Int2)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return StorageSize;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextSmallInt();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(short), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetInt16(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Int16>(expected, actual);
        }
    }

    internal sealed class SqlTextTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "text";

        public SqlTextTypeInfo()
            : base(PgDbType.Text)
        {
        }

        public override bool CanBeSparseColumn
        {
            get { return false; }
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return LargeDataRowUsage;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextAnsiArray(0, columnInfo.StorageSize);
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: false);
        }
    }

    internal sealed class SqlBinaryTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "bytea";
        
        public SqlBinaryTypeInfo()
            : base(PgDbType.Bytea)
        {
        }

        public override bool CanBeSparseColumn
        {
            get { return false; }
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return LargeDataRowUsage;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextByteArray(0, columnInfo.StorageSize);
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadByteArray(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareByteArray(expected, actual, allowIncomplete: false);
        }
    }

    internal sealed class SqlCharTypeInfo 
        : SqlRandomTypeInfo
    {
        private const int    MaxCharSize     = 8000;
        private const string TypePrefix      = "char";
        private const int    DefaultCharSize = 1;

        public SqlCharTypeInfo()
            : base(PgDbType.Char)
        {
        }

        private int GetCharSize(SqlRandomTableColumn columnInfo)
        {
            ValidateColumnInfo(columnInfo);

            int size = columnInfo.StorageSize.HasValue ? columnInfo.StorageSize.Value : DefaultCharSize;
            if (size < 1 || size > MaxCharSize)
            {
                throw new NotSupportedException("wrong size");   
            }
            return size;
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return GetCharSize(columnInfo);
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return string.Format("{0}({1})", TypePrefix, GetCharSize(columnInfo));
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            int size = rand.NextAllocationSizeBytes(1, MaxCharSize);
            return new SqlRandomTableColumn(this, options, size);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            int size = columnInfo.StorageSize.HasValue ? columnInfo.StorageSize.Value : DefaultCharSize;
            return rand.NextAnsiArray(0, size);
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: true);
        }
    }

    internal sealed class SqlBooleanTypeInfo
        : SqlRandomTypeInfo
    {
        public SqlBooleanTypeInfo()
            : base(PgDbType.Bool)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 0.125; // 8 bits => 1 byte
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "bool";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextBit();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(Boolean), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetBoolean(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Boolean>(expected, actual);
        }
    }

    internal sealed class SqlDecimalTypeInfo 
        : SqlRandomTypeInfo
    {
        private int _defaultPrecision = 18;

        public SqlDecimalTypeInfo()
            : base(PgDbType.Decimal)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            int precision = columnInfo.Precision.HasValue ? columnInfo.Precision.Value : _defaultPrecision;
            if (precision < 1 || precision > 38)
            {
                throw new ArgumentOutOfRangeException("wrong precision");
            }

            if (precision < 10)
            {
                return 5;
            }
            else if (precision < 20)
            {
                return 9;
            }
            else if (precision < 28)
            {
                return 13;
            }
            else
            {
                return 17;
            }
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "decimal";
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return (decimal)Math.Round(rand.NextDouble());
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(decimal), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetDecimal(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<decimal>(expected, actual);
        }
    }

    internal sealed class SqFloatTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "float4";
        private const int    StorageSize = 4;

        public SqFloatTypeInfo()
            : base(PgDbType.Float4)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return StorageSize;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return TypeSqlName;
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDouble(float.MinValue, float.MaxValue);   
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(double), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return reader.GetDouble(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Double>(expected, actual);
        }
    }

    internal sealed class SqlRowVersionTypeInfo 
        : SqlRandomTypeInfo
    {
        public SqlRowVersionTypeInfo()
            : base(PgDbType.Timestamp)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 8;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "oid";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextRowVersion();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadByteArray(reader, ordinal, asType);
        }

        public override bool CanBeSparseColumn
        {
            get
            {
                return false;
            }
        }

        public override bool CanCompareValues(SqlRandomTableColumn columnInfo)
        {
            // completely ignore TIMESTAMP value comparison
            return false;
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            throw new InvalidOperationException("should not be used for timestamp - use CanCompareValues before calling this method");
        }
    }

    internal sealed class SqlDateTypeInfo 
        : SqlRandomTypeInfo
    {
        public SqlDateTypeInfo()
            : base(PgDbType.Date)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 3;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "date";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDate();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadDateTime(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTime>(expected, actual);
        }
    }

    internal sealed class SqlDateTimeTypeInfo 
        : SqlRandomTypeInfo
    {
        public SqlDateTimeTypeInfo()
            : base(PgDbType.Timestamp)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 8;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "timestamp";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDateTime();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadDateTime(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTime>(expected, actual);
        }
    }

    internal sealed class SqlDateTimeOffsetTypeInfo 
        : SqlRandomTypeInfo
    {
        public SqlDateTimeOffsetTypeInfo()
            : base(PgDbType.TimestampTZ)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 10;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "timestamp with time zone";
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDateTimeOffset();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(DateTimeOffset), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return ((PgDataReader)reader).GetDateTimeOffset(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTimeOffset>(expected, actual);
        }
    }

    internal sealed class SqlTimeTypeInfo 
        : SqlRandomTypeInfo
    {
        public SqlTimeTypeInfo()
            : base(PgDbType.Time)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 5;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "time ";
        }

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextTime();
        }

        protected override object ReadInternal(DbDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(TimeSpan), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;   
            }
            return ((PgDataReader)reader).GetTimeSpan(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<TimeSpan>(expected, actual);
        }
    }
}