// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint3D
    {
        public static PgPoint3D Parse(string s)
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

            if (pointCoords == null || pointCoords.Length != 3)
            {
                throw new ArgumentException("s is not a valid point.");
            }

            double x = Double.Parse(pointCoords[0], System.Globalization.CultureInfo.InvariantCulture);
            double y = Double.Parse(pointCoords[1], System.Globalization.CultureInfo.InvariantCulture);
            double z = Double.Parse(pointCoords[2], System.Globalization.CultureInfo.InvariantCulture);

            return new PgPoint3D(x, y, z);
        }

        private readonly double _x;
        private readonly double _y;
        private readonly double _z;

        public double X => _x;
        public double Y => _y;
        public double Z => _z;

        public PgPoint3D(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public static bool operator ==(PgPoint3D lhs, PgPoint3D rhs)
        {
            return (lhs.X == rhs.X && lhs.Y == rhs.Y && lhs.Z == rhs.Z);
        }

        public static bool operator !=(PgPoint3D lhs, PgPoint3D rhs)
        {
            return (lhs.X != rhs.X || lhs.Y != rhs.Y || lhs.Z != rhs.Z);
        }

        public override string ToString()
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            return String.Format(culture, "{0} {1} {2}", _x, _y, _z);
        }

        public override int GetHashCode() => (_x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode());

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPoint3D))
            {
                return false;
            }

            PgPoint3D value = (PgPoint3D)obj;

            return ((PgPoint3D)value) == this;
        }
    }
}