// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private const byte AF_INET        = 2;
        private const byte PGSQL_AF_INET  = (AF_INET + 0);
        private const byte PGSQL_AF_INET6 = (AF_INET + 1);

        private void Write(IPAddress value)
        {
            var bytes = value.GetAddressBytes();
            EnsureCapacity(bytes.Length + 8);
            Write(bytes.Length + 4);

            var ipfamily = (value.AddressFamily == AddressFamily.InterNetworkV6) ? PGSQL_AF_INET6 : PGSQL_AF_INET; 
            WriteByte(ipfamily);                 // family
            WriteByte((byte)(bytes.Length * 8)); // bits
            WriteByte(0);                        // is_cidr
            WriteByte((byte)bytes.Length);       // address length
            Write(bytes);                        // address in network byte order
        }

        private void Write(PhysicalAddress value)
        {
            Write(value.GetAddressBytes());
        }
    }
}