// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgRowDescriptor
        : IEnumerable<PgFieldDescriptor>
    {
        private readonly List<PgFieldDescriptor> _fields;

        internal PgFieldDescriptor this[int index]
        {
            get { return _fields[index]; }
        }

        internal int Count
        {
            get { return _fields.Count; }
        }

        internal PgRowDescriptor()
        {
            _fields = new List<PgFieldDescriptor>();
        }

        internal void Add(PgFieldDescriptor descriptor)
        {
            _fields.Add(descriptor);
        }

        internal int IndexOf(string name)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (_fields[i].Name.CaseInsensitiveCompare(name))
                {
                    return i;
                }
            }

            return -1;            
        }

        internal void Resize(int count)
        {
            _fields.Clear();
            _fields.Capacity = count;
        }

        internal void Clear()
        {
            _fields.Clear();
        }

        IEnumerator<PgFieldDescriptor> IEnumerable<PgFieldDescriptor>.GetEnumerator()
        {
            return _fields.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _fields.GetEnumerator();
        }
    }
}