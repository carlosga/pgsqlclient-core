// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInt16
        : INullable, IComparable<PgInt16>, IComparable, IEquatable<PgInt16>
    {
        public static readonly PgInt16 MaxValue =  32767;
        public static readonly PgInt16 MinValue = -32768;
        public static readonly PgInt16 Null     = new PgInt16(false);
        public static readonly PgInt16 Zero     = 0;

        private readonly bool  _isNotNull;
        private readonly short _value;

        public PgInt16(bool isNotNull)
        { 
            _isNotNull = isNotNull;
            _value     = 0;
        }

        public PgInt16(short value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgInt16 operator -(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            int value = -x._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator -(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value - y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgBoolean operator !=(PgInt16 x, PgInt16 y) => !(x == y);

        public static PgInt16 operator %(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value % y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator &(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value & y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator *(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value * y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator /(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value / y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator ^(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value ^ y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator |(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value | y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator ~(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            int value = ~x._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgInt16 operator +(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = x._value + y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)value;
        }

        public static PgBoolean operator <(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }
        
        public static PgBoolean operator <=(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgInt16(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value;
        }

        public static explicit operator PgInt16(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.ByteValue;
        }

        public static explicit operator PgInt16(PgNumeric x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static explicit operator PgInt16(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static explicit operator short(PgInt16 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgInt16(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static explicit operator PgInt16(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static explicit operator PgInt16(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static explicit operator PgInt16(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgInt16)x.Value;
        }

        public static implicit operator PgInt16(short x)  => new PgInt16(x);

        public static implicit operator PgInt16(PgByte x) => new PgInt16(x.Value);

        public bool IsNull => !_isNotNull;

        public short Value
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
            if (obj == null || !(obj is PgInt16))
            {
                return -1;
            }

            return CompareTo((PgInt16)obj);
        }

        public int CompareTo(PgInt16 value)
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

        public bool Equals(PgInt16 other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgInt16))
            {
                return false;
            }
            return Equals((PgInt16)obj);
        }

        public override int GetHashCode() => ((IsNull) ? 0 : _value.GetHashCode());

        public static PgInt16   Add(PgInt16 x, PgInt16 y)                => (x + y);
        public static PgInt16   BitwiseAnd(PgInt16 x, PgInt16 y)         => (x & y);
        public static PgInt16   BitwiseOr(PgInt16 x, PgInt16 y)          => (x | y);
        public static PgInt16   Divide(PgInt16 x, PgInt16 y)             => (x / y);
        public static PgBoolean Equals(PgInt16 x, PgInt16 y)             => (x == y);
        public static PgBoolean GreaterThan(PgInt16 x, PgInt16 y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgInt16 x, PgInt16 y) => (x >= y);
        public static PgBoolean LessThan(PgInt16 x, PgInt16 y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgInt16 x, PgInt16 y)    => (x <= y);
        public static PgInt16   Mod(PgInt16 x, PgInt16 y)                => (x % y);
        public static PgInt16   Modulus(PgInt16 x, PgInt16 y)            => (x % y);
        public static PgInt16   Multiply(PgInt16 x, PgInt16 y)           => (x * y);
        public static PgBoolean NotEquals(PgInt16 x, PgInt16 y)          => (x != y);
        public static PgInt16   OnesComplement(PgInt16 x)                => ~x;
        public static PgInt16   Subtract(PgInt16 x, PgInt16 y)           => (x - y);
        public static PgInt16   Xor(PgInt16 x, PgInt16 y)                => (x ^ y);

        public static PgInt16 Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Int16.Parse(s, TypeInfoProvider.InvariantCulture);
        }

        public PgBit     ToPgBit()     => (PgBit)this;
        public PgBoolean ToPgBoolean() => (PgBoolean)this;
        public PgByte    ToPgByte()    => (PgByte)this;
        public PgNumeric ToPgNumeric() => (PgNumeric)this;
        public PgDouble  ToPgDouble()  => (PgDouble)this;
        public PgInt32   ToPgInt32()   => (PgInt32)this;
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
