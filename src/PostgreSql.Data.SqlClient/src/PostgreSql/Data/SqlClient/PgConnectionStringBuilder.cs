// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgConnectionStringBuilder
        : DbConnectionStringBuilder
    {
        public string ApplicationName
        {
            get { return GetString(DbConnectionStringKeywords.ApplicationName, DbConnectionStringDefaults.ApplicationName); }
            set { SetValue(DbConnectionStringKeywords.ApplicationName, value); }
        }

        public int CommandTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.CommandTimeout, DbConnectionStringDefaults.CommandTimeout); }
            set { SetValue(DbConnectionStringKeywords.CommandTimeout, value); }
        }

        public int ConnectRetryCount
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectRetryCount, DbConnectionStringDefaults.ConnectRetryCount); }
            set { SetValue(DbConnectionStringKeywords.ConnectRetryCount, value); }
        }

        public int ConnectRetryInterval
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectRetryInterval, DbConnectionStringDefaults.ConnectRetryInterval); }
            set { SetValue(DbConnectionStringKeywords.ConnectRetryInterval, value); }
        }

        public int ConnectTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectTimeout, DbConnectionStringDefaults.ConnectTimeout); }
            set { SetValue(DbConnectionStringKeywords.ConnectTimeout, value); }
        }

        public string DataSource
        {
            get { return GetString(DbConnectionStringKeywords.DataSource, DbConnectionStringDefaults.DataSource); }
            set { SetValue(DbConnectionStringKeywords.DataSource, value); }
        }

        public bool DefaultTransactionReadOnly
        {
            get { return GetBoolean(DbConnectionStringKeywords.DefaultTransactionReadOnly, DbConnectionStringDefaults.DefaultTransactionReadOnly); }
            set { SetValue(DbConnectionStringKeywords.DefaultTransactionReadOnly, value); }
        }

        public string DefaultTablespace
        {
            get { return GetString(DbConnectionStringKeywords.DefaultTablespace, DbConnectionStringDefaults.DefaultTablespace); }
            set { SetValue(DbConnectionStringKeywords.DefaultTablespace, value); }
        }

        public bool Encrypt
        {
            get { return GetBoolean(DbConnectionStringKeywords.Encrypt, DbConnectionStringDefaults.Encrypt); }
            set { SetValue(DbConnectionStringKeywords.Encrypt, value); }
        }

        public string InitialCatalog
        {
            get { return GetString(DbConnectionStringKeywords.InitialCatalog, DbConnectionStringDefaults.InitialCatalog); }
            set { SetValue(DbConnectionStringKeywords.InitialCatalog, value); }
        }

        public int LoadBalanceTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.LoadBalanceTimeout, DbConnectionStringDefaults.LoadBalanceTimeout); }
            set { SetValue(DbConnectionStringKeywords.LoadBalanceTimeout, value); }
        }

        public int LockTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.LockTimeout, DbConnectionStringDefaults.LockTimeout); }
            set { SetValue(DbConnectionStringKeywords.LockTimeout, value); }
        }

        public int PacketSize
        {
            get { return GetInt32(DbConnectionStringKeywords.PacketSize, DbConnectionStringDefaults.PacketSize); }
            set { SetValue(DbConnectionStringKeywords.PacketSize, value); }
        }

        public string Password
        {
            get { return GetString(DbConnectionStringKeywords.Password, DbConnectionStringDefaults.Password); }
            set { SetValue(DbConnectionStringKeywords.Password, value); }
        }

        public bool Pooling
        {
            get { return GetBoolean(DbConnectionStringKeywords.Pooling, DbConnectionStringDefaults.Pooling); }
            set { SetValue(DbConnectionStringKeywords.Pooling, value); }
        }

        public int PortNumber
        {
            get { return GetInt32(DbConnectionStringKeywords.PortNumber, DbConnectionStringDefaults.PortNumber); }
            set { SetValue(DbConnectionStringKeywords.PortNumber, value); }
        }

        public int MaxPoolSize
        {
            get { return GetInt32(DbConnectionStringKeywords.MaxPoolSize, DbConnectionStringDefaults.MaxPoolSize); }
            set { SetValue(DbConnectionStringKeywords.MaxPoolSize, value); }
        }

        public int MinPoolSize
        {
            get { return GetInt32(DbConnectionStringKeywords.MinPoolSize, DbConnectionStringDefaults.MinPoolSize); }
            set { SetValue(DbConnectionStringKeywords.MinPoolSize, value); }
        }

        public bool MultipleActiveResultSets
        {
            get { return GetBoolean(DbConnectionStringKeywords.MultipleActiveResultSets, DbConnectionStringDefaults.MultipleActiveResultSets); }
            set { SetValue(DbConnectionStringKeywords.MultipleActiveResultSets, value); }
        }

        public string SearchPath
        {
            get { return GetString(DbConnectionStringKeywords.SearchPath, DbConnectionStringDefaults.SearchPath); }
            set { SetValue(DbConnectionStringKeywords.SearchPath, value); }
        }

        public string UserID
        {
            get { return GetString(DbConnectionStringKeywords.UserId, DbConnectionStringDefaults.UserID); }
            set { SetValue(DbConnectionStringKeywords.UserId, value); }
        }

        public PgConnectionStringBuilder()
        {
        }

        public PgConnectionStringBuilder(string connectionString)
        {
            ConnectionString = new DbConnectionOptions(connectionString).ConnectionString;
        }

        private int GetInt32(string keyword, int @default)
        {
            var k = GetKeyword(keyword);
            return ContainsKey(k) ? Convert.ToInt32(this[k]) : @default;
        }

        private string GetString(string keyword, string @default)
        {
            var k = GetKeyword(keyword);
            return ContainsKey(k) ? Convert.ToString(this[k]) : @default;
        }

        private bool GetBoolean(string keyword, bool @default)
        {
            var k = GetKeyword(keyword);
            return ContainsKey(k) ? Convert.ToBoolean(this[k]) : @default;
        }

        private void SetValue(string keyword, object value) => this[GetKeyword(keyword)] = value;
        
        internal string GetKeyword(string synonym)
        {
            if (string.IsNullOrEmpty(synonym))
            {
                throw ADP.ArgumentNull(nameof(synonym));
            }
            if (!DbConnectionStringSynonyms.IsSynonym(synonym))
            {
                throw ADP.NotSupported($"Keyword not supported: {synonym}");
            }

            string synonymKey = DbConnectionStringSynonyms.Synonyms[synonym];

            // First check if there are yet a property for the requested keyword
            foreach (string key in Keys)
            {
                if (DbConnectionStringSynonyms.Synonyms.ContainsKey(key) 
                 && DbConnectionStringSynonyms.Synonyms[key] == synonymKey)
                {
                    synonymKey = key;
                    break;
                }
            }
            
            return synonymKey;
        }
    }
}
