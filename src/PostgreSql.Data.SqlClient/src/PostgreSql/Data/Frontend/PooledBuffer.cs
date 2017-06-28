using System;
using System.Buffers;

namespace PostgreSql.Data.Frontend
{
    internal static class PooledBuffer
    {
        internal static void Resize(ref byte[] buffer, int newSize)
        {
            byte[] newBuffer = ArrayPool<byte>.Shared.Rent(newSize);
            Buffer.BlockCopy(buffer, 0, newBuffer, 0, buffer.Length);
            ArrayPool<byte>.Shared.Return(buffer, true);
            buffer = newBuffer;
        }        
    }
}