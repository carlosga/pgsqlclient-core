// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgTypeCollection
        : IList<PgType>, ICollection<PgType>, IReadOnlyList<PgType>, IReadOnlyCollection<PgType>, IEnumerable<PgType>
    {
        private readonly List<PgType> _innerList;

        public int    Count             => _innerList.Count;
        public PgType this[int index]   => _innerList[index];
        public PgType this[string name] => _innerList[IndexOf(name)];

        int ICollection<PgType>.Count       => _innerList.Count;
        bool ICollection<PgType>.IsReadOnly => ((IList)_innerList).IsReadOnly;

        PgType IList<PgType>.this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        internal PgTypeCollection(int capacity)
        {
            _innerList = new List<PgType>(capacity);
        }

        public bool Contains(int oid) => (IndexOf(oid) != -1);

        public int IndexOf(int oid)
        {
            for (int i = 0; i < _innerList.Count; ++i)
            {
                if (_innerList[i].Oid == oid)
                {
                    return i;
                }
            }

            return -1;
        }

        public int IndexOf(string name)
        {
            for (int i = 0; i < _innerList.Count; ++i)
            {
                if (_innerList[i].Name.CaseInsensitiveCompare(name))
                {
                    return i;
                }
            }

            return -1;
        }

        public void RemoveAt(string name) => _innerList.RemoveAt(IndexOf(name));

        public void Add(int          oid
                      , string       name
                      , PgDbType     dataType
                      , int          elementType
                      , PgTypeFormat formatCode
                      , int          size)                      
            => _innerList.Add(new PgType(oid, name, dataType, elementType, formatCode, size));

        public void Add(int          oid
                      , string       name
                      , PgDbType     dataType
                      , int          elementType
                      , PgTypeFormat formatCode
                      , int          size
                      , string       delimiter)
            => _innerList.Add(new PgType(oid, name, dataType, elementType, formatCode, size, delimiter));

        public void Add(int          oid
                      , string       name
                      , PgDbType     dataType
                      , int          elementType
                      , PgTypeFormat formatCode
                      , int          size
                      , string       delimiter
                      , string       prefix)
            => _innerList.Add(new PgType(oid, name, dataType, elementType, formatCode, size, delimiter, prefix));

        int  IList<PgType>.IndexOf(PgType item)           => _innerList.IndexOf(item);
        void IList<PgType>.Insert(int index, PgType item) => _innerList.Insert(index, item);
        void IList<PgType>.RemoveAt(int index)            => _innerList.RemoveAt(index);
        void ICollection<PgType>.Add(PgType item)         => _innerList.Add(item);
        void ICollection<PgType>.Clear()                  => _innerList.Clear();
        bool ICollection<PgType>.Contains(PgType item)    => _innerList.Contains(item);
        bool ICollection<PgType>.Remove(PgType item)      => _innerList.Remove(item);
        
        void ICollection<PgType>.CopyTo(PgType[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);
        
        IEnumerator<PgType> IEnumerable<PgType>.GetEnumerator() => _innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()                 => _innerList.GetEnumerator();
    }
}
