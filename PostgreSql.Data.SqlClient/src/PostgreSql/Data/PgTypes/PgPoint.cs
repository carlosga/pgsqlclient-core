// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Globalization;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint
        : INullable, IEquatable<PgPoint>
    {
        public static readonly PgPoint Null = new PgPoint(false);

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

        private PgPoint(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _x         = 0;
            _y         = 0;
        }

        public PgPoint(double x, double y)
        {
            _isNotNull = true;
            _x         = x;
            _y         = y;
        }

        public static bool operator ==(PgPoint lhs, PgPoint rhs)
        {
            return (lhs.X == rhs.X && lhs.Y == rhs.Y);
        }

        public static bool operator !=(PgPoint lhs, PgPoint rhs)
        {
            return (lhs.X != rhs.X || lhs.Y != rhs.Y);
        }

        public PgString ToPgString()
        {
            return ToString();
        }

        public override string ToString()
        {
            if (IsNull)
            {
                return PgTypeInfoProvider.NullString;
            }
            return String.Format(PgTypeInfoProvider.InvariantCulture, "({0},{1})", _x, _y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_x.GetHashCode() ^ _y.GetHashCode());
        }

        public bool Equals(PgPoint other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgPoint))
            {
                return false;
            }
            return Equals((PgPoint)obj);
        }
    }
}
