// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;
using System.IO;
using PostgreSql.Data.Bindings;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
        : ITypeWriter
    {
        private TypeInfo _compositeTI;
        private int      _compositeIndex;

        void ITypeWriter.WriteValue<T>(T value)
        {
            (this as ITypeWriter).WriteValue((object)value);
        }

        void ITypeWriter.WriteValue(object value)
        {
            var oid      = _compositeTI.Attributes[_compositeIndex++].Oid;
            var typeInfo = _sessionData.TypeInfoProvider.GetTypeInfo(oid);

            if (typeInfo.IsComposite)
            {
                throw ADP.InvalidOperation("Nested composite attributes are not supported.");
            }

            Write(typeInfo.Oid);
            Write(typeInfo, value);
        }

        private void WriteComposite(TypeInfo typeInfo, object value)
        {
            var pinitial = _position;
            var provider = TypeBindingContext.GetProvider(_sessionData.ConnectionOptions.ConnectionString);
            if (provider == null)
            {
                throw ADP.InvalidOperation($"No registered bindings found for the given composite parameter value type ({value.GetType()}).");
            }

            var binding = provider.GetBinding(typeInfo.Schema, typeInfo.Name);
            if (binding == null)
            {
                throw ADP.InvalidOperation("No registered bindings found for the given composite parameter value  ({value.GetType()}).");
            }

            _compositeTI    = typeInfo;
            _compositeIndex = 0;

            Write(0);
            Write(typeInfo.Attributes.Length);
            binding.Write(this, value);

            var pcurrent = _position;
            var length   = _position - pinitial;

            Seek(pinitial, SeekOrigin.Begin);
            Write(length - 4);
            Seek(pcurrent, SeekOrigin.Begin);

            _compositeTI    = null;
            _compositeIndex = 0;
        }
    }
}