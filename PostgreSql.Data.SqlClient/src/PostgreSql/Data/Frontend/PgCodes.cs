// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Frontend
{
    internal static class PgCodes
    {
        // Protocol version 2.0
        internal const int PROTOCOL_VERSION2_MAYOR = 2;
        internal const int PROTOCOL_VERSION2_MINOR = 0;
        internal const int PROTOCOL_VERSION2       = (PROTOCOL_VERSION2_MAYOR << 16) | PROTOCOL_VERSION2_MINOR;

        // Protocol version 3.0
        internal const int PROTOCOL_VERSION3_MAYOR = 3;
        internal const int PROTOCOL_VERSION3_MINOR = 0;
        internal const int PROTOCOL_VERSION3       = (PROTOCOL_VERSION3_MAYOR << 16) | PROTOCOL_VERSION3_MINOR;

        // SSL Request code
        internal const int SSL_REQUEST_MOST  = 1234;
        internal const int SSL_REQUEST_LEAST = 5679;
        internal const int SSL_REQUEST       = (SSL_REQUEST_MOST << 16) | SSL_REQUEST_LEAST;

        // Cancel request code
        internal const int CANCEL_REQUEST_MOST  = 1234;
        internal const int CANCEL_REQUEST_LEAST = 5678;
        internal const int CANCEL_REQUEST       = (CANCEL_REQUEST_MOST << 16) | CANCEL_REQUEST_LEAST;

        // Backend & FrontEnd Message Formats
        internal const int COPY_DATA = 'd';
        internal const int COPY_DONE = 'c';

        // Authentication values
        internal const int AUTH_OK                 = 0;
        internal const int AUTH_KERBEROS_V4        = 1;
        internal const int AUTH_KERBEROS_V5        = 2;
        internal const int AUTH_CLEARTEXT_PASSWORD = 3;
        internal const int AUTH_CRYPT_PASSWORD     = 4;
        internal const int AUTH_MD5_PASSWORD       = 5;
        internal const int AUTH_SCM_CREDENTIAL     = 6;

        // Max keys for vector data type
        internal const int INDEX_MAX_KEYS = 32;

        // MD5 prefix
        internal static string MD5_PREFIX = "md5";

        // Format codes
        internal const short TEXT_FORMAT   = 0;
        internal const short BINARY_FORMAT = 1;
            
        // Statements and Portals
        internal const char STATEMENT = 'S';
        internal const char PORTAL    = 'P'; 

        // Error codes
        internal const string ERROR_SEVERITY = "ERROR";
        internal const string FATAL_SEVERITY = "FATAL";
        internal const string PANIC_SEVERITY = "PANIC";
    }
}
