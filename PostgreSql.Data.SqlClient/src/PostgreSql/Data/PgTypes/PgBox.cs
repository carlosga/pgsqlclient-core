// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBox
        : INullable, IEquatable<PgBox>
    {
        public static readonly PgBox Null = new PgBox(false); 

        public static PgBox Parse(string s)
        {
            throw new NotSupportedException();
        }

        private readonly bool    _isNotNull;
        private readonly PgPoint _upperRight;
        private readonly PgPoint _lowerLeft;
        
        public bool IsNull => !_isNotNull;

        public PgPoint UpperRight
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

        public PgPoint LowerLeft
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

        private PgBox(bool isNotNull)
        {
            _isNotNull  = isNotNull;
            _lowerLeft  = PgPoint.Null;
            _upperRight = PgPoint.Null;
        }

        public PgBox(PgPoint lowerLeft, PgPoint upperRight)
        {
            _isNotNull  = false;
            _lowerLeft  = lowerLeft;
            _upperRight = upperRight;
        }

        public PgBox(double x1, double y1, double x2, double y2)
        {
            _isNotNull  = false;
            _lowerLeft  = new PgPoint(x1, y1);
            _upperRight = new PgPoint(x2, y2);
        }

        public static PgBoolean operator ==(PgBox left, PgBox right)
        {
            if (left.IsNull || right.IsNull)
            {
                return PgBoolean.Null;
            }
            return (left.UpperRight == right.UpperRight && left.LowerLeft == right.LowerLeft);
        }

        public static PgBoolean operator !=(PgBox left, PgBox right)
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
                return PgTypeInfoProvider.NullString;
            }
            return String.Format(PgTypeInfoProvider.InvariantCulture
                               , "(({0},{1}),({2},{3}))"
                               , _lowerLeft.X
                               , _lowerLeft.Y
                               , _upperRight.X
                               , _upperRight.Y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (UpperRight.GetHashCode() ^ LowerLeft.GetHashCode());
        }

        public bool Equals(PgBox other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgBox))
            {
                return false;
            }

            return Equals((PgBox)obj);
        }
    }
}
