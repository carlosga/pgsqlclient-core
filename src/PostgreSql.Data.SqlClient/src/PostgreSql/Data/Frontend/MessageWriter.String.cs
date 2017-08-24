// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        internal void WriteNullString(string value)
        {
            if (value == null || value.Length == 0)
            {
                WriteByte(0);
            }
            else
            {
                var byteCount = _sessionData.ClientEncoding.GetByteCount(value);
                EnsureCapacity(byteCount + 1);
                _sessionData.ClientEncoding.GetBytes(value, 0, value.Length, _buffer, _position);
                _position += (byteCount + 1);
            }
        }

        internal void Write(string value)
        {
            if (value.Length == 0)
            {
                Write(0);
            }
            else
            {
                var byteCount = _sessionData.ClientEncoding.GetByteCount(value);
                EnsureCapacity(byteCount + 4);
                Write(byteCount);
                _sessionData.ClientEncoding.GetBytes(value, 0, value.Length, _buffer, _position);
                _position += byteCount;
            }
        }

        internal void Write(char[] value)
        {
            if (value.Length == 0)
            {
                Write(0);
            }
            else
            {
                var byteCount = _sessionData.ClientEncoding.GetByteCount(value);
                EnsureCapacity(byteCount + 4);
                Write(byteCount);
                _sessionData.ClientEncoding.GetBytes(value, 0, value.Length, _buffer, _position);
                _position += byteCount;
            }
        }

        private void WriteStringInternal(object value)
        {
            switch (value)
            {
            case string str:
                Write(str);
                break;

            case char[] chars:
                Write(chars);
                break;

            default:
                Write(Convert.ToString(value));
                break;
            }
        }
    }
}
