// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void Write(PgPoint point)
        {
            EnsureCapacity(16);

            Write(point.X);
            Write(point.Y);
        }

        private void Write(PgCircle circle)
        {
            EnsureCapacity(24);

            Write(circle.Center);
            Write(circle.Radius);
        }

        private void Write(PgLine line)
        {
            EnsureCapacity(32);

            Write(line.StartPoint);
            Write(line.EndPoint);
        }

        private void Write(PgLSeg lseg)
        {
            EnsureCapacity(32);

            Write(lseg.StartPoint);
            Write(lseg.EndPoint);
        }

        private void Write(PgBox box)
        {
            EnsureCapacity(32);

            Write(box.UpperRight);
            Write(box.LowerLeft);
        }

        private void Write(PgPolygon value)
        {
            Write(value.Points.Length);

            for (int i = 0; i < value.Points.Length; ++i)
            {
                Write(value.Points[i]);
            }
        }

        private void Write(PgPath value)
        {
            Write(value.IsClosedPath);
            Write(value.Points.Length);

            for (int i = 0; i < value.Points.Length; ++i)
            {
                Write(value.Points[i]);
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
    }
}