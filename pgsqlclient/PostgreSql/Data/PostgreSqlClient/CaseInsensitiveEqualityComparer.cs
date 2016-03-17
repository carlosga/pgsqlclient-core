// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class CaseInsensitiveEqualityComparer
        : EqualityComparer<string>
    {
        public override bool Equals(string x, string y)
        {
            return x.CaseInsensitiveCompare(y);
        }

        public override int GetHashCode(string obj)
        {
            return obj.ToLowerInvariant().GetHashCode();
        }
    }
}