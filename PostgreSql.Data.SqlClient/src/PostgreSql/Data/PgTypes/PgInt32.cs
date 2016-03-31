// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInt32
        : IComparable, INullable
    {
        public static readonly PgInt32 MaxValue =  2147483647;
        public static readonly PgInt32 MinValue = -2147483648;
        public static readonly PgInt32 Null     = new PgInt32();
        public static readonly PgInt32 Zero     = 0;

        private bool _isNotNull;
        private int  _value;

        public PgInt32(int value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgInt32 operator -(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return -x._value;
        }

        public static PgInt32 operator -(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x - y);
        }

        public static PgBoolean operator !=(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.False;
            }
            return !(x == y);
        }

        public static PgInt32 operator %(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x % y);
        }

        public static PgInt32 operator &(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value & y._value);
        }

        public static PgInt32 operator *(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x * y);
        }

        public static PgInt32 operator /(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value / y._value);
        }

        public static PgInt32 operator ^(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value ^ y._value);
        }

        public static PgInt32 operator |(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value | y._value);
        }

        public static PgInt32 operator ~(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (~x.Value);
        }

        public static PgInt32 operator +(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value + y._value);
        }

        public static PgBoolean operator <(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.False;
            }
            return (x < y);
        }

        public static PgBoolean operator <=(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.False;
            }
            return (x <= y);
        }

        public static PgBoolean operator ==(PgInt32 x, PgInt32 y)
        {
            if ((x.IsNull && !y.IsNull) || (!x.IsNull && y.IsNull))
            {
                return false;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgInt32(PgBoolean x)
        {
            return ((x.IsNull) ? Null : new PgInt32((int)(x.ByteValue)));
        }

        public static explicit operator PgInt32(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x > MaxValue || x < MinValue)
            {
                throw new OverflowException();
            }
            return (PgInt32)x;
        }

        public static explicit operator PgInt32(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x > MaxValue || x < MinValue)
            {
                throw new OverflowException();
            }
            return (PgInt32)x;
        }

        public static explicit operator int(PgInt32 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgInt32(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x > MaxValue || x < MinValue)
            {
                throw new OverflowException();
            }
            return (PgInt32)x;
        }

        public static explicit operator PgInt32(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x > MaxValue || x < MinValue)
            {
                throw new OverflowException();
            }
            return (PgInt32)x;
        }

        public static explicit operator PgInt32(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x > MaxValue || x < MinValue)
            {
                throw new OverflowException();
            }
            return (PgInt32)x;
        }

        public static explicit operator PgInt32(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgInt32(int x)
        {
            return new PgInt32(x);
        }

        public static implicit operator PgInt32(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt32(x.Value);
        }

        public static implicit operator PgInt32(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt32(x.Value);
        }

        public bool IsNull => !_isNotNull;

        public int Value
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }

                return _value;
            }
        }

        public static PgInt32 Add(PgInt32 x, PgInt32 y)
        {
            return (x + y);
        }

        public static PgInt32 BitwiseAnd(PgInt32 x, PgInt32 y)
        {
            return (x & y);
        }

        public static PgInt32 BitwiseOr(PgInt32 x, PgInt32 y)
        {
            return (x | y);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgInt32))
            {
                return -1;
            }

            return CompareTo((PgInt32)obj);
        }

        public int CompareTo(PgInt32 value)
        {
            if (IsNull)
            {
                return ((value.IsNull) ? 0 : -1);
            }
            else if (value.IsNull)
            {
                return 1;
            }

            if (this < value)
            {
                return -1;
            }
            if (this > value)
            {
                return 1;
            }
            return 0;
        }

        public static PgInt32 Divide(PgInt32 x, PgInt32 y)
        {
            return (x / y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgInt32))
            {
                return false;
            }
            return Equals(this, (PgInt32)obj).Value;
        }

        public static PgBoolean Equals(PgInt32 x, PgInt32 y)
        {
            return (x == y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static PgBoolean GreaterThan(PgInt32 x, PgInt32 y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgInt32 x, PgInt32 y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgInt32 x, PgInt32 y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgInt32 x, PgInt32 y)
        {
            return (x <= y);
        }

        public static PgInt32 Mod(PgInt32 x, PgInt32 y)
        {
            return (x % y);
        }

        public static PgInt32 Modulus(PgInt32 x, PgInt32 y)
        {
            return (x % y);
        }

        public static PgInt32 Multiply(PgInt32 x, PgInt32 y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgInt32 x, PgInt32 y)
        {
            return (x != y);
        }

        public static PgInt32 OnesComplement(PgInt32 x)
        {
            return ~x;
        }

        public static PgInt32 Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Int32.Parse(s, PgTypeInfoProvider.InvariantCulture);
        }

        public static PgInt32 Subtract(PgInt32 x, PgInt32 y)
        {
            return (x - y);
        }

        public PgBoolean ToPgBoolean()
        {
            return (PgBoolean)this;
        }

        public PgByte ToPgByte()
        {
            return (PgByte)this;
        }

        public PgDecimal ToPgDecimal()
        {
            return (PgDecimal)this;
        }

        public PgDouble ToPgDouble()
        {
            return (PgDouble)this;
        }

        public PgInt16 ToPgInt16()
        {
            return (PgInt16)this;
        }

        public PgInt64 ToPgInt64()
        {
            return (PgInt64)this;
        }

        public PgMoney ToPgMoney()
        {
            return (PgMoney)this;
        }

        public PgReal ToPgReal()
        {
            return (PgReal)this;
        }

        public PgString ToPgString()
        {
            return (PgString)this;
        }

        public override string ToString()
        {
            if (IsNull)
            {
                return PgTypeInfoProvider.NullString;
            }
            return _value.ToString(PgTypeInfoProvider.InvariantCulture);
        }

        public static PgInt32 Xor(PgInt32 x, PgInt32 y)
        {
            return (x ^ y);
        }
    }
}
