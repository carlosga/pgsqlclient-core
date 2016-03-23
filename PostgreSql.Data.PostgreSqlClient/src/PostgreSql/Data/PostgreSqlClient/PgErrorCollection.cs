// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgErrorCollection
        : ICollection
    {
        private List<PgError> _innerList;

        public PgError this[int errorIndex]
        {
            get { return _innerList[errorIndex]; }
            set { _innerList[errorIndex] = value; }
        }

        public int Count => _innerList.Count;

        object ICollection.SyncRoot       => (_innerList as ICollection).SyncRoot;
        bool   ICollection.IsSynchronized => (_innerList as ICollection).IsSynchronized;

        internal PgErrorCollection()
        {
            _innerList = new List<PgError>();
        }

        public IEnumerator GetEnumerator()      => _innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _innerList.GetEnumerator();        

        internal void Add(string severity, string message, string code) => Add(new PgError(severity, code, message));
        internal void Add(PgError error)                                => _innerList.Add(error);
        
        public void CopyTo(Array array, int index) => (_innerList as ICollection).CopyTo(array, index);
    }
}
