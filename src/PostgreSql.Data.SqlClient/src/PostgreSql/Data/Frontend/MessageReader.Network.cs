// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Net;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private IPAddress ReadIPAddress(int length)
        {
            _position  += _buffer[_position++] + 1;
            var rlength = ReadByte();
            if (rlength < 4)
            {
                var buffer  = new byte[4];
                var value   = ReadBytes(rlength);
                Buffer.BlockCopy(value, 0, buffer, 0, ((length > 4) ? 4 : length));
                return new IPAddress(buffer);
            }
            return new IPAddress(ReadBytes(rlength));
        }
    }
}