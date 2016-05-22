// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class ErrorMessageParts
    {
        internal const char Severity = 'S';
        internal const char Code     = 'C';
        internal const char Message  = 'M';
        internal const char Detail   = 'D';
        internal const char Hint     = 'H';
        internal const char Position = 'P';
        internal const char Where    = 'W';
        internal const char File     = 'F';
        internal const char Line     = 'L';
        internal const char Routine  = 'R';
        internal const char End      = '\0';
    }
}
