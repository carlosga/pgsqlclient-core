// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;
using PostgreSql.Data.Bindings;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
        : ITypeReader
    {
        T ITypeReader.ReadValue<T>()
        {
            object value = (this as ITypeReader).ReadValue();

            return (value == DBNull.Value) ? default(T) : (T)value;
        }

        object ITypeReader.ReadValue()
        {
            var oid   = ReadInt32();
            var tinfo = _sessionData.TypeInfoProvider.GetTypeInfo(oid);
            if (tinfo == null)
            {
                throw ADP.InvalidOperation($"Data type with OID='{oid}' has no registered binding or is not supported.");
            }

            return ReadValue(tinfo);
        }

        private object ReadComposite(TypeInfo typeInfo, int length)
        {
            var count    = ReadInt32();
            var provider = TypeBindingContext.GetProvider(_sessionData.ConnectionOptions.ConnectionString);
            if (provider == null)
            {
                return ReadComposite(typeInfo, length, count);
            }

            var binding = provider.GetBinding(typeInfo.Schema, typeInfo.Name);
            if (binding == null)
            {
                return ReadComposite(typeInfo, length, count);
            }

            return binding.Read(this);
        }

        private object[] ReadComposite(TypeInfo typeInfo, int length, int count)
        {
            var values = new object[count];

            for (int i = 0; i < values.Length; ++i)
            {
                int oid   = ReadInt32();
                var tinfo = _sessionData.TypeInfoProvider.GetTypeInfo(oid); 

                values[i] = ReadValue(tinfo);
            }

            return values;
        }
    }
}