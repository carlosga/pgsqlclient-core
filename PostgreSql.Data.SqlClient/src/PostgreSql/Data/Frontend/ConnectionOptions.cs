// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PostgreSql.Data.Frontend
{
    internal sealed class ConnectionOptions
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

        internal ConnectionOptions(string connectionString)
        {
            if (connectionString == null)
            {
                throw new InvalidOperationException("connectionString cannot be null.");
            }

            _connectionString           = connectionString;
            _applicationName            = "pgsqlclient";
            _commandTimeout             = 0;
            _connectionLifetime         = 0;
            _connectionTimeout          = 15;
            _dataSource                 = "localhost";
            _defaultTablespace          = null;
            _defaultTransactionReadOnly = false;
            _lockTimeout                = 0;
            _encrypt                    = false;
            _maxPoolSize                = 100;
            _minPoolSize                = 0;
            _multipleActiveResultSets   = false;
            _packetSize                 = 8192;
            _password                   = null;
            _pooling                    = true;
            _portNumber                 = 5432;
            _searchPath                 = null;
            _userId                     = "postgres";

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
                        case ConnectionStringSynonyms.App:
                        case ConnectionStringSynonyms.ApplicationName:
                            _applicationName = currentValue;
                            break;

                        case ConnectionStringSynonyms.CommandTimeout:
                        case ConnectionStringSynonyms.StatementTimeout:
                            _commandTimeout = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.ConnectionLifetime:
                            _connectionLifetime = Int64.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.ConnectionTimeout:
                        case ConnectionStringSynonyms.ConnectTimeout:
                        case ConnectionStringSynonyms.Timeout:
                            _connectionTimeout = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.DataSource:
                        case ConnectionStringSynonyms.Server:
                        case ConnectionStringSynonyms.Host:
                            _dataSource = currentValue;
                            break;

                        case ConnectionStringSynonyms.DefaultTablespace:
                            _defaultTablespace = currentValue;
                            break;

                        case ConnectionStringSynonyms.DefaultTransactionReadOnly:
                            _defaultTransactionReadOnly = Boolean.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.Encrypt:
                            _encrypt = Boolean.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.InitialCatalog:
                        case ConnectionStringSynonyms.Database:
                            _database = currentValue;
                            break;

                        case ConnectionStringSynonyms.LockTimeout:
                            _lockTimeout = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.PacketSize:
                            _packetSize = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.Password:
                        case ConnectionStringSynonyms.UserPassword:
                            _password = currentValue;
                            break;

                        case ConnectionStringSynonyms.Pooling:
                            _pooling = Boolean.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.PortNumber:
                        case ConnectionStringSynonyms.Port:
                            _portNumber = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.MaxPoolSize:
                            _maxPoolSize = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.MinPoolSize:
                            _minPoolSize = Int32.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.MultipleActiveResultSets:
                            _multipleActiveResultSets = Boolean.Parse(currentValue);
                            break;

                        case ConnectionStringSynonyms.SearchPath:
                            _searchPath = currentValue;
                            break;

                        case ConnectionStringSynonyms.UserId:
                        case ConnectionStringSynonyms.UserName:
                        case ConnectionStringSynonyms.User:
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
