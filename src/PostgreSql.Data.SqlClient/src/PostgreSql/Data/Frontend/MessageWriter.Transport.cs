// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        internal void WriteTo(Transport transport, SyncMode syncMode = SyncMode.None)
        {
            int length = _position;

            Seek(_offset);
            Write(length - _offset);
            Seek(length);

            switch (syncMode)
            {
            case SyncMode.Flush:
                WriteFrame(FrontendMessages.Flush);
                break;

            case SyncMode.Sync:
                WriteFrame(FrontendMessages.Sync);
                break;

            case SyncMode.SyncAndFlush:
                WriteFrame(FrontendMessages.Sync);
                WriteFrame(FrontendMessages.Flush);
                break;
            }

            transport.WriteFrame(_buffer, 0, _position);
        }

        private void WriteFrame(byte type)
        {
            WriteByte(type);
            Write(4);            
        }
    }
}