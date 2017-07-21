// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private Guid ReadUuid()
        {
            var a = ReadInt32();
            var b = ReadInt16();
            var c = ReadInt16();
            var d = ReadBytes(8);

            return new Guid(a, b, c, d);
        }
    }
}