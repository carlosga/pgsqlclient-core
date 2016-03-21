// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgConnectionStringBuilder
        : DbConnectionStringBuilder
    {
        private static readonly Dictionary<string, string> s_synonyms = InitializeSynonyms();

        private static Dictionary<string, string> InitializeSynonyms()
        {
            var synonyms = new Dictionary<string, string>(new CaseInsensitiveEqualityComparer());

            synonyms.Add("data source"          , "data source");
            synonyms.Add("server"               , "data source");
            synonyms.Add("host"                 , "data source");
            synonyms.Add("database"             , "initial catalog");
            synonyms.Add("initial catalog"      , "initial catalog");
            synonyms.Add("user id"              , "user id");
            synonyms.Add("user name"            , "user id");
            synonyms.Add("user"                 , "user id");
            synonyms.Add("user password"        , "password");
            synonyms.Add("password"             , "password");
            synonyms.Add("port number"          , "port number");
            synonyms.Add("port"                 , "port number");
            synonyms.Add("packet size"          , "packet size");
            synonyms.Add("connection timeout"   , "connection timeout");
            synonyms.Add("pooling"              , "pooling");
            synonyms.Add("connection lifetime"  , "connection lifetime");
            synonyms.Add("min pool size"        , "min pool size");
            synonyms.Add("max pool size"        , "max pool size");
            synonyms.Add("ssl"                  , "ssl");
            synonyms.Add("use database oids"    , "use database oids");

            return synonyms;
        }

        public string DataSource
        {
            get { return GetString("Data Source"); }
            set { SetValue("Data Source", value); }
        }

        public string InitialCatalog
        {
            get { return GetString("Initial Catalog"); }
            set { SetValue("Initial Catalog", value); }
        }

        public string UserID
        {
            get { return GetString("User ID"); }
            set { SetValue("User ID", value); }
        }

        public string Password
        {
            get { return GetString("Password"); }
            set { SetValue("Password", value); }
        }

        public int PortNumber
        {
            get { return GetInt32("Port Number"); }
            set { SetValue("Port Number", value); }
        }

        public int PacketSize
        {
            get { return GetInt32("Packet Size"); }
            set { SetValue("Packet Size", value); }
        }

        public int ConnectionTimeout
        {
            get { return GetInt32("Connection Timeout"); }
            set { SetValue("Connection Timeout", value); }
        }

        public bool Pooling
        {
            get { return GetBoolean("Pooling"); }
            set { SetValue("Pooling", value); }
        }

        public int ConnectionLifeTime
        {
            get { return GetInt32("Connection Lifetime"); }
            set { SetValue("Connection Lifetime", value); }
        }

        public int MinPoolSize
        {
            get { return GetInt32("Min Pool Size"); }
            set { SetValue("Min Pool Size", value); }
        }

        public int MaxPoolSize
        {
            get { return GetInt32("Max Pool Size"); }
            set { SetValue("Max Pool Size", value); }
        }

        public bool Ssl
        {
            get { return GetBoolean("Ssl"); }
            set { SetValue("Ssl", value); }
        }

        public bool UseDatabaseOids
        {
            get { return GetBoolean("use database oids"); }
            set { SetValue("use database oids", value); }
        }

        public PgConnectionStringBuilder()
        {
        }

        public PgConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private int    GetInt32(string keyword)   => Convert.ToInt32(GetKey(keyword));
        private string GetString(string keyword)  => Convert.ToString(GetKey(keyword));
        private bool   GetBoolean(string keyword) => Convert.ToBoolean(GetKey(keyword));
        
        private void SetValue(string keyword, object value) => this[GetKey(keyword)] = value;

        private string GetKey(string keyword)
        {
            string synonymKey = s_synonyms[keyword];

            // First check if there are yet a property for the requested keyword
            foreach (string key in Keys)
            {
                if (s_synonyms.ContainsKey(key) && s_synonyms[key] == synonymKey)
                {
                    synonymKey = key;
                    break;
                }
            }

            return synonymKey;
        }
    }
}