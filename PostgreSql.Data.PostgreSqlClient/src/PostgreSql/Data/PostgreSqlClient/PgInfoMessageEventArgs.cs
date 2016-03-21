// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgInfoMessageEventArgs
        : EventArgs
    {
        private readonly PgErrorCollection _errors  = new PgErrorCollection();
        private readonly string            _message = String.Empty;

        public PgErrorCollection Errors => _errors;
        public string Message           => _message;

        internal PgInfoMessageEventArgs(PgClientException ex)
        {
            _message = ex?.Message;

            foreach (PgClientError error in ex?.Errors)
            {
                var newError = new PgError
                {
                    Severity = error.Severity
                  , Code     = error.Code
                  , Message  = error.Message
                  , Detail   = error.Detail
                  , Hint     = error.Hint
                  , Line     = error.Line
                  , Where    = error.Where
                  , Position = error.Position
                  , Routine  = error.Routine
                };

                _errors.Add(newError);
            }
        }
    }
}