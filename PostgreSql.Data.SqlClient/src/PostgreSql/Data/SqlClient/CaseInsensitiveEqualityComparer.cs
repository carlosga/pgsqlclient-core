// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class CaseInsensitiveEqualityComparer
        : EqualityComparer<string>
    {
        public override bool Equals(string x, string y) => x.CaseInsensitiveCompare(y);

        public override int GetHashCode(string obj) => obj.ToLowerInvariant().GetHashCode();
    }
}
