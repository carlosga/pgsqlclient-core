// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    public struct PgPoint3D
        : INullable, IEquatable<PgPoint3D>
    {
        public static readonly PgPoint3D Null = new PgPoint3D(false);

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

            double x = Double.Parse(pointCoords[0], TypeInfoProvider.InvariantCulture);
            double y = Double.Parse(pointCoords[1], TypeInfoProvider.InvariantCulture);
            double z = Double.Parse(pointCoords[2], TypeInfoProvider.InvariantCulture);

            return new PgPoint3D(x, y, z);
        }

        private readonly bool   _isNotNull;
        private readonly double _x;
        private readonly double _y;
        private readonly double _z;

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

        public double Z
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return _z;
            }
        }

        private PgPoint3D(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _x         = 0;
            _y         = 0;
            _z         = 0;
        }

        public PgPoint3D(double x, double y, double z)
        {
            _isNotNull = true;
            _x         = x;
            _y         = y;
            _z         = z;
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
            if (IsNull)
            {
                return TypeInfoProvider.NullString;
            }
            return String.Format(TypeInfoProvider.InvariantCulture, "{0} {1} {2}", _x, _y, _z);
        }

        public override int GetHashCode() => ((IsNull) ? 0 : _x.GetHashCode() ^ _y.GetHashCode() ^ _z.GetHashCode());

        public bool Equals(PgPoint3D other) => (this == other);

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
            return Equals((PgPoint3D)obj);
        }
    }
}
