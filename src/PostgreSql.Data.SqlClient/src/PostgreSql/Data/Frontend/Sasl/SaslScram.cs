// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend.Security;
using System;
using System.Text;
using System.Security.Cryptography;

namespace PostgreSql.Data.Frontend.Sasl
{
    /// <summary>
    /// SASL SCRAM authentication mechanism.
    /// </summary>
    /// <remarks>
    /// References: https://tools.ietf.org/html/rfc5802 / https://tools.ietf.org/html/rfc7677
    /// </remarks>
    internal sealed class SaslScram
        : ISaslMechanism
    {
        internal static SaslScram Create(string name)
        {
            return Create(name, Encoding.UTF8);
        }

        internal static SaslScram Create(string name, Encoding encoding)
        {
            switch (name)
            {
            case "SCRAM-SHA-256":
                return new SaslScram(HashAlgorithmName.SHA256, encoding);
            
            default:
                throw new NotSupportedException("Unknown SASL SCRAM authentication mechanism");
            }
        }

        private readonly Encoding          _encoding;
        private readonly string            _conce;
        private readonly string            _clientFirstMessageBare;
        private readonly HashAlgorithmName _hashAlgorithmName;
        private string                     _serverSignature;        

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SaslScram"/> class.
        /// </summary>
        private SaslScram(HashAlgorithmName hashAlgorithmName, Encoding encoding)
        {
            _encoding               = encoding;
            _hashAlgorithmName      = hashAlgorithmName;
            _conce                  = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.GenerateRandom(32));
            _clientFirstMessageBare = $"n=,r={_conce}";
        }

        /// <summary>
        /// Starts the SASL negotiation process.
        /// </summary>
        /// <returns>A SASL auth initial message.</returns>
        public byte[] Auth()
        {
            return _encoding.GetBytes($"n,,{_clientFirstMessageBare}");
        }

        /// <summary>
        /// Process the SASL challenge message.
        /// </summary>
        /// <param name="challenge">The server challenge.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns>
        /// The challenge response.
        /// </returns>
        public byte[] Challenge(byte[] challenge, string userPassword)
        {
            var password           = _encoding.GetBytes(Saslprep.SaslPrep(userPassword));
            var serverFirstMessage = _encoding.GetString(challenge, 0, challenge.Length);
            var tokens             = SaslTokenizer.ToDictionary(serverFirstMessage);
            var nonce              = tokens["r"];
            var salt               = Convert.FromBase64String(tokens["s"]);
            
            if (!nonce.StartsWith(_conce))
            {
                return null;
            }
            
            if (!Int32.TryParse(tokens["i"], out var iterations))
            {
                return null;
            }

            // For the SCRAM-SHA-256 and SCRAM-SHA-256-PLUS SASL mechanisms, the
            // hash iteration-count announced by a server SHOULD be at least 4096.
            if (iterations < 4096)
            {
                return null;
            }

            // SaltedPassword  := Hi(Normalize(password), salt, i)
            // ClientKey       := HMAC(SaltedPassword, "Client Key")
            // StoredKey       := H(ClientKey)
            // AuthMessage     := client-first-message-bare + "," +
            //                    server-first-message + "," +
            //                    client-final-message-without-proof
            // ClientSignature := HMAC(StoredKey, AuthMessage)
            // ClientProof     := ClientKey XOR ClientSignature
            // ServerKey       := HMAC(SaltedPassword, "Server Key")
            // ServerSignature := HMAC(ServerKey, AuthMessage)

            var gs2Header            = Convert.ToBase64String(_encoding.GetBytes("n,,"));
            var clientFinalMessageWP = $"c={gs2Header},r={nonce}";
            var saltedPassword       = password.Rfc2898DeriveBytes(salt, iterations, BlockSize, _hashAlgorithmName);
            var clientKey            = ComputeHmac(saltedPassword, _encoding.GetBytes("Client Key"));
            var storedKey            = ComputeHash(clientKey);
            var authMessage          = $"{_clientFirstMessageBare},{serverFirstMessage},{clientFinalMessageWP}";
            var clientSignature      = ComputeHmac(storedKey, _encoding.GetBytes(authMessage));
            var clientProof          = Convert.ToBase64String(CryptographicBuffer.Xor(clientKey, clientSignature));
            var serverKey            = ComputeHmac(saltedPassword, _encoding.GetBytes("Server Key"));

            _serverSignature = Convert.ToBase64String(ComputeHmac(serverKey, _encoding.GetBytes(authMessage)));

            return _encoding.GetBytes($"{clientFinalMessageWP},p={clientProof}");
        }

        /// <summary>
        /// Verifies the SASL success message if needed.
        /// </summary>
        /// <param name="signature">The server signature</param>
        /// <returns>
        ///   <b>true</b> if the server signature has been verified; otherwise <b>false</b>
        /// </returns>
        public bool Verify(byte[] signature)
        {
            // The server verifies the nonce and the proof, verifies that the
            // authorization identity (if supplied by the client in the first
            // message) is authorized to act as the authentication identity, and,
            // finally, it responds with a "server-final-message", concluding the
            // authentication exchange.
            var serverFinalMessage = _encoding.GetString(signature, 0, signature.Length);
            var tokens             = SaslTokenizer.ToDictionary(serverFinalMessage);
            var serverSignature    = tokens["v"];

            return (serverSignature == _serverSignature);
        }

        private byte[] ComputeHash(byte[] buffer) 
        {
            using (var hash = CreateHashAlgorithm(_hashAlgorithmName))
            {
                return hash.ComputeHash(buffer);
            }
        }

        private byte[] ComputeHmac(byte[] keyMaterial, byte[] buffer)
        {
            using (var hmac = CreateHmacAlgorithm(_hashAlgorithmName, keyMaterial))
            {
                return hmac.ComputeHash(buffer);
            }
        }
        
        private int BlockSize
        {
            get 
            {
                if (_hashAlgorithmName == HashAlgorithmName.SHA256)
                {
                    return 32;
                }
                else if (_hashAlgorithmName == HashAlgorithmName.SHA384)
                {
                    return 48;
                }
                else if (_hashAlgorithmName == HashAlgorithmName.SHA512)
                {
                    return 64;
                }

                throw new NotSupportedException();                                
            }
        }

        private static HMAC CreateHmacAlgorithm(HashAlgorithmName name, byte[] keyMaterial)
        {
            if (name == HashAlgorithmName.SHA256)
            {
                return new HMACSHA256(keyMaterial);
            }
            else if (name == HashAlgorithmName.SHA384)
            {
                return new HMACSHA384(keyMaterial);
            }
            else if (name == HashAlgorithmName.SHA512)
            {
                return new HMACSHA512(keyMaterial);
            }

            throw new NotSupportedException();
        }

        private static HashAlgorithm CreateHashAlgorithm(HashAlgorithmName name)
        {
            if (name == HashAlgorithmName.SHA256)
            {
                return SHA256.Create();
            }
            else if (name == HashAlgorithmName.SHA384)
            {
                return SHA384.Create();
            }
            else if (name == HashAlgorithmName.SHA512)
            {
                return SHA512.Create();
            }
            
            throw new NotSupportedException();
        }
    }
}
