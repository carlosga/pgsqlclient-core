// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend.Security;
using System;
using System.Text;

namespace PostgreSql.Data.Frontend.Sasl
{
    /// <summary>
    /// SASL SCRAM-SHA-256 authentication mechanism.
    /// </summary>
    /// <remarks>
    /// References: https://tools.ietf.org/html/rfc5802 / https://tools.ietf.org/html/rfc7677
    /// </remarks>
    internal sealed class SaslScramSha256
        : ISaslMechanism
    {
        private readonly Encoding _encoding;
        private string            _conce;
        private string            _clientFirstMessageBare;
        private string            _serverSignature;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SaslScramSha256"/> class.
        /// </summary>
        internal SaslScramSha256(Encoding encoding)
        {
            _encoding = encoding;
        }

        /// <summary>
        /// Starts the SASL negotiation process.
        /// </summary>
        /// <returns>
        /// A SASL auth instance.
        /// </returns>
        public byte[] Auth(string username)
        {
            _conce                  = CryptographicBuffer.EncodeToHexString(CryptographicBuffer.GenerateRandom(32));
            _clientFirstMessageBare = $"n=,r={_conce}";

            return _encoding.GetBytes($"n,,{_clientFirstMessageBare}");
        }

        /// <summary>
        /// Process the SASL challenge message.
        /// </summary>
        /// <param name="challenge">The server challenge.</param>
        /// <param name="password">The user password.</param>
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
            var saltedPassword       = password.Rfc2898DeriveBytes(salt, iterations, 32);
            var clientKey            = saltedPassword.ComputeHmacSha256(_encoding.GetBytes("Client Key"));
            var storedKey            = clientKey.ComputeSha256Hash();
            var authMessage          = $"{_clientFirstMessageBare},{serverFirstMessage},{clientFinalMessageWP}";
            var clientSignature      = storedKey.ComputeHmacSha256(_encoding.GetBytes(authMessage));
            var clientProof          = Convert.ToBase64String(CryptographicBuffer.Xor(clientKey, clientSignature));
            var serverKey            = saltedPassword.ComputeHmacSha256(_encoding.GetBytes("Server Key"));

            _serverSignature = Convert.ToBase64String(serverKey.ComputeHmacSha256(_encoding.GetBytes(authMessage)));

            return _encoding.GetBytes($"{clientFinalMessageWP},p={clientProof}");
        }

        /// <summary>
        /// Verifies the SASL success message if needed.
        /// </summary>
        /// <param name="signature">The server signature</param>
        /// <returns>
        ///   <b>true</b> if the reponse has been verified; otherwise <b>false</b>
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
    }
}
