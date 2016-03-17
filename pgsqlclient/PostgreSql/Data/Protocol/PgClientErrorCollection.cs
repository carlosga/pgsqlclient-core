// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System.Collections;
using System.Collections.Generic;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientErrorCollection
        : ICollection<PgClientError>, IEnumerable<PgClientError>, IEnumerable
    {
        private readonly List<PgClientError> _innerList;

        internal PgClientErrorCollection()
        {
            _innerList = new List<PgClientError>();
        }

        internal void Add(PgClientError error)
        {
            _innerList.Add(error);
        }

        internal void Add(string severity, string message, string code)
        {
            _innerList.Add(new PgClientError(severity, code, message));
        }

        int ICollection<PgClientError>.Count
        {
            get { return _innerList.Count; }
        }

        bool ICollection<PgClientError>.IsReadOnly
        {
            get { return false; }
        }

        void ICollection<PgClientError>.Add(PgClientError item)
        {
            _innerList.Add(item);
        }

        void ICollection<PgClientError>.Clear()
        {
            _innerList.Clear();
        }

        bool ICollection<PgClientError>.Contains(PgClientError item)
        {
            return _innerList.Contains(item);
        }

        void ICollection<PgClientError>.CopyTo(PgClientError[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        bool ICollection<PgClientError>.Remove(PgClientError item)
        {
            return _innerList.Remove(item);
        }

        IEnumerator<PgClientError> IEnumerable<PgClientError>.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }
    }
}