// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class PgErrorCodes
    {
        internal const char SEVERITY = 'S';
        internal const char CODE     = 'C';
        internal const char MESSAGE  = 'M';
        internal const char DETAIL   = 'D';
        internal const char HINT     = 'H';
        internal const char POSITION = 'P';
        internal const char WHERE    = 'W';
        internal const char FILE     = 'F';
        internal const char LINE     = 'L';
        internal const char ROUTINE  = 'R';
        internal const char END      = '\0';
    }
}
