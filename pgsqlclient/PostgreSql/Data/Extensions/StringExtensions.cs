// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace System
{
    public static class StringExtensions
    {
        public static bool CaseInsensitiveCompare(this string strA, string strB)
        {
            return (strA.Equals(strB, StringComparison.OrdinalIgnoreCase));
        }
    }
}