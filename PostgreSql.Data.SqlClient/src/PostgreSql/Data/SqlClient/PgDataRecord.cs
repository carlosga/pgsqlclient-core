// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgDataRecord
        : DbDataRecord
    {
        private PgRowDescriptor _descriptor;
        private object[]        _values;

        public override int    FieldCount        => _descriptor.Count;
        public override object this[int i]       => GetValue(i);
        public override object this[string name] => GetValue(GetOrdinal(name));

        internal PgDataRecord(PgRowDescriptor descriptor, object[] values)
        {
            _descriptor = descriptor;
            _values     = values;
        }

        public override bool GetBoolean(int i)
        {
            CheckIndex(i);

            return Convert.ToBoolean(GetValue(i));
         }

        public override byte GetByte(int i)
        {
            CheckIndex(i);

            return Convert.ToByte(GetValue(i));
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            CheckIndex(i);

            if (IsDBNull(i))
            {
                return 0;
            }

            int bytesRead  = 0;
            int realLength = length;

            if (buffer == null)
            {
                byte[] data = GetValue(i) as byte[];

                return data.Length;
            }

            byte[] byteArray = GetValue(i) as byte[];

            if (length > (byteArray.Length - dataIndex))
            {
                realLength = byteArray.Length - (int)dataIndex;
            }

            Array.Copy(byteArray, (int)dataIndex, buffer, bufferIndex, realLength);

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

        public override char GetChar(int i)
        {
            CheckIndex(i);

            return Convert.ToChar(GetValue(i));
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            CheckIndex(i);

            if (IsDBNull(i))
            {
                return 0;
            }

            if (buffer == null)
            {
                char[] data = (GetValue(i) as string).ToCharArray();

                return data.Length;
            }

            int charsRead  = 0;
            int realLength = length;

            char[] charArray = (GetValue(i) as string).ToCharArray();

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

        public override string GetDataTypeName(int i)
        {
            CheckIndex(i);

            return _descriptor[i].TypeInfo.Name;
        }

        public override DateTime GetDateTime(int i)
        {
            CheckIndex(i);

            return Convert.ToDateTime(GetValue(i));
        }

        public override Decimal GetDecimal(int i)
        {
            CheckIndex(i);

            return Convert.ToDecimal(GetValue(i));
        }

        public override Double GetDouble(int i)
        {
            CheckIndex(i);

            return Convert.ToDouble(GetValue(i));
        }

        public override Type GetFieldType(int i)
        {
            CheckIndex(i);

            return _descriptor[i].TypeInfo.SystemType;
        }

        public override Single GetFloat(int i)
        {
            CheckIndex(i);

            return Convert.ToSingle(GetValue(i));
        }

        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException("Guid datatype is not supported");
        }

        public override Int16 GetInt16(int i)
        {
            CheckIndex(i);

            return Convert.ToInt16(GetValue(i));
        }

        public override Int32 GetInt32(int i)
        {
            CheckIndex(i);

            return Convert.ToInt32(GetValue(i));
        }

        public override Int64 GetInt64(int i)
        {
            CheckIndex(i);

            return Convert.ToInt64(GetValue(i));
        }

        public override string GetName(int i)
        {
            CheckIndex(i);

            return _descriptor[i].Name;
        }

        public override int GetOrdinal(string name)
        {
            return _descriptor.IndexOf(name);
        }

        public override string GetString(int i)
        {
            CheckIndex(i);

            return Convert.ToString(GetValue(i));
        }

        public override object GetValue(int i)
        {
            CheckIndex(i);

            return _values[i];
        }

        public override int GetValues(object[] values)
        {
            Array.Copy(_values, values, FieldCount);

            return values.Length;
        }

        public override bool IsDBNull(int i)
        {
            CheckIndex(i);

            return (_values[i] == DBNull.Value);
        }

        protected override DbDataReader GetDbDataReader(int i)
        {
            // NOTE: This method is virtual because we're required to implement
            //       it however most providers won't support it. Only the OLE DB
            //       provider supports it right now, and they can override it.
            throw new NotSupportedException();
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= FieldCount)
            {
                throw new IndexOutOfRangeException("Could not find specified column in results.");
            }
        }
    }
}
