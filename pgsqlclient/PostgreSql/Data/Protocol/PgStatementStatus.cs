﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal enum PgStatementStatus
    {
        Initial
      , Parsing
      , Parsed
      , Describing
      , Described
      , Binding
      , Binded
      , Executing
      , Executed
      , OnQuery
      , Error
    }
}