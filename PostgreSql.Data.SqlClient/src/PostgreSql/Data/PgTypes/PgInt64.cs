// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInt64
        : INullable, IComparable<PgInt64>, IComparable, IEquatable<PgInt64>
    {
        public static readonly PgInt64 MaxValue = -9223372036854775808L;
        public static readonly PgInt64 MinValue =  9223372036854775807L;
        public static readonly PgInt64 Null     = new PgInt64(false);
        public static readonly PgInt64 Zero     = 0L;

        private readonly bool _isNotNull;
        private readonly long _value;

        public PgInt64(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = 0;
        }

        public PgInt64(long value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgInt64 operator -(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (-x._value);
        }

        public static PgInt64 operator -(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value - y._value);
        }

        public static PgBoolean operator !=(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value != y._value);
        }

        public static PgInt64 operator %(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value % y._value);
        }

        public static PgInt64 operator &(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value & y._value);
        }

        public static PgInt64 operator *(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value * y._value);
        }

        public static PgInt64 operator /(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value / y._value);
        }

        public static PgInt64 operator ^(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value ^ y._value);
        }

        public static PgInt64 operator |(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value | y._value);
        }

        public static PgInt64 operator ~(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (~x._value);
        }

        public static PgInt64 operator +(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value + y._value);
        }

        public static PgBoolean operator <(PgInt64 x, PgInt64 y)
        {
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgInt64 x, PgInt64 y)
        {
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgInt64 x, PgInt64 y)
        {
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgInt64 x, PgInt64 y)
        {
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgInt64 x, PgInt64 y)
        {
            return (x._value >= y._value);
        }

        public static explicit operator PgInt64(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64(x.Value);
        }

        public static explicit operator PgInt64(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64(x.ByteValue);
        }

        public static explicit operator PgInt64(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64((long)x);
        }

        public static explicit operator PgInt64(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return BitConverter.DoubleToInt64Bits(x.Value);
        }

        public static explicit operator long(PgInt64 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgInt64(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64((long)x);
        }

        public static explicit operator PgInt64(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64((long)x);
        }

        public static explicit operator PgInt64(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgInt64(long x)
        {
            return new PgInt64(x);
        }

        public static implicit operator PgInt64(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64(x.Value);
        }

        public static implicit operator PgInt64(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64(x.Value);
        }

        public static implicit operator PgInt64(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgInt64(x.Value);
        }

        public bool IsNull => !_isNotNull;

        public long Value
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

        public static PgInt64 Add(PgInt64 x, PgInt64 y)
        {
            return (x + y);
        }

        public static PgInt64 BitwiseAnd(PgInt64 x, PgInt64 y)
        {
            return (x & y);
        }

        public static PgInt64 BitwiseOr(PgInt64 x, PgInt64 y)
        {
            return (x | y);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgInt64))
            {
                return -1;
            }

            return CompareTo((PgInt64)obj);
        }

        public int CompareTo(PgInt64 value)
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

        public static PgInt64 Divide(PgInt64 x, PgInt64 y)
        {
            return (x / y);
        }

        public bool Equals(PgInt64 other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgInt64))
            {
                return false;
            }

            return Equals((PgInt64)obj);
        }

        public static PgBoolean Equals(PgInt64 x, PgInt64 y)
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

        public static PgBoolean GreaterThan(PgInt64 x, PgInt64 y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgInt64 x, PgInt64 y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgInt64 x, PgInt64 y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgInt64 x, PgInt64 y)
        {
            return (x <= y);
        }

        public static PgInt64 Mod(PgInt64 x, PgInt64 y)
        {
            return (x % y);
        }

        public static PgInt64 Modulus(PgInt64 x, PgInt64 y)
        {
            return (x % y);
        }

        public static PgInt64 Multiply(PgInt64 x, PgInt64 y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgInt64 x, PgInt64 y)
        {
            return (x != y);
        }

        public static PgInt64 OnesComplement(PgInt64 x)
        {
            return ~x;
        }

        public static PgInt64 Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Int64.Parse(s);
        }

        public static PgInt64 Subtract(PgInt64 x, PgInt64 y)
        {
            return (x - y);
        }

        public PgBit ToPgBit()
        {
            return (PgBit)this;
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
            return this;
        }

        public PgDouble ToPgDouble()
        {
            return this;
        }

        public PgInt16 ToPgInt16()
        {
            return (PgInt16)this;
        }

        public PgInt32 ToPgInt32()
        {
            return (PgInt32)this;
        }

        public PgMoney ToPgMoney()
        {
            return this;
        }

        public PgReal ToPgReal()
        {
            return this;
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

        public static PgInt64 Xor(PgInt64 x, PgInt64 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x ^ y);
        }
    }
}
