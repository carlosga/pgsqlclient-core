// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientMessageEventArgs
        : EventArgs
    {
        private readonly PgClientException _exception;

        internal PgClientException Exception => _exception;

        internal PgClientMessageEventArgs(PgClientException exception)
        {
            _exception = exception;
        }
    }
}
