// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal enum StatementState
    {
        Default
      , Prepared
      , Executing
      , Suspended
      , Cancelling
      , Cancelled
    }
}
