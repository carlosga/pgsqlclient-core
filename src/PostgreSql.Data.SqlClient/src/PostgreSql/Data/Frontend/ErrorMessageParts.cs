// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend
{
    internal static class ErrorMessageParts
    {
        internal const byte Severity  = (byte)'S';
        internal const byte Severity2 = (byte)'V';
        internal const byte Code      = (byte)'C';
        internal const byte Message   = (byte)'M';
        internal const byte Detail    = (byte)'D';
        internal const byte Hint      = (byte)'H';
        internal const byte Position  = (byte)'P';
        internal const byte Where     = (byte)'W';
        internal const byte File      = (byte)'F';
        internal const byte Line      = (byte)'L';
        internal const byte Routine   = (byte)'R';
        internal const byte End       = (byte)'\0';
    }
}
