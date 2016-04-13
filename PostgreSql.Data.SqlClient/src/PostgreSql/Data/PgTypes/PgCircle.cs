// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    public struct PgCircle
        : INullable, IEquatable<PgCircle>
    {
        public static readonly PgCircle Null = new PgCircle(false);

        public static PgCircle Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly bool    _isNotNull;
        private readonly PgPoint _center;
        private readonly double  _radius;

        public bool IsNull => !_isNotNull;

        public PgPoint Center 
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                
                return _center;
            }
        }

        public double Radius
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                
                return _radius;
            }
        }

        private PgCircle(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _center    = PgPoint.Null;
            _radius    = 0;
        }

        public PgCircle(PgPoint center, double radius)
        {
            _isNotNull = true;
            _center    = center;
            _radius    = radius;
        }

        public PgCircle(double x, double y, double radius)
        {
            _isNotNull = true;
            _center    = new PgPoint(x, y);
            _radius    = radius;
        }

        public static PgBoolean operator ==(PgCircle left, PgCircle right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.Center == right.Center && left.Radius == right.Radius);
        }

        public static PgBoolean operator !=(PgCircle left, PgCircle right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.Center != right.Center || left.Radius != right.Radius);
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
            return $"<{_center},{_radius}>";
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _center.GetHashCode() ^ _radius.GetHashCode();
        }

        public bool Equals(PgCircle other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgCircle))
            {
                return false;
            }

            return Equals((PgCircle)obj);
        }
    }
}
