// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal static class PgBackendCodes
    {
        // Backend Message Formats
        internal const char AUTHENTICATION         = 'R';
        internal const char BACKEND_KEY_DATA       = 'K';
        internal const char BIND_COMPLETE          = '2';
        internal const char CLOSE_COMPLETE         = '3';
        internal const char COMMAND_COMPLETE       = 'C';
        internal const char COPY_IN_RESPONSE       = 'G';
        internal const char COPY_OUT_RESPONSE      = 'H';
        internal const char DATAROW                = 'D';
        internal const char EMPTY_QUERY_RESPONSE   = 'I';
        internal const char ERROR_RESPONSE         = 'E';
        internal const char FUNCTION_CALL_RESPONSE = 'V';
        internal const char NODATA                 = 'n';
        internal const char NOTICE_RESPONSE        = 'N';
        internal const char NOTIFICATION_RESPONSE  = 'A';
        internal const char PARAMETER_DESCRIPTION  = 't';
        internal const char PARAMETER_STATUS       = 'S';
        internal const char PARSE_COMPLETE         = '1';
        internal const char PORTAL_SUSPENDED       = 's';
        internal const char READY_FOR_QUERY        = 'Z';
        internal const char ROW_DESCRIPTION        = 'T';
    }
}