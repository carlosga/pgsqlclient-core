﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Collections.Generic
{
    internal static class IListOfTExtensions
    {
        internal static bool IsEmpty<T>(this IList<T> list) => (list == null || list.Count == 0);
    }
}