// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgErrorCollection
        : ICollection, IEnumerable, IEnumerable<PgError>
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

        IEnumerator          IEnumerable.GetEnumerator()          => _innerList.GetEnumerator();
        IEnumerator<PgError> IEnumerable<PgError>.GetEnumerator() => _innerList.GetEnumerator();

        internal void Add(string severity, string message, string code) => Add(new PgError(severity, code, message));
        internal void Add(PgError error)                                => _innerList.Add(error);

        internal void AddRange(List<PgError> errors)
        {
            if (errors != null && errors.Count > 0)
            {
                _innerList.AddRange(errors);
            }
        }

        public void CopyTo(Array array, int index) => (_innerList as ICollection).CopyTo(array, index);
    }
}
