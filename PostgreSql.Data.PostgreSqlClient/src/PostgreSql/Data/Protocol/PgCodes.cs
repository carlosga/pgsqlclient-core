// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Protocol
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

        //
        internal const char NULL_TERMINATOR = '\0';       

        // MD5 prefix
        internal static string MD5_PREFIX = "md5";

        // Format codes
        internal const short TEXT_FORMAT   = 0;
        internal const short BINARY_FORMAT = 1;

        // Date & Time codes
        internal static readonly DateTime BASE_DATE  = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal const string             DATE_STYLE = "ISO";

        // internal const long MicrosecondsPerDay    = 86400000000L;
        // internal const long MicrosecondsPerHour   = 3600000000L;
        // internal const long MicrosecondsPerMinute = 60000000L;
        // internal const long MicrosecondsPerSecond = 1000000L;  
        // internal const long SecondsPerDay	      = 86400L;
        
        // Julian-date equivalents of Day 0 in Unix and Postgres
        // internal const long UnixEpochDate         = 2440588; // 1970, 1, 1        
        // internal const long PostgresEpochDate     = 2451545; // 2000, 1, 1       
        // internal const long SecondsBetweenEpoch   = ((PostgresEpochDate - UnixEpochDate) * SecondsPerDay);

        // Numeric data type
        internal const int NUMERIC_SIGN_MASK     = 0xC000;
        internal const int NUMERIC_POS           = 0x0000;
        internal const int NUMERIC_NEG           = 0x4000;
        internal const int NUMERIC_NAN           = 0xC000;
        internal const int NUMERIC_MAX_PRECISION = 1000;
        internal const int NUMERIC_DSCALE_MASK   = 0x3FFF;
        internal const int NUMERIC_HDRSZ         = 10;

        // Error codes
        internal const string ERROR_SEVERITY = "ERROR";
        internal const string FATAL_SEVERITY = "FATAL";
        internal const string PANIC_SEVERITY = "PANIC";
    }
}
