// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System.Security.Cryptography;
using System.Text;
using System;

namespace PostgreSql.Data.Authentication
{
    internal static class MD5Authentication
    {
        internal static string Prefix = "md5";

        internal static string EncryptPassword(byte[] salt,string userId, string password)
        {
            // MD5-encrypted password is required

            string userHash = GetMD5Hash(Encoding.UTF8.GetBytes(userId), password);
            string hash     = GetMD5Hash(salt, userHash);

            return $"{Prefix}{hash}";
        }

        private static string GetMD5Hash(byte[] salt, string password)
        {
            using (HashAlgorithm csp = MD5.Create())
            {
                string md5    = string.Empty;
                int    length = ((String.IsNullOrEmpty(password) ? 0 : Encoding.UTF8.GetByteCount(password)));
                byte[] data   = new byte[salt.Length + length];

                if (length > 0)
                {
                    Encoding.UTF8.GetBytes(password, 0, length, data, 0);   
                }

                salt.CopyTo(data, length);

                // Calculate hash value
                byte[] hash = csp.ComputeHash(data);

                // Calculate MD5 string
                for (int i = 0; i < hash.Length; ++i)
                {
                    md5 += hash[i].ToString("x2");
                }

                return md5;
            }
        }
    }
}
