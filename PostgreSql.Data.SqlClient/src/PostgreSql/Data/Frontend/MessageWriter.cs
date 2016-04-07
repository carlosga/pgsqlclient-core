// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace PostgreSql.Data.Frontend
{
    internal sealed class MessageWriter
    {
        private readonly char        _messageType;
        private readonly SessionData _sessionData;
        private readonly int         _initialCapacity;
        private readonly int         _offset;

        private byte[] _buffer;
        private int    _position;

        internal char MessageType => _messageType;
        internal int  Position    => _position;
        internal int  Length      => _buffer.Length;

        internal MessageWriter(char messageType, SessionData sessionData)
        {
            _messageType     = messageType;
            _sessionData     = sessionData;
            _offset          = ((_messageType != FrontendMessages.Untyped) ? 1 : 0);
            _initialCapacity = 4 + _offset;
            _buffer          = new byte[_initialCapacity]; // First 4/5 bytes are for the packet length
            _position        = _initialCapacity;

            if (_messageType != FrontendMessages.Untyped)
            {
                _buffer[0] = (byte)_messageType;   
            }
        }

        internal void Write(byte[] buffer) => Write(buffer, 0, buffer.Length);
        internal void Write(char ch)       => WriteByte((byte)ch);
        internal void Write(bool value)    => WriteByte((byte)(value ? 1 : 0));

        internal void Write(byte[] buffer, int offset, int count)
        {
            Contract.Requires<ArgumentNullException>(buffer != null, nameof(buffer));
            Contract.Requires(offset >= 0);
            Contract.Requires(count >= 0);

            if (count > 0)
            {
                EnsureCapacity(count);

                Buffer.BlockCopy(buffer, offset, _buffer, _position, count);

                _position += count;
            }
        }
        
        internal void WriteByte(byte value)
        {
            EnsureCapacity(1);

            _buffer[_position++] = value;
        }

        internal void Write(short value)
        {
            EnsureCapacity(2);

            _buffer[_position++] = (byte)((value >> 8) & 0xFF);
            _buffer[_position++] = (byte)((value     ) & 0xFF);
        }

        internal void Write(int value)
        {
            EnsureCapacity(4);

            _buffer[_position++] = (byte)((value >> 24) & 0xFF);
            _buffer[_position++] = (byte)((value >> 16) & 0xFF);
            _buffer[_position++] = (byte)((value >>  8) & 0xFF);
            _buffer[_position++] = (byte)((value      ) & 0xFF);
        }

        internal void Write(long value)
        {
            EnsureCapacity(8);

            Write((int)(value >> 32));
            Write((int)(value));
        }

        internal void WriteNullString(string value)
        {
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));

            EnsureCapacity(value.Length + 1);

            if (value != null && value.Length > 0)
            {
                Write(_sessionData.ClientEncoding.GetBytes(value));
            }

            WriteByte(0);
        }

        internal void WriteString(string value)
        {
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));

            if (value.Length == 0)
            {
                Write(0);
            }
            else
            {
                EnsureCapacity(value.Length + 4);

                byte[] buffer = _sessionData.ClientEncoding.GetBytes(value);

                Write(buffer.Length);
                Write(buffer);
            }
        }

        internal void Write(decimal value)              => WriteString(value.ToString(PgTypeInfoProvider.InvariantCulture));
        internal void Write(float value)                => Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        internal void Write(double value)               => Write(BitConverter.DoubleToInt64Bits(value));
        internal void WriteDate(PgDate value)           => Write(value.DaysSinceEpoch);
        internal void WriteTime(PgTime value)           => Write(value.TotalMicroseconds);
        internal void WriteTimestamp(PgTimestamp value) => Write(value.TotalMicroseconds);
        internal void WriteInterval(PgInterval value)
        {
            EnsureCapacity(8);

            Write((value - TimeSpan.FromDays(value.TotalDays)).TotalSeconds);
            Write(value.Days / 30);
        }

