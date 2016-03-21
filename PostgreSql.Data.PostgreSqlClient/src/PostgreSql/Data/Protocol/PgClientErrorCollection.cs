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

        internal void Add(string severity, string message, string code)
        {
            _innerList.Add(new PgClientError(severity, code, message));
        }

        internal void Add(PgClientError error) => _innerList.Add(error);

        int  ICollection<PgClientError>.Count                        => _innerList.Count;
        bool ICollection<PgClientError>.Contains(PgClientError item) => _innerList.Contains(item);
        void ICollection<PgClientError>.Clear()                      => _innerList.Clear();
        bool ICollection<PgClientError>.IsReadOnly                   => false;
        void ICollection<PgClientError>.Add(PgClientError item)      => _innerList.Add(item);
        bool ICollection<PgClientError>.Remove(PgClientError item)   => _innerList.Remove(item);

        void ICollection<PgClientError>.CopyTo(PgClientError[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }
        
        IEnumerator<PgClientError> IEnumerable<PgClientError>.GetEnumerator() => _innerList.GetEnumerator();
        IEnumerator                IEnumerable.GetEnumerator()                => _innerList.GetEnumerator();
    }
}