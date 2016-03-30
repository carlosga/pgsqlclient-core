// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PgTypes
{
    public struct PgBoolean
        : IComparable, INullable
    {
        public static readonly PgBoolean False = false;
        public static readonly PgBoolean Null  = new PgBoolean();
        public static readonly PgBoolean One   = new PgBoolean(1);
        public static readonly PgBoolean True  = true;
        public static readonly PgBoolean Zero  = new PgBoolean(0);

        private readonly bool _isNotNull;
        private readonly bool _value;

        public PgBoolean(bool value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgBoolean(int value)
        {
            _value     = ((value == 0) ? false : true);
            _isNotNull = true;
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
            return new PgBoolean((x.Value != 0));
        }

        public static explicit operator PgBoolean(PgMoney x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean((x.Value != 0));
        }

        public static explicit operator PgBoolean(PgReal x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return new PgBoolean((x.Value != 0));
        }

        public static explicit operator PgBoolean(PgString x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return Parse(x.Value);
        }

        public static bool operator false(PgBoolean x)
        {
            return (x.Value == false);
        }
        
        public static implicit operator PgBoolean(bool x)
        {
            return (PgBoolean)x;
        }

        public static bool operator true(PgBoolean x)
        {
            return (x.Value == true);
        }

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

        public static PgBoolean And(PgBoolean x, PgBoolean y)
        {
            return (x & y);
        }

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

            return Equals(this, (PgBoolean)obj).Value;
        }
        
        public static PgBoolean Equals(PgBoolean x, PgBoolean y)
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
        
        public static PgBoolean GreaterThan(PgBoolean x, PgBoolean y)
        {
            return (x > y);
        }

        public static PgBoolean GreaterThanOrEquals(PgBoolean x, PgBoolean y)
        {
            return (x >= y);
        }

        public static PgBoolean LessThan(PgBoolean x, PgBoolean y)
        {
            return (x < y);
        }

        public static PgBoolean LessThanOrEquals(PgBoolean x, PgBoolean y)
        {
            return (x <= y);
        }

        public static PgBoolean NotEquals(PgBoolean x, PgBoolean y)
        {
            return (x != y);
        }

        public static PgBoolean OnesComplement(PgBoolean x)
        {
            return (~x);
        }

        public static PgBoolean Or(PgBoolean x, PgBoolean y)
        {
            return (x | y);
        }

        public static PgBoolean Parse(string s)
        {
            if (PgTypeInfo.IsNullString(s))
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
            return _value.ToString();
        }
        
        public static PgBoolean Xor(PgBoolean x, PgBoolean y)
        {
            return (x ^ y);
        }
    }
}
