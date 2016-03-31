// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;
using PostgreSql.Data.Frontend;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgConnectionStringBuilder
        : DbConnectionStringBuilder
    {
        public string DataSource
        {
            get { return GetString(ConnectionStringKeywords.DataSource); }
            set { SetValue(ConnectionStringKeywords.DataSource, value); }
        }

        public string InitialCatalog
        {
            get { return GetString(ConnectionStringKeywords.InitialCatalog); }
            set { SetValue(ConnectionStringKeywords.InitialCatalog, value); }
        }

        public string UserID
        {
            get { return GetString(ConnectionStringKeywords.UserId); }
            set { SetValue(ConnectionStringKeywords.UserId, value); }
        }

        public string Password
        {
            get { return GetString(ConnectionStringKeywords.Password); }
            set { SetValue(ConnectionStringKeywords.Password, value); }
        }

        public int PortNumber
        {
            get { return GetInt32(ConnectionStringKeywords.PortNumber); }
            set { SetValue(ConnectionStringKeywords.PortNumber, value); }
        }

        public int PacketSize
        {
            get { return GetInt32(ConnectionStringKeywords.PacketSize); }
            set { SetValue(ConnectionStringKeywords.PacketSize, value); }
        }

        public int ConnectTimeout
        {
            get { return GetInt32(ConnectionStringKeywords.ConnectionTimeout); }
            set { SetValue(ConnectionStringKeywords.ConnectionTimeout, value); }
        }

        public bool Pooling
        {
            get { return GetBoolean(ConnectionStringKeywords.Pooling); }
            set { SetValue(ConnectionStringKeywords.Pooling, value); }
        }

        public int ConnectionLifeTime
        {
            get { return GetInt32(ConnectionStringKeywords.ConnectionLifetime); }
            set { SetValue(ConnectionStringKeywords.ConnectionLifetime, value); }
        }

        public int MinPoolSize
        {
            get { return GetInt32(ConnectionStringKeywords.MinPoolSize); }
            set { SetValue(ConnectionStringKeywords.MinPoolSize, value); }
        }

        public int MaxPoolSize
        {
            get { return GetInt32(ConnectionStringKeywords.MaxPoolSize); }
            set { SetValue(ConnectionStringKeywords.MaxPoolSize, value); }
        }

        public bool Encrypt
        {
            get { return GetBoolean(ConnectionStringKeywords.Encrypt); }
            set { SetValue(ConnectionStringKeywords.Encrypt, value); }
        }

        public bool MultipleActiveResultSets
        {
            get { return GetBoolean(ConnectionStringKeywords.MultipleActiveResultSets); }
            set { SetValue(ConnectionStringKeywords.MultipleActiveResultSets, value); }
        }

        public string SearchPath
        {
            get { return GetString(ConnectionStringKeywords.SearchPath); }
            set { SetValue(ConnectionStringKeywords.SearchPath, value); }
        }

        public int FetchSize
        {
            get { return GetInt32(ConnectionStringKeywords.FetchSize); }
            set { SetValue(ConnectionStringKeywords.FetchSize, value); }
        }

        public PgConnectionStringBuilder()
        {
        }

        public PgConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        private int    GetInt32(string keyword)   => Convert.ToInt32(this[GetValue(keyword)]);
        private string GetString(string keyword)  => Convert.ToString(this[GetValue(keyword)]);
        private bool   GetBoolean(string keyword) => Convert.ToBoolean(this[GetValue(keyword)]);

        private void SetValue(string keyword, object value) => this[GetValue(keyword)] = value;
        
        internal string GetValue(string keyword)
        {
            string synonymKey = ConnectionStringSynonyms.Synonyms[keyword];

            // First check if there are yet a property for the requested keyword
            foreach (string key in Keys)
            {
                if (ConnectionStringSynonyms.Synonyms.ContainsKey(key) 
                 && ConnectionStringSynonyms.Synonyms[key] == synonymKey)
                {
                    synonymKey = key;
                    break;
                }
            }
            
            return synonymKey;
        }
    }
}
