// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        [StructLayout(LayoutKind.Explicit)]
        struct Uuid
        {
            [FieldOffset(0)]
            public int A;
            [FieldOffset(4)]
            public short B;
            [FieldOffset(6)]
            public short C;
            [FieldOffset(8)]
            public byte D;
            [FieldOffset(9)]
            public byte E;
            [FieldOffset(10)]
            public byte F;
            [FieldOffset(11)]
            public byte G;
            [FieldOffset(12)]
            public byte H;
            [FieldOffset(13)]
            public byte I;
            [FieldOffset(14)]
            public byte J;
            [FieldOffset(15)]
            public byte K;
            [FieldOffset(0)]
            public Guid Guid;
        }

        private void Write(Guid value)
        {
            EnsureCapacity(16);

            Uuid uuid = default(Uuid);
            uuid.Guid = value;
            
            Write(uuid.A);
            Write(uuid.B);
            Write(uuid.C);
            WriteByte(uuid.D);
            WriteByte(uuid.E);
            WriteByte(uuid.F);
            WriteByte(uuid.G);
            WriteByte(uuid.H);
            WriteByte(uuid.I);
            WriteByte(uuid.J);
            WriteByte(uuid.K);
        }
    }
}