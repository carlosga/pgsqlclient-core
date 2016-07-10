// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private Guid ReadUuid()
        {
            return new Guid(ReadBytes(16));
        }
    }
}