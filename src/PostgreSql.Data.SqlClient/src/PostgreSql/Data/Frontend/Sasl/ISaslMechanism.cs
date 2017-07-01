// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Frontend.Sasl
{
    /// <summary>
    /// Base class for SASL mechanims implementations.
    /// </summary>
    internal interface ISaslMechanism
    {
        /// <summary>
        /// Starts the SASL negotiation process.
        /// </summary>
        /// <returns>A SASL auth initial message.</returns>
        byte[] Auth();

        /// <summary>
        /// Process the SASL challenge message.
        /// </summary>
        /// <param name="challenge">The server challenge.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns>The challenge response.</returns>
        byte[] Challenge(byte[] challenge, string userPassword);

        /// <summary>
        /// Verifies the SASL success message if needed.
        /// </summary>
        /// <param name="signature">The server signature</param>
        /// <returns><b>true</b> if the server signature has been verified; otherwise <b>false</b></returns>
        bool Verify(byte[] signature);
    }
}
