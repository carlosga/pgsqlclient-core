// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.Bindings
{
    public interface ITypeBinding
    {
        string Schema
        {
            get;
        }

        string Name
        {
            get;
        }

        Type Type
        {
            get;
        }

        object Read(ITypeReader r);
        void Write(ITypeWriter w, object value);
    }
}
