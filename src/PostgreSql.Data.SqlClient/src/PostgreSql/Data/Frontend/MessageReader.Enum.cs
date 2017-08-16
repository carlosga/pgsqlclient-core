// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
    {
        private int ReadEnum(TypeInfo typeInfo, int length)
        {
            var enumMember = ReadString(length);

            for (int i = 0; i < typeInfo.Attributes.Length; ++i)
            {
                if (enumMember == typeInfo.Attributes[i].Name)
                {
                    return typeInfo.Attributes[i].Index;
                }
            }
            
            throw ADP.UnknownEnumValue(typeInfo.Name, enumMember);
        }
    }
}