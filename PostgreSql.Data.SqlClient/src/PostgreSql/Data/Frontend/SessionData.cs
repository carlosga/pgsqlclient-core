// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal sealed class SessionData
    {
        internal static readonly ReadOnlyDictionary<string, Encoding> Encodings;

        static SessionData()
        {
            Dictionary<string, Encoding> encodings = new Dictionary<string, Encoding>(4);

            // http://www.postgresql.org/docs/9.1/static/multibyte.html
            encodings["SQL_ASCII"] = new ASCIIEncoding();                       // ASCII
            encodings["UNICODE"]   = new UTF8Encoding(false);                   // Unicode (UTF-8)
            encodings["UTF8"]      = new UTF8Encoding(false);                   // UTF-8
            encodings["LATIN1"]    = Encoding.GetEncoding("iso-8859-1");        // ISO 8859-1/ECMA 94 (Latin alphabet no.1)

            // encodings["EUC_JP"]     = Encoding.GetEncoding("euc-jp");        // Japanese EUC
            // encodings["EUC_CN"]     = Encoding.GetEncoding("euc-cn");        // Chinese EUC
            // encodings["LATIN2"]     = Encoding.GetEncoding("iso-8859-2");    // ISO 8859-2/ECMA 94 (Latin alphabet no.2)
            // encodings["LATIN4"]     = Encoding.GetEncoding(1257);            // ISO 8859-4/ECMA 94 (Latin alphabet no.4)
            // encodings["ISO_8859_7"] = Encoding.GetEncoding(1253);            // ISO 8859-7/ECMA 118 (Latin/Greek)
            // encodings["LATIN9"]     = Encoding.GetEncoding("iso-8859-15");   // ISO 8859-15 (Latin alphabet no.9)
            // encodings["KOI8"]       = Encoding.GetEncoding("koi8-r");        // KOI8-R(U)
            // encodings["WIN"]        = Encoding.GetEncoding("windows-1251");  // Windows CP1251
            // encodings["WIN1251"]    = Encoding.GetEncoding("windows-1251");  // Windows CP1251
            // encodings["WIN1256"]    = Encoding.GetEncoding("windows-1256");  // Windows CP1256 (Arabic)
            // encodings["WIN1258"]    = Encoding.GetEncoding("windows-1258");  // TCVN-5712/Windows CP1258 (Vietnamese)
            // encodings["WIN1256"]    = Encoding.GetEncoding("windows-874");   // Windows CP874 (Thai)

            Encodings = new ReadOnlyDictionary<string, Encoding>(encodings);
        }

        private string           _serverVersion;
        private Encoding         _serverEncoding;
        private Encoding         _clientEncoding;
        private string           _applicationName;
        private bool             _isSuperUser;
        private string           _sessionAuthorization;
        private string           _dateStyle;
        private string           _intervalStyle;
        private TimeZoneInfo     _timeZoneInfo;
        private bool             _integerDateTimes;
        private string           _standardConformingStrings;
        private TypeInfoProvider _typeInfoProvider;

        internal string       ServerVersion             => _serverVersion;
        internal Encoding     ServerEncoding            => _serverEncoding;
        internal Encoding     ClientEncoding            => _clientEncoding;
        internal string       ApplicationName           => _applicationName;
        internal bool         IsSuperUser               => _isSuperUser;
        internal string       SessionAuthorization      => _sessionAuthorization;
        internal string       DateStyle                 => _dateStyle;
        internal string       IntervalStyle             => _intervalStyle;
        internal TimeZoneInfo TimeZoneInfo              => _timeZoneInfo;
        internal bool         IntegerDateTimes          => _integerDateTimes;
        internal string       StandardConformingStrings => _standardConformingStrings;
        
        internal TypeInfoProvider TypeInfoProvider
        {
            get { return _typeInfoProvider; }
            set { _typeInfoProvider = value; }
        }

        internal SessionData()
        {
            _clientEncoding = Encoding.UTF8;
        }

        internal void SetValue(string name, string value)
        {
            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(value))
            {
                return;
            }

            switch (name)
            {
                case "server_version":
                    _serverVersion = value;
                    break;

                case "server_encoding":
                    _serverEncoding = Encodings[value];
                    break;

                case "client_encoding":
                    _clientEncoding = Encodings[value];
                    break;

                case "application_name":
                    _applicationName = value;
                    break;

                case "is_superuser":
                    _isSuperUser = ((value) == "on");
                    break;

                case "session_authorization":
                    _sessionAuthorization = value;
                    break;

                case "DateStyle":
                    _dateStyle = value;
                    break;

                case "IntervalStyle":
                    _intervalStyle = value;
                    break;

                case "TimeZone":
#warning TODO: Add support for non local time zones
                    _timeZoneInfo = TimeZoneInfo.Local;
                    break;

                case "integer_datetimes":
                    _integerDateTimes = ((value) == "on");
                    break;

                case "standard_conforming_strings":
                    _standardConformingStrings = value;
                    break;
            }
        }
    }
}
