// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void Write(Guid uuid)
        {
            EnsureCapacity(16);
            Write(uuid.ToByteArray());
        }
    }
}