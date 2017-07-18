// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal sealed class SessionData
    {
        private static readonly ReadOnlyDictionary<string, Lazy<Encoding>> Encodings;

        static SessionData()
        {
            var encodings = new Dictionary<string, Lazy<Encoding>>(20);

            // http://www.postgresql.org/docs/9.1/static/multibyte.html
            encodings["SQL_ASCII"] = new Lazy<Encoding>(() => new ASCIIEncoding());         // ASCII
            encodings["UNICODE"]   = new Lazy<Encoding>(() => new UTF8Encoding(false));     // Unicode (UTF-8)
            encodings["UTF8"]      = new Lazy<Encoding>(() => new UTF8Encoding(false));     // UTF-8
            encodings["LATIN1"]    = new Lazy<Encoding>(() => Encoding.GetEncoding("iso-8859-1")); // ISO 8859-1/ECMA 94 (Latin alphabet no.1)
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

            Encodings = new ReadOnlyDictionary<string, Lazy<Encoding>>(encodings);
        }

        private readonly DbConnectionOptions _connectionOptions;

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

        internal DbConnectionOptions ConnectionOptions         => _connectionOptions;
        internal string              ServerVersion             => _serverVersion;
        internal Encoding            ServerEncoding            => _serverEncoding;
        internal Encoding            ClientEncoding            => _clientEncoding;
        internal string              ApplicationName           => _applicationName;
        internal bool                IsSuperUser               => _isSuperUser;
        internal string              SessionAuthorization      => _sessionAuthorization;
        internal string              DateStyle                 => _dateStyle;
        internal string              IntervalStyle             => _intervalStyle;
        internal TimeZoneInfo        TimeZoneInfo              => _timeZoneInfo;
        internal bool                IntegerDateTimes          => _integerDateTimes;
        internal string              StandardConformingStrings => _standardConformingStrings;
        internal TypeInfoProvider    TypeInfoProvider          => _typeInfoProvider;

        internal SessionData(DbConnectionOptions connectionOptions, TypeInfoProvider typeInfoProvider)
        {
            _connectionOptions = connectionOptions;
            _typeInfoProvider  = typeInfoProvider;
            _clientEncoding    = Encoding.UTF8;
        }

        internal void SetValue(string name, string value)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(value))
            {
                return;
            }

            switch (name)
            {
                case "server_version":
                    _serverVersion = value;
                    break;

                case "server_encoding":
                    _serverEncoding = Encodings[value].Value;
                    break;

                case "client_encoding":
                    _clientEncoding = Encodings[value].Value;
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
                    _timeZoneInfo = ((value == "UTC") ? TimeZoneInfo.Utc : TimeZoneInfo.Local);
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
