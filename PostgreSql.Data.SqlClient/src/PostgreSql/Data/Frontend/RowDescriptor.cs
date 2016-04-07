// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Frontend
{
    internal sealed class RowDescriptor
        : IEnumerable<FieldDescriptor>
    {
        private readonly List<FieldDescriptor> _descriptors;

        internal FieldDescriptor this[int index] => _descriptors[index];

        internal int Count => _descriptors.Count;

        internal RowDescriptor()
        {
            _descriptors = new List<FieldDescriptor>();
        }

        internal void Add(FieldDescriptor descriptor) => _descriptors.Add(descriptor);

        internal int IndexOf(string name)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (_descriptors[i].Name.CaseInsensitiveCompare(name))
                {
                    return i;
                }
            }

            return -1;
        }

        internal void Resize(int count)
        {
            _descriptors.Clear();
            _descriptors.Capacity = count;
        }

        internal void Clear() => _descriptors.Clear();

        IEnumerator<FieldDescriptor> IEnumerable<FieldDescriptor>.GetEnumerator() => _descriptors.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _descriptors.GetEnumerator();
    }
}