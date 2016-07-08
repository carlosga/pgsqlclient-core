// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Data.Common;
using System.IO;
using System.Diagnostics;
using PostgreSql.Data.Bindings;

namespace PostgreSql.Data.Frontend
{
    internal sealed class MessageWriter
        : ITypeWriter
    {
        private readonly byte        _messageType;
        private readonly SessionData _sessionData;
        private readonly int         _initialCapacity;
        private readonly int         _offset;
        
        private byte[]   _buffer;
        private int      _position;
        private TypeInfo _compositeTI;
        private int      _compositeIndex;

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

        internal void Write(byte[] buffer) => Write(buffer, 0, buffer.Length);
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

        internal unsafe void Write(short value)
        {
            EnsureCapacity(2);

            fixed (byte* pbuffer = _buffer)
            {
                *(pbuffer + _position++) = (byte)((value >> 8) & 0xFF);
                *(pbuffer + _position++) = (byte)((value     ) & 0xFF);
            }
        }

        internal unsafe void Write(int value)
        {
            EnsureCapacity(4);

            fixed (byte* pbuffer = _buffer)
            {
                *(pbuffer + _position++) = (byte)((value >> 24) & 0xFF);
                *(pbuffer + _position++) = (byte)((value >> 16) & 0xFF);
                *(pbuffer + _position++) = (byte)((value >>  8) & 0xFF);
                *(pbuffer + _position++) = (byte)((value      ) & 0xFF);
            }
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

        internal void Write(decimal value)
        {
            // Scale mask for the flags field. This byte in the flags field contains
            // the power of 10 to divide the Decimal value by. The scale byte must
            // contain a value between 0 and 28 inclusive.
            const int ScaleMask = 0x00FF0000;
            // Number of bits scale is shifted by.
            const int ScaleShift = 16;
            // decimal digits per NBASE digit
            const int DEC_DIGITS = 4;  

            bool  isNegative = (value < 0);
            var   absValue   = ((isNegative) ? value * -1.0M : value);
            int[] bits       = Decimal.GetBits(absValue);

            short sign    = (short)((isNegative) ? PgNumeric.NegativeMask : PgNumeric.PositiveMask);
            short dscale  = (short)((bits[3] & ScaleMask) >> ScaleShift);
            short weight  = 0;
            short ndigits = 0;

            if (absValue > 0)
            {
                // postgres: numeric::estimate_ln_dweight 
                weight  = (short)Math.Truncate(Math.Log((double)absValue, PgNumeric.NBase));
                // postgres: numeric::div_var
                ndigits = (short)(weight + 1 + (dscale + DEC_DIGITS - 1) / DEC_DIGITS);
            }

            int sizeInBytes = 8 + ndigits * sizeof(short);

            EnsureCapacity(sizeInBytes);

            Write(sizeInBytes);
            Write(ndigits);
            Write(weight);
            Write(sign);
            Write(dscale);

            if (ndigits > 0)
            {
                for (int i = weight + 7; i >= 0; --i)
                {
                    var digit = (short) (absValue / PgNumeric.Weights[i]);
                    if (digit > 0)
                    {
                        absValue -= (digit * PgNumeric.Weights[i]);
                    }
                    Write(digit);
                    if (absValue == 0)
                    {
                        break;
                    }
                }
            }
        }

        internal void Write(float value)    => Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        internal void Write(double value)   => Write(BitConverter.DoubleToInt64Bits(value));
        internal void Write(DateTime value) => Write(value.Date.Subtract(PgDate.EpochDateTime).Days);
        internal void Write(TimeSpan value) => Write((long)(value.Ticks * 0.1M));
        internal void WriteTimeTZ(DateTimeOffset value)
        {
            EnsureCapacity(12);

            Write(value.TimeOfDay);
            Write((int)(value.Offset.TotalSeconds));
        }

        internal void WriteTimeStamp(DateTime value)
        {
            Write((long)(value.Subtract(PgTimestamp.EpochDateTime).TotalMilliseconds * 1000));
        } 

        internal void WriteTimestampTZ(DateTimeOffset value)
        {
            var timestamp = (long)((value.ToUnixTimeMilliseconds() * 1000) - PgTimestamp.MicrosecondsBetweenEpoch)
                          + (int)value.Offset.TotalSeconds;

            Write(timestamp);
        }

        internal void Write(PgInterval value)
        {
            EnsureCapacity(8);

            Write((value - TimeSpan.FromDays(value.TotalDays)).TotalSeconds);
            Write(value.Days / 30);
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

        internal void WriteUuid(Guid uuid)
        {
            EnsureCapacity(16);
            Write(uuid.ToByteArray());
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

            case PgDbType.Uuid:
                Write(typeInfo.Size);
                WriteUuid((Guid)value);
                break;
            }
        }

        internal void WriteTo(Transport transport)
        {
            int length = _position;

            Seek(_offset);
            Write(length - _offset);

            transport.WriteFrame(_buffer, 0, length);
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
            EnsureCapacity(sizeInBytes + 4);
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
