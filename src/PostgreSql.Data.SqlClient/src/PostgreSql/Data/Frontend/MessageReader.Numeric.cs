// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;
using System.Runtime.CompilerServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal byte ReadByte() => _buffer[_position++];

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal short ReadInt16()
        {
            var result = (short)((_buffer[_position + 1] & 0xFF) | (_buffer[_position] & 0xFF) << 8);
        
            _position += 2;
        
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int ReadInt32()
        {
            var result = (_buffer[_position + 3] & 0xFF)
                       | (_buffer[_position + 2] & 0xFF) <<  8
                       | (_buffer[_position + 1] & 0xFF) << 16
                       | (_buffer[_position    ] & 0xFF) << 24;

            _position += 4;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private long ReadInt64()
        {
            var result = ((long)_buffer[_position + 7] & 0xFF)
                       | ((long)_buffer[_position + 6] & 0xFF) <<  8
                       | ((long)_buffer[_position + 5] & 0xFF) << 16
                       | ((long)_buffer[_position + 4] & 0xFF) << 24
                       | ((long)_buffer[_position + 3] & 0xFF) << 32
                       | ((long)_buffer[_position + 2] & 0xFF) << 40
                       | ((long)_buffer[_position + 1] & 0xFF) << 48
                       | ((long)_buffer[_position    ] & 0xFF) << 56;

            _position += 8;

            return result;
            
            // int v1 = ReadInt32();
            // int v2 = ReadInt32();

            // return (uint)v2 | ((long)v1 << 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe float ReadSingle()
        {
            int val = ReadInt32();
            return *((float*)&val);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe double ReadDouble()
        {
            long val = ReadInt64();
            return *((double*)&val);
        }

        private decimal ReadMoney()  => ((decimal)ReadInt64() / 100.0M);

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
    }
}