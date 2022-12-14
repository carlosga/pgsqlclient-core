// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    [Flags]
    public enum SqlRandomColumnOptions
    {
        None      = 0,
        Sparse    = 0x1,
        ColumnSet = 0x2, // should not be used with Sparse
    }
}
