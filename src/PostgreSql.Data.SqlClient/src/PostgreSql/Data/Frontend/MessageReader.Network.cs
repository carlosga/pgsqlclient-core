// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net;
using System.Net.NetworkInformation;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private IPAddress ReadIPAddress()
        {
            // byte ipfamily = ReadByte(); // family
            // byte bits     = ReadByte(); // bits
            // byte iscidr   = ReadByte(); // is_cidr
            
            _position   += 3;          // skip family, bits & is_cidr
            return new IPAddress(ReadBytes(ReadByte()));
        }

        private PhysicalAddress ReadMacAddress()
        {
            return new PhysicalAddress(ReadBytes(6));
        }
    }
}