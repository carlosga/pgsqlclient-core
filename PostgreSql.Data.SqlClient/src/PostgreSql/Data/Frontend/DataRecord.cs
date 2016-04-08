// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed class DataRecord
    {
        private readonly RowDescriptor _descriptor;
        private readonly object[]      _values;

        internal int    FieldCount        => _descriptor.Count;
        internal object this[int i]       => GetValue(i);
        internal object this[string name] => GetValue(name);

        internal DataRecord(RowDescriptor descriptor, object[] values)
        {
            _descriptor = descriptor;
            _values     = values;
        }

        internal int GetOrdinal(string name)
        {
            return _descriptor.IndexOf(name);
        }

        internal string GetName(int i)
        {
            CheckIndex(i);

            return _descriptor[i].Name;
        }

        internal Type GetFieldType(int i)
        {
            CheckIndex(i);

            return _descriptor[i].TypeInfo.SystemType;
        }

        internal string GetDataTypeName(int i)
        {
            CheckIndex(i);

            return _descriptor[i].TypeInfo.Name;
        }

        internal Type   GetProviderSpecificFieldType(int i)        => GetFieldType(i);
        internal object GetProviderSpecificValue(int i)            => GetValue(i);
        internal int    GetProviderSpecificValues(object[] values) => GetValues(values);

        internal bool IsDBNull(int i)
        {
            CheckIndex(i);

            return (_values[i] == DBNull.Value);
        }

        internal long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }

            int bytesRead  = 0;
            int realLength = length;

            if (buffer == null)
            {
                return ((byte[])_values[i]).Length;
            }

            if ((bufferIndex + length) > buffer.Length)
            {
                throw new IndexOutOfRangeException("The index passed was outside the range of {bufferIndex} through {length}.");
            }

            byte[] byteArray = (byte[])_values[i];

            if (length > (byteArray.Length - dataIndex))
            {
                realLength = byteArray.Length - (int)dataIndex;
            }

            Buffer.BlockCopy(byteArray, (int)dataIndex, buffer, bufferIndex, realLength);

            if ((byteArray.Length - dataIndex) < length)
            {
                bytesRead = byteArray.Length - (int)dataIndex;
            }
            else
            {
                bytesRead = length;
            }

            return bytesRead;
        }

        internal long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }

            if (buffer == null)
            {
                return ((string)_values[i]).Length;
            }

            if ((bufferIndex + length) > buffer.Length)
            {
                throw new IndexOutOfRangeException("The index passed was outside the range of {bufferIndex} through {length}.");
            }

            int    charsRead  = 0;
            int    realLength = length;
            char[] charArray  = ((string)_values[i]).ToCharArray();

            if (length > (charArray.Length - dataIndex))
            {
                realLength = charArray.Length - (int)dataIndex;
            }

            Array.Copy(charArray, (int)dataIndex, buffer, bufferIndex, realLength);

            if ((charArray.Length - dataIndex) < length)
            {
                charsRead = charArray.Length - (int)dataIndex;
            }
            else
            {
                charsRead = length;
            }

            return charsRead;
        }

        internal bool           GetBoolean(int i)        => GetValue<bool>(i);
        internal byte           GetByte(int i)           => GetValue<byte>(i);
        internal char           GetChar(int i)           => GetValue<char>(i);
        internal DateTimeOffset GetDateTimeOffset(int i) => GetValue<DateTimeOffset>(i);
        internal TimeSpan       GetTimeSpan(int i)       => GetValue<TimeSpan>(i);
        internal decimal        GetDecimal(int i)        => GetValue<decimal>(i);
        internal double         GetDouble(int i)         => GetValue<double>(i);
        internal float          GetFloat(int i)          => GetValue<float>(i);
        internal short          GetInt16(int i)          => GetValue<short>(i);
        internal int            GetInt32(int i)          => GetValue<int>(i);
        internal long           GetInt64(int i)          => GetValue<long>(i);
        internal string         GetString(int i)         => GetValue<string>(i);

        internal DateTime GetDateTime(int i)
        {
            ThrowIfNull(i);

            if (_values[i] is PgDate)
            {
                return ((PgDate)_values[i]).ToDateTime();
            }
            else if (_values[i] is PgTimestamp)
            {
                return ((PgTimestamp)_values[i]).Value;
            }
            return (DateTime)_values[i];
        }

        internal PgBinary    GetPgBinary(int i)    => GetValue<PgBinary>(i);
        internal PgBit       GetPgBit(int i)       => GetValue<PgBit>(i);
        internal PgBoolean   GetPgBoolean(int i)   => GetValue<PgBoolean>(i);
        internal PgBox       GetPgBox(int i)       => GetValue<PgBox>(i);
        internal PgBox2D     GetPgBox2D(int i)     => GetValue<PgBox2D>(i);
        internal PgBox3D     GetPgBox3D(int i)     => GetValue<PgBox3D>(i);
        internal PgByte      GetPgByte(int i)      => GetValue<PgByte>(i);
        internal PgCircle    GetPgCircle(int i)    => GetValue<PgCircle>(i);
        internal PgDate      GetPgDate(int i)      => GetValue<PgDate>(i);
        internal PgTimestamp GetPgTimestamp(int i) => GetValue<PgTimestamp>(i);
        internal PgInterval  GetPgInterval(int i)  => GetValue<PgInterval>(i);
        internal PgDecimal   GetPgNumeric(int i)   => GetValue<PgDecimal>(i);
        internal PgMoney     GetPgMoney(int i)     => GetValue<PgMoney>(i);
        internal PgReal      GetPgReal(int i)      => GetValue<PgReal>(i);
        internal PgDouble    GetPgDouble(int i)    => GetValue<PgDouble>(i);
        internal PgInt16     GetPgInt16(int i)     => GetValue<PgInt16>(i);
        internal PgInt32     GetPgInt32(int i)     => GetValue<PgInt32>(i);
        internal PgInt64     GetPgInt64(int i)     => GetValue<PgInt64>(i);
        internal PgLine      GetPgLine(int i)      => GetValue<PgLine>(i);
        internal PgLSeg      GetPgLSeg(int i)      => GetValue<PgLSeg>(i);
        internal PgPath      GetPgPath(int i)      => GetValue<PgPath>(i);
        internal PgPoint     GetPgPoint(int i)     => GetValue<PgPoint>(i);
        internal PgPoint2D   GetPgPoint2D(int i)   => GetValue<PgPoint2D>(i);
        internal PgPoint3D   GetPgPoint3D(int i)   => GetValue<PgPoint3D>(i);
        internal PgPolygon   GetPgPolygon(int i)   => GetValue<PgPolygon>(i);
        internal PgString    GetPgString(int i)    => GetValue<PgString>(i);

        internal object GetValue(string name)
        {
            return GetValue(GetOrdinal(name));
        }

        internal object GetValue(int i)
        {
            CheckIndex(i);

            return _values[i];
        }

        internal T GetValue<T>(int i)
        {
            ThrowIfNull(i);

            return (T)_values[i];
        }

        internal int GetValues(object[] values)
        {
            Array.Copy(_values, values, ((values.Length > FieldCount) ? FieldCount : values.Length));

            return values.Length;
        }

        private void ThrowIfNull(int i)
        {
            CheckIndex(i);

            if (IsDBNull(i))
            {
                throw new PgNullValueException("Data is Null. This method or property cannot be called on Null values.");
            }
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= FieldCount)
            {
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
            }
        }
    }
}
