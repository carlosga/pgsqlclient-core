// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBit
        : INullable, IComparable<PgBit>, IComparable, IEquatable<PgBit>
    {
        public static readonly PgBit MaxValue = new PgBit(true, 1);
        public static readonly PgBit MinValue = new PgBit(true, 0);
        public static readonly PgBit Null     = new PgBit(false);
        public static readonly PgBit One      = MaxValue;
        public static readonly PgBit Zero     = MinValue;

        private readonly bool _isNotNull;
        private readonly byte _value;

        private PgBit(bool isNotNull)
            : this(isNotNull, 0)
        {
        }

        private PgBit(bool isNotNull, byte value)
        {
            _isNotNull = isNotNull;
            _value     = value;
        }

        public PgBit(byte value)
        {
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            _value     = value;
            _isNotNull = true;
        }

        public static PgBit operator -(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value - y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBoolean operator !=(PgBit x, PgBit y) => !(x == y);

        public static PgBit operator %(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value % y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator &(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value & y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator *(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value * y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator /(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value / y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator ^(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value ^ y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator |(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value | y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBit operator ~(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgBit)(~x._value);
        }

        public static PgBit operator +(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            int value = (x._value + y._value);
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgBit)value;
        }

        public static PgBoolean operator <(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgBit x, PgBit y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }
        
        public static explicit operator PgBit(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value ? One : Zero;
        }

        public static explicit operator PgBit(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return x.Value;
        }

        public static explicit operator byte(PgBit x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgBit(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }

        public static explicit operator PgBit(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }
        
        public static explicit operator PgBit(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }
        
        public static explicit operator PgBit(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }
        
        public static explicit operator PgBit(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }

        public static explicit operator PgBit(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }

        public static explicit operator PgBit(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgBit((byte)x.Value);
        }

        public static implicit operator PgBit(byte x)
        {
            return (PgBit)x;
        }

        public bool IsNull => !_isNotNull;

        public byte Value
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

        public static PgBit Add(PgBit x, PgBit y)        => (x + y);
        public static PgBit BitwiseAnd(PgBit x, PgBit y) => (x & y);
        public static PgBit BitwiseOr(PgBit x, PgBit y)  => (x | y);

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgBit))
            {
                return -1;
            }

            return CompareTo((PgBit)obj);
        }

        public int CompareTo(PgBit value)
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

        public static PgBit Divide(PgBit x, PgBit y) => (x / y);

        public bool Equals(PgBit other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgBit))
            {
                return false;
            }

            return Equals((PgBit)obj);
        }

        public static PgBoolean Equals(PgBit x, PgBit y) => (x == y);

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_value.GetHashCode());
        }

        public static PgBoolean GreaterThan(PgBit x, PgBit y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgBit x, PgBit y) => (x >= y);
        public static PgBoolean LessThan(PgBit x, PgBit y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgBit x, PgBit y)    => (x >= y);
        public static PgBit     Mod(PgBit x, PgBit y)                => (x % y);
        public static PgBit     Modulus(PgBit x, PgBit y)            => (x % y);
        public static PgBit     Multiply(PgBit x, PgBit y)           => (x * y);
        public static PgBoolean NotEquals(PgBit x, PgBit y)          => (x != y);
        public static PgBit     OnesComplement(PgBit x)              => ~x;

        public static PgBit Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Byte.Parse(s, TypeInfoProvider.InvariantCulture);
        }

        public static PgBit Subtract(PgBit x, PgBit y)  => (x - y);

        public PgBoolean ToPgBoolean()  => (PgBoolean)this;
        public PgByte    ToPgByte()     => (PgByte)this;
        public PgDecimal ToPgDecimal()  => (PgDecimal)this;
        public PgDouble  ToPgDouble()   => (PgDouble)this;
        public PgInt16   ToPgInt16()    => (PgInt16)this;
        public PgInt32   ToPgInt32()    => (PgInt32)this;
        public PgInt64   ToPgInt64()    => (PgInt64)this;
        public PgMoney   ToPgMoney()    => (PgMoney)this;
        public PgReal    ToPgReal()     => (PgReal)this;

        public override string ToString()
        {
            if (IsNull)
            {
                return TypeInfoProvider.NullString;
            }
            return _value.ToString(TypeInfoProvider.InvariantCulture);
        }
        
        public static PgBit Xor(PgBit x, PgBit y) => (x ^ y);
    }
}
