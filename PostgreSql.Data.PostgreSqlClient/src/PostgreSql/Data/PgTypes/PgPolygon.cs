// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPolygon
    {
        public static PgPolygon Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly PgPoint[] _points;

        public PgPoint[] Points => _points;

        public PgPolygon(PgPoint[] points)
        {
            _points = (PgPoint[])points.Clone();
        }

        public static bool operator ==(PgPolygon left, PgPolygon right)
        {
            bool equals = false;

            if (left.Points.Length == right.Points.Length)
            {
                equals = true;

                for (int i = 0; i < left.Points.Length; ++i)
                {
                    if (left.Points[i] != right.Points[i])
                    {
                        equals = false;
                        break;
                    }
                }
            }

            return equals;
        }

        public static bool operator !=(PgPolygon left, PgPolygon right)
        {
            bool notequals = true;

            if (left.Points.Length == right.Points.Length)
            {
                notequals = false;

                for (int i = 0; i < left.Points.Length; ++i)
                {
                    if (left.Points[i] != right.Points[i])
                    {
                        notequals = true;
                        break;
                    }
                }
            }

            return notequals;
        }

        public override string ToString()
        {
            System.Text.StringBuilder b = new System.Text.StringBuilder();

            b.Append("(");

            for (int i = 0; i < _points.Length; i++)
            {
                if (b.Length > 1)
                {
                    b.Append(",");
                }

                b.Append(_points[i].ToString());
            }

            b.Append(")");

            return b.ToString();
        }

        public override int GetHashCode() => (_points.GetHashCode());

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPolygon))
            {
                return false;
            }

            PgPolygon value = (PgPolygon)obj;

            return ((PgPolygon)value) == this;
        }
    }
}