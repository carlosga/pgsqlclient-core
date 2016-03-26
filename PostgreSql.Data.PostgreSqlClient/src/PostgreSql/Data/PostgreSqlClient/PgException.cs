// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgException
        : DbException
    {
        private readonly PgErrorCollection _errors;

        public PgErrorCollection Errors => _errors;

        internal PgException() 
            : base()
        {
            _errors = new PgErrorCollection();
        }

        internal PgException(string message) 
            : base(message)
        {
            _errors = new PgErrorCollection();
        }

        internal PgException(PgClientException ex)
            : base(ex?.Message)
        {
            _errors = new PgErrorCollection();

            foreach (var error in ex?.Errors)
            {
                _errors.Add(new PgError(error));
            }
        }
    }
}
