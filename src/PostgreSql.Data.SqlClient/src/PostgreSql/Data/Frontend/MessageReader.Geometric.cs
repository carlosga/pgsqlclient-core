// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private PgPoint  ReadPoint()  => new PgPoint(ReadDouble(), ReadDouble());
        private PgCircle ReadCircle() => new PgCircle(ReadPoint(), ReadDouble());
        private PgLine   ReadLine()   => new PgLine(ReadPoint(), ReadPoint());
        private PgLSeg   ReadLSeg()   => new PgLSeg(ReadPoint(), ReadPoint());
        private PgBox    ReadBox()    => new PgBox(ReadPoint(), ReadPoint());

        private PgPolygon ReadPolygon()
        {
            PgPoint[] points = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = ReadPoint();
            }

            return new PgPolygon(points);
        }

        private PgPath ReadPath()
        {
            bool isClosedPath = ReadBoolean();
            var  points       = new PgPoint[ReadInt32()];

            for (int i = 0; i < points.Length; ++i)
            {
                points[i] = ReadPoint();
            }

            return new PgPath(points, isClosedPath);
        }
    }
}