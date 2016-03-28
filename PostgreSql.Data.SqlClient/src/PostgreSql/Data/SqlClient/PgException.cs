// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
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

        internal PgException(string message, PgError error) 
            : base(message)
        {
            _errors = new PgErrorCollection();
            _errors.Add(error);
        }
    }
}
