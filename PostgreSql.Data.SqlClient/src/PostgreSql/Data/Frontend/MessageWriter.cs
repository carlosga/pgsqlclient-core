// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Data.Common;
using System.IO;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Collections.Generic;

namespace PostgreSql.Data.Frontend
{
    internal sealed class MessageWriter
    {
        private readonly byte        _messageType;
        private readonly SessionData _sessionData;
        private readonly int         _initialCapacity;
        private readonly int         _offset;

        private byte[] _buffer;
        private int    _position;

        internal byte MessageType => _messageType;
        internal int  Position    => _position;
        internal int  Length      => _buffer.Length;

        internal MessageWriter(byte messageType, SessionData sessionData)
        {
            _messageType     = messageType;
            _sessionData     = sessionData;
            _offset          = ((_messageType != FrontendMessages.Untyped) ? 1 : 0);
            _initialCapacity = 4 + _offset;
            _buffer          = new byte[_initialCapacity]; // First 4/5 bytes are for the packet length
            _position        = _initialCapacity;
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

        internal void Write(byte[] buffer) => Write(buffer, 0, buffer.Length);
        internal void Write(char ch)       => WriteByte((byte)ch);
        internal void Write(bool value)    => WriteByte((byte)(value ? 1 : 0));

        internal void Write(byte[] buffer, int offset, int count)
        {
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

        /// typedef struct NumericVar
        /// {
        ///     int          ndigits; /* # of digits in digits[] - can be 0! */
        ///     int          weight;  /* weight of first digit */
        ///     int          sign;    /* NUMERIC_POS, NUMERIC_NEG, or NUMERIC_NAN */
        ///     int          dscale;  /* display scale */
        ///     NumericDigit *buf;    /* start of palloc'd space for digits[] */
        ///     NumericDigit *digits; /* base-NBASE digits */
        /// } NumericVar;

        struct Numeric
        {
            public short   ndigits; /* # of digits in digits[] - can be 0! */
            public short   weight;  /* weight of first digit */
            public short   sign;    /* NUMERIC_POS, NUMERIC_NEG, or NUMERIC_NAN */
            public short   dscale;  /* display scale */
            public short[] digits;  /* base-NBASE digits */
        }

        internal void Write(decimal value)
        {
            bool  isNegative = (value < 0);
            int[] bits       = Decimal.GetBits(Math.Abs(value));

            if (bits        == null
             || bits.Length != 4
             || (bits[3] & ~(PgDecimal.DecimalSignMask | PgDecimal.DecimalScaleMask)) != 0
             || (bits[3] & PgDecimal.DecimalScaleMask) > (28 << 16))
            {
                throw new ArgumentException("invalid Decimal", "value");
            }

            Numeric numeric;
            numeric.sign    = (short)((isNegative) ? PgDecimal.NegativeMask : PgDecimal.PositiveMask);
            numeric.dscale  = (short)((bits[3] & PgDecimal.DecimalScaleMask) >> 16); // 0-28, power of 10 to divide numerator by
            numeric.weight  = (short)((numeric.dscale - 7 + 1) < 0 ? 0 : (numeric.dscale - 7 + 1));
            numeric.ndigits = 0;
            numeric.digits  = new short[14];

            for (int i = (numeric.weight + 7); i >= 0; --i)
            {
                var digit = (short) (value / PgDecimal.Weights[i]);
                if (digit > 0)
                {
                    value -= (digit * PgDecimal.Weights[i]);
                }
                numeric.digits[numeric.ndigits++] = digit; 
                if (value == 0)
                {
                    break;
                }
            }

            int sizeInBytes = 8 + numeric.ndigits * sizeof(short);

            EnsureCapacity(sizeInBytes);

            Write(sizeInBytes);
            Write(numeric.ndigits);
            Write(numeric.weight);
            Write(numeric.sign);
            Write(numeric.dscale);

            for (int i = 0; i < numeric.ndigits; ++i)
            {
                Write(numeric.digits[i]);
            }
        }

        internal void Write(float value)    => Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        internal void Write(double value)   => Write(BitConverter.DoubleToInt64Bits(value));
        internal void Write(PgDate value)   => Write(value.DaysSinceEpoch);
        internal void Write(TimeSpan value) => Write((long)(value.Ticks * 0.1));
        internal void Write(DateTime value) => Write((long)(value.Subtract(PgTimestamp.EpochDateTime).TotalMilliseconds * 1000));
        internal void Write(PgInterval value)
        {
            EnsureCapacity(8);

            Write((value - TimeSpan.FromDays(value.TotalDays)).TotalSeconds);
            Write(value.Days / 30);
        }

        internal void WriteTimeTZ(DateTimeOffset value)
        {
            EnsureCapacity(12);

            Write(value.TimeOfDay);
            Write((int)(value.Offset.TotalSeconds));
        } 

        internal void WriteTimestampTZ(DateTimeOffset value)
        {
            var timestamp = (long)((value.ToUnixTimeMilliseconds() * 1000) - PgTimestamp.MicrosecondsBetweenEpoch)
                          + (int)value.Offset.TotalSeconds;

            Write(timestamp);
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

        internal void Write(PgPolygon value)
        {
            Write(value.Points.Length);

            for (int i = 0; i < value.Points.Length; ++i)
            {
                Write(value.Points[i]);
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
                Write((PgDate)value);
                break;

            case PgDbType.Time:
                Write(typeInfo.Size);
                Write((TimeSpan)value);
                break;

            case PgDbType.Timestamp:
                Write(typeInfo.Size);
                Write((DateTime)value);
                break;

            case PgDbType.TimeTZ:
                Write(typeInfo.Size);
                WriteTimeTZ((DateTimeOffset)value);
                break;

            case PgDbType.TimestampTZ:
                Write(typeInfo.Size);
                WriteTimestampTZ((DateTimeOffset)value);
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
            }
        }

        internal void WriteTo(Stream stream)
        {
            int length = _position;

            Seek(_offset);

            Write(length - _offset);

            stream.Write(_buffer, 0, length);

            Seek(length);

            Clear();
        }

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

        private void WritePathInternal(PgPath path)
        {
            int sizeInBytes = (16 * path.Points.Length) + 5;
            Write(sizeInBytes);
            Write(path);
        }

        private void WritePolygonInternal(PgPolygon polygon)
        {
            int sizeInBytes = (16 * polygon.Points.Length) + 4;
            EnsureCapacity(sizeInBytes + 4);
            Write(sizeInBytes);
            Write(polygon);
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
