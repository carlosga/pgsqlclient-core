﻿// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
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
        private readonly char           _message;
        private readonly byte[]         _contents;
        private readonly PgServerConfig _serverConfig;
        
        private int _position;

        internal char Message
        {
            get { return _message; }
        }

        internal int Length
        {
            get { return _contents.Length; }
        }

        internal int Position
        {
            get { return _position; }
        }

        internal bool EOF
        {
            get { return (_position >= _contents.Length); }
        }

        internal bool IsReadyForQuery
        {
            get { return (_message == PgBackendCodes.READY_FOR_QUERY); }
        }

        internal bool IsCommandComplete
        {
            get { return (_message == PgBackendCodes.COMMAND_COMPLETE); }
        }

        internal bool IsPortalSuspended
        {
            get { return (_message == PgBackendCodes.PORTAL_SUSPENDED); }
        }

        internal bool IsNoData
        {
            get { return (_message == PgBackendCodes.NODATA); }
        }

        internal bool IsCloseComplete
        {
            get { return (_message == PgBackendCodes.CLOSE_COMPLETE); }
        }

        internal bool IsRowDescription
        {
            get { return (_message == PgBackendCodes.ROW_DESCRIPTION); }
        }

        internal PgInputPacket(char message, byte[] contents, PgServerConfig serverConfig)
        {
            _message      = message;
            _contents     = contents;
            _serverConfig = serverConfig;
            _position     = 0;
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
           var value = (char)_contents[_position];
           
           _position++;
           
           return value;
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
                                    
            return (count == 0) ? String.Empty : _serverConfig.ClientEncoding.GetString(_contents, start, count);
        }

        internal string ReadString(int count)
        {
            var data = _serverConfig.ClientEncoding.GetString(_contents, _position, count);

            _position += count;

            return data;
        }

        internal string ReadString()
        {
            return ReadString(ReadInt32());
        }

        internal bool ReadBoolean()
        {
            return BitConverter.ToBoolean(_contents, _position++);
        }

        internal byte ReadByte()
        {
            return _contents[_position++];
        }

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
            long value = (_contents[_position + 7])
                       | (_contents[_position + 6] <<  8)
                       | (_contents[_position + 5] << 16)
                       | (_contents[_position + 4] << 24) 
                       | (_contents[_position + 3] << 32)
                       | (_contents[_position + 2] << 40)
                       | (_contents[_position + 1] << 48)
                       | (_contents[_position    ] << 56);
 
            _position += 8;

            return value;
        }

        internal float ReadSingle()
        {
            var value = BitConverter.ToSingle(_contents, 0);

            _position += sizeof(float);

            return value;
        }

        internal float ReadCurrency()
        {
            var value = (float)ReadInt32();

            return (value / 100);
        }

        internal double ReadDouble()
        {
            var value = BitConverter.ToDouble(_contents, 0);

            _position += sizeof(long);

            return value;
        }

        internal DateTime ReadDate()
        {
            return PgCodes.BASE_DATE.AddDays(ReadInt32());
        }

        internal TimeSpan ReadInterval()
        {
            var interval = TimeSpan.FromSeconds(ReadDouble());

            return interval.Add(TimeSpan.FromDays(ReadInt32() * 30));
        }

        internal DateTime ReadTime(int length)
        {
            // milliseconds since January 1, 1970, 00:00:00 GMT. 
            // A negative number is the number of milliseconds before January 1, 1970, 00:00:00 GMT.

            return DateTime.ParseExact(ReadString(length), PgTypeStringFormats.TimeFormats, CultureInfo.CurrentCulture, DateTimeStyles.None);
        }

        internal DateTime ReadTimeWithTZ(int length)
        {
            // milliseconds since January 1, 1970, 00:00:00 GMT. 
            // A negative number is the number of milliseconds before January 1, 1970, 00:00:00 GMT.
            return DateTime.Parse(ReadString(length));
        }

        internal DateTime ReadTimestamp(int length)
        {
            // milliseconds since January 1, 1970, 00:00:00 GMT. 
            // A negative number is the number of milliseconds before January 1, 1970, 00:00:00 GMT.

            return DateTime.Parse(ReadString(length));
        }

        internal DateTime ReadTimestampWithTZ(int length)
        {
            var value = ReadInt64();
                                 
            // milliseconds since January 1, 1970, 00:00:00 GMT. 
            // A negative number is the number of milliseconds before January 1, 1970, 00:00:00 GMT.
                                               
            System.Console.WriteLine(value);
                                   
            return DateTime.Now;
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
            PgType elementType = _serverConfig.DataTypes.Single(x => x.Oid == oid);

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
            var elementType = _serverConfig.DataTypes.Single(x => x.Oid == type.ElementType);
            var data        =  Array.CreateInstance(elementType.SystemType, (length / elementType.Size));

            for (int i = 0; i < data.Length; ++i)
            {
                data.SetValue(ReadValue(elementType, elementType.Size), i);
            }

            return data;
        }

        internal PgPoint ReadPoint()
        {
           return new PgPoint(ReadDouble(), ReadDouble());
        }

        internal PgCircle ReadCircle()
        {
           return new PgCircle(ReadPoint(), ReadDouble());
        }

        internal PgLine ReadLine()
        {
           return new PgLine(ReadPoint(), ReadPoint());
        }

        internal PgLSeg ReadLSeg()
        {
           return new PgLSeg(ReadPoint(), ReadPoint());
        }

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
            PgType elementType = _serverConfig.DataTypes.Single(x => x.Oid == type.ElementType);
            Array  data        = null;

            string   contents = ReadString(length);
            string[] elements = contents.Substring(1, contents.Length - 2).Split(',');

            data = Array.CreateInstance(elementType.SystemType, elements.Length);

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