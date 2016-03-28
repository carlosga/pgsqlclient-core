// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.SqlClient
{
    /// http://www.postgresql.org/docs/9.2/static/errcodes-appendix.html
    public sealed class PgError
    {
        private readonly string _severity;
        private readonly string _message;
        private readonly string _code;
        private readonly string _detail;
        private readonly string _hint;
        private readonly string _where;
        private readonly string _position;
        private readonly string _file;
        private readonly int    _line;
        private readonly string _routine;

        public string Severity  => _severity;
        public string Message   => _message;
        public string Code      => _code;
        public string Detail    => _detail;
        public string Hint      => _hint;
        public string Where     => _where;
        public string Position  => _position;
        public string File      => _file;
        public int    Line      => _line; 
        public string Routine   => _routine;

        internal PgError()
        {
        }

        internal PgError(string message)
        {
            _message = message;
        }

        internal PgError(string severity, string code, string message)
        {
            _severity = severity;
            _code     = code;
            _message  = message;
        }

        internal PgError(string severity
                       , string message
                       , string code
                       , string detail
                       , string hint
                       , string where
                       , string position
                       , string file
                       , int    line
                       , string routine)
        {
            _severity = severity;
            _message  = message;
            _code     = code;
            _detail   = detail;
            _hint     = hint;
            _where    = where;
            _position = position;
            _file     = file;
            _line     = line;
            _routine  = routine;
        }
    }
}
