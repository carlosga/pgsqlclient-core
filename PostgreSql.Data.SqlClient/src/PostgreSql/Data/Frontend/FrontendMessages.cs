// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class FrontendMessages
    {
        internal const byte Bind            = (byte)'B';
        internal const byte Close           = (byte)'C';
        internal const byte CopyData        = (byte)'d';
        internal const byte CopyDone        = (byte)'c';
        internal const byte CopyFail        = (byte)'f';
        internal const byte Describe        = (byte)'D';
        internal const byte Execute         = (byte)'E';
        internal const byte Flush           = (byte)'H';
        internal const byte FunctionCall    = (byte)'F';
        internal const byte Parse           = (byte)'P';
        internal const byte PasswordMessage = (byte)'p';
        internal const byte Query           = (byte)'Q';
        internal const byte Sync            = (byte)'S';
        internal const byte Terminate       = (byte)'X';
        internal const byte Untyped         = (byte)' ';
    }
}
