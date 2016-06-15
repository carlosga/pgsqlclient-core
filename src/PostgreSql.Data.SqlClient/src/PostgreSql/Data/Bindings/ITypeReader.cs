// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public interface ITypeReader
    {
        object ReadComposite();
        object ReadCompositeValue();
    }
}
