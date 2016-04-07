// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class BackendMessages
    {
        internal const char Authentication         = 'R';
        internal const char BackendKeyData         = 'K';
        internal const char BindComplete           = '2';
        internal const char CloseComplete          = '3';
        internal const char CommandComplete        = 'C';
        internal const char CopyDone               = 'c';
        internal const char CopyInResponse         = 'G';
        internal const char CopyOutResponse        = 'H';
        internal const char DataRow                = 'D';
        internal const char EmptyQueryResponse     = 'I';
        internal const char ErrorResponse          = 'E';
        internal const char FunctionCallResponse   = 'V';
        internal const char NoData                 = 'n';
        internal const char NoticeResponse         = 'N';
        internal const char NotificationResponse   = 'A';
        internal const char ParameterDescription   = 't';
        internal const char ParameterStatus        = 'S';
        internal const char ParseComplete          = '1';
        internal const char PortalSuspended        = 's';
        internal const char ReadyForQuery          = 'Z';
        internal const char RowDescription         = 'T';
    }
}
