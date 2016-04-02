// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBit
        : INullable, IComparable<PgBit>, IComparable, IEquatable<PgBit>
    {
        public static readonly PgBit MaxValue = 1;
        public static readonly PgBit MinValue = 0;
        public static readonly PgBit Null     = new PgBit(false);
        public static readonly PgBit One      = new PgBit(1);
        public static readonly PgBit Zero     = new PgBit(0);

        private readonly bool _isNotNull;
        private readonly byte _value;

        private PgBit(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = 0;
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

        public static PgBoolean operator !=(PgBit x, PgBit y)
        {
            return !(x == y);
        }

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

        public static explicit operator PgBit(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
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

        public static PgBit Add(PgBit x, PgBit y)
        {
            return (x + y);
        }

        public static PgBit BitwiseAnd(PgBit x, PgBit y)
        {
            return (x & y);
        }

        public static PgBit BitwiseOr(PgBit x, PgBit y)
        {
            return (x | y);
        }

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

        public static PgBit Divide(PgBit x, PgBit y)
        {
            return (x / y);
        }

        public bool Equals(PgBit other)
        {
            return (this == other).Value;
        }

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

        public static PgBoolean Equals(PgBit x, PgBit y)
        {
            return (x == y);
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return (_value.GetHashCode());
        }

        public static PgBoolean GreaterThan(PgBit x, PgBit y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgBit x, PgBit y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgBit x, PgBit y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgBit x, PgBit y)
        {
            return (x >= y);
        }

        public static PgBit Mod(PgBit x, PgBit y)
        {
            return (x % y);
        }

        public static PgBit Modulus(PgBit x, PgBit y)
        {
            return (x % y);
        }

        public static PgBit Multiply(PgBit x, PgBit y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgBit x, PgBit y)
        {
            return (x != y);
        }

        public static PgBit OnesComplement(PgBit x)
        {
            return ~x;
        }

        public static PgBit Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Byte.Parse(s, PgTypeInfoProvider.InvariantCulture);
        }

        public static PgBit Subtract(PgBit x, PgBit y)
        {
            return (x - y);
        }

        public PgBoolean ToPgBoolean()
        {
            return (PgBoolean)this;
        }

        public PgByte TpPgByte()
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

        public PgInt32 ToPgInt32()
        {
            return (PgInt32)this;
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
        
        public static PgBit Xor(PgBit x, PgBit y)
        {
            return (x ^ y);
        }
    }
}
