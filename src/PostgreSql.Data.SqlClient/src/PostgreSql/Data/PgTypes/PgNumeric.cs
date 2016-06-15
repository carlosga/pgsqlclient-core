// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    /// http://grokbase.com/t/postgresql/pgsql-interfaces/046evv5wyw/libpq-binary-transfer-of-the-numeric-data-type
    /// The value represented by a NumericVar is determined by the sign, weight,
    /// ndigits, and digits[] array.
    /// Note: the first digit of a NumericVar's value is assumed to be multiplied
    /// by NBASE ** weight.	Another way to say it is that there are weight+1
    /// digits before the decimal point. It is possible to have weight < 0.
    /// 
    /// dscale, or display scale, is the nominal precision expressed as number
    /// of digits after the decimal point (it must always be >= 0 at present).
    /// dscale may be more than the number of physically stored fractional digits,
    /// implying that we have suppressed storage of significant trailing zeroes.
    /// It should never be less than the number of stored digits, since that would
    /// imply hiding digits that are present. NOTE that dscale is always expressed
    /// in *decimal* digits, and so it may correspond to a fractional number of
    /// base-NBASE digits --- divide by DEC_DIGITS to convert to NBASE digits.

    public struct PgNumeric
        : INullable, IComparable<PgNumeric>, IComparable, IEquatable<PgNumeric>
    {
        internal const int MaxResultScale   = (MaxPrecision * 2);
        internal const int PositiveMask     = 0x0000;
        internal const int NegativeMask     = 0x4000;
        internal const int NaNMask          = 0xC000;
        internal const int DecimalScaleMask = 0x3FFF;
        internal const int NBase            = 10000;
        internal const int MaxPrecision     = 1000;
        internal const int MaxScale         = 28;

        // [Decimal 128 decimal 1.0 × 10^-28 to 7.9 × 10^28, 28-digit precision]
        internal static readonly decimal[] Weights = new decimal[]
        {
              1E-28M
            , 1E-24M
            , 1E-20M
            , 1E-16M
            , 1E-12M
            , 1E-8M
            , 1E-4M
            , 1M
            , 1E+4M
            , 1E+8M
            , 1E+12M
            , 1E+16M
            , 1E+20M
            , 1E+24M
            , 1E+28M
        };

        public static readonly PgNumeric MaxValue = Decimal.MaxValue;
        public static readonly PgNumeric MinValue = Decimal.MinValue;
        public static readonly PgNumeric Null     = new PgNumeric();

        private readonly bool    _isNotNull;
        private readonly decimal _value;

        public PgNumeric(decimal value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgNumeric(double dVal)
        {
            _value     = (decimal)dVal;
            _isNotNull = true;
        }

        public PgNumeric(int value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgNumeric(long value)
        {
            _value     = value;
            _isNotNull = true;
        }

        // public PgNumeric(byte bPrecision, byte bScale, bool fPositive, int[] bits)
        // {
        //     throw new NotImplementedException();
        // }

        // public PgNumeric(byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4)
        // {
        //     throw new NotImplementedException();
        // }

        public static PgNumeric operator -(PgNumeric x)
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

        public static PgNumeric operator -(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
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

        public static PgBoolean operator !=(PgNumeric x, PgNumeric y) => !(x == y);

        public static PgNumeric operator *(PgNumeric x, PgNumeric y)
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

        public static PgNumeric operator /(PgNumeric x, PgNumeric y)
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

        public static PgNumeric operator +(PgNumeric x, PgNumeric y)
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

        public static PgBoolean operator <(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value < y._value);
        }

        public static PgBoolean operator <=(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value <= y._value);
        }

        public static PgBoolean operator ==(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value == y._value);
        }

        public static PgBoolean operator >(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value > y._value);
        }

        public static PgBoolean operator >=(PgNumeric x, PgNumeric y)
        {
            if (x.IsNull || y.IsNull)
            {
                return PgBoolean.Null;
            }
            return (x._value >= y._value);
        }

        public static explicit operator PgNumeric(double x) => new PgNumeric(x);

        public static explicit operator PgNumeric(PgBit x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value);
        }

        public static explicit operator PgNumeric(PgBoolean x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.ByteValue);
        }

        public static explicit operator decimal(PgNumeric x)
        {
            if (x.IsNull)
            {
                throw new PgNullValueException();
            }
            return x._value;
        }

        public static explicit operator PgNumeric(PgDouble x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value);
        }

        public static explicit operator PgNumeric(PgReal x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value);
        }

        public static implicit operator PgNumeric(decimal x) => new PgNumeric(x);

        public static implicit operator PgNumeric(long x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgNumeric(PgByte x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x);
        }
        
        public static implicit operator PgNumeric(PgInt16 x)  
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value); 
        }

        public static implicit operator PgNumeric(PgInt32 x)  
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value);
        }

        public static implicit operator PgNumeric(PgInt64 x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value); 
        }

        public static implicit operator PgNumeric(PgMoney x)
        {
            if (x.IsNull)
            {
                return Null;
            }
            return new PgNumeric(x.Value);
        }

        public byte[] BinData
        {
            get { throw new NotImplementedException(); }
        }

        public int[] Data
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsNull => !_isNotNull;

        public bool IsPositive
        {
            get { return (_value > 0); }
        }

        public byte Precision
        {
            get { throw new NotImplementedException(); }
        }

        public byte Scale
        {
            get { throw new NotImplementedException(); }
        }

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

        public static PgNumeric Abs(PgNumeric n)
        {
            return Math.Abs(n._value);
        }

        public static PgNumeric Add(PgNumeric x, PgNumeric y) => (x + y);

        public static PgNumeric AdjustScale(PgNumeric n, int digits, bool fRound)
        {
            throw new NotImplementedException();
        }

        public static PgNumeric Ceiling(PgNumeric n)
        {
            return Math.Ceiling(n._value);
        }

        public int CompareTo(object obj)
        {
            if (obj == null || !(obj is PgInt16))
            {
                return -1;
            }

            return CompareTo((PgNumeric)obj);
        }

        public int CompareTo(PgNumeric value)
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

        public static PgNumeric ConvertToPrecScale(PgNumeric n, int precision, int scale)
        {
            throw new NotImplementedException();
        }

        public static PgNumeric Divide(PgNumeric x, PgNumeric y) => (x / y);

        public bool Equals(PgNumeric other) => (this == other).Value;

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (!(obj is PgNumeric))
            {
                return false;
            }
            return Equals((PgNumeric)obj);
        }

        public static PgBoolean Equals(PgNumeric x, PgNumeric y) => (x == y);

        public static PgNumeric Floor(PgNumeric n)
        {
            return Math.Floor(n._value);
        }

        public override int GetHashCode() => ((IsNull) ? 0 : _value.GetHashCode());

        public static PgBoolean GreaterThan(PgNumeric x, PgNumeric y)        => (x > y);
        public static PgBoolean GreaterThanOrEqual(PgNumeric x, PgNumeric y) => (x >= y);
        public static PgBoolean LessThan(PgNumeric x, PgNumeric y)           => (x < y);
        public static PgBoolean LessThanOrEqual(PgNumeric x, PgNumeric y)    => (x <= y);
        public static PgNumeric Multiply(PgNumeric x, PgNumeric y)           => (x * y);
        public static PgBoolean NotEquals(PgNumeric x, PgNumeric y)          => (x != y); 
        
        public static PgNumeric Parse(string s)
        {
            if (TypeInfoProvider.IsNullString(s))
            {
                return Null;
            }
            return Decimal.Parse(s, TypeInfoProvider.InvariantCulture);
        }

        public static PgNumeric Power(PgNumeric n, double exp)
        {
            throw new NotImplementedException();
        }

        public static PgNumeric Round(PgNumeric n, int position)
        {
            return Math.Round(n._value, position);
        }

        public static PgInt32 Sign(PgNumeric n)
        {
            throw new NotImplementedException();
        }

        public static PgNumeric Subtract(PgNumeric x, PgNumeric y) => (x - y);

        public double ToDouble()
        {
            if (IsNull)
            {
                throw new PgNullValueException();
            }
            return (double)_value;
        }

        public PgBit     ToPgBit()     => (PgBit)this;
        public PgBoolean ToPgBoolean() => (PgBoolean)this;
        public PgByte    ToPgByte()    => (PgByte)this;
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
            return _value.ToString(TypeInfoProvider.InvariantCulture);
        }

        public static PgNumeric Truncate(PgNumeric n, int position)
        {
            throw new NotImplementedException();
        }
    }
}
