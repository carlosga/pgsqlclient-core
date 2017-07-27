// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        internal string ReadNullString()
        {
            int start = _position;

            while (_position < _length && _buffer[_position] != 0) 
            { 
                ++_position;
            }

            int count = _position - start;

            if (_position < _buffer.Length)
            {
                ++_position;
            }

            return (count == 0) ? string.Empty : _sessionData.ClientEncoding.GetString(_buffer, start, count);
        }

        internal char ReadChar() => (char)_buffer[_position++];

        private string ReadString(int count)
        {
            var data = _sessionData.ClientEncoding.GetString(_buffer, _position, count);

            _position += count;

            return data;
        }        
    }
}