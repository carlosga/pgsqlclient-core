// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.PgTypes
{
    /// BigDecimal implementation sample https://gist.github.com/nberardi/2667136 ??
    /// http://www.postgresql.org/message-id/491DC5F3D279CD4EB4B157DDD62237F404E27FE9@zipwire.esri.com
    /// http://www.postgresql.org/message-id/16572.1091489720@sss.pgh.pa.us
    /// http://doxygen.postgresql.org/backend_2utils_2adt_2numeric_8c.html#a3ae98a87bbc2d0dfc9cbe3d5845e0035
    /// http://doxygen.postgresql.org/backend_2utils_2adt_2numeric_8c.html#a57a8f8ab552bae24926d252180956958
    ///
    /// https://labs.omniti.com/pgsoltools/trunk/contrib/scratch/pg_type_to_numeric.c
    /// http://git.postgresql.org/gitweb/?p=postgresql.git;a=blob;f=src/include/utils/numeric.h;h=7fd5d240c66905971870ff82d1ecfe2121750997;hb=3063e7a84026ced2aadd2262f75eebbe6240f85b
    /// http://git.postgresql.org/gitweb/?p=postgresql.git;a=blob;f=src/backend/utils/adt/numeric.c;h=07b264572d9c92787b4c0e74df9aa8a6826265b3;hb=3063e7a84026ced2aadd2262f75eebbe6240f85b
    /// http://git.postgresql.org/gitweb/?p=postgresql.git;a=blob;f=src/backend/utils/adt/numutils.c;h=c1986193c8236605a7657529c3d4d48a30f8d64b;hb=3063e7a84026ced2aadd2262f75eebbe6240f85b
    ///
    /// typedef struct NumericVar
    /// {
    ///     int         ndigits;        /* # of digits in digits[] - can be 0! */
    ///     int         weight;         /* weight of first digit */
    ///     int         sign;           /* NUMERIC_POS, NUMERIC_NEG, or NUMERIC_NAN */
    ///     int         dscale;         /* display scale */
    ///     NumericDigit *buf;          /* start of palloc'd space for digits[] */
    ///     NumericDigit *digits;       /* base-NBASE digits */
    /// } NumericVar;
    public struct PgDecimal
        : IComparable, INullable
    {
        private const int NUMERIC_SIGN_MASK     = 0xC000;
        private const int NUMERIC_POS           = 0x0000;
        private const int NUMERIC_NEG           = 0x4000;
        private const int NUMERIC_NAN           = 0xC000;
        private const int NUMERIC_MAX_PRECISION = 1000;
        private const int NUMERIC_DSCALE_MASK   = 0x3FFF;
        private const int NUMERIC_HDRSZ         = 10;

        public static readonly int       MaxPrecision = NUMERIC_MAX_PRECISION;
        public static readonly int       MaxScale     = 38;
        public static readonly PgDecimal MaxValue     = Decimal.MaxValue;
        public static readonly PgDecimal MinValue     = Decimal.MinValue;
        public static readonly PgDecimal Null         = new PgDecimal();

        private readonly bool    _isNotNull;
        private readonly decimal _value;

        public PgDecimal(decimal value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgDecimal(double dVal)
        {
            _value     = (decimal)dVal;
            _isNotNull = true;
        }

        public PgDecimal(int value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgDecimal(long value)
        {
            _value     = value;
            _isNotNull = true;
        }

        public PgDecimal(byte bPrecision, byte bScale, bool fPositive, int[] bits)
        {
            throw new NotImplementedException();
        }

        public PgDecimal(byte bPrecision, byte bScale, bool fPositive, int data1, int data2, int data3, int data4)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal operator -(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal operator -(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator !=(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal operator *(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal operator /(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal operator +(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator <=(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator ==(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean operator >=(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDecimal(double x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDecimal(PgBoolean x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator decimal(PgDecimal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDecimal(PgDouble x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDecimal(PgReal x)
        {
            throw new NotImplementedException();
        }

        public static explicit operator PgDecimal(PgString x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(decimal x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(long x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(PgByte x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(PgInt32 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(PgInt64 x)
        {
            throw new NotImplementedException();
        }

        public static implicit operator PgDecimal(PgMoney x)
        {
            throw new NotImplementedException();
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
            get { throw new NotImplementedException(); }
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
            get { throw new NotImplementedException(); }
        }

        public static PgDecimal Abs(PgDecimal n)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Add(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal AdjustScale(PgDecimal n, int digits, bool fRound)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Ceiling(PgDecimal n)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object value)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(PgDecimal value)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal ConvertToPrecScale(PgDecimal n, int precision, int scale)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Divide(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public override bool Equals(object value)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean Equals(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Floor(PgDecimal n)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThan(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean GreaterThanOrEqual(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThan(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean LessThanOrEqual(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Multiply(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgBoolean NotEquals(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Parse(string s)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Power(PgDecimal n, double exp)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Round(PgDecimal n, int position)
        {
            throw new NotImplementedException();
        }

        public static PgInt32 Sign(PgDecimal n)
        {
            throw new NotImplementedException();
        }

        public static PgDecimal Subtract(PgDecimal x, PgDecimal y)
        {
            throw new NotImplementedException();
        }

        public double ToDouble()
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

        public static PgDecimal Truncate(PgDecimal n, int position)
        {
            throw new NotImplementedException();
        }
    }
}
