using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public interface ITypeWriter
    {
        void WriteCompositeValue<T>(T? value) where T: struct;
        void WriteComposite<T>(T value);
    }
}
