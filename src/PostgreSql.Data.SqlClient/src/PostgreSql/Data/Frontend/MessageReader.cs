// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Data.Common;
using System.Diagnostics;

namespace PostgreSql.Data.Frontend
{
    internal sealed class MessageReader
        : ITypeReader
    {
        private byte        _messageType;
        private byte[]      _buffer;
        private int         _position;
        private int         _length;
        private int         _capacity;
        private byte        _pendingMessage;
        private SessionData _sessionData;

        internal byte MessageType       => _messageType;
        internal int  Length            => _length;
        internal int  Position          => _position;
        internal bool IsRowDescription  => (_messageType == BackendMessages.RowDescription);
        internal bool IsEmptyQuery      => (_messageType == BackendMessages.EmptyQueryResponse);
        internal bool IsNoData          => (_messageType == BackendMessages.NoData);
        internal bool IsPortalSuspended => (_messageType == BackendMessages.PortalSuspended);
        internal bool IsCommandComplete => (_messageType == BackendMessages.CommandComplete);
        internal bool IsCloseComplete   => (_messageType == BackendMessages.CloseComplete);
        internal bool IsReadyForQuery   => (_messageType == BackendMessages.ReadyForQuery);

        internal MessageReader(SessionData sessionData)
        {
            _sessionData = sessionData;
            _capacity    = sessionData.ConnectionOptions.PacketSize;
            _buffer      = new byte[_capacity];
        }

        internal byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];

            Buffer.BlockCopy(_buffer, _position, buffer, 0, count);

            _position += count;

