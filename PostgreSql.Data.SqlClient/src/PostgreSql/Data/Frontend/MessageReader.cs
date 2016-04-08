// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal sealed class MessageReader
    {
        private readonly char        _messageType;
        private readonly byte[]      _contents;
        private readonly SessionData _sessionData;

        private int _position;

        internal char MessageType       => _messageType;
        internal int  Length            => _contents.Length;
        internal int  Position          => _position;
        internal bool EOF               => (_position >= _contents.Length);
        internal bool IsReadyForQuery   => (_messageType == BackendMessages.ReadyForQuery);
        internal bool IsCommandComplete => (_messageType == BackendMessages.CommandComplete);
        internal bool IsPortalSuspended => (_messageType == BackendMessages.PortalSuspended);
        internal bool IsNoData          => (_messageType == BackendMessages.NoData);
        internal bool IsCloseComplete   => (_messageType == BackendMessages.CloseComplete);
        internal bool IsRowDescription  => (_messageType == BackendMessages.RowDescription);

        internal MessageReader(char messageType, byte[] contents, SessionData sessionData)
        {
            _messageType = messageType;
            _contents    = contents;
            _sessionData = sessionData;
            _position    = 0;
        }

        internal byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];

            Buffer.BlockCopy(_contents, _position, buffer, 0, count);

            _position += count;

            return buffer;
        }

        internal char ReadChar() => (char)_contents[_position++];

        internal string ReadNullString()
        {
            int start = _position;

            while (_position < _contents.Length && _contents[_position] != 0) 
            { 
                _position++;
            }

            int count = _position - start;

            if (_position < _contents.Length)
            {
                _position++;
            }

            return (count == 0) ? String.Empty : _sessionData.ClientEncoding.GetString(_contents, start, count);
        }

        internal string ReadString(int count)
        {
            var data = _sessionData.ClientEncoding.GetString(_contents, _position, count);

            _position += count;

            return data;
        }

        internal string ReadString()  => ReadString(ReadInt32());
        internal byte   ReadByte()    => _contents[_position++];
        internal bool   ReadBoolean() => Convert.ToBoolean(ReadByte());
        
        internal short ReadInt16()
        {
            short value = (short)((_contents[_position + 1] & 0xFF)
                                | (_contents[_position + 0] & 0xFF) << 8);

            _position += 2;

            return value;
        }

        internal int ReadInt32()
        {
            int value = (_contents[_position + 3] & 0xFF)
                      | (_contents[_position + 2] & 0xFF) <<  8
                      | (_contents[_position + 1] & 0xFF) << 16
                      | (_contents[_position    ] & 0xFF) << 24;

            _position += 4;

            return value;
        }

        internal long ReadInt64()
        {
            int v1 = ReadInt32();
            int v2 = ReadInt32();

            return (uint)v2 | ((long)v1 << 32);
        }

        internal decimal ReadNumeric(int length) => Decimal.Parse(ReadString(length), PgTypeInfoProvider.InvariantCulture);

        internal float       ReadSingle()    => BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32()), 0);
        internal decimal     ReadMoney()     => ((decimal)ReadInt64() / 100);
        internal double      ReadDouble()    => BitConverter.Int64BitsToDouble(ReadInt64());
        internal PgDate      ReadDate()      => PgDate.Epoch.AddDays(ReadInt32());
        internal TimeSpan    ReadTime()      => TimeSpan.FromMilliseconds(ReadInt64() * 0.001);
        internal PgTimestamp ReadTimestamp() => new PgTimestamp(ReadInt64());
        internal PgInterval  ReadInterval()  => PgInterval.FromInterval(ReadDouble(), ReadInt32());

        internal TimeSpan ReadTimeWithTZ()
        {
            var value = ReadInt64();
#warning TODO: Handle the time zone offset
            var tz    = ReadInt32(); // time zone in seconds

            return TimeSpan.FromMilliseconds(value * 0.001);
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

        internal PgBox ReadBox()
        {
            PgPoint upperRight = ReadPoint();
            PgPoint lowerLeft  = ReadPoint();

            return new PgBox(lowerLeft, upperRight);
        }

        internal PgPolygon ReadPolygon()
        {
            PgPoint[] points = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = ReadPoint();
            }

            return new PgPolygon(points);
        }

        internal PgPath ReadPath()
        {
            bool isClosedPath = ReadBoolean();
            var  points       = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; i++)
            {
                points[i] = ReadPoint();
            }

            return new PgPath(isClosedPath, points);
        }

        internal object ReadValue(FieldDescriptor descriptor, int length)
        {
            return ReadValue(descriptor.TypeInfo, length);
        }

        internal object ReadValue(TypeInfo typeInfo, int length)
        {
            switch (typeInfo.PgDbType)
            {
                case PgDbType.Void:
                    return DBNull.Value;

                case PgDbType.Bytea:
                    return ReadBytes(length);

                case PgDbType.Char:
                    return ReadString(length).TrimEnd();

                case PgDbType.VarChar:
                case PgDbType.Text:
                    return ReadString(length);

                case PgDbType.Bool:
                    return ReadBoolean();

                case PgDbType.Byte:
                    return ReadByte();

                case PgDbType.Numeric:
                    return ReadNumeric(length);

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

                case PgDbType.TimeTZ:
                    return ReadTimeWithTZ();

                case PgDbType.Timestamp:
                    return ReadTimestamp();

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

                default:
                    return ReadBytes(length);
            }
        }

        private Array ReadArray(TypeInfo type, int length)
        {
            // Read number of dimensions
            int dimensions = ReadInt32();

            // Create arrays for the lengths and lower bounds
            int[] lengths     = new int[dimensions];
            int[] lowerBounds = new int[dimensions];

            // Read flags value
            int flags = ReadInt32();
            if (flags != 0)
            {
                throw new NotSupportedException("Invalid flags value");
            }

            // Read array element type
            int oid         = ReadInt32();
            var elementType = PgTypeInfoProvider.Types[oid];

            // Read array lengths and lower bounds
            for (int i = 0; i < dimensions; ++i)
            {
                lengths[i]     = ReadInt32();
                lowerBounds[i] = ReadInt32();
            }

            // Read Array data
            if (elementType.IsPrimitive)
            {
                return ReadPrimitiveArray(elementType, length, dimensions, flags, lengths, lowerBounds);
            }
            else
            {
                return ReadNonPrimitiveArray(elementType, length, dimensions, flags, lengths, lowerBounds);
            }
        }

        private Array ReadPrimitiveArray(TypeInfo elementType
                                       , int        length
                                       , int        dimensions
                                       , int        flags
                                       , int[]      lengths
                                       , int[]      lowerBounds)
        {
            Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

            // Read array data
            byte[] sourceArray = DecodeArrayData(elementType, data.Length, length);

            Buffer.BlockCopy(sourceArray, 0, data, 0, sourceArray.Length);

            return data;
        }

        private Array ReadNonPrimitiveArray(TypeInfo elementType
                                          , int        length
                                          , int        dimensions
                                          , int        flags
                                          , int[]      lengths
                                          , int[]      lowerBounds)
        {
            Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

            for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
            {
                int elementLen = ReadInt32();
                data.SetValue(ReadValue(elementType, elementType.Size), i);
            }

            return data;
        }

        private byte[] DecodeArrayData(TypeInfo type, int elementCount, int length)
        {
            byte[] data   = new byte[length];
            int    offset = 0;

            for (int i = 0; i < elementCount; ++i)
            {
                int byteCount = ReadInt32();

                Buffer.BlockCopy(_contents, _position, data, offset, byteCount);

                offset    += byteCount;
                _position += byteCount;
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
