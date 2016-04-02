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

        public static explicit operator PgDouble(PgString x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return Parse(x.Value);
        }

        public static implicit operator PgDouble(double x)
        {
            return new PgDouble(x);
        }

        public static implicit operator PgDouble(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgDouble(x.Value);
        }

        public static implicit operator PgDouble(PgDecimal x)
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

        public static PgDouble Add(PgDouble x, PgDouble y)
        {
            return (x + y);
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

        public static PgDouble Divide(PgDouble x, PgDouble y)
        {
            return (x / y);
        }

        public bool Equals(PgDouble other)
        {
            return (this == other).Value;
        }

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

        public static PgBoolean Equals(PgDouble x, PgDouble y)
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

        public static PgBoolean GreaterThan(PgDouble x, PgDouble y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEqual(PgDouble x, PgDouble y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgDouble x, PgDouble y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEqual(PgDouble x, PgDouble y)
        {
            return (x >= y);
        }

        public static PgDouble Multiply(PgDouble x, PgDouble y)
        {
            return (x * y);
        }

        public static PgBoolean NotEquals(PgDouble x, PgDouble y)
        {
            return (x != y);
        }

        public static PgDouble Parse(string s)
        {
            if (PgTypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Double.Parse(s, PgTypeInfoProvider.InvariantCulture);
        }

        public static PgDouble Subtract(PgDouble x, PgDouble y)
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
    }
}
