// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using PostgreSql.Data.Frontend.Security;

namespace PostgreSql.Data.Frontend.Sasl
{
    public static class SaslExtensions
    {
        /// <summary>
        /// Computes the SHA1 hash of a given byte array
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static byte[] ComputeSha256Hash(this byte[] buffer)
        {
            using (var algorithm = SHA256.Create())
            {
                return algorithm.ComputeHash(buffer);
            }
        }

        /// <summary>
        /// HMAC(key, str)  := Apply the HMAC keyed hash algorithm (defined in [RFC2104])
        /// </summary>
        public static byte[] ComputeHmacSha256(this byte[] keyMaterial, byte[] value)
        {
            using (var hmac = new HMACSHA256(keyMaterial))
            {
                return hmac.ComputeHash(value);
            }
        }

        /// <summary>
        /// HMAC(key, str)  := Apply the HMAC keyed hash algorithm (defined in [RFC2104])
        /// </summary>
        public static byte[] ComputeHmacSha256(this byte[] value)
        {
            using (var hmac = new HMACSHA256())
            {
                return hmac.ComputeHash(value);
            }
        }

        /// <summary>
        /// password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on HMACSHA1.
        /// </summary>
        /// <param name="password">The password used to derive the key.</param>
        /// <param name="salt">The key salt used to derive the key.</param>
        /// <param name="iterations">The number of iterations for the operation.</param>
        /// <returns>The generated pseudo-random key.</returns>
        internal static byte[] Rfc2898DeriveBytes(this byte[] password, byte[] salt, int iterations, int cb)
        {
            using (var algorithm = new PBKDF2(password, salt, iterations))
            {
                return algorithm.GetBytes(cb);
            }
        }
    }
}
