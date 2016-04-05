// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal sealed class PgOutputPacket
    {
        private readonly byte[]       _buffer;
        private readonly MemoryStream _stream;
        private readonly SessionData  _sessionData;
        private readonly char         _packetType;

        internal char PacketType => _packetType;
        internal int  Position   => (int)_stream.Position;
        internal int  Length     => (int)_stream.Length;

        internal PgOutputPacket(char packetType, SessionData sessionData)
        {
            _packetType  = packetType;
            _stream      = new MemoryStream();
            _buffer      = new byte[8];
            _sessionData = sessionData;
        }

        internal void Reset()
        {
            _stream.SetLength(0);
        }

        internal void Write(byte[] buffer)                       => Write(buffer, 0, buffer.Length);
        internal void Write(byte[] buffer, int index, int count) => _stream.Write(buffer, index, count);
        internal void Write(char ch)                             => _stream.WriteByte((byte)ch);
        internal void Write(bool value)                          => _stream.WriteByte((byte)(value ? 1 : 0));
        internal void WriteByte(byte value)                      => _stream.WriteByte(value);

        internal void WriteNullString(string value)
        {
            if (value != null && value.Length > 0)
            {
                Write(_sessionData.ClientEncoding.GetBytes(value));
            }
            WriteByte(0);
        }

        internal void WriteString(string value)
        {
            if (value == null || value.Length == 0)
            {
                Write(0);
            }
            else
            {
                byte[] buffer = _sessionData.ClientEncoding.GetBytes(value);

                Write(buffer.Length);
                Write(buffer);
            }
        }

        internal void Write(short value)
        {
            _buffer[0] = (byte)((value >> 8) & 0xFF);
            _buffer[1] = (byte)((value     ) & 0xFF);

            _stream.Write(_buffer, 0, 2);
        }

        internal void Write(int value)
        {
            _buffer[0] = (byte)((value >> 24) & 0xFF);
            _buffer[1] = (byte)((value >> 16) & 0xFF);
            _buffer[2] = (byte)((value >>  8) & 0xFF);
            _buffer[3] = (byte)((value      ) & 0xFF);

            _stream.Write(_buffer, 0, 4);
        }

        internal void Write(long value)
        {
            Write((int)(value >> 32));
            Write((int)(value));
        }

        internal void Write(float value)                => Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        internal void Write(double value)               => Write(BitConverter.DoubleToInt64Bits(value));
        internal void WriteDate(PgDate value)           => Write(value.DaysSinceEpoch);
        internal void WriteTime(PgTime value)           => Write(value.TotalMicroseconds);
        internal void WriteTimestamp(PgTimestamp value) => Write(value.TotalMicroseconds);
        internal void WriteInterval(PgInterval value)
        {
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
            Write(point.X);
            Write(point.Y);
        }

        internal void Write(PgCircle circle)
        {
            Write(circle.Center);
            Write(circle.Radius);
        }

        internal void Write(PgLine line)
        {
            Write(line.StartPoint);
            Write(line.EndPoint);
        }

        internal void Write(PgLSeg lseg)
        {
            Write(lseg.StartPoint);
            Write(lseg.EndPoint);
        }

        internal void Write(PgBox box)
        {
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

        internal void Write(PgTypeInfo typeInfo, object value)
        {
            switch (typeInfo.PgDbType)
            {
                case PgDbType.Array:
                case PgDbType.Vector:
                    WriteArray(typeInfo, value);
                    break;
                    
                case PgDbType.Bytea:
                    WriteBuffer((PgBinary)value);
                    break;

                case PgDbType.Bit:
                case PgDbType.Byte:
                case PgDbType.Bool:
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
                    WriteString(((PgDecimal)value).ToString());
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

        internal byte[] ToArray() => _stream.ToArray();

        internal void WriteTo(Stream stream)
        {
            if (_packetType != PgFrontEndCodes.UNTYPED)
            {
                // Write packet Type
                stream.WriteByte((byte)_packetType);
            }

            // Write packet length
            var length = _stream.Length + 4;

            _buffer[0] = (byte)(length >> 24);
            _buffer[1] = (byte)(length >> 16);
            _buffer[2] = (byte)(length >> 8);
            _buffer[3] = (byte)(length);

            stream.Write(_buffer, 0, 4);

            // Write packet contents
            _stream.WriteTo(stream);
        }

        private void WriteArray(PgTypeInfo typeInfo, object value)
        {
            // Handle this type as Array values
            var array = value as System.Array;

            // Get array element type
            var elementType = typeInfo.ElementType;
            var packet      = new PgOutputPacket(PgFrontEndCodes.UNTYPED, _sessionData);

            // Write the number of dimensions
            packet.Write(array.Rank);

            // Write flags (always 0)
            packet.Write(0);

            // Write base type of the array elements
            packet.Write(typeInfo.ElementType.Oid);

            // Write lengths and lower bounds
            for (int i = 0; i < array.Rank; ++i)
            {
                packet.Write(array.GetLength(i));
                packet.Write(array.GetLowerBound(i) + 1);
            }

            // Write array values
            for (int i = 0; i < array.Length; ++i)
            {
                packet.Write(elementType, array.GetValue(i));
            }

            // Write parameter size
            Write(packet.Length);

            // Write parameter data
            Write(packet.ToArray());
        }

        internal void WriteBuffer(PgBinary value)
        {
            byte[] buffer = value.Value;
            Write(buffer.Length);
            Write(buffer);
        }
    }
}
