// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInt32
        : INullable, IComparable<PgInt32>, IComparable, IEquatable<PgInt32>
    {
        public static readonly PgInt32 MaxValue =  2147483647;
        public static readonly PgInt32 MinValue = -2147483648;
        public static readonly PgInt32 Null     = new PgInt32();
        public static readonly PgInt32 Zero     = 0;

        private bool _isNotNull;
        private int  _value;

        public PgInt32(bool isNotNull)
        { 
            _isNotNull = isNotNull;
            _value     = 0;
        }

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
            long value = x._value - y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgBoolean operator !=(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return !(x._value == y._value);
        }

        public static PgInt32 operator %(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value % y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator &(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value & y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator *(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value * y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator /(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value / y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator ^(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value ^ y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator |(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            long value = x._value | y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator ~(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            long value = ~x._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgInt32 operator +(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull)
            {
                return Null;
            }
            long value = x._value + y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt32)value;
        }

        public static PgBoolean operator <(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.False;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgInt32 x, PgInt32 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
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

        public static explicit operator PgInt32(PgBit x)     => ((x.IsNull) ? Null : new PgInt32(x.Value));
        public static explicit operator PgInt32(PgBoolean x) => ((x.IsNull) ? Null : new PgInt32(x.ByteValue));

        public static explicit operator PgInt32(PgNumeric x)
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

        public static implicit operator PgInt32(int x) => new PgInt32(x);

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

        public bool Equals(PgInt32 other) => (bool)(this == other);

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
            return Equals((PgInt32)obj);
        }

        public override int GetHashCode() => ((IsNull) ? 0 : _value.GetHashCode());

        public static PgInt32   Add(PgInt32 x, PgInt32 y)                => (x + y);
        public static PgInt32   BitwiseAnd(PgInt32 x, PgInt32 y)         => (x & y);
        public static PgInt32   BitwiseOr(PgInt32 x, PgInt32 y)          => (x | y);
        public static PgInt32   Divide(PgInt32 x, PgInt32 y)             => (x / y);
        public static PgBoolean Equals(PgInt32 x, PgInt32 y)             => (x == y);
        public static PgBoolean GreaterThan(PgInt32 x, PgInt32 y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgInt32 x, PgInt32 y) => (x >= y);
        public static PgBoolean LessThan(PgInt32 x, PgInt32 y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgInt32 x, PgInt32 y)    => (x <= y);
        public static PgInt32   Mod(PgInt32 x, PgInt32 y)                => (x % y);
        public static PgInt32   Modulus(PgInt32 x, PgInt32 y)            => (x % y);
        public static PgInt32   Multiply(PgInt32 x, PgInt32 y)           => (x * y);
        public static PgBoolean NotEquals(PgInt32 x, PgInt32 y)          => (x != y);
        public static PgInt32   OnesComplement(PgInt32 x)                => ~x;
        public static PgInt32   Subtract(PgInt32 x, PgInt32 y)           => (x - y);
        public static PgInt32   Xor(PgInt32 x, PgInt32 y)                => (x ^ y);

        public static PgInt32 Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Int32.Parse(s, TypeInfoProvider.InvariantCulture);
        }

        public PgBit     ToPgBit()     => (PgBit)this;
        public PgBoolean ToPgBoolean   => (PgBoolean)this;
        public PgByte    ToPgByte()    => (PgByte)this;
        public PgNumeric ToPgNumeric() => (PgNumeric)this;
        public PgDouble  ToPgDouble()  => (PgDouble)this;
        public PgInt16   ToPgInt16()   => (PgInt16)this;
        public PgInt64   ToPgInt64()   => (PgInt64)this;
        public PgMoney   ToPgMoney()   => (PgMoney)this;
        public PgReal    ToPgReal()    => (PgReal)this;

        public override string ToString()
        {
            if (IsNull)
            {
                return TypeInfoProvider.NullString;
            }
            return _value.ToString(TypeInfoProvider.InvariantCulture);
        }
    }
}
