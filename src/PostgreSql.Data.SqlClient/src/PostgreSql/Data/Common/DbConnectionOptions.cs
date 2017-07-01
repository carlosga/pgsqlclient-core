// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Net.Security;
using System.Text.RegularExpressions;
using PostgreSql.Data.Frontend;

namespace System.Data.Common
{
    internal sealed class DbConnectionOptions
    {
        private static readonly Regex s_tokenizer = new Regex(@"([\w\s\d]*)\s*=\s*([^;]*)", RegexOptions.Compiled);

        private string _connectionString;
        private string _applicationName;
        private int    _commandTimeout;
        private int    _connectRetryCount;
        private int    _connectRetryInterval;
        private int    _connectTimeout;
        private string _dataSource;
        private bool   _defaultTransactionReadOnly;
        private string _defaultTablespace;
        private bool   _encrypt;
        private string _initialCatalog;
        private int    _loadBalanceTimeout;
        private int    _lockTimeout;
        private int    _packetSize;
        private string _password;
        private int    _portNumber;
        private int    _maxPoolSize;
        private int    _minPoolSize;
        private bool   _multipleActiveResultSets;
        private bool   _pooling;
        private string _searchPath;
        private string _userId;

        internal string ConnectionString           => _connectionString;
        internal string ApplicationName            => _applicationName;
        internal int    CommandTimeout             => _commandTimeout;
        internal int    ConnectRetryCount          => _connectRetryCount;
        internal int    ConnectRetryInterval       => _connectRetryInterval;
        internal int    ConnectTimeout             => _connectTimeout;
        internal string DataSource                 => _dataSource;
        internal bool   DefaultTransactionReadOnly => _defaultTransactionReadOnly;
        internal string DefaultTablespace          => _defaultTablespace;
        internal bool   Encrypt                    => _encrypt;
        internal string InitialCatalog             => _initialCatalog;
        internal int    LoadBalanceTimeout         => _loadBalanceTimeout;
        internal int    LockTimeout                => _lockTimeout;
        internal int    PacketSize                 => _packetSize;
        internal int    MaxPoolSize                => _maxPoolSize;
        internal int    MinPoolSize                => _minPoolSize;
        internal bool   MultipleActiveResultSets   => _multipleActiveResultSets;
        internal string Password                   => _password;
        internal int    PortNumber                 => _portNumber;
        internal bool   Pooling                    => _pooling;
        internal string SearchPath                 => _searchPath;
        internal string UserID                     => _userId;
        internal string InternalUrl                => $"{DataSource}://{InitialCatalog}";

        internal bool IsEmpty => string.IsNullOrEmpty(_connectionString);

        internal NotificationCallback Notification
        {
            get;
            set;
        }

        internal InfoMessageCallback InfoMessage
        {
            get;
            set;
        }

        internal RemoteCertificateValidationCallback UserCertificateValidation
        {
            get;
            set;
        }

        internal LocalCertificateSelectionCallback UserCertificateSelection
        {
            get;
            set;
        }

        internal DbConnectionOptions(string connectionString)
        {
            if (connectionString == null)
            {
                throw ADP.ArgumentNull(nameof(connectionString));
            }

            _connectionString           = connectionString;
            _applicationName            = DbConnectionStringDefaults.ApplicationName;
            _commandTimeout             = DbConnectionStringDefaults.CommandTimeout;
            _connectRetryCount          = DbConnectionStringDefaults.ConnectRetryCount;
            _connectRetryInterval       = DbConnectionStringDefaults.ConnectRetryInterval;
            _connectTimeout             = DbConnectionStringDefaults.ConnectTimeout;
            _dataSource                 = DbConnectionStringDefaults.DataSource;
            _defaultTablespace          = DbConnectionStringDefaults.DefaultTablespace;
            _defaultTransactionReadOnly = DbConnectionStringDefaults.DefaultTransactionReadOnly;
            _encrypt                    = DbConnectionStringDefaults.Encrypt;
            _initialCatalog             = DbConnectionStringDefaults.InitialCatalog;
            _loadBalanceTimeout         = DbConnectionStringDefaults.LoadBalanceTimeout;
            _lockTimeout                = DbConnectionStringDefaults.LockTimeout;
            _maxPoolSize                = DbConnectionStringDefaults.MaxPoolSize;
            _minPoolSize                = DbConnectionStringDefaults.MinPoolSize;
            _multipleActiveResultSets   = DbConnectionStringDefaults.MultipleActiveResultSets;
            _packetSize                 = DbConnectionStringDefaults.PacketSize;
            _password                   = DbConnectionStringDefaults.Password;
            _pooling                    = DbConnectionStringDefaults.Pooling;
            _portNumber                 = DbConnectionStringDefaults.PortNumber;
            _searchPath                 = DbConnectionStringDefaults.SearchPath;
            _userId                     = DbConnectionStringDefaults.UserID;

            ParseConnectionString(connectionString);
        }

        internal void ChangeDatabase(string value)
        {
            _initialCatalog   = value;
            _connectionString = UsersConnectionString(false);
        }

