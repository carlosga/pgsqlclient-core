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

        public int Count
        {
            get { return _innerList.Count; }
        }

        object ICollection.SyncRoot
        {
            get { return (_innerList as ICollection).SyncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return (_innerList as ICollection).IsSynchronized; }
        }

        internal PgErrorCollection()
        {
            _innerList = new List<PgError>();
        }

        public IEnumerator GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        internal void Add(string severity, string message, string code)
        {
            Add(new PgError(severity, code, message));
        }

        internal void Add(PgError error)
        {
            _innerList.Add(error);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            (_innerList as ICollection).CopyTo(array, index);
        }
    }
}