// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace System.Data.Common
{
    internal static class DbConnectionStringSynonyms
    {
        internal static readonly ReadOnlyDictionary<string, string> Synonyms;

        static DbConnectionStringSynonyms()
        {
            var synonyms = new Dictionary<string, string>(22);

            synonyms.Add(DbConnectionStringSynonyms.App                       , DbConnectionStringKeywords.ApplicationName);
            synonyms.Add(DbConnectionStringSynonyms.ApplicationName           , DbConnectionStringKeywords.ApplicationName);
            synonyms.Add(DbConnectionStringSynonyms.CommandTimeout            , DbConnectionStringKeywords.CommandTimeout);
            synonyms.Add(DbConnectionStringSynonyms.StatementTimeout          , DbConnectionStringKeywords.CommandTimeout);
            synonyms.Add(DbConnectionStringSynonyms.ConnectRetryCount         , DbConnectionStringKeywords.ConnectRetryCount);
            synonyms.Add(DbConnectionStringSynonyms.ConnectRetryInterval      , DbConnectionStringKeywords.ConnectRetryInterval);
            synonyms.Add(DbConnectionStringSynonyms.ConnectTimeout            , DbConnectionStringKeywords.ConnectTimeout);
            synonyms.Add(DbConnectionStringSynonyms.ConnectionTimeout         , DbConnectionStringKeywords.ConnectTimeout);
            synonyms.Add(DbConnectionStringSynonyms.DataSource                , DbConnectionStringKeywords.DataSource);
            synonyms.Add(DbConnectionStringSynonyms.Host                      , DbConnectionStringKeywords.DataSource);
            synonyms.Add(DbConnectionStringSynonyms.Server                    , DbConnectionStringKeywords.DataSource);
            synonyms.Add(DbConnectionStringSynonyms.DefaultTablespace         , DbConnectionStringKeywords.DefaultTablespace);
            synonyms.Add(DbConnectionStringSynonyms.DefaultTransactionReadOnly, DbConnectionStringKeywords.DefaultTransactionReadOnly);
            synonyms.Add(DbConnectionStringSynonyms.InitialCatalog            , DbConnectionStringKeywords.InitialCatalog);
            synonyms.Add(DbConnectionStringSynonyms.Database                  , DbConnectionStringKeywords.InitialCatalog);
            synonyms.Add(DbConnectionStringSynonyms.LoadBalanceTimeout        , DbConnectionStringKeywords.LoadBalanceTimeout);
            synonyms.Add(DbConnectionStringSynonyms.ConnectionLifetime        , DbConnectionStringKeywords.LoadBalanceTimeout);
            synonyms.Add(DbConnectionStringSynonyms.LockTimeout               , DbConnectionStringKeywords.LockTimeout);
            synonyms.Add(DbConnectionStringSynonyms.PacketSize                , DbConnectionStringKeywords.PacketSize);
            synonyms.Add(DbConnectionStringSynonyms.UserPassword              , DbConnectionStringKeywords.Password);
            synonyms.Add(DbConnectionStringSynonyms.Password                  , DbConnectionStringKeywords.Password);
            synonyms.Add(DbConnectionStringSynonyms.PortNumber                , DbConnectionStringKeywords.PortNumber);
            synonyms.Add(DbConnectionStringSynonyms.Port                      , DbConnectionStringKeywords.PortNumber);
            synonyms.Add(DbConnectionStringSynonyms.Pooling                   , DbConnectionStringKeywords.Pooling);
            synonyms.Add(DbConnectionStringSynonyms.MinPoolSize               , DbConnectionStringKeywords.MinPoolSize);
            synonyms.Add(DbConnectionStringSynonyms.MaxPoolSize               , DbConnectionStringKeywords.MaxPoolSize);
            synonyms.Add(DbConnectionStringSynonyms.Encrypt                   , DbConnectionStringKeywords.Encrypt);
            synonyms.Add(DbConnectionStringSynonyms.MultipleActiveResultSets  , DbConnectionStringKeywords.MultipleActiveResultSets);
            synonyms.Add(DbConnectionStringSynonyms.SearchPath                , DbConnectionStringKeywords.SearchPath);
            synonyms.Add(DbConnectionStringSynonyms.UserId                    , DbConnectionStringKeywords.UserId);
            synonyms.Add(DbConnectionStringSynonyms.UserName                  , DbConnectionStringKeywords.UserId);
            synonyms.Add(DbConnectionStringSynonyms.User                      , DbConnectionStringKeywords.UserId);

            Synonyms = new ReadOnlyDictionary<string, string>(synonyms);
        }

        internal const string App                        = "app";
        internal const string ApplicationName            = "application name";
        internal const string CommandTimeout             = "command timeout";
        internal const string ConnectRetryCount          = "connection retry count";
        internal const string ConnectRetryInterval       = "connection retry interval";
        internal const string ConnectTimeout             = "connect timeout";
        internal const string ConnectionTimeout          = "connection timeout";
        internal const string ConnectionLifetime         = "connection lifetime";
        internal const string Database                   = "database";
        internal const string DataSource                 = "data source";
        internal const string DefaultTransactionReadOnly = "default transaction read only";
        internal const string DefaultTablespace          = "default tablespace";
        internal const string Encrypt                    = "encrypt";
        internal const string Host                       = "host";
        internal const string InitialCatalog             = "initial catalog";
        internal const string LoadBalanceTimeout         = "load balance timeout";
        internal const string LockTimeout                = "lock timeout";
        internal const string Password                   = "password";
        internal const string Port                       = "port";
        internal const string PortNumber                 = "port number";
        internal const string MaxPoolSize                = "max pool size";
        internal const string MinPoolSize                = "min pool size";
        internal const string MultipleActiveResultSets   = "multipleactiveresultsets";
        internal const string PacketSize                 = "packet size";
        internal const string Pooling                    = "pooling";
        internal const string SearchPath                 = "search path";
        internal const string Server                     = "server";
        internal const string StatementTimeout           = "statement timeout";
        internal const string Timeout                    = "timeout";
        internal const string User                       = "user";
        internal const string UserId                     = "user id";
        internal const string UserName                   = "user name";
        internal const string UserPassword               = "user password";

        internal static bool IsSynonym(string key)
        {
            return Synonyms.ContainsKey(key);
        }

        internal static string GetSynonym(string key)
        {
            return Synonyms[key];
        }
    }
}