        private void ParseConnectionString(string connectionString)
        {
            var tokens = s_tokenizer.Matches(connectionString);

            foreach (Match token in tokens)
            {
                var currentValue = token.Groups[2].Value?.Trim(); 
                
                if (!String.IsNullOrEmpty(currentValue))
                {
                    switch (token.Groups[1].Value.Trim().ToLower())
                    {
                    case DbConnectionStringSynonyms.App:
                    case DbConnectionStringSynonyms.ApplicationName:
                        _applicationName = currentValue;
                        break;

                    case DbConnectionStringSynonyms.CommandTimeout:
                    case DbConnectionStringSynonyms.StatementTimeout:
                        _commandTimeout = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.ConnectRetryCount:
                        _connectRetryCount = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.ConnectRetryInterval:
                        _connectRetryInterval = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.ConnectTimeout:
                    case DbConnectionStringSynonyms.ConnectionTimeout:
                    case DbConnectionStringSynonyms.Timeout:
                        _connectTimeout = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.DataSource:
                    case DbConnectionStringSynonyms.Server:
                    case DbConnectionStringSynonyms.Host:
                        _dataSource = currentValue;
                        break;

                    case DbConnectionStringSynonyms.DefaultTablespace:
                        _defaultTablespace = currentValue;
                        break;

                    case DbConnectionStringSynonyms.DefaultTransactionReadOnly:
                        _defaultTransactionReadOnly = Boolean.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.Encrypt:
                        _encrypt = Boolean.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.InitialCatalog:
                    case DbConnectionStringSynonyms.Database:
                        _initialCatalog = currentValue;
                        break;

                    case DbConnectionStringSynonyms.LoadBalanceTimeout:
                    case DbConnectionStringSynonyms.ConnectionLifetime:
                        _loadBalanceTimeout = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.LockTimeout:
                        _lockTimeout = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.PacketSize:
                        _packetSize = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.Password:
                    case DbConnectionStringSynonyms.UserPassword:
                        _password = currentValue;
                        break;

                    case DbConnectionStringSynonyms.Pooling:
                        _pooling = Boolean.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.PortNumber:
                    case DbConnectionStringSynonyms.Port:
                        _portNumber = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.MaxPoolSize:
                        _maxPoolSize = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.MinPoolSize:
                        _minPoolSize = Int32.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.MultipleActiveResultSets:
                        _multipleActiveResultSets = Boolean.Parse(currentValue);
                        break;

                    case DbConnectionStringSynonyms.SearchPath:
                        _searchPath = currentValue;
                        break;

                    case DbConnectionStringSynonyms.UserId:
                    case DbConnectionStringSynonyms.UserName:
                    case DbConnectionStringSynonyms.User:
                        _userId = currentValue;
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(_userId) || string.IsNullOrEmpty(_dataSource))
            {
                throw ADP.InvalidConnectionStringArgument();
            }
            else if (_packetSize < 512 || _packetSize > 32767)
            {
                throw ADP.InvalidPacketSizeValue(_packetSize);
            }
            else if (_connectRetryCount < 0 || _connectRetryCount > 255)
            {
                throw ADP.InvalidConnectRetryCountValue(_connectRetryCount);
            }
            else if (_connectRetryInterval < 1 || _connectRetryInterval > 60)
            {
                throw ADP.InvalidConnectRetryIntervalValue(_connectRetryInterval);
            }
            else if (_connectTimeout < 0 || _connectTimeout > 2147483647)
            {
                throw ADP.InvalidConnectTimeoutValue(_connectTimeout);
            }
            else if (_commandTimeout < 0 || _commandTimeout > 2147483647)
            {
                throw ADP.InvalidCommandTimeoutValue(_commandTimeout);
            }
            else if (_lockTimeout < 0 || _lockTimeout > 2147483647)
            {
                throw ADP.InvalidLockTimeoutValue(_lockTimeout);
            }
            else if (_minPoolSize < 0 || _maxPoolSize < 0 || _minPoolSize > _maxPoolSize)
            {
                throw ADP.InvalidMinMaxPoolSizeValues();
            }
        }

        internal string UsersConnectionString(bool hidePassword)
        {
            var builder = new PostgreSql.Data.SqlClient.PgConnectionStringBuilder();

            builder.ApplicationName            = ApplicationName;
            builder.CommandTimeout             = CommandTimeout;
            builder.ConnectRetryCount          = ConnectRetryCount;
            builder.ConnectRetryInterval       = ConnectRetryInterval;
            builder.ConnectTimeout             = ConnectTimeout;
            builder.DataSource                 = DataSource;
            builder.DefaultTransactionReadOnly = DefaultTransactionReadOnly;
            builder.DefaultTablespace          = DefaultTablespace;
            builder.Encrypt                    = Encrypt;
            builder.InitialCatalog             = InitialCatalog;
            builder.LoadBalanceTimeout         = LoadBalanceTimeout;
            builder.LockTimeout                = LockTimeout;
            builder.PacketSize                 = PacketSize;
            builder.Password                   = ((hidePassword) ? String.Empty : Password);
            builder.Pooling                    = Pooling;
            builder.PortNumber                 = PortNumber;
            builder.MaxPoolSize                = MaxPoolSize;
            builder.MinPoolSize                = MinPoolSize;
            builder.MultipleActiveResultSets   = MultipleActiveResultSets;
            builder.SearchPath                 = SearchPath;
            builder.UserID                     = UserID;

            return builder.ToString();
        }
    }
}
