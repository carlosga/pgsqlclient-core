// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;
using System.Data.Common;
using System.Net;
using System.Net.NetworkInformation;

namespace PostgreSql.Data.Frontend
{
    internal sealed class DataRecord
    {
        private RowDescriptor _descriptor;
        private object[]      _values;

        internal int    FieldCount        => _descriptor.Count;
        internal object this[int i]       => GetValue(i);
        internal object this[string name] => GetValue(name);

        internal DataRecord()
        {
        }

        internal DataRecord(RowDescriptor descriptor, object[] values)
        {
            _descriptor = descriptor;
            _values     = values;
        }

        internal bool ReadFrom(Statement statement)
        {
            _descriptor = statement.RowDescriptor;
            _values     = statement.FetchRow();

            return (_values != null);
        }

        internal void Clear()
        {
            _descriptor = null;
            _values     = null;
        }

        internal int GetOrdinal(string name) => _descriptor.IndexOf(name);

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

        internal Type GetProviderSpecificFieldType(int i)
        {
            CheckIndex(i);

            return _descriptor[i].TypeInfo.PgType;
        }

        internal object GetProviderSpecificValue(int i)            => GetValue(i);
        internal int    GetProviderSpecificValues(object[] values) => GetValues(values);

        internal bool IsDBNull(int i)
        {
            CheckIndex(i);

            return ADP.IsNull(_values[i]);
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

        internal bool            GetBoolean(int i)        => GetFieldValue<bool>(i);
        internal byte            GetByte(int i)           => GetFieldValue<byte>(i);
        internal char            GetChar(int i)           => GetFieldValue<char>(i);
        internal DateTime        GetDateTime(int i)       => GetFieldValue<DateTime>(i);
        internal DateTimeOffset  GetDateTimeOffset(int i) => GetFieldValue<DateTimeOffset>(i);
        internal decimal         GetDecimal(int i)        => GetFieldValue<decimal>(i);
        internal double          GetDouble(int i)         => GetFieldValue<double>(i);
        internal float           GetFloat(int i)          => GetFieldValue<float>(i);
        internal Guid            GetGuid(int i)           => GetFieldValue<Guid>(i);
        internal IPAddress       GetIPAddress(int i)      => GetFieldValue<IPAddress>(i);
        internal short           GetInt16(int i)          => GetFieldValue<short>(i);
        internal int             GetInt32(int i)          => GetFieldValue<int>(i);
        internal long            GetInt64(int i)          => GetFieldValue<long>(i);
        internal PhysicalAddress GetMacAddress(int i)     => GetFieldValue<PhysicalAddress>(i); 
        internal string          GetString(int i)         => GetFieldValue<string>(i);
        internal TimeSpan        GetTimeSpan(int i)       => GetFieldValue<TimeSpan>(i);
        internal object          GetValue(string name)    => GetValue(GetOrdinal(name));

        internal object GetValue(int i)
        {
            CheckIndex(i);

            return _values[i];
        }

        internal T GetFieldValue<T>(int i)
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
                throw ADP.InvalidRead();
            }
        }
    }
}
