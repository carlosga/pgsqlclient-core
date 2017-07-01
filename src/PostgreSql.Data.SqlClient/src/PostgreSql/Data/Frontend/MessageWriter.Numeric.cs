// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        internal void WriteByte(byte value)
        {
            EnsureCapacity(1);

            _buffer[_position++] = value;
        }

        internal unsafe void Write(short value)
        {
            EnsureCapacity(2);

            fixed (byte* pbuffer = _buffer)
            {
                *(pbuffer + _position++) = (byte)((value >> 8) & 0xFF);
                *(pbuffer + _position++) = (byte)((value     ) & 0xFF);
            }
        }

        internal unsafe void Write(int value)
        {
            EnsureCapacity(4);

            fixed (byte* pbuffer = _buffer)
            {
                *(pbuffer + _position++) = (byte)((value >> 24) & 0xFF);
                *(pbuffer + _position++) = (byte)((value >> 16) & 0xFF);
                *(pbuffer + _position++) = (byte)((value >>  8) & 0xFF);
                *(pbuffer + _position++) = (byte)((value      ) & 0xFF);
            }
        }

        private void Write(long value)
        {
            EnsureCapacity(8);

            Write((int)(value >> 32));
            Write((int)(value));
        }

        private void Write(float value)    => Write(BitConverter.ToInt32(BitConverter.GetBytes(value), 0));
        private void Write(double value)   => Write(BitConverter.DoubleToInt64Bits(value));

        private void Write(decimal value)
        {
            // Scale mask for the flags field. This byte in the flags field contains
            // the power of 10 to divide the Decimal value by. The scale byte must
            // contain a value between 0 and 28 inclusive.
            const int ScaleMask = 0x00FF0000;
            // Number of bits scale is shifted by.
            const int ScaleShift = 16;
            // decimal digits per NBASE digit
            const int DEC_DIGITS = 4;  

            bool  isNegative = (value < 0);
            var   absValue   = ((isNegative) ? value * -1.0M : value);
            int[] bits       = Decimal.GetBits(absValue);

            short sign    = (short)((isNegative) ? PgNumeric.NegativeMask : PgNumeric.PositiveMask);
            short dscale  = (short)((bits[3] & ScaleMask) >> ScaleShift);
            short weight  = 0;
            short ndigits = 0;

            if (absValue > 0)
            {
                // postgres: numeric::estimate_ln_dweight 
                weight  = (short)Math.Truncate(Math.Log((double)absValue, PgNumeric.NBase));
                // postgres: numeric::div_var
                ndigits = (short)(weight + 1 + (dscale + DEC_DIGITS - 1) / DEC_DIGITS);
            }

            int sizeInBytes = 8 + ndigits * sizeof(short);

            EnsureCapacity(sizeInBytes);

            Write(sizeInBytes);
            Write(ndigits);
            Write(weight);
            Write(sign);
            Write(dscale);

            if (ndigits > 0)
            {
                for (int i = weight + 7; i >= 0; --i)
                {
                    var digit = (short) (absValue / PgNumeric.Weights[i]);
                    if (digit > 0)
                    {
                        absValue -= (digit * PgNumeric.Weights[i]);
                    }
                    Write(digit);
                    if (absValue == 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}