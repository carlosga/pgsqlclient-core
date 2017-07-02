// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;
using System.Data.ProviderBase;
using System.Diagnostics;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgConnectionFactory 
        : DbConnectionFactory
    {
        internal static readonly PgConnectionFactory SingletonInstance = new PgConnectionFactory();

        internal static DbConnectionOptions FindDbConnectionOptions(DbConnectionPoolKey key)
        {
            var connectionOptions = SingletonInstance.FindConnectionOptions(key);
            if (connectionOptions == null)
            {
                connectionOptions = new DbConnectionOptions(key.ConnectionString);
            }
            if (connectionOptions.IsEmpty)
            {
                throw ADP.NoConnectionString();
            }
            return connectionOptions;
        }

        private PgConnectionFactory() 
            : base() 
        {
        }

        internal override DbProviderFactory ProviderFactory => PgSqlClientFactory.Instance;

        internal override DbConnectionPoolProviderInfo CreateConnectionPoolProviderInfo(DbConnectionOptions connectionOptions)
        {
            //DbConnectionPoolProviderInfo providerInfo = null;

            //if (connectionOptions.UserInstance)
            //{
            //    providerInfo = new DbConnectionPoolProviderInfo();
            //}

            //return providerInfo;
            return null;
        }

        protected override DbConnectionInternal CreateConnection(DbConnectionOptions               options
                                                               , DbConnectionPoolKey               poolKey
                                                               , DbConnectionPoolGroupProviderInfo poolGroupProviderInfo
                                                               , DbConnectionPool                  pool
                                                               , DbConnection                      owningConnection)
        {
            return CreateConnection(options, poolKey, poolGroupProviderInfo, pool, owningConnection, userOptions: null);
        }

        protected override DbConnectionInternal CreateConnection(DbConnectionOptions               options
                                                               , DbConnectionPoolKey               poolKey
                                                               , DbConnectionPoolGroupProviderInfo poolGroupProviderInfo
                                                               , DbConnectionPool                  pool
                                                               , DbConnection                      owningDbConnection
                                                               , DbConnectionOptions               userOptions)
        {
            var owningConnection            = (PgConnection)owningDbConnection;
            var identity                    = DbConnectionPoolIdentity.NoIdentity;
            var applyTransientFaultHandling = ((owningConnection != null) ? owningConnection.ApplyTransientFaultHandling : false);

            DbConnectionOptions userOpt = null;
            if (userOptions != null)
            {
                userOpt = userOptions;
            }
            else if (owningConnection != null)
            {
                userOpt = owningConnection.UserConnectionOptions;
            }

            return new PgConnectionInternal(identity
                                          , options
                                          , poolGroupProviderInfo
                                          , userOpt
                                          , applyTransientFaultHandling);
        }

        protected override DbConnectionOptions CreateConnectionOptions(string connectionString, DbConnectionOptions previous)
        {
            Debug.Assert(!string.IsNullOrEmpty(connectionString), "empty connectionString");
            return new DbConnectionOptions(connectionString);
        }

        protected override DbConnectionPoolGroupOptions CreateConnectionPoolGroupOptions(DbConnectionOptions connectionOptions)
        {
            DbConnectionPoolGroupOptions poolingOptions = null;

            if (connectionOptions.Pooling) // never pool context connections.
            {
                int connectionTimeout = connectionOptions.ConnectTimeout;

                if ((0 < connectionTimeout) && (connectionTimeout < int.MaxValue / 1000))
                {
                    connectionTimeout *= 1000;
                }
                else if (connectionTimeout >= int.MaxValue / 1000)
                {
                    connectionTimeout = int.MaxValue;
                }

                poolingOptions = new DbConnectionPoolGroupOptions(false //opt.IntegratedSecurity,
                                                                , connectionOptions.MinPoolSize
                                                                , connectionOptions.MaxPoolSize
                                                                , connectionTimeout
                                                                , connectionOptions.LoadBalanceTimeout);
            }
            return poolingOptions;
        }

        internal override DbConnectionPoolGroupProviderInfo CreateConnectionPoolGroupProviderInfo(DbConnectionOptions connectionOptions)
        {
            return new DbConnectionPoolGroupProviderInfo();
        }

        internal override DbConnectionPoolGroup GetConnectionPoolGroup(DbConnection connection)
        {
            var c = connection as PgConnection;
            if (c != null)
            {
                return c.PoolGroup;
            }
            return null;
        }

        internal override DbConnectionInternal GetInnerConnection(DbConnection connection)
        {
            var c = connection as PgConnection;
            if (c != null)
            {
                return c.InnerConnection;
            }
            return null;
        }

        internal override void PermissionDemand(DbConnection outerConnection)
        {
            var c = outerConnection as PgConnection;
            if (c != null)
            {
                c.PermissionDemand();
            }
        }

        internal override void SetConnectionPoolGroup(DbConnection outerConnection, DbConnectionPoolGroup poolGroup)
        {
            var c = outerConnection as PgConnection;
            if (c != null)
            {
                c.PoolGroup = poolGroup;
            }
        }

        internal override void SetInnerConnectionEvent(DbConnection owningObject, DbConnectionInternal to)
        {
            var c = owningObject as PgConnection;
            if (c != null)
            {
                c.SetInnerConnectionEvent(to);
            }
        }

        internal override bool SetInnerConnectionFrom(DbConnection owningObject, DbConnectionInternal to, DbConnectionInternal from)
        {
            var c = owningObject as PgConnection;
            if (c != null)
            {
                return c.SetInnerConnectionFrom(to, from);
            }
            return false;
        }

        internal override void SetInnerConnectionTo(DbConnection owningObject, DbConnectionInternal to)
        {
            var c = owningObject as PgConnection;
            if (c != null)
            {
                c.SetInnerConnectionTo(to);
            }
        }
    }
}

