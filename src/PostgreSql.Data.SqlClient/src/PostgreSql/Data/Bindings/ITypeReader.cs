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
