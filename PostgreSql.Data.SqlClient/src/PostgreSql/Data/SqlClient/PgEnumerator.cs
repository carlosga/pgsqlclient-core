// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgEnumerator 
        : IEnumerator
    {
        private readonly PgDataReader _reader;
        private readonly bool         _closeReader;
        
        private DbDataRecord _current;

        public object Current => _current;

        internal PgEnumerator(PgDataReader reader, bool closeReader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException("reader");
            }
            _reader      = reader;
            _closeReader = closeReader;
        }

        public bool MoveNext()
        {
            _current = null;

            if (_reader.Read())
            {
                _current = _reader.GetDataRecord();
                return true;
            }
            
            if (_closeReader)
            {
                _reader.Dispose();
            }
            
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }
}
