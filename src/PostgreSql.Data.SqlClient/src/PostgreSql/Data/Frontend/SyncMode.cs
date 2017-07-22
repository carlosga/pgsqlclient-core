// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.Frontend
{
    internal enum SyncMode
        : int
    {
        None         = 0,
        Sync         = 1,
        Flush        = 2,
        SyncAndFlush = 3
    }
}