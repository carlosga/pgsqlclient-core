// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private Array ReadArray(TypeInfo typeInfo, int length)
        {
            // Read number of dimensions
            var dimensions = ReadInt32();

            if (dimensions > 3)
            {
                throw ADP.NotSupported("Arrays with more than three dimensions are not supported.");
            }

            // Create arrays for the lengths and lower bounds
            var lengths     = new int[dimensions];
            var lowerBounds = new int[dimensions];

            // Read flags value
            var flags = ReadInt32();

            // Read array element type
            var oid         = ReadInt32();
            var elementType = _sessionData.TypeInfoProvider.GetTypeInfo(oid);
            if (elementType == null)
            {
                throw ADP.InvalidOperation($"Data type with OID='{oid}' has no registered binding or is not supported.");
            }

            // Read array lengths and lower bounds
            for (int i = 0; i < dimensions; ++i)
            {
                lengths[i]     = ReadInt32();
                lowerBounds[i] = ReadInt32();
            }

            // Create array instance
            Array data = null;
            if (dimensions == 1)
            {
                data = Array.CreateInstance(elementType.SystemType, lengths[0]);
            }
            else
            {
                data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);
            }

            // Read Array values
            if (dimensions == 1)
            {
                for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
                {
                    var value = ReadValue(elementType);
                    data.SetValue((ADP.IsNull(value)) ? null : value, i);
                }
            }
            else if (dimensions == 2)
            {
                for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
                {
                    for (int j = data.GetLowerBound(1); j <= data.GetUpperBound(1); ++j)
                    {
                        var value = ReadValue(elementType);
                        data.SetValue((ADP.IsNull(value)) ? null : value, i, j);
                    }
                }
            } 
            else if (dimensions == 3)
            {
                for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
                {
                    for (int j = data.GetLowerBound(1); j <= data.GetUpperBound(1); ++j)
                    {
                        for (int k = data.GetLowerBound(2); k <= data.GetUpperBound(2); ++k)
                        {
                            var value = ReadValue(elementType);
                            data.SetValue((ADP.IsNull(value)) ? null : value, i, j, k);
                        }
                    }
                }
            }

            return data;
        }

        private Array ReadVector(TypeInfo type, int length)
        {
            var elementType = type.ElementType;
            var data        =  Array.CreateInstance(elementType.SystemType, (length / elementType.Size));

            for (int i = 0; i < data.Length; ++i)
            {
                data.SetValue(ReadValue(elementType, elementType.Size), i);
            }

            return data;
        }
    }
}