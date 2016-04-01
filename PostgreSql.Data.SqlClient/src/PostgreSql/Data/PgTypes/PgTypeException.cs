// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public class PgTypeException
        : Exception
    {
        internal PgTypeException() 
            : base()
        {
        }

        internal PgTypeException(string message) 
            : base(message)
        {
        }
    }
}
