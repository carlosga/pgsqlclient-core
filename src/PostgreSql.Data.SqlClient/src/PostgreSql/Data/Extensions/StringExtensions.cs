// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System
{
    internal static class StringExtensions
    {
        internal static bool CaseInsensitiveCompare(this string strA, string strB)
            =>  (strA.Equals(strB, StringComparison.OrdinalIgnoreCase));
    }
}
