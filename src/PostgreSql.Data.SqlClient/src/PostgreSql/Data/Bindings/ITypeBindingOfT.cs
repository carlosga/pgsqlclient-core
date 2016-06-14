using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public interface ITypeBinding<T>
        : ITypeBinding
    {
        T Read(ITypeReader r);
        void Write(ITypeWriter w, T value);
    }
}
