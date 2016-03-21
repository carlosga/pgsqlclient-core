// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace System.Collections.Generic
{
    internal static class IListOfTExtensions
    {
        internal static bool IsEmpty<T>(this IList<T> list) => (list == null || list.Count == 0);
    }
}
