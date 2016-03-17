// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientError
    {
        internal string Severity
        {
            get;
            set;
        }

        internal string Message
        {
            get;
            set;
        }

        internal string Code
        {
            get;
            set;
        }

        internal string Detail
        {
            get;
            set;
        }

        internal string Hint
        {
            get;
            set;
        }

        internal string Where
        {
            get;
            set;
        }

        internal string Position
        {
            get;
            set;
        }

        internal string File
        {
            get;
            set;
        }

        internal int Line
        {
            get;
            set;
        }

        internal string Routine
        {
            get;
            set;
        }

        internal PgClientError()
        {
        }

        internal PgClientError(string message)
        {
            Message = message;
        }

        internal PgClientError(string severity, string code, string message)
        {
            Severity = severity;
            Code     = code;
            Message  = message;
        }
    }
}