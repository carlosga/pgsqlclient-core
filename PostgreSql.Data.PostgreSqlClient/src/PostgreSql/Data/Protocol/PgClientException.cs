// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgClientException
        : Exception
    {
        private readonly PgClientErrorCollection _errors;

        internal PgClientErrorCollection Errors => _errors;

        internal PgClientException(string message)
            : this(message, null)
        {
        }

        internal PgClientException(string message, PgClientError error)
            : base(message)
        {
            _errors = new PgClientErrorCollection();
            
            if (error != null)
            {
                _errors.Add(error);
            }
        }
    }
}
