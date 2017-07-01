// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed class RowDescriptor
    {
        private FieldDescriptor[] _descriptors;

        internal FieldDescriptor this[int index]
        {
            get { return _descriptors[index]; }
            set { _descriptors[index] = value; }
        }

        internal int Count => _descriptors.Length;

        internal RowDescriptor()
        {
            _descriptors = Array.Empty<FieldDescriptor>();
        }

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
        }

        internal void Clear()
        {
            if (_descriptors.Length > 0)
            {
                Allocate(0);
            }            
        }
    }
}