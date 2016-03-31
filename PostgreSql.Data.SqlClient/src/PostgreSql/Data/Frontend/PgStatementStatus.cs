// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal enum PgStatementStatus
    {
        Initial
      , Broken
      , Parsing
      , Parsed
      , Describing
      , Described
      , Binding
      , Binded
      , Executing
      , Executed
      , OnQuery
    }
}
