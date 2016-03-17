// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox3D
    {
        private readonly PgPoint3D _upperRight;
        private readonly PgPoint3D _lowerLeft;

        public PgPoint3D UpperRight
        {
            get { return _upperRight; }
        }

        public PgPoint3D LowerLeft
        {
            get { return _lowerLeft; }
        }

        public PgBox3D(PgPoint3D lowerLeft, PgPoint3D upperRight)
        {
            _lowerLeft  = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox3D(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            _lowerLeft  = new PgPoint3D(x1, y1, z1);
            _upperRight = new PgPoint3D(x2, y2, z2);
        }

        public static bool operator ==(PgBox3D left, PgBox3D right)
        {
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static bool operator !=(PgBox3D left, PgBox3D right)
        {
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        public override string ToString()
        {
            return String.Format("BOX3D({0},{1})", _lowerLeft.ToString(), _upperRight.ToString());
        }

        public override int GetHashCode()
        {
            return (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PgBox3D))
            {
                return false;
            }
            if (obj == null)
            {
                return false;
            }

            PgBox3D value = (PgBox3D)obj;

            return ((PgBox3D)value) == this;
        }

        public static PgBox3D Parse(string s)
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

            return new PgBox3D(PgPoint3D.Parse(boxPoints[0]), PgPoint3D.Parse(boxPoints[1]));
        }
    }
}