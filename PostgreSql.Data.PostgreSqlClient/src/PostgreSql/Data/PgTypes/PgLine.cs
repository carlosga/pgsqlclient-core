// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgLine
    {
        public static PgLine Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly PgPoint _startPoint;
        private readonly PgPoint _endPoint;

        public PgPoint StartPoint => _startPoint;
        public PgPoint EndPoint   => _endPoint;

        public PgLine(PgPoint startPoint, PgPoint endPoint)
        {
            _startPoint = startPoint;
            _endPoint   = endPoint;
        }

        public PgLine(double x1, double y1, double x2, double y2)
        {
            _startPoint = new PgPoint(x1, y1);
            _endPoint   = new PgPoint(x2, y2);
        }

        public static bool operator ==(PgLine left, PgLine right)
        {
            return (left.StartPoint == right.StartPoint && left.EndPoint == right.EndPoint);
        }

        public static bool operator !=(PgLine left, PgLine right)
        {
            return (left.StartPoint != right.StartPoint || left.EndPoint != right.EndPoint);
        }

        public override string ToString()
        {
            return String.Format("(({0},{1}),({2},{3}))"
                               , _startPoint.X
                               , _startPoint.Y
                               , _endPoint.X
                               , _endPoint.Y);
        }

        public override int GetHashCode() => _startPoint.GetHashCode() ^ _endPoint.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgLine))
            {
                return false;
            }

            PgLine value = (PgLine)obj;

            return ((PgLine)value) == this;
        }
    }
}