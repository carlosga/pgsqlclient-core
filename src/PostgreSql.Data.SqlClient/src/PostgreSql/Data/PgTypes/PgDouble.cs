// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgDouble
        : INullable, IComparable<PgDouble>, IComparable, IEquatable<PgDouble>
    {
        public static readonly PgDouble MaxValue = Double.MaxValue;
        public static readonly PgDouble MinValue = Double.MinValue;
        public static readonly PgDouble Null     = new PgDouble(false);
        public static readonly PgDouble Zero     = 0.0d;

        private readonly bool   _isNotNull;
        private readonly double _value;

        private PgDouble(bool isNotNull)
        {
            _isNotNull = isNotNull;
            _value     = 0;
        }

        public PgDouble(double value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgDouble operator -(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return -x._value;
        }

        public static PgDouble operator -(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value - y._value);
        }

        public static PgBoolean operator !=(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgDouble operator *(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value * y._value);
        }

        public static PgDouble operator /(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value / y._value);
        }

        public static PgDouble operator +(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value + y._value);
        }

        public static PgBoolean operator <(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgDouble x, PgDouble y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgDouble(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.Value;
        }

        public static explicit operator PgDouble(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return x.ByteValue;
        }

        public static explicit operator double(PgDouble x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x.Value;
        }

        public static implicit operator PgDouble(double x) => new PgDouble(x);

        public static implicit operator PgDouble(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public static implicit operator PgDouble(PgNumeric x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble((double)x.Value);
        }

        public static implicit operator PgDouble(PgInt16 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public static implicit operator PgDouble(PgInt32 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public static implicit operator PgDouble(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public static implicit operator PgDouble(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble((double)x.Value);
        }

        public static implicit operator PgDouble(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public bool IsNull => !_isNotNull;

        public double Value 
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

        public int CompareTo(PgDouble value)
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

        public bool Equals(PgDouble other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgDouble))
            {
                return false;
            }
            return Equals((PgDouble)obj);
        }

        public override int GetHashCode() => ((IsNull) ? 0 : _value.GetHashCode());

        public static PgDouble  Add(PgDouble x, PgDouble y)                => (x + y);
        public static PgDouble  Divide(PgDouble x, PgDouble y)             => (x / y);
        public static PgBoolean Equals(PgDouble x, PgDouble y)             => (x == y);
        public static PgBoolean GreaterThan(PgDouble x, PgDouble y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgDouble x, PgDouble y) => (x >= y);
        public static PgBoolean LessThan(PgDouble x, PgDouble y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgDouble x, PgDouble y)    => (x >= y);
        public static PgDouble  Multiply(PgDouble x, PgDouble y)           => (x * y);
        public static PgBoolean NotEquals(PgDouble x, PgDouble y)          => (x != y);
        public static PgDouble  Subtract(PgDouble x, PgDouble y)           => (x - y);

        public static PgDouble Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Double.Parse(s, TypeInfoProvider.InvariantCulture);
        }

        public PgBit     ToPgBit()     => (PgBit)this;
        public PgBoolean ToPgBoolean() => (PgBoolean)this;
        public PgByte    ToPgByte()    => (PgByte)this;
        public PgNumeric ToPgNumeric() => (PgNumeric)this;
        public PgInt16   ToPgInt16()   => (PgInt16)this;
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
