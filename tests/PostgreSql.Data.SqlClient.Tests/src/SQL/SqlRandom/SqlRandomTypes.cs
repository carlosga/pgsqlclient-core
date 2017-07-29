// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using PostgreSql.Data.PgTypes;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace PostgreSql.Data.SqlClient.Tests
{
    internal sealed class SqlBinaryTypeInfo 
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "bytea";

        public SqlBinaryTypeInfo()
            : base(PgDbType.Bytea)
        {
        }

        public override bool CanBeSparseColumn => false;

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => LargeDataRowUsage;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextByteArray(0, columnInfo.StorageSize);
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadByteArray(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareByteArray(expected, actual, allowIncomplete: false);
        }
    }

    internal sealed class SqlBooleanTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "bool";
        private const double StorageSize = 0.125; // 8 bits => 1 byte

        public SqlBooleanTypeInfo()
            : base(PgDbType.Boolean)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextBit();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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
            return rand.NextUnicodeArray(0, size);
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: true);
        }
    }

    internal sealed class SqlVarCharTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypePrefix      = "character varying";
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
            int storageSize = columnInfo.StorageSize.HasValue ? columnInfo.StorageSize.Value : DefaultCharSize;
            return rand.NextUnicodeArray(0, storageSize);
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: false);
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

        public override bool CanBeSparseColumn => false;
        
        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => LargeDataRowUsage;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextUnicodeArray(0, columnInfo.StorageSize);
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadCharData(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareCharArray(expected, actual, allowIncomplete: false);
        }
    }

    internal sealed class SqlSmallIntTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "smallint";
        private const int    StorageSize = 2;

        public SqlSmallIntTypeInfo()
            : base(PgDbType.SmallInt)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextSmallInt();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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

    internal sealed class SqlSmallIntArrayTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "smallint[]";
        private const int    StorageSize = -1;

        public SqlSmallIntArrayTypeInfo()
            : base(PgDbType.Array)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextSmallIntArray();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(short[]), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return (short[])reader.GetValue(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareSmallIntArray(expected, actual, false);
        }
    }

    internal sealed class SqlIntTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "int";
        private const int    StorageSize = 4;

        public SqlIntTypeInfo()
            : base(PgDbType.Integer)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextIntInclusive();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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

    internal sealed class SqlBigIntTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "bigint";
        private const int    StorageSize = 8;

        public SqlBigIntTypeInfo()
            : base(PgDbType.BigInt)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextBigInt();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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

    internal sealed class SqlDecimalTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName      = "numeric";
        private const int    DefaultPrecision = 18;

        public SqlDecimalTypeInfo()
            : base(PgDbType.Numeric)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            int precision = columnInfo.Precision.HasValue ? columnInfo.Precision.Value : DefaultPrecision;
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

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return (decimal)Math.Round(rand.NextDouble());
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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

    internal sealed class SqlMoneyTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeTSqlName = "money";
        private const int    StorageSize  = 8;

        public SqlMoneyTypeInfo()
            : base(PgDbType.Money)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeTSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextMoney();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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

    internal sealed class SqlFloatTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "real";
        private const int    StorageSize = 4;

        public SqlFloatTypeInfo()
            : base(PgDbType.Real)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextReal();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(float), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetFloat(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<float>(expected, actual);
        }
    }

    internal sealed class SqlDoubleTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "double precision";
        private const int    StorageSize = 8;

        public SqlDoubleTypeInfo()
            : base(PgDbType.Double)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDouble();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
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
            return CompareValues<double>(expected, actual);
        }
    }

    internal sealed class SqlDateTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "date";
        private const int    StorageSize = 4;

        public SqlDateTypeInfo()
            : base(PgDbType.Date)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDate();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadDateTime(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTime>(expected, actual);
        }
    }

    internal sealed class SqlTimeTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "time";
        private const int    StorageSize = 8;

        public SqlTimeTypeInfo()
            : base(PgDbType.Time)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextTime();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(TimeSpan), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetTimeSpan(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<TimeSpan>(expected, actual);
        }
    }

    internal sealed class SqlTimestampTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "timestamp";
        private const int    StorageSize = 8;

        public SqlTimestampTypeInfo()
            : base(PgDbType.Timestamp)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDateTime();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            return ReadDateTime(reader, ordinal, asType);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTime>(expected, actual);
        }
    }

    internal sealed class SqlTimeTzTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "time with time zone";
        private const int    StorageSize = 12;
        
        public SqlTimeTzTypeInfo()
            : base(PgDbType.TimeTZ)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextTimeTZ();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(DateTimeOffset), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetDateTimeOffset(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTimeOffset>(expected, actual);
        }
    }

    internal sealed class SqlTimestampTzTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "timestamp with time zone";
        private const int    StorageSize = 8;

        public SqlTimestampTzTypeInfo()
            : base(PgDbType.TimestampTZ)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextDateTimeOffset();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(DateTimeOffset), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetDateTimeOffset(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<DateTimeOffset>(expected, actual);
        }
    }

    internal sealed class SqlIntervalTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "interval"; 
        private const int    StorageSize = 16;

        public SqlIntervalTypeInfo()
            : base(PgDbType.Interval)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextTime();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgInterval), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgInterval(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<TimeSpan>(expected, actual);
        }
    }

    internal sealed class SqlPointTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "point";
        private const int    StorageSize = 16;

        public SqlPointTypeInfo()
            : base(PgDbType.Point)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextPoint();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgPoint), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgPoint(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgPoint>(expected, actual);
        }
    }

    internal sealed class SqlBoxTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "box";
        private const int    StorageSize = 32;

        public SqlBoxTypeInfo()
            : base(PgDbType.Box)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextBox();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgBox), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgBox(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgBox>(expected, actual);
        }
    }

    internal sealed class SqlCircleTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "circle";
        private const int    StorageSize = 24;

        public SqlCircleTypeInfo()
            : base(PgDbType.Circle)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextCircle();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgCircle), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgCircle(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgCircle>(expected, actual);
        }
    }

    internal sealed class SqlLineTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "line";
        private const int    StorageSize = 32;

        public SqlLineTypeInfo()
            : base(PgDbType.Line)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextLine();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgLine), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgLine(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgLine>(expected, actual);
        }
    }

    internal sealed class SqlLSegTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "lseg";
        private const int    StorageSize = 32;

        public SqlLSegTypeInfo()
            : base(PgDbType.LSeg)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextLSeg();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgLSeg), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgLSeg(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgLSeg>(expected, actual);
        }
    }

    internal sealed class SqlPathTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "path";
        private const int    StorageSize = -1;

        public SqlPathTypeInfo()
            : base(PgDbType.Path)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextOpenPath();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgPath), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgPath(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgPath>(expected, actual);
        }
    }

    internal sealed class SqlPolygonTypeInfo
        : SqlRandomTypeInfo
    {
        private const string TypeSqlName = "polygon";
        private const int    StorageSize = -1;

        public SqlPolygonTypeInfo()
            : base(PgDbType.Polygon)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)         => StorageSize;
        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo) => TypeSqlName;

        public override SqlRandomTableColumn CreateRandomColumn(SqlRandomizer rand, SqlRandomColumnOptions options)
        {
            return CreateDefaultColumn(options);
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextPolygon();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PgPolygon), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetPgPolygon(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PgPolygon>(expected, actual);
        }
    }

    internal sealed class SqlUuidTypeInfo
        : SqlRandomTypeInfo
    {
        public SqlUuidTypeInfo()
            : base(PgDbType.Uuid)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 16;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "uuid";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            // this method does not use Guid.NewGuid since it is not based on the given rand object
            return rand.NextUniqueIdentifier();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(Guid), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetGuid(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<Guid>(expected, actual);
        }
    }

    internal sealed class SqlIPAddressTypeInfo
        : SqlRandomTypeInfo
    {
        public SqlIPAddressTypeInfo()
            : base(PgDbType.Inet)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 16;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "inet";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextIPAddress();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(IPAddress), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetIPAddress(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<IPAddress>(expected, actual);
        }
    }

    internal sealed class SqlMacAddressTypeInfo
        : SqlRandomTypeInfo
    {
        public SqlMacAddressTypeInfo()
            : base(PgDbType.MacAddress)
        {
        }

        protected override double GetInRowSizeInternal(SqlRandomTableColumn columnInfo)
        {
            return 6;
        }

        protected override string GetSqlTypeDefinitionInternal(SqlRandomTableColumn columnInfo)
        {
            return "macaddr";
        }

        protected override object CreateRandomValueInternal(SqlRandomizer rand, SqlRandomTableColumn columnInfo)
        {
            return rand.NextMacAddress();
        }

        protected override object ReadInternal(PgDataReader reader, int ordinal, SqlRandomTableColumn columnInfo, Type asType)
        {
            ValidateReadType(typeof(PhysicalAddress), asType);
            if (reader.IsDBNull(ordinal))
            {
                return DBNull.Value;
            }
            return reader.GetMacAddress(ordinal);
        }

        protected override bool CompareValuesInternal(SqlRandomTableColumn columnInfo, object expected, object actual)
        {
            return CompareValues<PhysicalAddress>(expected, actual);
        }
    }
}
