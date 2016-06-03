// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed class RowDescriptor
    {
        private FieldDescriptor[] _descriptors;
        private int               _index;

        internal FieldDescriptor this[int index] => _descriptors[index];

        internal int Count => _index;

        internal RowDescriptor()
        {
            _descriptors = Array.Empty<FieldDescriptor>();
            _index       = 0;
        }

        internal void Add(FieldDescriptor descriptor) => _descriptors[_index++] = descriptor;

        internal int IndexOf(string name)
        {
            for (int i = 0; i < _descriptors.Length; ++i)
            {
                if (_descriptors[i].Name.CaseInsensitiveCompare(name))
                {
                    return i;
                }
            }

            return -1;
        }

        internal void Allocate(int count)
        {
            Array.Resize<FieldDescriptor>(ref _descriptors, count);
            _index = 0;
        }

        internal void Clear()
        {
            _descriptors = Array.Empty<FieldDescriptor>();
            _index       = 0;
        }
    }
}
