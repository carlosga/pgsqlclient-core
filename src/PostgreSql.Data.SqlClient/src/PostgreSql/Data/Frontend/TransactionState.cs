// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal enum TransactionState : int
    {
        // 'I' if idle (not in a transaction block)
        Default = 73
        // 'T' if in a transaction block
      , Active = 84
        // 'E' if in a failed transaction block (queries will be rejected until block is ended).
      , Broken = 69
    }
}
