// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgCharacterSet
    {
        internal static readonly PgCharactersetCollection Charsets = new PgCharactersetCollection(16);

        static PgCharacterSet()
        {
            // http://www.postgresql.org/docs/9.1/static/multibyte.html
            Charsets.Add("SQL_ASCII" , "ascii");		    // ASCII
            // Charsets.Add("EUC_JP"    , "euc-jp");		// Japanese EUC
            // Charsets.Add("EUC_CN"    , "euc-cn");		// Chinese EUC
            Charsets.Add("UNICODE"   , "UTF-8");		    // Unicode (UTF-8)
            Charsets.Add("UTF8"      , "UTF-8");		    // UTF-8
            Charsets.Add("LATIN1"    , "iso-8859-1");	    // ISO 8859-1/ECMA 94 (Latin alphabet no.1)
            // Charsets.Add("LATIN2"    , "iso-8859-2");	// ISO 8859-2/ECMA 94 (Latin alphabet no.2)
            // Charsets.Add("LATIN4"    , 1257);			// ISO 8859-4/ECMA 94 (Latin alphabet no.4)
            // Charsets.Add("ISO_8859_7", 1253);			// ISO 8859-7/ECMA 118 (Latin/Greek)
            // Charsets.Add("LATIN9"    , "iso-8859-15");	// ISO 8859-15 (Latin alphabet no.9)
            // Charsets.Add("KOI8"      , "koi8-r");		// KOI8-R(U)
            // Charsets.Add("WIN"       , "windows-1251");	// Windows CP1251
            // Charsets.Add("WIN1251"   , "windows-1251");  // Windows CP1251
            // Charsets.Add("WIN1256"   , "windows-1256");	// Windows CP1256 (Arabic)
            // Charsets.Add("WIN1258"   , "windows-1258");	// TCVN-5712/Windows CP1258 (Vietnamese)
            // Charsets.Add("WIN1256"   , "windows-874");	// Windows CP874 (Thai)
        }

        private readonly string   _name;
        private readonly Encoding _encoding;

        internal string   Name     => _name;
        internal Encoding Encoding => _encoding;

        internal PgCharacterSet(string name, string systemCharset)
        {
            _name     = name;
            _encoding = Encoding.GetEncoding(systemCharset);
        }

        internal PgCharacterSet(string name, int cp)
        {
            _name     = name;
            _encoding = Encoding.GetEncoding(cp);
        }
    }
}
