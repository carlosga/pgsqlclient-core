using System;
using System.Collections.Generic;

namespace PostgreSql.Data.Bindings
{
    public interface ITypeBinding
    {
        string Name	
        {
            get;
        }

        object Read(ITypeReader r);
        void Write(ITypeWriter w, object value);
    }
}
