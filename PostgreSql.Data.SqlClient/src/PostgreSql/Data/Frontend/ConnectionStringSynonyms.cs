// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PostgreSql.Data.Frontend
{
    internal static class ConnectionStringSynonyms
    {
        internal static readonly ReadOnlyDictionary<string, string> Synonyms;

        static ConnectionStringSynonyms()
        {
            var synonyms = new Dictionary<string, string>(22);

            synonyms.Add(ConnectionStringSynonyms.DataSource                , ConnectionStringKeywords.DataSource);
            synonyms.Add(ConnectionStringSynonyms.Server                    , ConnectionStringKeywords.DataSource);
            synonyms.Add(ConnectionStringSynonyms.Host                      , ConnectionStringKeywords.DataSource);
            synonyms.Add(ConnectionStringSynonyms.Database                  , ConnectionStringKeywords.InitialCatalog);
            synonyms.Add(ConnectionStringSynonyms.InitialCatalog            , ConnectionStringKeywords.InitialCatalog);
            synonyms.Add(ConnectionStringSynonyms.UserId                    , ConnectionStringKeywords.UserId);
            synonyms.Add(ConnectionStringSynonyms.UserName                  , ConnectionStringKeywords.UserId);
            synonyms.Add(ConnectionStringSynonyms.User                      , ConnectionStringKeywords.UserId);
            synonyms.Add(ConnectionStringSynonyms.UserPassword              , ConnectionStringKeywords.Password);
            synonyms.Add(ConnectionStringSynonyms.Password                  , ConnectionStringKeywords.Password);
            synonyms.Add(ConnectionStringSynonyms.PortNumber                , ConnectionStringKeywords.PortNumber);
            synonyms.Add(ConnectionStringSynonyms.Port                      , ConnectionStringKeywords.PortNumber);
            synonyms.Add(ConnectionStringSynonyms.PacketSize                , ConnectionStringKeywords.PacketSize);
            synonyms.Add(ConnectionStringSynonyms.ConnectionTimeout         , ConnectionStringKeywords.ConnectionTimeout);
            synonyms.Add(ConnectionStringSynonyms.Pooling                   , ConnectionStringKeywords.Pooling);
            synonyms.Add(ConnectionStringSynonyms.ConnectionLifetime        , ConnectionStringKeywords.ConnectionLifetime);
            synonyms.Add(ConnectionStringSynonyms.MinPoolSize               , ConnectionStringKeywords.MinPoolSize);
            synonyms.Add(ConnectionStringSynonyms.MaxPoolSize               , ConnectionStringKeywords.MaxPoolSize);
            synonyms.Add(ConnectionStringSynonyms.Encrypt                   , ConnectionStringKeywords.Encrypt);
            synonyms.Add(ConnectionStringSynonyms.MultipleActiveResultSets  , ConnectionStringKeywords.MultipleActiveResultSets);
            synonyms.Add(ConnectionStringSynonyms.SearchPath                , ConnectionStringKeywords.SearchPath);
            synonyms.Add(ConnectionStringSynonyms.ApplicationName           , ConnectionStringKeywords.ApplicationName);
            synonyms.Add(ConnectionStringSynonyms.CommandTimeout            , ConnectionStringKeywords.CommandTimeout);
            synonyms.Add(ConnectionStringSynonyms.StatementTimeout          , ConnectionStringKeywords.CommandTimeout);
            synonyms.Add(ConnectionStringSynonyms.LockTimeout               , ConnectionStringKeywords.LockTimeout);
            synonyms.Add(ConnectionStringSynonyms.DefaultTransactionReadOnly, ConnectionStringKeywords.DefaultTransactionReadOnly);
            
            Synonyms = new ReadOnlyDictionary<string, string>(synonyms);
        }

        internal const string DataSource                 = "data source";
        internal const string Server                     = "server";
        internal const string Host                       = "host";
        internal const string Database                   = "database";
        internal const string InitialCatalog             = "initial catalog";
        internal const string UserId                     = "user id";
        internal const string UserName                   = "user name";
        internal const string User                       = "user";
        internal const string UserPassword               = "user password";
        internal const string Password                   = "password";
        internal const string PortNumber                 = "port number";
        internal const string Port                       = "port";
        internal const string PacketSize                 = "packet size";
        internal const string ConnectionTimeout          = "connection timeout";
        internal const string ConnectTimeout             = "connect timeout";
        internal const string Timeout                    = "timeout";
        internal const string Pooling                    = "pooling";
        internal const string ConnectionLifetime         = "connection lifetime";
        internal const string MinPoolSize                = "min pool size";
        internal const string MaxPoolSize                = "max pool size";
        internal const string Encrypt                    = "encrypt";
        internal const string MultipleActiveResultSets   = "multipleactiveresultsets";
        internal const string SearchPath                 = "search path";
        internal const string ApplicationName            = "application name";
        internal const string CommandTimeout             = "command timeout";
        internal const string StatementTimeout           = "statement timeout"; 
        internal const string LockTimeout                = "lock timeout";
        internal const string DefaultTransactionReadOnly = "default transaction read only"; 

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
