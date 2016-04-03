// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgReal
        : INullable, IComparable<PgReal>, IComparable, IEquatable<PgReal>
    {
        public static readonly PgReal MaxValue = new PgReal(true, Single.MaxValue);
        public static readonly PgReal MinValue = new PgReal(true, Single.MinValue);
        public static readonly PgReal Null     = new PgReal(false);
        public static readonly PgReal Zero     = new PgReal(true, 0.0f);

        private readonly bool  _isNotNull;
        private readonly float _value;

        private PgReal(bool isNotNull)
            : this(isNotNull, 0.0f)
        {
        }

        private PgReal(bool isNotNull, float value)
        {
            _isNotNull = isNotNull;
            _value     = value;
        }

        public PgReal(double value)
        {
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            _value     = (float)value;
            _isNotNull = true;
        }

        public PgReal(float value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgReal operator -(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            double value = -x._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)value;
        }

        public static PgReal operator -(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double value = x._value - y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)value;
        }

        public static PgBoolean operator !=(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgReal operator *(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double value = x._value * y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)value;
        }

        public static PgReal operator /(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double value = x._value / y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)value;
        }

        public static PgReal operator +(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            double value = x._value + y._value;
            if (value < MinValue.Value || value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)value;
        }

        public static PgBoolean operator <(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgReal x, PgReal y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgReal(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value;
        }

        public static explicit operator PgReal(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.ByteValue;
        }

        public static explicit operator PgReal(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return (PgReal)x.Value;
        }

        public static explicit operator float(PgReal x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgReal(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgReal(float x)
        {
            return new PgReal(x);
        }

        public static implicit operator PgReal(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgReal(x.Value);
        }

        public static implicit operator PgReal(PgDecimal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < (decimal)MinValue.Value || x.Value > (decimal)MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgReal((float)x.Value);
        }

        public static implicit operator PgReal(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgReal(x.Value);
        }

        public static implicit operator PgReal(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgReal(x.Value);
        }

        public static implicit operator PgReal(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < MinValue.Value || x.Value > MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgReal(x.Value);
        }

        public static implicit operator PgReal(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            if (x.Value < (decimal)MinValue.Value || x.Value > (decimal)MaxValue.Value)
            {
                throw new OverflowException();
            }
            return new PgReal((float)x.Value);
        }

        public bool IsNull => !_isNotNull;

        public float Value 
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

        public static PgReal Add(PgReal x, PgReal y)
        {
            return (x + y);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgReal))
            {
                return -1;
            }

            return CompareTo((PgReal)obj);
        }

        public int CompareTo(PgReal value)
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

        public static PgReal Divide(PgReal x, PgReal y)
        {
            return (x / y);
        }

        public bool Equals(PgReal other)
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
            return Equals((PgReal)obj);
        }

        public static PgBoolean Equals(PgReal x, PgReal y)
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

        public static PgBoolean GreaterThan(PgReal x, PgReal y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgReal x, PgReal y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgReal x, PgReal y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgReal x, PgReal y)
        {
            return (x <= y);
        }

        public static PgReal Multiply(PgReal x, PgReal y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgReal x, PgReal y)
        {
            return (x != y);
        }

        public static PgReal Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Single.Parse(s, PgTypeInfoProvider.InvariantCulture);
        }

        public static PgReal Subtract(PgReal x, PgReal y)
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
