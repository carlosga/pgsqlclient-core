// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using PostgreSql.Data.PgTypes;

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
        
        private readonly char        _message;
        private readonly byte[]      _contents;
        private readonly SessionData _sessionData;
        
        private int _position;

        internal char Message           => _message;
        internal int  Length            => _contents.Length;
        internal int  Position          => _position;
        internal bool EOF               => (_position >= _contents.Length);
        internal bool IsReadyForQuery   => (_message == PgBackendCodes.READY_FOR_QUERY);
        internal bool IsCommandComplete => (_message == PgBackendCodes.COMMAND_COMPLETE);
        internal bool IsPortalSuspended => (_message == PgBackendCodes.PORTAL_SUSPENDED);
        internal bool IsNoData          => (_message == PgBackendCodes.NODATA);
        internal bool IsCloseComplete   => (_message == PgBackendCodes.CLOSE_COMPLETE);
        internal bool IsRowDescription  => (_message == PgBackendCodes.ROW_DESCRIPTION);

        internal PgInputPacket(char message, byte[] contents, SessionData sessionData)
        {
            _message     = message;
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
        internal bool   ReadBoolean() => BitConverter.ToBoolean(_contents, _position++);
        internal byte   ReadByte()    => _contents[_position++];

        internal short ReadInt16()
        {
            short value = (short)((_contents[_position + 1])
                                | (_contents[_position + 0] << 8));

            _position += 2;

            return value;
        }

        internal int ReadInt32()
        {                        
            int value = (_contents[_position + 3])
                      | (_contents[_position + 2] << 8)
                      | (_contents[_position + 1] << 16)
                      | (_contents[_position    ] << 24);
            
            _position += 4;

            return value;
        }

        internal long ReadInt64()
        {
            int v1 = ReadInt32();
            int v2 = ReadInt32();
 
            return (uint)v2 | ((long)v1 << 32);
        }

        internal float ReadSingle()
        {
            var value = BitConverter.ToSingle(_contents, 0);

            _position += sizeof(float);

            return value;
        }

        internal float ReadCurrency() => ((float)ReadInt32() / 100);

        internal double ReadDouble()
        {
            var value = BitConverter.ToDouble(_contents, 0);

            _position += sizeof(long);

            return value;
        }

        internal DateTime ReadDate() => PgCodes.BASE_DATE.AddDays(ReadInt32());

        internal PgTimeSpan ReadInterval()
        {
            var interval = TimeSpan.FromSeconds(ReadDouble());

            return new PgTimeSpan(interval.Add(TimeSpan.FromDays(ReadInt32() * 30)));
        }

        internal DateTime ReadTime(int length)
        {
            return DateTime.ParseExact(ReadString(length), PgTypeStringFormats.TimeFormats, CultureInfo.CurrentCulture, DateTimeStyles.None);
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
            if (!_sessionData.IntegerDateTimes)
            {
                throw new NotSupportedException("non integer datetimes are no supported.");
            }
            
            var value = ReadInt64();           
            var dt    = PgCodes.BASE_DATE.AddMilliseconds((long)(value * 0.001));          
            
            return TimeZoneInfo.ConvertTime(dt, _sessionData.TimeZoneInfo);
        }        
        
        internal Array ReadArray(PgType type, int length)
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
            int    oid         = ReadInt32();
            PgType elementType = _sessionData.DataTypes.Single(x => x.Oid == oid);

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

        internal Array ReadVector(PgType type, int length)
        {
            var elementType = _sessionData.DataTypes.Single(x => x.Oid == type.ElementType);
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

        internal object ReadFormattedValue(PgType type, PgTypeFormat format, int length)
        {
            if (format == PgTypeFormat.Text)
            {
                return ReadValueFromString(type, length);
            }
            else
            {
                return ReadValue(type, length);
            }
        }

        internal object ReadValue(PgType type, int length)
        {
            switch (type.DataType)
            {
                case PgDataType.Array:
                    return ReadArray(type, length);

                case PgDataType.Vector:
                    return ReadVector(type, length);

                case PgDataType.Binary:
                    return ReadBytes(length);

                case PgDataType.Char:
                    return ReadString(length).TrimEnd();

                case PgDataType.VarChar:
                case PgDataType.Refcursor:
                    return ReadString(length);

                case PgDataType.Boolean:
                    return ReadBoolean();

                case PgDataType.Byte:
                    return ReadByte();

                case PgDataType.Decimal:
                    return Decimal.Parse(ReadString(length), NumberFormatInfo.InvariantInfo);

                case PgDataType.Currency:
                    return ReadCurrency();

                case PgDataType.Float:
                    return ReadSingle();

                case PgDataType.Double:
                    return ReadDouble();

                case PgDataType.Int2:
                    return ReadInt16();

                case PgDataType.Int4:
                    return ReadInt32();

                case PgDataType.Int8:
                    return ReadInt64();

                case PgDataType.Interval:
                    return ReadInterval();

                case PgDataType.Date:
                    return ReadDate();

                case PgDataType.Time:
                    return ReadTime(length);

                case PgDataType.TimeWithTZ:
                    return ReadTimeWithTZ(length);

                case PgDataType.Timestamp:
                    return ReadTimestamp(length);

                case PgDataType.TimestampWithTZ:
                    return ReadTimestampWithTZ(length);

                case PgDataType.Point:
                   return ReadPoint();

                case PgDataType.Circle:
                   return ReadCircle();

                case PgDataType.Line:
                   return ReadLine();

                case PgDataType.LSeg:
                   return ReadLSeg();

                case PgDataType.Box:
                   return ReadBox();

                case PgDataType.Polygon:
                   return ReadPolygon();

                case PgDataType.Path:
                   return ReadPath();

                default:
                    return ReadBytes(length);
            }
        }

        internal object ReadValueFromString(PgType type, int length)
        {
            if (type.IsArray)
            {
                return ReadStringArray(type, length);
            }

            string stringValue = ReadString(length);

            switch (type.DataType)
            {
                case PgDataType.Binary:
                    return null;

                case PgDataType.Char:
                    return stringValue.TrimEnd();

                case PgDataType.VarChar:
                case PgDataType.Refcursor:
                case PgDataType.Text:
                    return stringValue;

                case PgDataType.Boolean:
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

                case PgDataType.Byte:
                    return Byte.Parse(stringValue);

                case PgDataType.Decimal:
                    return Decimal.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Currency:
                case PgDataType.Float:
                    return Single.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Double:
                    return Double.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Int2:
                    return Int16.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Int4:
                    return Int32.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Int8:
                    return Int64.Parse(stringValue, NumberFormatInfo.InvariantInfo);

                case PgDataType.Interval:
                    return null;

                case PgDataType.Date:
                case PgDataType.Timestamp:
                case PgDataType.Time:
                case PgDataType.TimeWithTZ:
                case PgDataType.TimestampWithTZ:
                    return DateTime.Parse(stringValue);

                case PgDataType.Point:
                   return PgPoint.Parse(stringValue);

                case PgDataType.Circle:
                   return PgCircle.Parse(stringValue);

                case PgDataType.Line:
                   return PgLine.Parse(stringValue);

                case PgDataType.LSeg:
                   return PgLSeg.Parse(stringValue);

                case PgDataType.Box:
                   return PgBox.Parse(stringValue);

                case PgDataType.Polygon:
                   return PgPolygon.Parse(stringValue);

                case PgDataType.Path:
                   return PgPath.Parse(stringValue);

                case PgDataType.Box2D:
                   return PgBox2D.Parse(stringValue);

                case PgDataType.Box3D:
                   return PgBox3D.Parse(stringValue);

                default:
                    return ReadBytes(length);
            }
        }

        private Array ReadPrimitiveArray(PgType elementType
                                       , int    length
                                       , int    dimensions
                                       , int    flags
                                       , int[]  lengths
                                       , int[]  lowerBounds)
        {
            Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

            // Read array data
            byte[] sourceArray = DecodeArrayData(elementType, data.Length, length);

            Buffer.BlockCopy(sourceArray, 0, data, 0, sourceArray.Length);

            return data;
        }

        private Array ReadNonPrimitiveArray(PgType elementType
                                          , int    length
                                          , int    dimensions
                                          , int    flags
                                          , int[]  lengths
                                          , int[]  lowerBounds)
        {
            Array data = Array.CreateInstance(elementType.SystemType, lengths, lowerBounds);

            for (int i = data.GetLowerBound(0); i <= data.GetUpperBound(0); ++i)
            {
                int elementLen = ReadInt32();
                data.SetValue(ReadValue(elementType, elementType.Size), i);
            }

            return data;
        }

        private Array ReadStringArray(PgType type, int length)
        {
            string   contents    = ReadString(length);
            string[] elements    = contents.Substring(1, contents.Length - 2).Split(',');
            PgType   elementType = _sessionData.DataTypes.Single(x => x.Oid == type.ElementType);
            Array    data        = Array.CreateInstance(elementType.SystemType, elements.Length);

            for (int i = 0; i < elements.Length; ++i)
            {
                data.SetValue(elements[i], i);
            }

            return data;
        }

        private byte[] DecodeArrayData(PgType type, int elementCount, int length)
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
