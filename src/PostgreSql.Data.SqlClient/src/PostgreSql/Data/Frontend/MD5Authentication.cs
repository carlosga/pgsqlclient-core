// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal static class MD5Authentication
    {
        private static readonly string Prefix = "md5";

        internal static string EncryptPassword(byte[] salt, string userId, string password)
        {
            string userHash = GetMD5Hash(Encoding.UTF8.GetBytes(userId), password);
            string hash     = GetMD5Hash(salt, userHash);

            return Prefix + hash;
        }

        private static string GetMD5Hash(byte[] salt, string password)
        {
            using (HashAlgorithm csp = MD5.Create())
            {
                int    length = ((string.IsNullOrEmpty(password) ? 0 : Encoding.UTF8.GetByteCount(password)));
                byte[] data   = new byte[salt.Length + length];

                if (length > 0)
                {
                    Encoding.UTF8.GetBytes(password, 0, length, data, 0);
                }

                Buffer.BlockCopy(salt, 0, data, length, salt.Length);

                // Calculate hash value
                var hash = csp.ComputeHash(data);
                var md5  = new StringBuilder(hash.Length * 2);

                // Calculate MD5 string
                for (int i = 0; i < hash.Length; ++i)
                {
                    md5.Append(hash[i].ToString("x2"));
                }

                return md5.ToString();
            }
        }
    }
}
