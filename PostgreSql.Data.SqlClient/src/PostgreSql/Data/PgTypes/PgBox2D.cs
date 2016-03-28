// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox2D
    {
        public static PgBox2D Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s cannot be null");
            }

            if (s.IndexOf("(") > 0)
            {
                s = s.Substring(s.IndexOf("(") + 1, s.IndexOf(")") - s.IndexOf("(") - 1);
            }

            string[] delimiters = new string[] { "," };
            string[] boxPoints  = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            return new PgBox2D(PgPoint2D.Parse(boxPoints[0]), PgPoint2D.Parse(boxPoints[1]));
        }

        private readonly PgPoint2D _upperRight;
        private readonly PgPoint2D _lowerLeft;

        public PgPoint2D UpperRight => _upperRight;
        public PgPoint2D LowerLeft  => _lowerLeft;

        public PgBox2D(PgPoint2D lowerLeft, PgPoint2D upperRight)
        {
            _lowerLeft  = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox2D(double x1, double y1, double x2, double y2)
        {
            _lowerLeft  = new PgPoint2D(x1, y1);
            _upperRight = new PgPoint2D(x2, y2);
        }

        public static bool operator ==(PgBox2D left, PgBox2D right)
        {
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static bool operator !=(PgBox2D left, PgBox2D right)
        {
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        public override string ToString() => $"BOX({_lowerLeft.ToString()},{_upperRight.ToString()})";
        public override int GetHashCode() => (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgBox2D))
            {
                return false;
            }

            PgBox2D value = (PgBox2D)obj;

            return ((PgBox2D)value) == this;
        }
    }
}
