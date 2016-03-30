// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgInputPacket
    {
        // private static TimeSpan ToTimeSpan(long value)
        // {
        //     var time = value;            
        //     var hour = time / PgCodes.MicrosecondsPerHour;
        //     time -= (hour) * PgCodes.MicrosecondsPerHour;
            
        //     var min = time / PgCodes.MicrosecondsPerMinute;
        //     time -= (min) * PgCodes.MicrosecondsPerMinute;
            
        //     var sec = time / PgCodes.MicrosecondsPerSecond;
        //     var fsec = time - (sec * PgCodes.MicrosecondsPerSecond);

        //     return new TimeSpan(0, (int)hour, (int)min, (int)sec, (int)(fsec * 0.001));
        // }

        private readonly char        _packetType;
        private readonly byte[]      _contents;
        private readonly SessionData _sessionData;

        private int _position;

        internal char PacketType        => _packetType;
        internal int  Length            => _contents.Length;
        internal int  Position          => _position;
        internal bool EOF               => (_position >= _contents.Length);
        internal bool IsReadyForQuery   => (_packetType == PgBackendCodes.READY_FOR_QUERY);
        internal bool IsCommandComplete => (_packetType == PgBackendCodes.COMMAND_COMPLETE);
        internal bool IsPortalSuspended => (_packetType == PgBackendCodes.PORTAL_SUSPENDED);
        internal bool IsNoData          => (_packetType == PgBackendCodes.NODATA);
        internal bool IsCloseComplete   => (_packetType == PgBackendCodes.CLOSE_COMPLETE);
        internal bool IsRowDescription  => (_packetType == PgBackendCodes.ROW_DESCRIPTION);

        internal PgInputPacket(char packetType, byte[] contents, SessionData sessionData)
        {
            _packetType  = packetType;
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

        internal char ReadChar()
        {
           return (char)_contents[_position++];
        }

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
        internal bool   ReadBoolean() => Convert.ToBoolean(ReadByte());
        internal byte   ReadByte()    => _contents[_position++];
        
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

        // internal unsafe float ReadSingle()
        // {
        //     var value = ReadInt32();

        //      return *((float*)&value);
        // }

        internal float ReadSingle()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(ReadInt32()), 0);
        }

        internal float    ReadMoney()  => ((float)ReadInt32() / 100);
        internal double   ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());
        internal DateTime ReadDate()   => PgCodes.BASE_DATE.AddDays(ReadInt32());

        internal PgTimeSpan ReadInterval()
        {
            var interval = TimeSpan.FromSeconds(ReadDouble());

            return new PgTimeSpan(interval.Add(TimeSpan.FromDays(ReadInt32() * 30)));
        }

        internal DateTime ReadTime(int length)
        {
            return DateTime.ParseExact(ReadString(length)
                                     , PgTypeStringFormats.TimeFormats
                                     , CultureInfo.CurrentCulture
                                     , DateTimeStyles.None);
        }

        internal DateTime ReadTimeWithTZ(int length)
        {
           return DateTime.Parse(ReadString(length));
        }

        internal DateTime ReadTimestamp(int length)
        {
            return DateTime.Parse(ReadString(length));
        }

        internal DateTimeOffset ReadTimestampWithTZ(int length)
        {
            var value = ReadInt64();
            var dt    = PgCodes.BASE_DATE.AddMilliseconds((long)(value * 0.001));
            
            return TimeZoneInfo.ConvertTime(dt, _sessionData.TimeZoneInfo);
        }

        internal Array ReadArray(PgTypeInfo type, int length)
        {
            if (type.Format == PgTypeFormat.Text)
            {
                return ReadStringArray(type, length);
            }

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
            var elementType = _sessionData.TypeInfo.Single(x => x.Oid == oid);

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

        internal Array ReadVector(PgTypeInfo type, int length)
        {
            var elementType = _sessionData.TypeInfo.Single(x => x.Oid == type.ElementType);
            var data        =  Array.CreateInstance(elementType.SystemType, (length / elementType.Size));

            for (int i = 0; i < data.Length; ++i)
            {
                data.SetValue(ReadValue(elementType, elementType.Size), i);
            }

            return data;
        }

        internal PgPoint  ReadPoint()   => new PgPoint(ReadDouble(), ReadDouble());
        internal PgCircle ReadCircle()  => new PgCircle(ReadPoint(), ReadDouble());
        internal PgLine   ReadLine()    => new PgLine(ReadPoint(), ReadPoint());
        internal PgLSeg   ReadLSeg()    => new PgLSeg(ReadPoint(), ReadPoint());

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
           bool      isClosedPath = ReadBoolean();
           PgPoint[] points       = new PgPoint[ReadInt32()];

           for (int i = 0; i < points.Length; i++)
           {
               points[i] = ReadPoint();
           }

           return new PgPath(isClosedPath, points);
        }

        internal object ReadValue(PgFieldDescriptor descriptor, int length)
        {
            return ReadValue(descriptor.TypeInfo, length);
        }

        internal object ReadValue(PgTypeInfo typeInfo, int length)
        {
            if (typeInfo.Format == PgTypeFormat.Text)
            {
                return ReadStringValue(typeInfo, length);
            }
            else
            {
                return ReadBinaryValue(typeInfo, length);
            }
        }

        private object ReadBinaryValue(PgTypeInfo type, int length)
        {
            switch (type.ProviderType)
            {
                case PgDbType.Array:
                    return ReadArray(type, length);

                case PgDbType.Vector:
                    return ReadVector(type, length);

                case PgDbType.Bytea:
                    return ReadBytes(length);

                case PgDbType.Char:
                    return ReadString(length).TrimEnd();

                case PgDbType.VarChar:
                case PgDbType.Refcursor:
                    return ReadString(length);

                case PgDbType.Bool:
                    return ReadBoolean();

                case PgDbType.Byte:
                    return ReadByte();

                case PgDbType.Money:
                    return ReadMoney();

                case PgDbType.Single:
                    return ReadSingle();

                case PgDbType.Double:
                    return ReadDouble();

                case PgDbType.Int16:
                    return ReadInt16();

                case PgDbType.Int32:
                    return ReadInt32();

                case PgDbType.Int64:
                    return ReadInt64();

                case PgDbType.Interval:
                    return ReadInterval();

                case PgDbType.Date:
                    return ReadDate();

                case PgDbType.Time:
                    return ReadTime(length);

                case PgDbType.TimeTZ:
                    return ReadTimeWithTZ(length);

                case PgDbType.Timestamp:
                    return ReadTimestamp(length);

                case PgDbType.TimestampTZ:
                    return ReadTimestampWithTZ(length);

                case PgDbType.Point:
                   return ReadPoint();

                case PgDbType.Circle:
                   return ReadCircle();

                case PgDbType.Line:
                   return ReadLine();

                case PgDbType.LSeg:
                   return ReadLSeg();

                case PgDbType.Box:
                   return ReadBox();

                case PgDbType.Polygon:
                   return ReadPolygon();

                case PgDbType.Path:
                   return ReadPath();

                default:
                    return ReadBytes(length);
            }
        }

        internal object ReadStringValue(PgTypeInfo type, int length)
        {
            if (type.IsArray)
            {
                return ReadStringArray(type, length);
            }

            string stringValue = ReadString(length);

            switch (type.ProviderType)
            {
                case PgDbType.Bytea:
                    return null;

                case PgDbType.Char:
                    return stringValue.TrimEnd();

                case PgDbType.VarChar:
                case PgDbType.Refcursor:
                case PgDbType.Text:
                    return stringValue;

                case PgDbType.Bool:
                    switch (stringValue.ToLower())
                    {
                        case "t":
                        case "true":
                        case "y":
                        case "yes":
                        case "1":
                            return true;

                        default:
                            return false;
                    }

                case PgDbType.Byte:
                    return Byte.Parse(stringValue);

                case PgDbType.Money:
                case PgDbType.Decimal:
                case PgDbType.Numeric:
                    return Decimal.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Single:
                    return Single.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Double:
                    return Double.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Int16:
                    return Int16.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Int32:
                    return Int32.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Int64:
                    return Int64.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDbType.Interval:
                    return null;

                case PgDbType.Date:
                case PgDbType.Timestamp:
                case PgDbType.Time:
                case PgDbType.TimeTZ:
                case PgDbType.TimestampTZ:
                    return DateTime.Parse(stringValue);

                case PgDbType.Point:
                   return PgPoint.Parse(stringValue);

                case PgDbType.Circle:
                   return PgCircle.Parse(stringValue);

                case PgDbType.Line:
                   return PgLine.Parse(stringValue);

                case PgDbType.LSeg:
                   return PgLSeg.Parse(stringValue);

                case PgDbType.Box:
                   return PgBox.Parse(stringValue);

                case PgDbType.Polygon:
                   return PgPolygon.Parse(stringValue);

                case PgDbType.Path:
                   return PgPath.Parse(stringValue);

                case PgDbType.Box2D:
                   return PgBox2D.Parse(stringValue);

                case PgDbType.Box3D:
                   return PgBox3D.Parse(stringValue);

                default:
                    return ReadBytes(length);
            }
        }

        private Array ReadPrimitiveArray(PgTypeInfo elementType
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

        private Array ReadNonPrimitiveArray(PgTypeInfo elementType
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

        private Array ReadStringArray(PgTypeInfo type, int length)
        {
            string     contents    = ReadString(length);
            string[]   elements    = contents.Substring(1, contents.Length - 2).Split(',');
            PgTypeInfo elementType = _sessionData.TypeInfo.Single(x => x.Oid == type.ElementType);
            Array      data        = Array.CreateInstance(elementType.SystemType, elements.Length);

            for (int i = 0; i < elements.Length; ++i)
            {
                data.SetValue(elements[i], i);
            }

            return data;
        }

        private byte[] DecodeArrayData(PgTypeInfo type, int elementCount, int length)
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
    }
}
