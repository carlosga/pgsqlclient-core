// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgError
    {
        public string Severity
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string Code
        {
            get;
            set;
        }

        public string Detail
        {
            get;
            set;
        }

        public string Hint
        {
            get;
            set;
        }

        public string Where
        {
            get;
            set;
        }

        public string Position
        {
            get;
            set;
        }

        public string File
        {
            get;
            set;
        }

        public int Line
        {
            get;
            set;
        }

        public string Routine
        {
            get;
            set;
        }

        internal PgError()
        {
        }

        internal PgError(string message)
        {
            Message = message;
        }

        internal PgError(string severity, string code, string message)
        {
            Severity = severity;
            Code     = code;
            Message  = message;
        }
    }
}