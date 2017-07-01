// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal enum AuthenticationStage
        : int
    {
        Done                    = 0
      , Kerberosv4              = 1
      , Kerberosv5              = 2
      , ClearText               = 3
      , Crypt                   = 4
      , MD5                     = 5
      , AuthenticationSASL      = 10
      , SASLContinue            = 11
      , AuthenticationSASLFinal = 12
    }
}
