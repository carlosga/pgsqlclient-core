// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgInfoMessageEventArgs
        : EventArgs
    {
        private readonly PgException _exception;
        
        public PgErrorCollection Errors => _exception.Errors;
        public string Message           => _exception.Message;

        internal PgInfoMessageEventArgs(PgException exception)
        {
            _exception = exception;
        }
    }
}
