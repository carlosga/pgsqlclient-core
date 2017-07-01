// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace PostgreSql.Data.Frontend.Security
{
    internal static class CryptographicBuffer
    {
        internal static byte[] DecodeFromHexString(string value)
        {
            if (value == null)
            {
                throw ADP.ArgumentNull(nameof(value));
            }
            var hex = value.Replace(" ", string.Empty);
            if ((hex.Length % 2) != 0)
            {
                throw ADP.Format(nameof(value));
            }

            var buffer = new byte[hex.Length / 2];
            int index  = 0;

            for (int i = 0; i < hex.Length; i += 2)
            {
                buffer[index++] = byte.Parse(hex.Substring(i,2), NumberStyles.HexNumber);
            }

            return buffer;
        }

        internal static string EncodeToHexString(byte[] buffer) => EncodeToHexString(buffer, string.Empty);

        internal static string EncodeToHexString(byte[] buffer, string separator)
        {
            if (buffer == null)
            {
                throw ADP.ArgumentNull(nameof(buffer));
            }

            var hex = new StringBuilder((buffer.Length * 2) * 2);

            for (int i = 0; i < buffer.Length; i++)
            {
                if (!string.IsNullOrEmpty(separator) && hex.Length > 0)
                {
                    hex.Append(separator);
                }
                hex.Append(buffer[i].ToString("X2"));
            }

            return hex.ToString();
        }        
        internal static byte[] GenerateRandom(int count)
        {
            if (count <= 0)
            {
                throw ADP.ArgumentOutOfRange(nameof(count));
            }

            var buffer = new byte[count];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            return buffer;
        }

        internal static byte[] Xor(byte[] buffer1, byte[] buffer2)
        {
            if (buffer1 == null)
            {
                throw ADP.ArgumentNull(nameof(buffer1));
            }
            if (buffer2 == null)
            {
                throw ADP.ArgumentNull(nameof(buffer2));
            }
            if (buffer1.Length != buffer2.Length)
            {
                throw ADP.Argument($"{nameof(buffer1)} and {nameof(buffer2)} should have the same length");
            }

            var buffer = new byte[buffer1.Length];

            for (int i = 0; i < buffer1.Length; i++)
            {
                buffer[i] = (byte)(buffer1[i] ^ buffer2[i]);
            }

            return buffer;
        }

       internal static byte[] Clone(byte[] buffer)
        {
            if (buffer == null)
            {
                throw ADP.ArgumentNull(nameof(buffer));
            }
            if (buffer.Length == 0)
            {
                return Array.Empty<byte>();
            }
            var cloned = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, cloned, 0, cloned.Length);
            return cloned;
        }        
    }
}
