// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgEnumerator 
        : IEnumerator
    {
        private PgDataReader _reader;
        private PgDataRecord _current;
        private bool         _closeReader;

        public object Current
        {
            get { return _current;  }
        }

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