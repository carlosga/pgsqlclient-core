// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PostgreSql.Data.Frontend.Sasl
{
    internal static class SaslTokenizer
    {
        private static readonly Regex s_saslRegex = new Regex
        (
            @"(?<key>[\w\s\d^=])=(?<value>[^,]*)", RegexOptions.Singleline | RegexOptions.ExplicitCapture | RegexOptions.Compiled
        );

        internal static Dictionary<string, string> ToDictionary(string value)
        {
            var matches = s_saslRegex.Matches(value);
            var kvp     = new Dictionary<string, string>(matches.Count);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    kvp.Add(match.Groups["key"].Value, match.Groups["value"].Value);
                }
            }

            return kvp;
        }
    }
}
