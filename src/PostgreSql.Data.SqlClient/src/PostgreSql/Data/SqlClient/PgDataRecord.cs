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
        private readonly DataRecord _record;

        public override int    FieldCount        => _record.FieldCount;
        public override object this[int i]       => _record.GetValue(i);
        public override object this[string name] => _record.GetValue(name);

        internal PgDataRecord(DataRecord record)
        {
            _record = record;
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            return _record.GetBytes(i, dataIndex, buffer, bufferIndex, length);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            return _record.GetChars(i, dataIndex, buffer, bufferIndex, length);
        }

        public override bool     GetBoolean(int i)          => _record.GetBoolean(i);
        public override byte     GetByte(int i)             => _record.GetByte(i);
        public override char     GetChar(int i)             => _record.GetChar(i);
        public override string   GetDataTypeName(int i)     => _record.GetDataTypeName(i);
        public override DateTime GetDateTime(int i)         => _record.GetDateTime(i);
        public override decimal  GetDecimal(int i)          => _record.GetDecimal(i);
        public override double   GetDouble(int i)           => _record.GetDouble(i);
        public override Type     GetFieldType(int i)        => _record.GetFieldType(i);
        public override float    GetFloat(int i)            => _record.GetFloat(i);
        public override Guid     GetGuid(int i)             => _record.GetGuid(i);
        public override short    GetInt16(int i)            => _record.GetInt16(i);
        public override int      GetInt32(int i)            => _record.GetInt32(i);
        public override long     GetInt64(int i)            => _record.GetInt64(i);
        public override string   GetName(int i)             => _record.GetName(i);
        public override int      GetOrdinal(string name)    => _record.GetOrdinal(name);
        public override string   GetString(int i)           => _record.GetString(i);
        public override object   GetValue(int i)            => _record.GetValue(i);
        public override int      GetValues(object[] values) => _record.GetValues(values);
        public override bool     IsDBNull(int i)            => _record.IsDBNull(i);

        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }
    }
}
