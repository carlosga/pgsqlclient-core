// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using PostgreSql.Data.Frontend.Security;

namespace PostgreSql.Data.Frontend.Sasl
{
    internal static class SaslExtensions
    {
        /// <summary>
        /// password-based key derivation functionality, PBKDF2, by using a pseudo-random number generator based on HMACSHA1.
        /// </summary>
        /// <param name="password">The password used to derive the key.</param>
        /// <param name="salt">The key salt used to derive the key.</param>
        /// <param name="iterations">The number of iterations for the operation.</param>
        /// <returns>The generated pseudo-random key.</returns>
        internal static byte[] Rfc2898DeriveBytes(this byte[] password, byte[] salt, int iterations, int cb, HashAlgorithmName name)
        {
            using (var algorithm = new PBKDF2(password, salt, iterations, name))
            {
                return algorithm.GetBytes(cb);
            }
        }
    }
}
