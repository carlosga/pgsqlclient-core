// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal enum PgTransactionStatus
    {
        // 'I' if idle (not in a transaction block)
        Default
        // 'T' if in a transaction block
      , Active
        // 'E' if in a failed transaction block (queries will be rejected until block is ended).
      , Broken
    }
}