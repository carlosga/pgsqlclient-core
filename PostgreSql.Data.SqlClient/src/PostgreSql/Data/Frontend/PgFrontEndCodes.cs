// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class PgFrontEndCodes
    {
        internal const char BIND             = 'B';
        internal const char CLOSE            = 'C';
        internal const char COPY_FAIL        = 'f';
        internal const char DESCRIBE         = 'D';
        internal const char EXECUTE          = 'E';
        internal const char FLUSH            = 'H';
        internal const char FUNCTION_CALL    = 'F';
        internal const char PARSE            = 'P';
        internal const char PASSWORD_MESSAGE = 'p';
        internal const char QUERY            = 'Q';
        internal const char SYNC             = 'S';
        internal const char TERMINATE        = 'X';
        internal const char UNTYPED          = ' ';
    }
}
