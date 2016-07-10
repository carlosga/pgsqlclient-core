// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Data.Common;
using System.IO;
using System.Diagnostics;
using PostgreSql.Data.Bindings;
using System.Net;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
        : ITypeWriter
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
            _buffer          = new byte[_initialCapacity]; // First 4/5 bytes are for the packet length
            if (_messageType != FrontendMessages.Untyped)
            {
                _buffer[0] = _messageType;
            }
        }

        internal void Clear()
        {
            if (_position != _initialCapacity)
            {
                Array.Resize<byte>(ref _buffer, _initialCapacity);
                if (_initialCapacity > 4)
                {
                    Array.Clear(_buffer, 1, 4);
                }
                _position = _initialCapacity;
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

        internal void WriteNullString(string value)
        {
            if (value == null || value.Length == 0)
            {
                WriteByte(0);
            }
            else
            {
                byte[] buffer = _sessionData.ClientEncoding.GetBytes(value);
                EnsureCapacity(buffer.Length + 1);
                Write(buffer);
                WriteByte(0);
            }
        }

        internal void Write(string value)
        {
            if (value.Length == 0)
            {
                Write(0);
            }
            else
            {
                byte[] buffer = _sessionData.ClientEncoding.GetBytes(value);
                EnsureCapacity(buffer.Length + 4);
                Write(buffer.Length);
                Write(buffer);
            }
        }

        internal void Write(char[] value)
        {
            if (value.Length == 0)
            {
                Write(0);
            }
            else
            {
                byte[] buffer = _sessionData.ClientEncoding.GetBytes(value);
                EnsureCapacity(buffer.Length + 4);
                Write(buffer.Length);
                Write(buffer);
            }
        }

        internal void WriteTo(Transport transport)
        {
            int length = _position;

            Seek(_offset);
            Write(length - _offset);

            transport.WriteFrame(_buffer, 0, length);
        }

        internal void Write(TypeInfo typeInfo, object value)
        {
            if (ADP.IsNull(value))
            {
                Write(-1);  // -1 indicates a NULL value
                return;
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
                Write(typeInfo.Size);
                Write((bool)value);
                break;

            case PgDbType.Bit:
            case PgDbType.Byte:
                Write(typeInfo.Size);
                WriteByte((byte)value);
                break;

            case PgDbType.Char:
            case PgDbType.VarChar:
            case PgDbType.Text:
                WriteStringInternal(value);
                break;

            case PgDbType.SmallInt:
                Write(typeInfo.Size);
                Write((short)value);
                break;

            case PgDbType.Integer:
                Write(typeInfo.Size);
                Write((int)value);
                break;

            case PgDbType.BigInt:
                Write(typeInfo.Size);
                Write((long)value);
                break;

            case PgDbType.Numeric:
                Write((decimal)value);
                break;

            case PgDbType.Real:
                Write(typeInfo.Size);
                Write((float)value);
                break;

            case PgDbType.Double:
                Write(typeInfo.Size);
                Write((double)value);
                break;

            case PgDbType.Money:
                Write(typeInfo.Size);
                Write((long)((decimal)value * 100));
                break;

            case PgDbType.Interval:
                Write(typeInfo.Size);
                Write((PgInterval)value);
                break;

            case PgDbType.Date:
                Write(typeInfo.Size);
                Write((DateTime)value);
                break;

            case PgDbType.Time:
                Write(typeInfo.Size);
                Write((TimeSpan)value);
                break;

            case PgDbType.Timestamp:
                Write(typeInfo.Size);
                WriteTimeStamp((DateTime)value);
                break;

            case PgDbType.TimeTZ:
                Write(typeInfo.Size);
                WriteTimeTZ((DateTimeOffset)value);
                break;

            case PgDbType.TimestampTZ:
                Write(typeInfo.Size);
                WriteTimestampTZ((DateTimeOffset)value);
                break;

            case PgDbType.Uuid:
                Write(typeInfo.Size);
                Write((Guid)value);
                break;

            case PgDbType.IPAddress:
                Write((IPAddress)value);
                break;

            case PgDbType.Point:
                Write(typeInfo.Size);
                Write((PgPoint)value);
                break;

            case PgDbType.Circle:
                Write(typeInfo.Size);
                Write((PgCircle)value);
                break;

            case PgDbType.Line:
                Write(typeInfo.Size);
                Write((PgLine)value);
                break;

            case PgDbType.LSeg:
                Write(typeInfo.Size);
                Write((PgLSeg)value);
                break;

            case PgDbType.Box:
                Write(typeInfo.Size);
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
            }
        }

        private void WriteBufferInternal(byte[] buffer)
        {
            EnsureCapacity(buffer.Length + 4);
            Write(buffer.Length);
            Write(buffer);
        }

        private void WriteStringInternal(object value)
        {
            var str = value as string;
            if (str != null)
            {
                Write(str);
            }
            else
            {
                var chars = value as char[];
                if (chars != null)
                {
                    Write(chars);
                }
                else
                {
                    Write(Convert.ToString(value));
                }
            }
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

                // round up to 16 bytes, to reduce fragmentation
                int size = Align((int)newSize);

                Array.Resize(ref _buffer, size);
            }

            Debug.Assert(_buffer != null && _buffer.Length >= offset);
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
