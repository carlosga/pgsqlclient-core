﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal delegate void NotificationCallback(int processId, string condition, string aditional);
}