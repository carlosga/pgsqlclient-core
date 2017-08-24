// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;
using System.Collections.Generic;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        [StructLayout(LayoutKind.Explicit)]
        struct Numeric
        {
            [FieldOffset(0)]
            public int Flags;
            [FieldOffset(4)]
            private int Hi;
            [FieldOffset(8)]
            private int Lo;
            [FieldOffset(12)]
            private int Mid;

            [FieldOffset(0)]
            public decimal Number;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void WriteByte(byte value)
        {
            EnsureCapacity(1);

            _buffer[_position++] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Write(short value)
        {
            EnsureCapacity(2);

            _buffer[_position++] = (byte)((value >> 8) & 0xFF);
            _buffer[_position++] = (byte)((value     ) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal unsafe void Write(int value)
        {
            EnsureCapacity(4);

            _buffer[_position++] = (byte)((value >> 24) & 0xFF);
            _buffer[_position++] = (byte)((value >> 16) & 0xFF);
            _buffer[_position++] = (byte)((value >>  8) & 0xFF);
            _buffer[_position++] = (byte)((value      ) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(long value)
        {
            EnsureCapacity(8);

            // Write((int)(value >> 32));
            // Write((int)(value));
            _buffer[_position++] = (byte)((value >> 56) & 0xFF);
            _buffer[_position++] = (byte)((value >> 48) & 0xFF);
            _buffer[_position++] = (byte)((value >> 40) & 0xFF);
            _buffer[_position++] = (byte)((value >> 32) & 0xFF);
            _buffer[_position++] = (byte)((value >> 24) & 0xFF);
            _buffer[_position++] = (byte)((value >> 16) & 0xFF);
            _buffer[_position++] = (byte)((value >>  8) & 0xFF);
            _buffer[_position++] = (byte)((value      ) & 0xFF);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write(float value)
            => Write(*((int*)&value));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write(double value)
            => Write(*((long*)&value));

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
            short weight     = 0;
            short ndigits    = 0;
            short sign       = (short)((isNegative) ? PgNumeric.NegativeMask : PgNumeric.PositiveMask);
            var   numeric    = default(Numeric);

            numeric.Number = absValue;

            short dscale = (short)((numeric.Flags & ScaleMask) >> ScaleShift);

            if (absValue > 0)
            {
                // postgres: numeric::estimate_ln_dweight
                weight  = (short)((int)Math.Log10((double)absValue) >> 2);
                // postgres: numeric::div_var
                ndigits = (short)(weight + 1 + (dscale + DEC_DIGITS - 1) >> 2);
            }

            short[] digits     = null;
            short   realDigits = 0;

            if (ndigits > 0)
            {
                digits = ArrayPool<short>.Shared.Rent(ndigits);
                for (int i = weight + 7; i >= 0; --i)
                {
                    var digit = (short) (absValue / PgNumeric.Weights[i]);
                    if (digit > 0)
                    {
                        absValue -= (digit * PgNumeric.Weights[i]);
                    }
                    digits[realDigits++] = digit;
                    if (absValue == 0)
                    {
                        break;
                    }
                }
            }

            int sizeInBytes = 8 + (realDigits * sizeof(short));

            EnsureCapacity(sizeInBytes);

            Write(sizeInBytes);
            Write(realDigits);
            Write(weight);
            Write(sign);
            Write(dscale);

            if (realDigits > 0)
            {
                for (int i = 0; i < realDigits; i++)
                {
                    Write(digits[i]);
                }
            }

            if (digits != null)
            {
                ArrayPool<short>.Shared.Return(digits, true);
            }
        }
    }
}
