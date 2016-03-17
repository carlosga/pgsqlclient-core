// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Text.RegularExpressions;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgConnectionOptions
    {
        private string _dataSource;
        private string _database;
        private string _userId;
        private string _password;
        private int    _portNumber;
        private int    _packetSize;
        private int    _connectionTimeout;
        private long   _connectionLifetime;
        private int    _minPoolSize;
        private int    _maxPoolSize;
        private bool   _pooling;
        private bool   _ssl;
        private bool   _useDatabaseOids;

        internal string DataSource
        {
            get { return _dataSource; }
        }

        internal string Database
        {
            get { return _database; }
        }

        internal string UserID
        {
            get { return _userId; }
        }

        internal string Password
        {
            get { return _password; }
        }

        internal int PacketSize
        {
            get { return _packetSize; }
        }

        internal int PortNumber
        {
            get { return _portNumber; }
        }

        internal int ConnectionTimeout
        {
            get { return _connectionTimeout; }
        }

        internal long ConnectionLifeTime
        {
            get { return _connectionLifetime; }
        }

        internal int MinPoolSize
        {
            get { return _minPoolSize; }
        }

        internal int MaxPoolSize
        {
            get { return _maxPoolSize; }
        }

        internal bool Pooling
        {
            get { return _pooling; }
        }

        internal bool Ssl
        {
            get { return _ssl; }
        }

        internal bool UseDatabaseOids
        {
            get { return _useDatabaseOids; }
        }

        internal PgConnectionOptions(string connectionString)
        {
            if (connectionString == null)
            {
                throw new InvalidOperationException("connectionString cannot be null.");
            }

            _dataSource         = "localhost";
            _userId             = "postgres";
            _password           = null;
            _portNumber         = 5432;
            _packetSize         = 8192;
            _pooling            = true;
            _connectionTimeout  = 15;
            _connectionLifetime = 0;
            _minPoolSize        = 0;
            _maxPoolSize        = 100;
            _ssl                = false;
            _useDatabaseOids    = false;
            
            ParseConnectionString(connectionString);
        }

        private void ParseConnectionString(string connectionString)
        {
            var search   = new Regex(@"([\w\s\d]*)\s*=\s*([^;]*)");
            var elements = search.Matches(connectionString);

            foreach (Match element in elements)
            {
                if (!String.IsNullOrEmpty(element.Groups[2].Value))
                {
                    switch (element.Groups[1].Value.Trim().ToLower())
                    {
                        case "data source":
                        case "server":
                        case "host":
                            _dataSource = element.Groups[2].Value.Trim();
                            break;

                        case "database":
                        case "initial catalog":
                            _database = element.Groups[2].Value.Trim();
                            break;

                        case "user name":
                        case "user id":
                        case "user":
                            _userId = element.Groups[2].Value.Trim();
                            break;

                        case "user password":
                        case "password":
                            _password = element.Groups[2].Value.Trim();
                            break;

                        case "port number":
                            _portNumber = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "connection timeout":
                            _connectionTimeout = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "packet size":
                            _packetSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "pooling":
                            _pooling = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "connection lifetime":
                            _connectionLifetime = Int64.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "min pool size":
                            _minPoolSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "max pool size":
                            _maxPoolSize = Int32.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "ssl":
                            _ssl = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;

                        case "use database oids":
                            _useDatabaseOids = Boolean.Parse(element.Groups[2].Value.Trim());
                            break;
                    }
                }
            }

            if (String.IsNullOrEmpty(_userId) || String.IsNullOrEmpty(_dataSource))
            {
                throw new ArgumentException("An invalid connection string argument has been supplied or a required connection string argument has not been supplied.");
            }
            else
            {
                if (_packetSize < 512 || _packetSize > 32767)
                {
                    string msg = $"'Packet Size' value of {_packetSize} is not valid.\r\nThe value should be an integer >= 512 and <= 32767.";

                    throw new ArgumentException(msg);
                }
            }
        }
    }
}