            return buffer;
        }

        internal char ReadChar() => (char)_buffer[_position++];

        internal string ReadNullString()
        {
            int start = _position;

            while (_position < _length && _buffer[_position] != 0) 
            { 
                ++_position;
            }

            int count = _position - start;

            if (_position < _buffer.Length)
            {
                ++_position;
            }

            return (count == 0) ? string.Empty : _sessionData.ClientEncoding.GetString(_buffer, start, count);
        }

        internal string ReadString(int count)
        {
            var data = _sessionData.ClientEncoding.GetString(_buffer, _position, count);

            _position += count;

            return data;
        }

        internal string ReadString()  => ReadString(ReadInt32());
        internal byte   ReadByte()    => _buffer[_position++];
        internal bool   ReadBoolean() => (ReadByte() == 1);

        internal unsafe short ReadInt16()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                _position += 2;
                return (short)((*(pbuffer + 1) & 0xFF) | (*(pbuffer) & 0xFF) << 8);
            }
        }

        internal unsafe int ReadInt32()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                _position += 4;
                return (*(pbuffer + 3) & 0xFF)
                     | (*(pbuffer + 2) & 0xFF) <<  8
                     | (*(pbuffer + 1) & 0xFF) << 16
                     | (*(pbuffer    ) & 0xFF) << 24;
            }
        }

        internal long ReadInt64()
        {
            int v1 = ReadInt32();
            int v2 = ReadInt32();

            return (uint)v2 | ((long)v1 << 32);
        }

        internal unsafe float ReadSingle()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                int val = ReadInt32();
                return *((float*)&val);
            }
        }

        internal decimal ReadNumeric()
        {
            int ndigits = 0; // # of digits in digits[] - can be 0!
            int weight  = 0; // weight of first digit
            int sign    = 0; // NUMERIC_POS, NUMERIC_NEG, or NUMERIC_NAN
            int dscale  = 0; // display scale
            var res     = 0.0M;

            ndigits = ReadInt16();

            if (ndigits < 0 || ndigits > PgNumeric.MaxLength)
            {
                throw new FormatException("invalid length in \"numeric\" value");
            }

            weight = ReadInt16() + 7;
            sign   = ReadInt16();

            if (sign != PgNumeric.PositiveMask && sign != PgNumeric.NegativeMask && sign != PgNumeric.NaNMask)
            {
                throw new FormatException("invalid sign in \"numeric\" value");
            }

            dscale = ReadInt16();

            // base-NBASE digits
            for (int i = 0; i < ndigits; ++i)
            {
                short digit = ReadInt16();

                if (digit < 0 || digit >= PgNumeric.NBase)
                {
                    throw new FormatException("invalid digit in external \"numeric\" value");
                }

                res += digit * PgNumeric.Weights[weight - i];
            }

            return ((sign == PgNumeric.NegativeMask) ? -res : res);
        }

        internal double     ReadDouble()    => BitConverter.Int64BitsToDouble(ReadInt64());
        internal decimal    ReadMoney()     => ((decimal)ReadInt64() / 100.0M);
        internal PgDate     ReadDate()      => PgDate.Epoch.AddDays(ReadInt32());
        internal TimeSpan   ReadTime()      => new TimeSpan(ReadInt64() * 10);
        internal DateTime   ReadTimestamp() => PgTimestamp.EpochDateTime.AddMilliseconds(ReadInt64() * 0.001);
        internal PgInterval ReadInterval()  => PgInterval.FromInterval(ReadInt64(), ReadInt64());

        internal DateTimeOffset ReadTimeWithTZ()
        {
            return new DateTimeOffset(ReadInt64() * 10, TimeSpan.FromSeconds(ReadInt32()));
        }

        internal DateTimeOffset ReadTimestampWithTZ()
        {
            var dt = PgTimestamp.EpochDateTime.AddMilliseconds(ReadInt64() * 0.001);
            return TimeZoneInfo.ConvertTime(dt, _sessionData.TimeZoneInfo);
        }

        internal PgPoint  ReadPoint()  => new PgPoint(ReadDouble(), ReadDouble());
        internal PgCircle ReadCircle() => new PgCircle(ReadPoint(), ReadDouble());
        internal PgLine   ReadLine()   => new PgLine(ReadPoint(), ReadPoint());
        internal PgLSeg   ReadLSeg()   => new PgLSeg(ReadPoint(), ReadPoint());
        internal PgBox    ReadBox()    => new PgBox(ReadPoint(), ReadPoint());

        internal PgPolygon ReadPolygon()
        {
            PgPoint[] points = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = ReadPoint();
            }

            return new PgPolygon(points);
        }

        internal PgPath ReadPath()
        {
            bool isClosedPath = ReadBoolean();
            var  points       = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = ReadPoint();
            }

            return new PgPath(points, isClosedPath);
        }

        internal void ReadFrom(Transport transport)
        {
            _position = 0;

            if (_pendingMessage != 0)
            {
                _messageType    = _pendingMessage;
                _pendingMessage = 0;

                if (_buffer.Length > (_capacity * 2))
                {
                    Array.Resize<byte>(ref _buffer, _capacity);
                }
            }
            else
            {
                _messageType = transport.ReadByte();
            }

            if (_messageType == BackendMessages.DataRow)
            {
                _pendingMessage = _messageType;
                _length         = 0;

                do
                {
                    _length        += transport.ReadFrame(ref _buffer, _length);
                    _pendingMessage = transport.ReadByte();
                } while (_pendingMessage == _messageType);
            }
            else
            {
                _length = transport.ReadFrame(ref _buffer);
            }
        }

        internal object ReadValue(TypeInfo typeInfo)
        {
            return ReadValue(typeInfo, ReadInt32());
        }

        private object ReadValue(TypeInfo typeInfo, int length)
        {
            if (length == -1)
            {
                return DBNull.Value;
            }

            Debug.Assert((_position + length) <= _length);

            switch (typeInfo.PgDbType)
            {
            case PgDbType.Void:
                return DBNull.Value;

            case PgDbType.Bytea:
                return ReadBytes(length);

            case PgDbType.Char:
                return ReadString(length).TrimEnd(null);

            case PgDbType.VarChar:
            case PgDbType.Text:
                return ReadString(length);

            case PgDbType.Boolean:
                return ReadBoolean();

            case PgDbType.Byte:
                return ReadByte();

            case PgDbType.Numeric:
                return ReadNumeric();

            case PgDbType.Money:
                return ReadMoney();

            case PgDbType.Real:
                return ReadSingle();

            case PgDbType.Double:
                return ReadDouble();

            case PgDbType.SmallInt:
                return ReadInt16();

            case PgDbType.Integer:
                return ReadInt32();

            case PgDbType.BigInt:
                return ReadInt64();

            case PgDbType.Interval:
                return ReadInterval();

            case PgDbType.Date:
                return ReadDate();

            case PgDbType.Time:
                return ReadTime();

            case PgDbType.Timestamp:
                return ReadTimestamp();

            case PgDbType.TimeTZ:
                return ReadTimeWithTZ();

            case PgDbType.TimestampTZ:
                return ReadTimestampWithTZ();

            case PgDbType.Point:
                return ReadPoint();

            case PgDbType.Circle:
                return ReadCircle();

            case PgDbType.Line:
                return ReadLine();

            case PgDbType.LSeg:
                return ReadLSeg();

            case PgDbType.Polygon:
                return ReadPolygon();

            case PgDbType.Path:
                return ReadPath();

            case PgDbType.Box:
            case PgDbType.Box2D:
            case PgDbType.Box3D:
                return ReadBox();

            case PgDbType.Array:
                return ReadArray(typeInfo, length);

            case PgDbType.Vector:
                return ReadVector(typeInfo, length);

            case PgDbType.Composite:
                return ReadComposite(typeInfo, length);

            default:
                return ReadBytes(length);
            }
        }

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

        T ITypeReader.ReadValue<T>()
        {
            object value = (this as ITypeReader).ReadValue();

            return (value == DBNull.Value) ? default(T) : (T)value;
        }

        object ITypeReader.ReadValue()
        {
            var oid   = ReadInt32();
            var tinfo = _sessionData.TypeInfoProvider.GetTypeInfo(oid);
            if (tinfo == null)
            {
                throw ADP.InvalidOperation($"Data type with OID='{oid}' has no registered binding or is not supported.");
            }

            return ReadValue(tinfo);
        }

        private object ReadComposite(TypeInfo typeInfo, int length)
        {
            var count    = ReadInt32();
            var provider = TypeBindingContext.GetProvider(_sessionData.ConnectionOptions.ConnectionString);

            if (provider == null)
            {
                return ReadComposite(typeInfo, length, count);
            }

            var binding = provider.GetBinding(typeInfo.Schema, typeInfo.Name);

            if (binding == null)
            {
                return ReadComposite(typeInfo, length, count);
            }

            return binding.Read(this);
        }

        private object[] ReadComposite(TypeInfo typeInfo, int length, int count)
        {
            var values = new object[count];

            for (int i = 0; i < values.Length; ++i)
            {
                int oid   = ReadInt32();
                var tinfo = _sessionData.TypeInfoProvider.GetTypeInfo(oid); 

                values[i] = ReadValue(tinfo);
            }

            return values;
        }
    }
}
