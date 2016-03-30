// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class SessionData
    {
        private string               _serverVersion;
        private Encoding             _serverEncoding;
        private Encoding             _clientEncoding;
        private string               _applicationName;
        private bool                 _isSuperUser;
        private string               _sessionAuthorization;
        private string               _dateStyle;
        private string               _intervalStyle;
        private TimeZoneInfo         _timeZoneInfo;
        private bool                 _integerDateTimes;
        private string               _standardConformingStrings;
        private PgTypeInfoCollection _typeInfo;

        internal string               ServerVersion             => _serverVersion;
        internal Encoding             ServerEncoding            => _serverEncoding;
        internal Encoding             ClientEncoding            => _clientEncoding;
        internal string               ApplicationName           => _applicationName;       
        internal bool                 IsSuperUser               => _isSuperUser;       
        internal string               SessionAuthorization      => _sessionAuthorization;
        internal string               DateStyle                 => _dateStyle;
        internal string               IntervalStyle             => _intervalStyle;       
        internal TimeZoneInfo         TimeZoneInfo              => _timeZoneInfo;
        internal bool                 IntegerDateTimes          => _integerDateTimes;
        internal string               StandardConformingStrings => _standardConformingStrings;
        internal PgTypeInfoCollection TypeInfo                  => _typeInfo;

        internal SessionData()
        {
            _clientEncoding = Encoding.UTF8;
            _typeInfo       = PgTypeInfo.GetSupportedTypes();
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
                    _serverEncoding = PgCharacterSet.Charsets[value].Encoding;
                    break;

                case "client_encoding":
                    _clientEncoding = PgCharacterSet.Charsets[value].Encoding;
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