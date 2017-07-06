// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// -------------------------------------------------------------------------------------------------------------
// Adapted to SASL profile from GNU Libidn Stringprep implementation
// -------------------------------------------------------------------------------------------------------------

using Gnu.Inet.Encoding;
using System.Text;
using System.Data.Common;

namespace PostgreSql.Data.Frontend.Sasl
{
    internal static class Saslprep
    {
        internal static string[] C11Replace = new string[] {
            '\u0020'.ToString(),	/* 0020; SPACE */
        };

        /// <summary>
        /// Apply the SASLprep profile [RFC4013] of the "stringprep" algorithm [RFC3454] as the normalization
        /// algorithm to a UTF-8 [RFC3629] encoded string.
        /// Unassigned code points are not allowed.
        /// </summary>
        /// <param name="input">the name to prep.</param>
        /// <returns>
        /// the prepped name.
        /// @throws StringprepException If the name cannot be prepped with
        /// this profile.
        /// @throws NullPointerException If the name is null.
        /// </returns>
        internal static string SaslPrep(string input)
        {
            return SaslPrep(input, false);
        }

        /// <summary>
        /// Apply the SASLprep profile [RFC4013] of the "stringprep" algorithm [RFC3454] as the normalization
        /// algorithm to a UTF-8 [RFC3629] encoded string.
        /// </summary>
        /// <param name="input">the string to prep.</param>
        /// <param name="allowUnassigned">true if the name may contain unassigned code points.</param>
        /// <returns> the prepped name.
        /// @throws StringprepException If the name cannot be prepped with
        /// this profile.
        /// @throws NullPointerException If the name is null.
        /// </returns>
        internal static string SaslPrep(string input, bool allowUnassigned)
        {
            if (input == null)
            {
                throw ADP.ArgumentNull(nameof(input));
            }

            StringBuilder s = new StringBuilder(input);

            // Unassigned Code Points
            if (!allowUnassigned && Stringprep.Contains(s, RFC3454.A1))
            {
                throw new StringprepException(StringprepException.CONTAINS_UNASSIGNED);
            }

            // Mapping
            Stringprep.Filter(s, RFC3454.B1);
            Stringprep.Map(s, RFC3454.C12, C11Replace);

            // Normalization
            s = new StringBuilder(NFKC.NormalizeNFKC(s.ToString()));

            // Prohibited Output
            if (Stringprep.Contains(s, RFC3454.C12) // - Non-ASCII space characters [StringPrep, C.1.2]
             || Stringprep.Contains(s, RFC3454.C21) // - ASCII control characters [StringPrep, C.2.1]
             || Stringprep.Contains(s, RFC3454.C22) // - Non-ASCII control characters [StringPrep, C.2.2]
             || Stringprep.Contains(s, RFC3454.C3)  // - Private Use characters [StringPrep, C.3]
             || Stringprep.Contains(s, RFC3454.C4)  // - Non-character code points [StringPrep, C.4]
             || Stringprep.Contains(s, RFC3454.C5)  // - Surrogate code points [StringPrep, C.5]
             || Stringprep.Contains(s, RFC3454.C6)  // - Inappropriate for plain text characters [StringPrep, C.6]
             || Stringprep.Contains(s, RFC3454.C7)  // - Inappropriate for canonical representation characters [StringPrep, C.7]
             || Stringprep.Contains(s, RFC3454.C8)) // - Change display properties or deprecated characters [StringPrep, C.8]
            {
                // Table C.9 only contains code points > 0xFFFF which Java doesn't handle
                throw new StringprepException(StringprepException.CONTAINS_PROHIBITED);
            }

            // Bidirectional Characters
            bool r = Stringprep.Contains(s, RFC3454.D1);
            bool l = Stringprep.Contains(s, RFC3454.D2);

            // RFC 3454, section 6, requirement 1: already handled above (table C.8)

            // RFC 3454, section 6, requirement 2
            if (r && l)
            {
                throw new StringprepException(StringprepException.BIDI_BOTHRAL);
            }

            // RFC 3454, section 6, requirement 3
            if (r)
            {
                if (!Stringprep.Contains(s[0], RFC3454.D1) || !Stringprep.Contains(s[s.Length - 1], RFC3454.D1))
                {
                    throw new StringprepException(StringprepException.BIDI_LTRAL);
                }
            }

            return s.ToString();
        }
    }
}
