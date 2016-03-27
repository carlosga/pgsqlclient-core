// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgTypeInfoCollection
        : IList<PgTypeInfo>, ICollection<PgTypeInfo>, IReadOnlyList<PgTypeInfo>, IReadOnlyCollection<PgTypeInfo>, IEnumerable<PgTypeInfo>
    {
        private readonly List<PgTypeInfo> _innerList;

        public int        Count             => _innerList.Count;
        public PgTypeInfo this[int index]   => _innerList[index];
        public PgTypeInfo this[string name] => _innerList[IndexOf(name)];

        int  ICollection<PgTypeInfo>.Count       => _innerList.Count;
        bool ICollection<PgTypeInfo>.IsReadOnly => ((IList)_innerList).IsReadOnly;

        PgTypeInfo IList<PgTypeInfo>.this[int index]
        {
            get { return _innerList[index]; }
            set { _innerList[index] = value; }
        }

        internal PgTypeInfoCollection(int capacity)
        {
            _innerList = new List<PgTypeInfo>(capacity);
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
            => _innerList.Add(new PgTypeInfo(oid, name, dataType, elementType, formatCode, size));

        public void Add(int          oid
                      , string       name
                      , PgDbType     dataType
                      , int          elementType
                      , PgTypeFormat formatCode
                      , int          size
                      , string       delimiter)
            => _innerList.Add(new PgTypeInfo(oid, name, dataType, elementType, formatCode, size, delimiter));

        public void Add(int          oid
                      , string       name
                      , PgDbType     dataType
                      , int          elementType
                      , PgTypeFormat formatCode
                      , int          size
                      , string       delimiter
                      , string       prefix)
            => _innerList.Add(new PgTypeInfo(oid, name, dataType, elementType, formatCode, size, delimiter, prefix));

        int  IList<PgTypeInfo>.IndexOf(PgTypeInfo item)           => _innerList.IndexOf(item);
        void IList<PgTypeInfo>.Insert(int index, PgTypeInfo item) => _innerList.Insert(index, item);
        void IList<PgTypeInfo>.RemoveAt(int index)                => _innerList.RemoveAt(index);
        void ICollection<PgTypeInfo>.Add(PgTypeInfo item)         => _innerList.Add(item);
        void ICollection<PgTypeInfo>.Clear()                      => _innerList.Clear();
        bool ICollection<PgTypeInfo>.Contains(PgTypeInfo item)    => _innerList.Contains(item);
        bool ICollection<PgTypeInfo>.Remove(PgTypeInfo item)      => _innerList.Remove(item);
        
        void ICollection<PgTypeInfo>.CopyTo(PgTypeInfo[] array, int arrayIndex) => _innerList.CopyTo(array, arrayIndex);
        
        IEnumerator<PgTypeInfo> IEnumerable<PgTypeInfo>.GetEnumerator() => _innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()                         => _innerList.GetEnumerator();
    }
}
