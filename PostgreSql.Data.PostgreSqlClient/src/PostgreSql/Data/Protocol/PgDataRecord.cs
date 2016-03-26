// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Protocol;
using System;
using System.Data.Common;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgDataRecord 
        : DbDataRecord
    {    
        private PgRowDescriptor _descriptor;
        private object[]        _values;
        
        public override int    FieldCount        => _descriptor.Count;
        public override object this[int i]       => GetValue(i);
        public override object this[string name] => GetValue(name);

        internal PgDataRecord(PgRowDescriptor descriptor, object[] values)
        {
            _descriptor = descriptor;
            _values     = values;
        }

        public override bool GetBoolean(int i)
        {
            CheckNull(i);
            
            return Convert.ToBoolean(_values[i]);
         }

        public override byte GetByte(int i)
        {
            CheckNull(i);

            return Convert.ToByte(GetValue(i));            
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }           
            
            int bytesRead  = 0;
            int realLength = length;

            if (buffer == null)
            {
                byte[] data = (byte[])GetValue(i);

                return data.Length;
            }

            byte[] byteArray = (byte[])GetValue(i);

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
            CheckNull(i);

            return Convert.ToChar(GetValue(i));            
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            if (IsDBNull(i))
            {
                return 0;
            }

            if (buffer == null)
            {
                char[] data = ((string)GetValue(i)).ToCharArray();

                return data.Length;
            }

            int charsRead  = 0;
            int realLength = length;

            char[] charArray = ((string)GetValue(i)).ToCharArray();

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

            return _descriptor[i].Type.Name;            
        }

        public override DateTime GetDateTime(int i)
        {
            CheckNull(i);

            return Convert.ToDateTime(GetValue(i));            
        }

        public override Decimal GetDecimal(int i)
        {
            CheckNull(i);

            return Convert.ToDecimal(GetValue(i));            
        }

        public override Double GetDouble(int i)
        {
            CheckNull(i);

            return Convert.ToDouble(GetValue(i));            
        }

        public override Type GetFieldType(int i)
        {
            CheckNull(i);

            return _descriptor[i].Type.SystemType;            
        }

        public override Single GetFloat(int i)
        {
            CheckNull(i);

            return Convert.ToSingle(GetValue(i));            
        }

        public override Guid GetGuid(int i)
        {            
            throw new NotSupportedException("Guid datatype is not supported");            
        }

        public override Int16 GetInt16(int i)
        {
            CheckNull(i);

            return Convert.ToInt16(GetValue(i));            
        }

        public override Int32 GetInt32(int i)
        {
            CheckNull(i);

            return Convert.ToInt32(GetValue(i));            
        }

        public override Int64 GetInt64(int i)
        {
            CheckNull(i);

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
            CheckNull(i);

            return Convert.ToString(GetValue(i));            
        }

        public override object GetValue(int i)
        {
            CheckIndex(i);

            return _values[i];            
        }
        
        public object GetValue(string name)
        {
            return GetValue(GetOrdinal(name));
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

        public TimeSpan   GetTimeSpan(int i)   => GetPgTimeSpan(i).Value;
        public PgTimeSpan GetPgTimeSpan(int i) => GetProviderSpecificValue<PgTimeSpan>(i);
        public PgPoint    GetPgPoint(int i)    => GetProviderSpecificValue<PgPoint>(i);
        public PgBox      GetPgBox(int i)      => GetProviderSpecificValue<PgBox>(i);
        public PgLSeg     GetPgLSeg(int i)     => GetProviderSpecificValue<PgLSeg>(i);
        public PgCircle   GetPgCircle(int i)   => GetProviderSpecificValue<PgCircle>(i);
        public PgPath     GetPgPath(int i)     => GetProviderSpecificValue<PgPath>(i);
        public PgPolygon  GetPgPolygon(int i)  => GetProviderSpecificValue<PgPolygon>(i);
        public PgBox2D    GetPgBox2D(int i)    => GetProviderSpecificValue<PgBox2D>(i);
        
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
        
        private void CheckNull(int i)
        {
            if (IsDBNull(i))
            {
                throw new PgNullValueException("Data is Null. This method or property cannot be called on Null values.");
            }
        }

        private object GetValueWithNullCheck(int i)
        {
            CheckNull(i);
            
            return _values[i];
        }

        private T GetProviderSpecificValue<T>(int i)
        {
            CheckNull(i);

            return (T)_values[i];
        }                                
    }    
}
