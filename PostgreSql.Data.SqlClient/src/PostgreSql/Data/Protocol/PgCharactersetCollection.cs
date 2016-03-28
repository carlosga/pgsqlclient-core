// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgCharactersetCollection
        : ICollection<PgCharacterSet>
    {
        private readonly List<PgCharacterSet> _innerList;

        int  ICollection<PgCharacterSet>.Count      => _innerList.Count;
        bool ICollection<PgCharacterSet>.IsReadOnly => false;

        public PgCharacterSet this[string name] => _innerList[IndexOf(name)];

        internal PgCharactersetCollection(int capacity)
        {
            _innerList = new List<PgCharacterSet>(capacity);
        }

        internal int IndexOf(string characterset)
        {
            for (int i = 0; i < _innerList.Count; ++i)
            {
                if (_innerList[i].Name.CaseInsensitiveCompare(characterset))
                {
                    return i;
                }
            }

            return -1;
        }

        internal void Add(string charset, string systemCharset) 
            => _innerList.Add(new PgCharacterSet(charset, systemCharset));

        internal void Add(string charset, int cp) => _innerList.Add(new PgCharacterSet(charset, cp));
        
        internal void Add(string charset, Encoding encoding) => _innerList.Add(new PgCharacterSet(charset, encoding));

        void ICollection<PgCharacterSet>.Add(PgCharacterSet item) => _innerList.Add(item);

        void ICollection<PgCharacterSet>.Clear() => _innerList.Clear();

        bool ICollection<PgCharacterSet>.Contains(PgCharacterSet item) => _innerList.Contains(item);

        void ICollection<PgCharacterSet>.CopyTo(PgCharacterSet[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        bool ICollection<PgCharacterSet>.Remove(PgCharacterSet item) => _innerList.Remove(item);

        IEnumerator<PgCharacterSet> IEnumerable<PgCharacterSet>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();
    }
}
