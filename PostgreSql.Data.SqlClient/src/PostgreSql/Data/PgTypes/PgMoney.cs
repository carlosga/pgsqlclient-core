// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgMoney
        : INullable, IComparable<PgMoney>, IComparable, IEquatable<PgMoney>
    {
        public static readonly PgMoney MaxValue = (decimal) 92233720368547758.07;
        public static readonly PgMoney MinValue = (decimal)-92233720368547758.08;
        public static readonly PgMoney Null     = new PgMoney(false);
        public static readonly PgMoney Zero;

        private readonly bool    _isNotNull;
        private readonly decimal _value; 

        private PgMoney(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = 0;
        }

        public PgMoney(decimal value)
        {
            _isNotNull = true;
            _value     = value;
        }

        public PgMoney(double value)
        {
            if (value < (double)MinValue.Value || value > (double)MaxValue.Value)
            {
                throw new OverflowException();
            }
            _isNotNull = true;
            _value     = (decimal)value;
        }

        public PgMoney(int value)
        {
            _isNotNull = true;
            _value     = (decimal)value;
        }

        public PgMoney(long value)
        {
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            _isNotNull = true;
            _value     = (decimal)value;
        }

        public static PgMoney operator -(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            decimal value = -x._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgMoney operator -(PgMoney x, PgMoney y)
        {
            if (x.IsNull)
            {
                return Null;
            }
            decimal value = x._value - y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgBoolean operator !=(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value != y._value);
        }

        public static PgMoney operator *(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            decimal value = x._value * y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgMoney operator /(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            decimal value = x._value / y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgMoney operator +(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            decimal value = x._value + y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return value;
        }

        public static PgBoolean operator <(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgMoney x, PgMoney y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgMoney(double x)
        {
            return (PgMoney)x;
        }

        public static explicit operator PgMoney(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgMoney)x.Value;
        }

        public static explicit operator PgMoney(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgMoney)x.ByteValue;
        }

        public static explicit operator PgMoney(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value;
        }

        public static explicit operator PgMoney(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgMoney)x.Value;
        }

        public static explicit operator decimal(PgMoney x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgMoney(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return (PgMoney)x.Value;
        }

        public static explicit operator PgMoney(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgMoney(decimal x)
        {
            return new PgMoney(x);
        }

        public static implicit operator PgMoney(long x)
        {
            return new PgMoney(x);
        }

        public static implicit operator PgMoney(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgMoney(x.Value);
        }

        public static implicit operator PgMoney(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgMoney(x.Value);
        }

        public static implicit operator PgMoney(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgMoney(x.Value);
        }

        public static implicit operator PgMoney(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgMoney(x.Value);
        }

        public bool IsNull => !_isNotNull;

        public decimal Value 
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

        public static PgMoney Add(PgMoney x, PgMoney y)
        {
            return (x + y);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgReal))
            {
                return -1;
            }

            return CompareTo((PgMoney)obj);
        }

        public int CompareTo(PgMoney value)
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

        public static PgMoney Divide(PgMoney x, PgMoney y)
        {
            return (x / y);
        }

        public bool Equals(PgMoney other)
        {
            return (this == other).Value;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgReal))
            {
                return false;
            }
            return Equals((PgMoney)obj);
        }

        public static PgBoolean Equals(PgMoney x, PgMoney y)
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

        public static PgBoolean GreaterThan(PgMoney x, PgMoney y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgMoney x, PgMoney y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgMoney x, PgMoney y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgMoney x, PgMoney y)
        {
            return (x <= y);
        }

        public static PgMoney Multiply(PgMoney x, PgMoney y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgMoney x, PgMoney y)
        {
            return (x != y);
        }

        public static PgMoney Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Decimal.Parse(s, PgTypeInfoProvider.InvariantCulture);
        }

        public static PgMoney Subtract(PgMoney x, PgMoney y)
        {
            return (x - y);
        }

        public decimal ToDecimal()
        {
            if (IsNull)
            {
                throw new PgNullValueException();
            }
            return _value;
        }

        public double ToDouble()
        {
            if (IsNull)
            {
                throw new PgNullValueException();
            }
            return (double)_value;
        }

        public int ToInt32()
        {
            if (IsNull)
            {
                throw new PgNullValueException();
            }
            return (int)_value;
        }

        public long ToInt64()
        {
            if (IsNull)
            {
                throw new PgNullValueException();
            }
            return (long)_value;
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
    }
}
