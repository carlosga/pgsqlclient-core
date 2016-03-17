// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace System.Collections.Generic
{
    public static class IListOfTExtensions
    {
        public static bool IsEmpty<T>(this IList<T> list)
        {
            return (list == null || list.Count == 0);
        }
    }
}
