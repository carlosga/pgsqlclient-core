// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        internal byte ReadByte() => _buffer[_position++];

        internal unsafe short ReadInt16()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                _position += 2;
                return (short)((*(pbuffer + 1) & 0xFF) | (*(pbuffer) & 0xFF) << 8);
            }
        }

        internal unsafe int ReadInt32()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                _position += 4;
                return (*(pbuffer + 3) & 0xFF)
                     | (*(pbuffer + 2) & 0xFF) <<  8
                     | (*(pbuffer + 1) & 0xFF) << 16
                     | (*(pbuffer    ) & 0xFF) << 24;
            }
        }

        private long ReadInt64()
        {
            int v1 = ReadInt32();
            int v2 = ReadInt32();

            return (uint)v2 | ((long)v1 << 32);
        }

        private unsafe float ReadSingle()
        {
            fixed (byte* pbuffer = &_buffer[_position])
            {
                int val = ReadInt32();
                return *((float*)&val);
            }
        }

        private decimal ReadNumeric()
        {
            int ndigits = 0; // # of digits in digits[] - can be 0!
            int weight  = 0; // weight of first digit
            int sign    = 0; // NUMERIC_POS, NUMERIC_NEG, or NUMERIC_NAN
            int dscale  = 0; // display scale
            var res     = 0.0M;

            ndigits = ReadInt16();

            if (ndigits < 0 || ndigits > PgNumeric.MaxLength)
            {
                throw new FormatException("invalid length in \"numeric\" value");
            }

            weight = ReadInt16() + 7;
            sign   = ReadInt16();

            if (sign != PgNumeric.PositiveMask && sign != PgNumeric.NegativeMask && sign != PgNumeric.NaNMask)
            {
                throw new FormatException("invalid sign in \"numeric\" value");
            }

            dscale = ReadInt16();

            // base-NBASE digits
            for (int i = 0; i < ndigits; ++i)
            {
                short digit = ReadInt16();

                if (digit < 0 || digit >= PgNumeric.NBase)
                {
                    throw new FormatException("invalid digit in external \"numeric\" value");
                }

                res += digit * PgNumeric.Weights[weight - i];
            }

            return ((sign == PgNumeric.NegativeMask) ? -res : res);
        }

        private double  ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());
        private decimal ReadMoney()  => ((decimal)ReadInt64() / 100.0M);
    }
}