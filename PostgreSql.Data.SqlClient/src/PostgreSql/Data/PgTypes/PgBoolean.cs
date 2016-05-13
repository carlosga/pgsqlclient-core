// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBoolean
        : INullable, IComparable<PgBoolean>, IComparable, IEquatable<PgBoolean>
    {
        public static readonly PgBoolean False = false;
        public static readonly PgBoolean Null  = new PgBoolean((bool?)null);
        public static readonly PgBoolean One   = new PgBoolean(1);
        public static readonly PgBoolean True  = true;
        public static readonly PgBoolean Zero  = new PgBoolean(0);

        private readonly bool _isNotNull;
        private readonly bool _value;

        private PgBoolean(bool? isNotNull)
        {
            _isNotNull = ((isNotNull.HasValue) ? isNotNull.Value : false);
            _value     = false;
        }

        public PgBoolean(bool value)
        {
            _isNotNull = true;
            _value     = value;
        }

        public PgBoolean(int value)
        {
            _isNotNull = true;
            _value     = ((value == 0) ? false : true);
        }

        public static PgBoolean operator !(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return !x._value;
        }

        public static PgBoolean operator !=(PgBoolean x, PgBoolean y)
        {
            if ((x.IsNull && !y.IsNull) || (!x.IsNull && y.IsNull))
            {
                return true;
            }
            return (x._value != y._value);
        }

        public static PgBoolean operator &(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value & y._value);
        }

        public static PgBoolean operator ^(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value ^ y._value);
        }

        public static PgBoolean operator |(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value | y._value);
        }

        public static PgBoolean operator ~(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgBoolean(~x.ByteValue);
        }

        public static PgBoolean operator <(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x.ByteValue < y.ByteValue);
        }

        public static PgBoolean operator <=(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value & y._value);
        }
        
        public static PgBoolean operator ==(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x.ByteValue > y.ByteValue);
        }

        public static PgBoolean operator >=(PgBoolean x, PgBoolean y)
        {
            if (x.IsNull || y.IsNull)
            {
                return Null;
            }
            return (x.ByteValue >= y.ByteValue);
        }

        public static explicit operator bool(PgBoolean x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgBoolean(PgBit x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value);
        }

        public static explicit operator PgBoolean(PgByte x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value);
        }

        public static explicit operator PgBoolean(PgDecimal x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean((x.Value != 0));
        }

        public static explicit operator PgBoolean(PgDouble x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean((x.Value != 0));
        }

        public static explicit operator PgBoolean(PgInt16 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value);
        }

        public static explicit operator PgBoolean(PgInt32 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value);
        }

        public static explicit operator PgBoolean(PgInt64 x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value != 0);
        }

        public static explicit operator PgBoolean(PgMoney x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value != 0);
        }

        public static explicit operator PgBoolean(PgReal x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean(x.Value != 0);
        }

        public static bool operator false(PgBoolean x)    => (x.Value == false);
        public static implicit operator PgBoolean(bool x) => new PgBoolean(x);
        public static bool operator true(PgBoolean x)     => (x.Value == true);

        public byte ByteValue 
        {
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return ((_value) ? (byte)1 : (byte)0);
            }
        }

        public bool IsFalse
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return (_value == false);
            }
        }

        public bool IsNull => !_isNotNull;

        public bool IsTrue
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return (_value == true);
            }
        }

        public bool Value
        { 
            get
            {
                if (IsNull)
                {
                    throw new PgNullValueException();
                }
                return (_value);
            }
        }

        public static PgBoolean And(PgBoolean x, PgBoolean y) => (x & y);

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgBoolean))
            {
                return -1;
            }

            return CompareTo((PgBoolean)obj);
        }

        public int CompareTo(PgBoolean value)
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

        public bool Equals(PgBoolean other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgBoolean))
            {
                return false;
            }

            return Equals((PgBoolean)obj);
        }

        public static PgBoolean Equals(PgBoolean x, PgBoolean y) => (x == y);

        public override int GetHashCode() => ((IsNull) ? 0 : _value.GetHashCode()); 
        
        public static PgBoolean GreaterThan(PgBoolean x, PgBoolean y)         => (x > y);
        public static PgBoolean GreaterThanOrEquals(PgBoolean x, PgBoolean y) => (x >= y);
        public static PgBoolean LessThan(PgBoolean x, PgBoolean y)            => (x < y);
        public static PgBoolean LessThanOrEquals(PgBoolean x, PgBoolean y)    => (x <= y);
        public static PgBoolean NotEquals(PgBoolean x, PgBoolean y)           => (x != y);
        public static PgBoolean OnesComplement(PgBoolean x)                   => (~x);
        public static PgBoolean Or(PgBoolean x, PgBoolean y)                  => (x | y);

        public static PgBoolean Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }

            switch (s)
            {
                case "t":
                case "T":
                case "true":
                case "True":
                case "TRUE":
                case "y":
                case "Y":
                case "yes":
                case "Yes":
                case "YES":
                case "1":
                case "on":
                case "On":
                case "ON":
                    return true;

                default:
                    return false;
            }
        }

        public PgBit     ToPgBit()     => (PgBit)this;
        public PgByte    ToPgByte()    => (PgByte)this;
        public PgDecimal ToPgDecimal() => (PgDecimal)this;
        public PgDouble  ToPgDouble()  => (PgDouble)this;
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
            return _value.ToString();
        }
        
        public static PgBoolean Xor(PgBoolean x, PgBoolean y) => (x ^ y);
    }
}
