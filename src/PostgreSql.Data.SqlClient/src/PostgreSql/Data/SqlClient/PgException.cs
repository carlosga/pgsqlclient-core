// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgException
        : DbException
    {
        private PgErrorCollection _errors;

        public PgErrorCollection Errors 
        {
            get
            {
                if (_errors == null)
                {
                    _errors = new PgErrorCollection();
                }
                return _errors;
            }
        }

        internal PgException() 
            : base()
        {
        }

        internal PgException(string message) 
            : base(message)
        {
        }

        internal PgException(string message, PgError error) 
            : base(message)
        {
            Errors.Add(error);
        }

        internal PgException(string message, List<PgError> errors) 
            : base(message)
        {
            Errors.AddRange(errors);
        }

        internal PgException InternalClone()
        {
            List<PgError> errors = null; 

            if (_errors != null && _errors.Count > 0)
            {
                errors = new List<PgError>(_errors.Count);

                for (int i = 0; i < _errors.Count; ++i)
                {
                    errors.Add(_errors[i].InternalClone());
                }
            }

            return new PgException(Message, errors);
        }
    }
}