// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data.Common;
using System.Net;
using System.Net.NetworkInformation;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgDataRecord
        : DbDataRecord
    {
        private readonly DataRow _dataRow;

        public override int    FieldCount        => _dataRow.FieldCount;
        public override object this[int i]       => _dataRow.GetValue(i);
        public override object this[string name] => _dataRow.GetValue(name);

        internal PgDataRecord(DataRow record)
        {
            _dataRow = record;
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            return _dataRow.GetBytes(i, dataIndex, buffer, bufferIndex, length);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            return _dataRow.GetChars(i, dataIndex, buffer, bufferIndex, length);
        }

        public override bool            GetBoolean(int i)          => _dataRow.GetBoolean(i);
        public override byte            GetByte(int i)             => _dataRow.GetByte(i);
        public override char            GetChar(int i)             => _dataRow.GetChar(i);
        public override string          GetDataTypeName(int i)     => _dataRow.GetDataTypeName(i);
        public override DateTime        GetDateTime(int i)         => _dataRow.GetDateTime(i);
        public override decimal         GetDecimal(int i)          => _dataRow.GetDecimal(i);
        public override double          GetDouble(int i)           => _dataRow.GetDouble(i);
        public override Type            GetFieldType(int i)        => _dataRow.GetFieldType(i);
        public override float           GetFloat(int i)            => _dataRow.GetFloat(i);
        public override Guid            GetGuid(int i)             => _dataRow.GetGuid(i);
        public          IPAddress       GetIPAddress(int i)        => _dataRow.GetIPAddress(i);
        public override short           GetInt16(int i)            => _dataRow.GetInt16(i);
        public override int             GetInt32(int i)            => _dataRow.GetInt32(i);
        public override long            GetInt64(int i)            => _dataRow.GetInt64(i);
        internal        PhysicalAddress GetMacAddress(int i)       => _dataRow.GetMacAddress(i);
        public override string          GetName(int i)             => _dataRow.GetName(i);
        public override int             GetOrdinal(string name)    => _dataRow.GetOrdinal(name);
        public override string          GetString(int i)           => _dataRow.GetString(i);
        public override object          GetValue(int i)            => _dataRow.GetValue(i);
        public override int             GetValues(object[] values) => _dataRow.GetValues(values);
        public override bool            IsDBNull(int i)            => _dataRow.IsDBNull(i);

        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }
    }
}
