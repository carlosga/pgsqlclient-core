// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint
    {
        private readonly double _x;
        private readonly double _y;

        public double X
        {
            get { return _x; }
        }

        public double Y
        {
            get { return _y; }
        }

        public PgPoint(double x, double y)
        {
            _x = x;
            _y = y;
        }

        public static bool operator ==(PgPoint left, PgPoint right)
        {
            return (left.X == right.X && left.Y == right.Y);
        }

        public static bool operator !=(PgPoint left, PgPoint right)
        {
            return (left.X != right.X || left.Y != right.Y);
        }

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return String.Format(culture, "({0},{1})", _x, _y);
        }

        public override int GetHashCode()
        {
            return (_x.GetHashCode() ^ _y.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgPoint))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgPoint value = (PgPoint)obj;

            return ((PgPoint)value) == this;
        }

        public static PgPoint Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s cannot be null");
            }

            string[] delimiters  = new string[] { "," };
            string[] pointCoords = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (pointCoords == null || pointCoords.Length != 2)
            {
                throw new ArgumentException("s is not a valid point.");
            }

            return new PgPoint(Double.Parse(pointCoords[0]), Double.Parse(pointCoords[1]));
        }
    }
}