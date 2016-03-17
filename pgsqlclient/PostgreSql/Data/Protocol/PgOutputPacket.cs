// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

// using Microsoft.IO;
using PostgreSql.Data.PgTypes;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgOutputPacket
    {
#warning TODO: restore dependency
        // private static readonly RecyclableMemoryStreamManager s_memoryStreamManager = new RecyclableMemoryStreamManager();

        private readonly byte[]         _buffer;
        private readonly MemoryStream   _stream;
        private readonly PgServerConfig _serverConfig;

        internal int Position
        {
            get { return (int)_stream.Position; }
        }

        internal int Length
        {
            get { return (int)_stream.Length; }
        }

        internal PgOutputPacket(PgServerConfig serverConfig)
        {
            // _stream      = s_memoryStreamManager.GetStream("PgOutputPacket");
            _stream       = new MemoryStream();
            _buffer       = new byte[8];
            _serverConfig = serverConfig;
        }

        internal void Reset()
        {
            _stream.SetLength(0);
            _stream.Position = 0;
        }

        internal void Write(byte[] buffer)
        {
            Write(buffer, 0, buffer.Length);
        }

        internal void Write(byte[] buffer, int index, int count)
        {
            _stream.Write(buffer, index, count);
        }

        internal void Write(char ch)
        {
            _stream.WriteByte((byte)ch);
        }

        internal void WriteNullString(string value)
        {
            Write(_serverConfig.ClientEncoding.GetBytes(value));
            WriteByte(0);
        }

        internal void WriteString(string value)
        {
            byte[] buffer = _serverConfig.ClientEncoding.GetBytes(value);

            Write(buffer.Length);
            Write(buffer);
        }

        internal void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        internal void Write(short value)
        {
            _buffer[0] = (byte)(value >> 8);
            _buffer[1] = (byte)(value);
            
            _stream.Write(_buffer, 0, 2);
        }
        
        internal void Write(int value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value);
            
            _stream.Write(_buffer, 0, 4);
        }

        internal void Write(long value)
        {
            _buffer[0] = (byte)(value >> 56);
            _buffer[1] = (byte)(value >> 48);
            _buffer[2] = (byte)(value >> 40);
            _buffer[3] = (byte)(value >> 32);
            _buffer[4] = (byte)(value >> 24);
            _buffer[5] = (byte)(value >> 16);
            _buffer[6] = (byte)(value >> 8);
            _buffer[7] = (byte)(value);

            _stream.Write(_buffer, 0 , 8);
        }

        internal void Write(float value)
        {
            Write(Convert.ToInt32(value));
        }

        internal void Write(double value)
        {
            Write(Convert.ToInt64(value));
        }

        internal void Write(bool value)
        {
            WriteByte(Convert.ToByte(value));
        }

        internal void WriteDate(DateTime date)
        {
            Write(date.Subtract(PgCodes.BASE_DATE).Days);
        }

        internal void WriteInterval(TimeSpan interval)
        {
            // int months = (interval.Days / 30);
            // int days   = (interval.Days % 30);

            // Write(interval.Subtract(TimeSpan.FromDays(months * 30)).TotalSeconds);
            // Write(months);

            Write(interval.Subtract(TimeSpan.FromDays(interval.Days)).TotalSeconds);
            Write(interval.Days / 30);
        }

        internal void WriteTime(DateTime time)
        {
            WriteString(time.ToString("HH:mm:ss.fff"));
        }

        internal void WriteTimeWithTZ(DateTime time)
        {
            WriteString(time.ToString("HH:mm:ss.fff zz"));
        }

        internal void WriteTimestamp(DateTime timestamp)
        {
            WriteString(timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff"));
        }

        internal void WriteTimestampWithTZ(DateTime timestamp)
        {
            WriteString(timestamp.ToString("yyyy/MM/dd HH:mm:ss.fff zz"));
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

        internal void Write(PgPath path)
        {
            Write(path.IsClosedPath);
            Write(path.Points.Length);

            for (int i = 0; i < path.Points.Length; ++i)
            {
                Write(path.Points[i]);
            }
        }

        internal void Write(PgParameter parameter)
        {
            if (parameter.Value == System.DBNull.Value || parameter.Value == null)
            {
                // -1 indicates a NULL argument value
                Write(-1);
            }
            else
            {
                if (parameter.DataType.DataType == PgDataType.Array
                 || parameter.DataType.DataType == PgDataType.Vector)
                {
                    WriteArray(parameter);
                }
                else
                {
                    WriteParameter(this, parameter.DataType.DataType, parameter.DataType.Size, parameter.Value);
                }
            }
        }

        internal byte[] ToArray()
        {
            return _stream.ToArray();
        }
        
        private void WriteArray(PgParameter parameter)
        {
            // Handle this type as Array values
            System.Array array = parameter.Value as System.Array;

            // Get array element type
            PgType elementType = _serverConfig.DataTypes.Single(x => x.Oid == parameter.DataType.ElementType);

            var packet = new PgOutputPacket(_serverConfig);

            // Reset packet
            packet.Reset();

            // Write the number of dimensions
            packet.Write(array.Rank);

            // Write flags (always 0)
            packet.Write(0);

            // Write base type of the array elements
            packet.Write(parameter.DataType.ElementType);

            // Write lengths and lower bounds
            for (int i = 0; i < array.Rank; ++i)
            {
                packet.Write(array.GetLength(i));
                packet.Write(array.GetLowerBound(i) + 1);
            }

            // Write array values
            for (int i = 0; i < array.Length; ++i)
            {
                WriteParameter(packet, elementType.DataType, elementType.Size, array.GetValue(i));
            }

            // Write parameter size
            Write(packet.Length);

            // Write parameter data
            Write(packet.ToArray());
        }

        private void WriteParameter(PgOutputPacket packet, PgDataType dataType, int size, object value)
        {
            switch (dataType)
            {
                case PgDataType.Binary:
                    {
                        byte[] paramValue = value as byte[]; 
                        packet.Write(paramValue.Length);
                        packet.Write(paramValue);
                    }
                    break;

                case PgDataType.Byte:
                    packet.Write(size);
                    packet.WriteByte((byte)value);
                    break;

                case PgDataType.Boolean:
                    packet.Write(size);
                    packet.WriteByte(Convert.ToByte(value));
                    break;

                case PgDataType.Char:
                case PgDataType.VarChar:
                case PgDataType.Text:
                    packet.WriteString(value.ToString());
                    break;

                case PgDataType.Int2:
                    packet.Write(size);
                    packet.Write(Convert.ToInt16(value));
                    break;

                case PgDataType.Int4:
                    packet.Write(size);
                    packet.Write(Convert.ToInt32(value));
                    break;

                case PgDataType.Int8:
                    packet.Write(size);
                    packet.Write(Convert.ToInt64(value));
                    break;

                case PgDataType.Interval:
                    packet.Write(size);
                    packet.WriteInterval(TimeSpan.Parse(value.ToString()));
                    break;

                case PgDataType.Decimal:
                    {
                        string paramValue = Convert.ToDecimal(value).ToString(CultureInfo.InvariantCulture);
                        packet.Write(_serverConfig.ClientEncoding.GetByteCount(paramValue));
                        packet.WriteString(paramValue);
                    }
                    break;

                case PgDataType.Double:
                    packet.Write(size);
                    packet.Write(Convert.ToDouble(value));
                    break;

                case PgDataType.Float:
                    {
                        string paramValue = Convert.ToSingle(value).ToString(CultureInfo.InvariantCulture);
                        packet.Write(_serverConfig.ClientEncoding.GetByteCount(paramValue));
                        packet.WriteString(paramValue);
                    }
                    break;

                case PgDataType.Currency:
                    packet.Write(size);
                    packet.Write(Convert.ToInt32(Convert.ToSingle(value) * 100));
                    break;

                case PgDataType.Date:
                    packet.Write(size);
                    packet.WriteDate(Convert.ToDateTime(value));
                    break;

                case PgDataType.Time:
                    packet.WriteTime(Convert.ToDateTime(value));
                    break;

                case PgDataType.TimeWithTZ:
                    packet.WriteTimeWithTZ(Convert.ToDateTime(value));
                    break;

                case PgDataType.Timestamp:
                    packet.WriteTimestamp(Convert.ToDateTime(value));
                    break;

                case PgDataType.TimestampWithTZ:
                    packet.WriteTimestampWithTZ(Convert.ToDateTime(value));
                    break;

                case PgDataType.Point:
                    packet.Write(size);
                    packet.Write((PgPoint)value);
                    break;

                case PgDataType.Circle:
                    packet.Write(size);
                    packet.Write((PgCircle)value);
                    break;

                case PgDataType.Line:
                    packet.Write(size);
                    packet.Write((PgLine)value);
                    break;

                case PgDataType.LSeg:
                    packet.Write(size);
                    packet.Write((PgLSeg)value);
                    break;

                case PgDataType.Box:
                    packet.Write(size);
                    packet.Write((PgBox)value);
                    break;

                case PgDataType.Polygon:
                    PgPolygon polygon = (PgPolygon)value;
                    packet.Write((int)((size * polygon.Points.Length) + 4));
                    packet.Write(polygon);
                    break;

                case PgDataType.Path:
                    PgPath path = (PgPath)value;
                    packet.Write((int)((size * path.Points.Length) + 5));
                    packet.Write(path);
                    break;
            }
        }
    }
}