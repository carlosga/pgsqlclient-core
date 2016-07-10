// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void Write(IPAddress value)
        {
            var bytes = value.GetAddressBytes();
            EnsureCapacity(bytes.Length + 8);
            Write(bytes.Length + 4);
            WriteByte(2);
            WriteByte(0);
            WriteByte(0);
            WriteByte(4);
            Write(bytes);
        }
    }
}