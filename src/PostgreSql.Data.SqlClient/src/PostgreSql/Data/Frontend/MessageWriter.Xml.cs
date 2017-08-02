// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;
using System.Xml;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageWriter
    {
        private void WriteXml(object value)
        {
            switch (value)
            {
            case string str:
                Write(str);
                break;

            case XmlDocument document:
                Write(document.OuterXml);
                break;

            case XmlElement element:
                Write(element.OuterXml);
                break;

            default:
                throw ADP.InvalidDataType(value.GetType().Name);
            }            
        }
    }
}