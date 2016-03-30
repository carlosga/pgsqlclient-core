// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    public struct PgInt16
        : INullable, IComparable
    {
        public static readonly PgInt16 MaxValue =  32767;
        public static readonly PgInt16 MinValue = -32768;
        public static readonly PgInt16 Null     = new PgInt16();
        public static readonly PgInt16 Zero     = 0;

        private readonly bool  _isNotNull;
        private readonly short _value;

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

        public static PgBoolean operator !=(PgInt16 x, PgInt16 y)
        {
            if (x.IsNull && y.IsNull)
            {
                return true;
            }
            else if ((x.IsNull && !y.IsNull) || (!x.IsNull && x.IsNull))
            {
                return false;
            }
            return (x._value != y._value);
        }

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

        public static explicit operator PgInt16(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgInt16)x.ByteValue;
        }

        public static explicit operator PgInt16(PgDecimal x)
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

        public static explicit operator PgInt16(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgInt16(short x)
        {
            return new PgInt16(x);
        }

        public static implicit operator PgInt16(PgByte x)
        {
            return new PgInt16(x.Value);
        }

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

        public static PgInt16 Add(PgInt16 x, PgInt16 y)
        {
            return (x + y);
        }

        public static PgInt16 BitwiseAnd(PgInt16 x, PgInt16 y)
        {
            return (x & y);
        }

        public static PgInt16 BitwiseOr(PgInt16 x, PgInt16 y)
        {
            return (x | y);
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgInt16 value)
        {
            throw new NotImplementedException();
        }

        public static PgInt16 Divide(PgInt16 x, PgInt16 y)
        {
            return (x / y);
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgInt16 x, PgInt16 y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            if (IsNull)
            {
                return 0;
            }
            return _value.GetHashCode();
        }

        public static PgBoolean GreaterThan(PgInt16 x, PgInt16 y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgInt16 x, PgInt16 y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgInt16 x, PgInt16 y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgInt16 x, PgInt16 y)
        {
            return (x <= y);
        }

        public static PgInt16 Mod(PgInt16 x, PgInt16 y)
        {
            return (x % y);
        }

        public static PgInt16 Modulus(PgInt16 x, PgInt16 y)
        {
            return (x % y);
        }

        public static PgInt16 Multiply(PgInt16 x, PgInt16 y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgInt16 x, PgInt16 y)
        {
            return (x != y);
        }

        public static PgInt16 OnesComplement(PgInt16 x)
        {
            return ~x;
        }

        public static PgInt16 Parse(string s)
        {
            if (PgTypeInfo.IsNullString(s))
            {
                return Null;
            }
            return (PgInt16)Int16.Parse(s, PgTypeInfo.InvariantCulture);
        }

        public static PgInt16 Subtract(PgInt16 x, PgInt16 y)
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
            throw new NotImplementedException();
        }

        public PgDouble ToPgDouble()
        {
            return (PgDouble)this;
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
            if (IsNull)
            {
                return PgString.Null;
            }
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

        public static PgInt16 Xor(PgInt16 x, PgInt16 y)
        {
            return (x ^ y);
        }
    }
}
