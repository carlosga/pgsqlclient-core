// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox3D
        : INullable, IEquatable<PgBox3D>
    {
        public static readonly PgBox3D Null = new PgBox3D(false);

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

        private readonly bool      _isNotNull;
        private readonly PgPoint3D _upperRight;
        private readonly PgPoint3D _lowerLeft;

        public bool IsNull => !_isNotNull;

        public PgPoint3D UpperRight
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _upperRight;
            }
        }

        public PgPoint3D LowerLeft
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _lowerLeft;
            }
        }

        private PgBox3D(bool isNotNull)
        {
            _isNotNull  = isNotNull;
            _upperRight = PgPoint3D.Null;
            _lowerLeft  = PgPoint3D.Null;
        }

        public PgBox3D(PgPoint3D lowerLeft, PgPoint3D upperRight)
        {
            _isNotNull  = false;
            _lowerLeft  = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox3D(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            _isNotNull  = false;
            _lowerLeft  = new PgPoint3D(x1, y1, z1);
            _upperRight = new PgPoint3D(x2, y2, z2);
        }

        public static PgBoolean operator ==(PgBox3D left, PgBox3D right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static PgBoolean operator !=(PgBox3D left, PgBox3D right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
        }

        public override string ToString() => $"BOX3D({_lowerLeft.ToString()},{_upperRight.ToString()})";
        public override int GetHashCode() => (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());

        public bool Equals(PgBox3D other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgBox3D))
            {
                return false;
            }
            return Equals((PgBox3D)obj);
        }
    }
}
