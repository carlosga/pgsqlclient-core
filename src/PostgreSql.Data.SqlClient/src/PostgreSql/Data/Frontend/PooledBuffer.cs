using System;
using System.Buffers;

namespace PostgreSql.Data.Frontend
{
    internal static class PooledBuffer
    {
        internal static void Resize(ref byte[] buffer, int newSize)
        {
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, ((newSize < buffer.Length) ? newSize : buffer.Length));
            ArrayPool<byte>.Shared.Return(buffer, true);
            buffer = newBuffer;
        }

        internal static void Reset(ref byte[] buffer, int newSize)
        {
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            ArrayPool<byte>.Shared.Return(buffer, true);
            buffer = newBuffer;
        }

        internal static void ResizeAligned(ref byte[] buffer, int newSize) 
        {           
            Resize(ref buffer, Align(newSize));
        }

        /// FoundationDB client (BSD License)
        private static int Align(int size)
        {
            const int ALIGNMENT = 16; // MUST BE A POWER OF TWO!
            const int MASK      = (-ALIGNMENT) & int.MaxValue;

            if (size <= ALIGNMENT)
            {
                if (size < 0)
                {
                    throw new ArgumentOutOfRangeException("size", "Size cannot be negative");
                }
                return ALIGNMENT;
            }

            // force an exception if we overflow above 2GB
            checked { return (size + (ALIGNMENT - 1)) & MASK; }
        }        
    }
}