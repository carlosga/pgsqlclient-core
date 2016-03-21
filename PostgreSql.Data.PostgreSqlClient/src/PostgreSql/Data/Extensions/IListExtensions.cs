﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace System.Collections
{
    internal static class IListExtensions
    {
        internal static bool IsEmpty(this IList list) => (list == null || list.Count == 0);
    }
}