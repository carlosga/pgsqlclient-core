// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgMoney
        : IComparable, INullable
    {
        public static readonly PgMoney MaxValue = (decimal) 92233720368547758.07;
        public static readonly PgMoney MinValue = (decimal)-92233720368547758.08;
        public static readonly PgMoney Null     = new PgMoney();
        public static readonly PgMoney Zero;

        public PgMoney(decimal value)
        {
            throw new NotImplementedException();
        }

        public PgMoney(double value)
        {
            throw new NotImplementedException();
        }

        public PgMoney(int value)
        {
            throw new NotImplementedException();
        }

        public PgMoney(long value)
        {
            throw new NotImplementedException();
        }

        public static PgMoney operator -(PgMoney x)
        {
            throw new NotImplementedException();
        }

        public static PgMoney operator -(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgMoney operator *(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgMoney operator /(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgMoney operator +(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(double x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(PgBoolean x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator decimal(PgMoney x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(PgReal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgMoney(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(decimal x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(long x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(PgByte x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(PgInt16 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(PgInt32 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgMoney(PgInt64 x)
        {
            throw new NotImplementedException();
        }

        public bool IsNull 
        {
            get { throw new NotImplementedException(); } 
        }

        public decimal Value 
        {
            get { throw new NotImplementedException(); } 
        }

        public static PgMoney Add(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgMoney value)
        {
            throw new NotImplementedException();
        }

        public static PgMoney Divide(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgMoney Multiply(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public static PgMoney Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgMoney Subtract(PgMoney x, PgMoney y)
        {
            throw new NotImplementedException();
        }

        public decimal ToDecimal()
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
        {
            throw new NotImplementedException();
        }

        public int ToInt32()
        {
            throw new NotImplementedException();
        }

        public long ToInt64()
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
