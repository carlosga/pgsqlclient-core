// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint2D
        : INullable, IEquatable<PgPoint2D>
    {
        public static readonly PgPoint2D Null = new PgPoint2D(false);

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

            double x = Double.Parse(pointCoords[0], TypeInfoProvider.InvariantCulture);
            double y = Double.Parse(pointCoords[1], TypeInfoProvider.InvariantCulture);

            return new PgPoint2D(x, y);
        }

        private readonly bool   _isNotNull;
        private readonly double _x;
        private readonly double _y;

        public bool IsNull => !_isNotNull;

        public double X
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _x;
            }
        }

        public double Y
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _y;
            }
        }

        private PgPoint2D(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _x         = 0;
            _y         = 0;
        }

        public PgPoint2D(double x, double y)
        {
            _isNotNull = true;
            _x         = x;
            _y         = y;
        }

        public static bool operator ==(PgPoint2D left, PgPoint2D right)
        {
            return (left.X == right.X && left.Y == right.Y);
        }

        public static bool operator !=(PgPoint2D left, PgPoint2D right)
        {
            return (left.X != right.X || left.Y != right.Y);
        }

        public PgString ToPgString()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (IsNull)
            {
                return TypeInfoProvider.NullString;
            }
            return String.Format(TypeInfoProvider.InvariantCulture, "{0} {1}", _x, _y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_x.GetHashCode() ^ _y.GetHashCode());;
        }

        public bool Equals(PgPoint2D other)
        {
            return (this == other);
        }

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
            return Equals((PgPoint2D)obj);
        }
    }
}
