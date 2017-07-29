// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Buffers;
using System.Data.Common;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
        : ITypeWriter, IDisposable
    {
        private readonly byte        _messageType;
        private readonly SessionData _sessionData;
        private readonly int         _initialCapacity;
        private readonly int         _offset;
        
        private byte[]   _buffer;
        private int      _position;

        internal byte MessageType => _messageType;
        internal int  Position    => _position;
        internal int  Length      => _buffer.Length;

        internal MessageWriter(byte messageType, SessionData sessionData)
        {
            _messageType     = messageType;
            _sessionData     = sessionData;
            _offset          = ((_messageType != FrontendMessages.Untyped) ? 1 : 0);
            _initialCapacity = 4 + _offset;
            _position        = _initialCapacity;
            _buffer          = ArrayPool<byte>.Shared.Rent(_initialCapacity); // First 4/5 bytes are for the packet length
            if (_messageType != FrontendMessages.Untyped)
            {
                _buffer[0] = _messageType;
            }
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_buffer != null)
                    {
                        ArrayPool<byte>.Shared.Return(_buffer, true);
                        _buffer = null;
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion        

        internal void Reset()
        {
            if (_position != _initialCapacity)
            {
                PooledBuffer.Reset(ref _buffer, _initialCapacity);
                _position  = _initialCapacity;
                _buffer[0] = _messageType;
            }
        }

        internal void Write(bool value)    => WriteByte((byte)(value ? 1 : 0));
        internal void Write(byte[] buffer) => Write(buffer, 0, buffer.Length);
        internal void Write(byte[] buffer, int offset, int count)
        {
            if (count > 0)
            {
                EnsureCapacity(count);

                Buffer.BlockCopy(buffer, offset, _buffer, _position, count);

                _position += count;
            }
        }

        internal void Write(TypeInfo typeInfo, object value)
        {
            if (ADP.IsNull(value))
            {
                Write(-1);  // -1 indicates a NULL value
                return;
            }

            if (typeInfo.Size != -1)
            {
                Write(typeInfo.Size);
            }

            switch (typeInfo.PgDbType)
            {
            case PgDbType.Void:
                Write(-1);
                break;

            case PgDbType.Array:
            case PgDbType.Vector:
                WriteArray(typeInfo, value);
                break;

            case PgDbType.Bytea:
                WriteBufferInternal((byte[])value);
                break;

            case PgDbType.Boolean:
                Write((bool)value);
                break;

            case PgDbType.Bit:
                WriteByte((byte)value);
                break;

            case PgDbType.Char:
            case PgDbType.VarChar:
            case PgDbType.Text:
                WriteStringInternal(value);
                break;

            case PgDbType.SmallInt:
                Write(Convert.ToInt16(value));
                break;

            case PgDbType.Integer:
                Write(Convert.ToInt32(value));
                break;

            case PgDbType.BigInt:
                Write(Convert.ToInt64(value));
                break;

            case PgDbType.Numeric:
                Write((decimal)value);
                break;

            case PgDbType.Real:
                Write((float)value);
                break;

            case PgDbType.Double:
                Write((double)value);
                break;

            case PgDbType.Money:
                Write((long)((decimal)value * 100));
                break;

            case PgDbType.Interval:
                Write((PgInterval)value);
                break;

            case PgDbType.Date:
                Write((DateTime)value);
                break;

            case PgDbType.Time:
                Write((TimeSpan)value);
                break;

            case PgDbType.Timestamp:
                WriteTimeStamp((DateTime)value);
                break;

            case PgDbType.TimeTZ:
                WriteTimeTZ((DateTimeOffset)value);
                break;

            case PgDbType.TimestampTZ:
                WriteTimestampTZ((DateTimeOffset)value);
                break;

            case PgDbType.Uuid:
                Write((Guid)value);
                break;

            case PgDbType.Inet:
                Write((IPAddress)value);
                break;

            case PgDbType.MacAddress:
                Write((PhysicalAddress)value);
                break;

            case PgDbType.Point:
                Write((PgPoint)value);
                break;

            case PgDbType.Circle:
                Write((PgCircle)value);
                break;

            case PgDbType.Line:
                Write((PgLine)value);
                break;

            case PgDbType.LSeg:
                Write((PgLSeg)value);
                break;

            case PgDbType.Box:
                Write((PgBox)value);
                break;

            case PgDbType.Polygon:
                WritePolygonInternal((PgPolygon)value);
                break;

            case PgDbType.Path:
                WritePathInternal((PgPath)value);
                break;

            case PgDbType.Composite:
                WriteComposite(typeInfo, value);
                break;

            case PgDbType.Enum:
                WriteEnum(typeInfo, value);
                break;
            }
        }

        private void WriteBufferInternal(byte[] buffer)
        {
            EnsureCapacity(buffer.Length + 4);
            Write(buffer.Length);
            Write(buffer);
        }

        /// FoundationDB client (BSD License)
        private void EnsureCapacity(int count)
        {
            Debug.Assert(count >= 0);

            var offset = _position + count;

            if (_buffer == null || offset > _buffer.Length)
            {
                // double the size of the buffer, or use the minimum required
                long newSize = Math.Max(_buffer == null ? 0 : (((long)_buffer.Length) << 1), offset);

                // .NET (as of 4.5) cannot allocate an array with more than 2^31 - 1 items...
                if (newSize > 0x7fffffffL) 
                {
                    throw new OutOfMemoryException();
                }

                PooledBuffer.ResizeAligned(ref _buffer, (int)newSize);
            }

            Debug.Assert(_buffer != null && _buffer.Length >= offset);
        }

        private int Seek(int offset, SeekOrigin origin = SeekOrigin.Begin)
        {
            int newPosition = 0;

            switch (origin)
            {
            case SeekOrigin.Begin:
                newPosition = offset;
                break;
            case SeekOrigin.Current:
            case SeekOrigin.End:
                newPosition = _position + offset;
                break;
            }

            if (newPosition > _buffer.Length)
            {
                _position = _buffer.Length;
            }
            else if (newPosition < 0)
            {
                _position = 0;
            }
            else
            {
                _position = newPosition;
            }

            return _position;
        }
    }
}
