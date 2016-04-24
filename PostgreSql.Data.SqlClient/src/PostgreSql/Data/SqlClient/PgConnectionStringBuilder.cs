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
        public string ApplicationName
        {
            get { return GetString(DbConnectionStringKeywords.ApplicationName); }
            set { SetValue(DbConnectionStringKeywords.ApplicationName, value); }
        }

        public int CommandTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.CommandTimeout); }
            set { SetValue(DbConnectionStringKeywords.CommandTimeout, value); }
        }

        public int ConnectRetryCount
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectRetryCount); }
            set { SetValue(DbConnectionStringKeywords.ConnectRetryCount, value); }
        }

        public int ConnectRetryInterval
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectRetryInterval); }
            set { SetValue(DbConnectionStringKeywords.ConnectRetryInterval, value); }
        }

        public int ConnectTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.ConnectTimeout); }
            set { SetValue(DbConnectionStringKeywords.ConnectTimeout, value); }
        }

        public string DataSource
        {
            get { return GetString(DbConnectionStringKeywords.DataSource); }
            set { SetValue(DbConnectionStringKeywords.DataSource, value); }
        }

        public bool DefaultTransactionReadOnly
        {
            get { return GetBoolean(DbConnectionStringKeywords.DefaultTransactionReadOnly); }
            set { SetValue(DbConnectionStringKeywords.DefaultTransactionReadOnly, value); }
        }

        public string DefaultTablespace
        {
            get { return GetString(DbConnectionStringKeywords.DefaultTablespace); }
            set { SetValue(DbConnectionStringKeywords.DefaultTablespace, value); }
        }

        public bool Encrypt
        {
            get { return GetBoolean(DbConnectionStringKeywords.Encrypt); }
            set { SetValue(DbConnectionStringKeywords.Encrypt, value); }
        }

        public string InitialCatalog
        {
            get { return GetString(DbConnectionStringKeywords.InitialCatalog); }
            set { SetValue(DbConnectionStringKeywords.InitialCatalog, value); }
        }

        public int LoadBalanceTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.LoadBalanceTimeout); }
            set { SetValue(DbConnectionStringKeywords.LoadBalanceTimeout, value); }
        }

        public int LockTimeout
        {
            get { return GetInt32(DbConnectionStringKeywords.LockTimeout); }
            set { SetValue(DbConnectionStringKeywords.LockTimeout, value); }
        }

        public int PacketSize
        {
            get { return GetInt32(DbConnectionStringKeywords.PacketSize); }
            set { SetValue(DbConnectionStringKeywords.PacketSize, value); }
        }

        public string Password
        {
            get { return GetString(DbConnectionStringKeywords.Password); }
            set { SetValue(DbConnectionStringKeywords.Password, value); }
        }

        public bool Pooling
        {
            get { return GetBoolean(DbConnectionStringKeywords.Pooling); }
            set { SetValue(DbConnectionStringKeywords.Pooling, value); }
        }

        public int PortNumber
        {
            get { return GetInt32(DbConnectionStringKeywords.PortNumber); }
            set { SetValue(DbConnectionStringKeywords.PortNumber, value); }
        }

        public int MaxPoolSize
        {
            get { return GetInt32(DbConnectionStringKeywords.MaxPoolSize); }
            set { SetValue(DbConnectionStringKeywords.MaxPoolSize, value); }
        }

        public int MinPoolSize
        {
            get { return GetInt32(DbConnectionStringKeywords.MinPoolSize); }
            set { SetValue(DbConnectionStringKeywords.MinPoolSize, value); }
        }

        public bool MultipleActiveResultSets
        {
            get { return GetBoolean(DbConnectionStringKeywords.MultipleActiveResultSets); }
            set { SetValue(DbConnectionStringKeywords.MultipleActiveResultSets, value); }
        }

        public string SearchPath
        {
            get { return GetString(DbConnectionStringKeywords.SearchPath); }
            set { SetValue(DbConnectionStringKeywords.SearchPath, value); }
        }

        public string UserID
        {
            get { return GetString(DbConnectionStringKeywords.UserId); }
            set { SetValue(DbConnectionStringKeywords.UserId, value); }
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
            string synonymKey = DbConnectionStringSynonyms.Synonyms[keyword];

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
