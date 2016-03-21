// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgParameter
    {
        private readonly int _dataTypeOid;

        internal PgType DataType
        {
            get;
            set;
        }

        internal object Value
        {
            get;
            set;
        }

        internal int DataTypeOid => _dataTypeOid;

        internal PgParameter()
        {
        }

        internal PgParameter(int dataTypeOid)
            : this(dataTypeOid, null)
        {
        }

        internal PgParameter(PgType dataType)
        {
            DataType     = dataType;
            Value        = null;
            _dataTypeOid = dataType.Oid;
        }

        internal PgParameter(int dataTypeOid, object value)
        {
            _dataTypeOid = dataTypeOid;
            Value        = value;
        }
    }
}