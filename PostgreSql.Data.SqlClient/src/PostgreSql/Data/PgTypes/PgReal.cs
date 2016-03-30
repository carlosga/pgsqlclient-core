// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgReal
        : IComparable, INullable
    {
        public static readonly PgReal MaxValue = Single.MaxValue;
        public static readonly PgReal MinValue = Single.MinValue;
        public static readonly PgReal Null     = new PgReal();
        public static readonly PgReal Zero     = 0.0f;

        private readonly bool  _isNotNull;
        private readonly float _value;

        public PgReal(double value)
        {
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
            throw new NotImplementedException();
        }

        public static PgReal operator -(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgReal operator *(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgReal operator /(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgReal operator +(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgReal(PgBoolean x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgReal(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator float(PgReal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgReal(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(float x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgByte x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgInt16 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgInt32 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgInt64 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgReal(PgMoney x)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgReal value)
        {
            throw new NotImplementedException();
        }

        public static PgReal Divide(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgReal Multiply(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgReal x, PgReal y)
        {
            throw new NotImplementedException();
        }

        public static PgReal Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgReal Subtract(PgReal x, PgReal y)
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

        public PgDouble ToPgDouble()
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
