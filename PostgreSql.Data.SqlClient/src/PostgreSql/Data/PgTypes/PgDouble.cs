// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    public struct PgDouble
        : IComparable, INullable
    {
        public static readonly PgDouble MaxValue = Double.MaxValue;
        public static readonly PgDouble MinValue = Double.MinValue;
        public static readonly PgDouble Null     = new PgDouble();
        public static readonly PgDouble Zero     = 0.0d;

        private readonly bool   _isNotNull;
        private readonly double _value;

        public PgDouble(double value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public static PgDouble operator -(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static PgDouble operator -(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgDouble operator *(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgDouble operator /(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgDouble operator +(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDouble(PgBoolean x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator double(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDouble(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(double x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgByte x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgInt16 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgInt32 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgInt64 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgMoney x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDouble(PgReal x)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgDouble value)
        {
            throw new NotImplementedException();
        }

        public static PgDouble Divide(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgDouble Multiply(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public static PgDouble Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgDouble Subtract(PgDouble x, PgDouble y)
        {
            throw new NotImplementedException();
        }

        public PgBoolean ToPgBoolean()
        {
            throw new NotImplementedException();
        }

        public PgByte ToPgByte()
        {
            throw new NotImplementedException();
        }

        public PgDecimal ToPgDecimal()
        {
            throw new NotImplementedException();
        }

        public PgInt16 ToPgInt16()
        {
            throw new NotImplementedException();
        }

        public PgInt32 ToPgInt32()
        {
            throw new NotImplementedException();
        }

        public PgInt64 ToPgInt64()
        {
            throw new NotImplementedException();
        }

        public PgMoney ToPgMoney()
        {
            throw new NotImplementedException();
        }

        public PgReal ToPgReal()
        {
            throw new NotImplementedException();
        }

        public PgString ToPgString()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
