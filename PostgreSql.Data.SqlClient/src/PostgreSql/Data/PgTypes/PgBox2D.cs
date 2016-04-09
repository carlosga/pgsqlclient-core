// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox2D
        : INullable, IEquatable<PgBox2D>
    {
        public static readonly PgBox2D Null = new PgBox2D(false);

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

        private readonly bool      _isNotNull;
        private readonly PgPoint2D _upperRight;
        private readonly PgPoint2D _lowerLeft;

        public bool IsNull => !_isNotNull;
        
        public PgPoint2D UpperRight
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

        public PgPoint2D LowerLeft
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

        private PgBox2D(bool isNotNull)
        {
            _isNotNull  = isNotNull;
            _upperRight = PgPoint2D.Null;
            _lowerLeft  = PgPoint2D.Null;
        }

        public PgBox2D(PgPoint2D lowerLeft, PgPoint2D upperRight)
        {
            _isNotNull  = true;
            _lowerLeft  = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox2D(double x1, double y1, double x2, double y2)
        {
            _isNotNull  = true;
            _lowerLeft  = new PgPoint2D(x1, y1);
            _upperRight = new PgPoint2D(x2, y2);
        }

        public static PgBoolean operator ==(PgBox2D left, PgBox2D right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static PgBoolean operator !=(PgBox2D left, PgBox2D right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.UpperRight != right.UpperRight || left.LowerLeft != right.LowerLeft);
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
            return $"BOX({_lowerLeft},{_upperRight})";
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());
        }

        public bool Equals(PgBox2D other)
        {
            return (this == other).Value;
        }

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

            return Equals((PgBox2D)obj);
        }
    }
}
