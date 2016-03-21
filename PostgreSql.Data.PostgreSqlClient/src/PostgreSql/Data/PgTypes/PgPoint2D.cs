// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint2D
    {
        public static PgPoint2D Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s cannot be null");
            }

            if (s.IndexOf("(") > 0)
            {
                s = s.Substring(s.IndexOf("("), s.Length - s.IndexOf("("));
            }

            string[] pointCoords = s.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (pointCoords == null || pointCoords.Length != 2)
            {
                throw new ArgumentException("s is not a valid point.");
            }

            double x = Double.Parse(pointCoords[0], CultureInfo.InvariantCulture);
            double y = Double.Parse(pointCoords[1], CultureInfo.InvariantCulture);

            return new PgPoint2D(x, y);
        }

        private readonly double _x;
        private readonly double _y;

        public double X => _x;
        public double Y => _y;

        public PgPoint2D(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public static bool operator ==(PgPoint2D left, PgPoint2D right)
        {
            return (left.X == right.X && left.Y == right.Y);
        }

        public static bool operator !=(PgPoint2D left, PgPoint2D right)
        {
            return (left.X != right.X || left.Y != right.Y);
        }

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return String.Format(culture, "{0} {1}", _x, _y);
        }

        public override int GetHashCode() => (_x.GetHashCode() ^ _y.GetHashCode());

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPoint2D))
            {
                return false;
            }

            PgPoint2D value = (PgPoint2D)obj;

            return ((PgPoint2D)value) == this;
        }
    }
}