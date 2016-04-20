// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace System.Data.Common
{
    internal sealed class DbConnectionOptions
    {
        private static readonly Regex s_tokenizer = new Regex(@"([\w\s\d]*)\s*=\s*([^;]*)", RegexOptions.Compiled);

        private string _connectionString;
        private string _applicationName;
        private int    _commandTimeout;
        private long   _connectionLifetime;
        private int    _connectionTimeout;
        private string _database;
        private string _dataSource;
        private bool   _defaultTransactionReadOnly;
        private string _defaultTablespace;
        private bool   _encrypt;
        private int    _packetSize;
        private string _password;
        private int    _portNumber;
        private int    _maxPoolSize;
        private int    _minPoolSize;
        private bool   _multipleActiveResultSets;
        private bool   _pooling;
        private string _searchPath;
        private int    _lockTimeout;
        private string _userId;
        private int    _loadBalanceTimeout;

        internal string ConnectionString           => _connectionString;
        internal string ApplicationName            => _applicationName;
        internal int    CommandTimeout             => _commandTimeout;
        internal long   ConnectionLifeTime         => _connectionLifetime;
        internal int    ConnectionTimeout          => _connectionTimeout;
        internal string Database                   => _database;
        internal string DataSource                 => _dataSource;
        internal bool   DefaultTransactionReadOnly => _defaultTransactionReadOnly;
        internal string DefaultTablespace          => _defaultTablespace;
        internal bool   Encrypt                    => _encrypt;
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
        internal int    LoadBalanceTimeout         => _loadBalanceTimeout;

        internal bool   IsEmpty => String.IsNullOrEmpty(_connectionString);  

        internal DbConnectionOptions(string connectionString)
        {
            if (connectionString == null)
            {
                throw new InvalidOperationException("connectionString cannot be null.");
            }

            _connectionString           = connectionString;
            _applicationName            = DbConnectionStringDefaults.ApplicationName;
            _commandTimeout             = DbConnectionStringDefaults.CommandTimeout;
            _connectionLifetime         = DbConnectionStringDefaults.ConnectionLifetime;
            _connectionTimeout          = DbConnectionStringDefaults.ConnectionTimeout;
            _dataSource                 = DbConnectionStringDefaults.DataSource;
            _defaultTablespace          = DbConnectionStringDefaults.DefaultTablespace;
            _defaultTransactionReadOnly = DbConnectionStringDefaults.DefaultTransactionReadOnly;
            _lockTimeout                = DbConnectionStringDefaults.LockTimeout;
            _encrypt                    = DbConnectionStringDefaults.Encrypt;
            _maxPoolSize                = DbConnectionStringDefaults.MaxPoolSize;
            _minPoolSize                = DbConnectionStringDefaults.MinPoolSize;
            _multipleActiveResultSets   = DbConnectionStringDefaults.MultipleActiveResultSets;
            _packetSize                 = DbConnectionStringDefaults.PacketSize;
            _password                   = DbConnectionStringDefaults.Password;
            _pooling                    = DbConnectionStringDefaults.Pooling;
            _portNumber                 = DbConnectionStringDefaults.PortNumber;
            _searchPath                 = DbConnectionStringDefaults.SearchPath;
            _userId                     = DbConnectionStringDefaults.UserID;
            _loadBalanceTimeout         = 0;

            ParseConnectionString(connectionString);
        }

        internal void ChangeDatabase(string database)
        {
#warning TODO: Rebuild the connection string ??
            _database = database;
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

                        case DbConnectionStringSynonyms.ConnectionLifetime:
                            _connectionLifetime = Int64.Parse(currentValue);
                            break;

                        case DbConnectionStringSynonyms.ConnectionTimeout:
                        case DbConnectionStringSynonyms.ConnectTimeout:
                        case DbConnectionStringSynonyms.Timeout:
                            _connectionTimeout = Int32.Parse(currentValue);
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
                            _database = currentValue;
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

            if (String.IsNullOrEmpty(_userId) || String.IsNullOrEmpty(_dataSource))
            {
                throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
            }
            else if (_packetSize < 512 || _packetSize > 32767)
            {
                string msg = $"'Packet Size' value of {_packetSize} is not valid.\r\nThe value should be an integer >= 512 and <= 32767.";

                throw new ArgumentException(msg);
            }
            else if (_connectionTimeout < 0 || _connectionTimeout > 2147483647)
            {
                string msg = $"'Connection Timeout' value of {_connectionTimeout} is not valid.\r\nThe value should be an integer >= 0 and <= 2147483647.";

                throw new ArgumentException(msg);
            }
            else if (_commandTimeout < 0 || _commandTimeout > 2147483647)
            {
                string msg = $"'Command Timeout' value of {_commandTimeout} is not valid.\r\nThe value should be an integer >= 0 and <= 2147483647.";

                throw new ArgumentException(msg);
            }
            else if (_lockTimeout < 0 || _lockTimeout > 2147483647)
            {
                string msg = $"'Lock Timeout' value of {_lockTimeout} is not valid.\r\nThe value should be an integer >= 0 and <= 2147483647.";

                throw new ArgumentException(msg);
            }
        }
    }
}
