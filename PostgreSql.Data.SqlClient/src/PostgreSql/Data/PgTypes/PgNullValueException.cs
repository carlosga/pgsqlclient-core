// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System.Data.Common;

namespace PostgreSql.Data.PgTypes
{
    public sealed class PgNullValueException
        : PgTypeException
    {
        internal PgNullValueException() 
            : base()
        {
        }

        internal PgNullValueException(string message) 
            : base(message)
        {
        }
    }
}