#warning TODO: Need a custom PgTimeTZ or let PgTime handle the time zone offset
        internal void WriteTimeWithTZ(PgTime time) => Write(time.TotalMicroseconds);
        internal void WriteTimestampWithTZ(DateTimeOffset timestamp)
        {
            Write((long)((timestamp.ToUnixTimeMilliseconds() * 1000) - PgTimestamp.MicrosecondsBetweenEpoch));
        }

        internal void Write(PgPoint point)
        {
            EnsureCapacity(16);

            Write(point.X);
            Write(point.Y);
        }

        internal void Write(PgCircle circle)
        {
            EnsureCapacity(24);

            Write(circle.Center);
            Write(circle.Radius);
        }

        internal void Write(PgLine line)
        {
            EnsureCapacity(32);

            Write(line.StartPoint);
            Write(line.EndPoint);
        }

        internal void Write(PgLSeg lseg)
        {
            EnsureCapacity(32);

            Write(lseg.StartPoint);
            Write(lseg.EndPoint);
        }

        internal void Write(PgBox box)
        {
            EnsureCapacity(32);

            Write(box.UpperRight);
            Write(box.LowerLeft);
        }

        internal void Write(PgPolygon polygon)
        {
            Write(polygon.Points.Length);

            for (int i = 0; i < polygon.Points.Length; ++i)
            {
                Write(polygon.Points[i]);
            }
        }

        internal void Write(PgPath value)
        {
            Write(value.IsClosedPath);
            Write(value.Points.Length);

            for (int i = 0; i < value.Points.Length; ++i)
            {
                Write(value.Points[i]);
            }
        }

        internal void Write(TypeInfo typeInfo, object value)
        {
            Contract.Requires<ArgumentNullException>(typeInfo != null, nameof(typeInfo));
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));

            switch (typeInfo.PgDbType)
            {
                case PgDbType.Array:
                case PgDbType.Vector:
                    WriteArray(typeInfo, value);
                    break;

                case PgDbType.Bytea:
                    Write((byte[])value);
                    break;

                case PgDbType.Bool:
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
                    WriteString(value.ToString());
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
                    Write((int)((decimal)value * 100));
                    break;

                case PgDbType.Interval:
                    Write(typeInfo.Size);
                    WriteInterval((PgInterval)value);
                    break;

                case PgDbType.Date:
                    Write(typeInfo.Size);
                    WriteDate((PgDate)value);
                    break;

                case PgDbType.Time:
                    Write(typeInfo.Size);
                    WriteTime((PgTime)value);
                    break;

                case PgDbType.TimeTZ:
                    Write(typeInfo.Size);
                    WriteTimeWithTZ((PgTime)value);
                    break;

                case PgDbType.Timestamp:
                    Write(typeInfo.Size);
                    WriteTimestamp((PgTimestamp)value);
                    break;

                case PgDbType.TimestampTZ:
                    Write(typeInfo.Size);
                    WriteTimestampWithTZ((DateTimeOffset)value);
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
                    var polygon = (PgPolygon)value;
                    Write(polygon.SizeInBytes);
                    Write(polygon);
                    break;

                case PgDbType.Path:
                    var path = (PgPath)value;
                    Write(path.SizeInBytes);
                    Write(path);
                    break;
            }
        }

        internal void WriteTo(Stream stream)
        {
            // Save the current position
            int length = _position;

            Seek(_offset);

            Write(length - _offset);

            stream.Write(_buffer, 0, length);

            Seek(length);
        }

        private void WriteArray(TypeInfo typeInfo, object value)
        {
            // Handle this type as Array values
            var array = value as System.Array;

            // Get array element type
            var elementType = typeInfo.ElementType;
            
            // Save current position
            var startPosition = _position;
            
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
            Write(endPosition - startPosition);
            Seek(endPosition);
        }

        /// FoundationDB client (BSD License)
        private void EnsureCapacity(int count)
        {
            Contract.Requires(count >= 0);

            if (_buffer == null || (_position + count) > _buffer.Length)
            {
                // double the size of the buffer, or use the minimum required
                long newSize = Math.Max(_buffer == null ? 0 : (((long)_buffer.Length) << 1), _position + count);

                // .NET (as of 4.5) cannot allocate an array with more than 2^31 - 1 items...
                if (newSize > 0x7fffffffL) 
                {
                    throw new OutOfMemoryException();
                }

                // round up to 16 bytes, to reduce fragmentation
                int size = Align((int)newSize);

                Array.Resize(ref _buffer, size);
            }

            Contract.Ensures(_buffer != null && _buffer.Length >= _position + count);
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
