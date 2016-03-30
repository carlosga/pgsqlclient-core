// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    public struct PgByte
        : IComparable, INullable
    {
        public static readonly PgByte MaxValue = Byte.MaxValue;
        public static readonly PgByte MinValue = Byte.MinValue;
        public static readonly PgByte Null     = new PgByte();
        public static readonly PgByte One      = new PgByte(1);
        public static readonly PgByte Zero     = new PgByte(0);

        private readonly bool _isNotNull;
        private readonly byte _value;

        public PgByte(byte value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgByte operator -(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgBoolean operator !=(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value != y._value);
        }

        public static PgByte operator %(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator &(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator *(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator /(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator ^(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator |(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgByte operator ~(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgByte)(~x._value);
        }

        public static PgByte operator +(PgByte x, PgByte y)
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
            return (PgByte)value;
        }

        public static PgBoolean operator <(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgByte x, PgByte y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }
        
        public static explicit operator PgByte(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value ? One : Zero;
        }

        public static explicit operator byte(PgByte x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgByte(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }

        public static explicit operator PgByte(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }
        
        public static explicit operator PgByte(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }
        
        public static explicit operator PgByte(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }
        
        public static explicit operator PgByte(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }

        public static explicit operator PgByte(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }

        public static explicit operator PgByte(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgByte((byte)x.Value);
        }

        public static explicit operator PgByte(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgByte(byte x)
        {
            return (PgByte)x;
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

        public static PgByte Add(PgByte x, PgByte y)
        {
            return (x + y);
        }

        public static PgByte BitwiseAnd(PgByte x, PgByte y)
        {
            return (x & y);
        }

        public static PgByte BitwiseOr(PgByte x, PgByte y)
        {
            return (x | y);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgByte))
            {
                return -1;
            }

            return CompareTo((PgByte)obj);
        }

        public int CompareTo(PgByte value)
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

        public static PgByte Divide(PgByte x, PgByte y)
        {
            return (x / y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgByte))
            {
                return false;
            }

            return Equals(this, (PgByte)obj).Value;
        }

        public static PgBoolean Equals(PgByte x, PgByte y)
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

        public static PgBoolean GreaterThan(PgByte x, PgByte y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgByte x, PgByte y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgByte x, PgByte y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgByte x, PgByte y)
        {
            return (x >= y);
        }

        public static PgByte Mod(PgByte x, PgByte y)
        {
            return (x % y);
        }

        public static PgByte Modulus(PgByte x, PgByte y)
        {
            return (x % y);
        }

        public static PgByte Multiply(PgByte x, PgByte y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgByte x, PgByte y)
        {
            return (x != y);
        }

        public static PgByte OnesComplement(PgByte x)
        {
            return ~x;
        }

        public static PgByte Parse(string s)
        {
            if (PgTypeInfo.IsNullString(s))
            {
                return Null;
            }
            return Byte.Parse(s, PgTypeInfo.InvariantCulture);
        }

        public static PgByte Subtract(PgByte x, PgByte y)
        {
            return (x - y);
        }

        public PgBoolean ToPgBoolean()
        {
            return (PgBoolean)this;
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
                return PgTypeInfo.NullString;
            }
            return _value.ToString(PgTypeInfo.InvariantCulture);
        }
        
        public static PgByte Xor(PgByte x, PgByte y)
        {
            return (x ^ y);
        }
    }
}