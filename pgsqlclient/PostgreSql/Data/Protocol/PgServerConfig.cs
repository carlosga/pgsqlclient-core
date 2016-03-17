// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgServerConfig
    {
        private string           _serverVersion;
        private Encoding         _serverEncoding;
        private Encoding         _clientEncoding;
        private string           _applicationName;
        private bool             _isSuperUser;
        private string           _sessionAuthorization;
        private string           _dateStyle;
        private string           _intervalStyle;
        private string           _timeZone;
        private bool             _integerDateTimes;
        private string           _standardConformingStrings;
        private PgTypeCollection _dataTypes;

        internal string ServerVersion
        {
            get { return _serverVersion; }
        }
        
        internal Encoding ServerEncoding
        {
            get { return _serverEncoding; }
        }
        
        internal Encoding ClientEncoding
        {
            get { return _clientEncoding; }
        }
        
        internal string ApplicationName
        {
            get { return _applicationName; }
        }
        
        internal bool IsSuperUser
        {
            get { return _isSuperUser;  }
        }
        
        internal string SessionAuthorization
        {
            get { return _sessionAuthorization; }
        }
        
        internal string DateStyle
        {
            get { return _dateStyle; }
        }
        
        internal string IntervalStyle
        {
            get { return _intervalStyle; }
        }
        
        internal string TimeZone
        {
            get { return _timeZone; }
        }
        
        internal bool IntegerDateTimes
        {
            get { return _integerDateTimes; }
        }
        
        internal string StandardConformingStrings
        {
            get { return _standardConformingStrings; }
        }

        internal PgTypeCollection DataTypes
        {
            get { return _dataTypes; }
        }

        internal PgServerConfig()
        {
            _clientEncoding = Encoding.UTF8;
            _dataTypes      = PgType.GetSupportedTypes();
        }
        
        internal void SetValue(string name, string value)
        {
            if (String.IsNullOrEmpty(name)
             || String.IsNullOrEmpty(value))
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
                    _isSuperUser = Convert.ToBoolean(value);
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
                    _timeZone = value;
                    break;
                    
                case "integer_datetimes":
                    _integerDateTimes = Convert.ToBoolean(value);
                    break;
                    
                case "standard_conforming_strings":
                    _standardConformingStrings = value;
                    break;                    
            }
        }
    }
}