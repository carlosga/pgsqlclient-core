// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void WriteArray(TypeInfo typeInfo, object value)
        {
            // Handle this type as Array values
            var array = value as System.Array;

            // Get array element type
            var elementType = typeInfo.ElementType;

            // Save current position
            var startPosition = _position;

            // Ensure buffer capacity (approximated, should use lengths and lower bounds)
            EnsureCapacity(array.Length * elementType.Size + 4);

            // Reserve space for the array size
            Write(0);

            // Write the number of dimensions
            Write(array.Rank);

            // Write flags (always 0)
            Write(0);

            // Write the array elements type Oid
            Write(typeInfo.ElementType.Oid);

            // Write lengths and lower bounds
            for (int i = 0; i < array.Rank; ++i)
            {
                Write(array.GetLength(i));
                Write(array.GetLowerBound(i) + 1);
            }

            // Write array values
            for (int i = 0; i < array.Length; ++i)
            {
                Write(elementType, array.GetValue(i));
            }

            // Save current position
            int endPosition = _position;

            // Write array size
            Seek(startPosition);
            Write(endPosition - startPosition - 4);
            Seek(endPosition);
        }
    }
}