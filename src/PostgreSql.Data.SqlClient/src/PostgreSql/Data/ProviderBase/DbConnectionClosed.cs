// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Data.Common;
using System.Diagnostics;
using System.Threading.Tasks;
using PostgreSql.Data.Frontend;

namespace System.Data.ProviderBase
{
    internal abstract class DbConnectionClosed 
        : DbConnectionInternal
    {
        // Construct an "empty" connection
        protected DbConnectionClosed(ConnectionState state, bool hidePassword, bool allowSetConnectionString) 
            : base(state, hidePassword, allowSetConnectionString)
        {
        }

        internal override string ServerVersion
        {
            get { throw ADP.ClosedConnectionError(); }
        }

        protected override void Activate()
        {
            throw ADP.ClosedConnectionError();
        }

        internal override DbTransaction BeginTransaction(IsolationLevel il)
        {
            throw ADP.ClosedConnectionError();
        }

        internal override void ChangeDatabase(string database)
        {
            throw ADP.ClosedConnectionError();
        }

        internal override void CloseConnection(DbConnection owningObject, DbConnectionFactory connectionFactory)
        {
            // not much to do here...
        }

        internal override void ValidateConnectionForExecute(DbCommand command) 
        {
            throw ADP.ClosedConnectionError();
        }

        internal override Statement CreateStatement()
        {
            throw ADP.ClosedConnectionError();        
        }

        internal override Statement CreateStatement(string stmtText)
        {
            throw ADP.ClosedConnectionError();
        }

        protected override void Deactivate()
        {
            throw ADP.ClosedConnectionError();
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            throw ADP.ClosedConnectionError();
        }

        internal override bool TryOpenConnection(DbConnection                               outerConnection
                                               , DbConnectionFactory                        connectionFactory
                                               , TaskCompletionSource<DbConnectionInternal> retry
                                               , DbConnectionOptions                        userOptions)
        {
            return base.TryOpenConnectionInternal(outerConnection, connectionFactory, retry, userOptions);
        }
    }

    internal abstract class DbConnectionBusy 
        : DbConnectionClosed
    {
        protected DbConnectionBusy(ConnectionState state) 
            : base(state, true, false)
        {
        }

        internal override bool TryOpenConnection(DbConnection                               outerConnection
                                               , DbConnectionFactory                        connectionFactory
                                               , TaskCompletionSource<DbConnectionInternal> retry
                                               , DbConnectionOptions                        userOptions)
        {
            throw ADP.ConnectionAlreadyOpen(State);
        }
    }

    internal sealed class DbConnectionClosedBusy 
        : DbConnectionBusy
    {
        // Closed Connection, Currently Busy - changing connection string
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedBusy();

        private DbConnectionClosedBusy()
            : base(ConnectionState.Closed)
        {
        }
    }

    internal sealed class DbConnectionOpenBusy 
        : DbConnectionBusy
    {
        // Open Connection, Currently Busy - closing connection
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionOpenBusy();

        private DbConnectionOpenBusy() 
            : base(ConnectionState.Open)
        {
        }
    }

    internal sealed class DbConnectionClosedConnecting 
        : DbConnectionBusy
    {
        // Closed Connection, Currently Connecting
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedConnecting();

        private DbConnectionClosedConnecting() 
            : base(ConnectionState.Connecting)
        {
        }

        internal override void CloseConnection(DbConnection owningObject, DbConnectionFactory connectionFactory)
        {
            connectionFactory.SetInnerConnectionTo(owningObject, DbConnectionClosedPreviouslyOpened.SingletonInstance);
        }

        internal override bool TryReplaceConnection(DbConnection                               outerConnection
                                                  , DbConnectionFactory                        connectionFactory
                                                  , TaskCompletionSource<DbConnectionInternal> retry
                                                  , DbConnectionOptions                        userOptions)
        {
            return TryOpenConnection(outerConnection, connectionFactory, retry, userOptions);
        }

        internal override bool TryOpenConnection(DbConnection                               outerConnection
                                               , DbConnectionFactory                        connectionFactory
                                               , TaskCompletionSource<DbConnectionInternal> retry
                                               , DbConnectionOptions                        userOptions)
        {
            if (retry == null || !retry.Task.IsCompleted)
            {
                // retry is null if this is a synchronous call

                // if someone calls Open or OpenAsync while in this state, 
                // then the retry task will not be completed

                throw ADP.ConnectionAlreadyOpen(State);
            }

            // we are completing an asynchronous open
            Debug.Assert(retry.Task.Status == TaskStatus.RanToCompletion, "retry task must be completed successfully");
            DbConnectionInternal openConnection = retry.Task.Result;
            if (openConnection == null)
            {
                connectionFactory.SetInnerConnectionTo(outerConnection, this);
                throw ADP.InternalConnectionError(ADP.ConnectionError.GetConnectionReturnsNull);
            }
            connectionFactory.SetInnerConnectionEvent(outerConnection, openConnection);

            return true;
        }
    }

    internal sealed class DbConnectionClosedNeverOpened 
        : DbConnectionClosed
    {
        // Closed Connection, Has Never Been Opened
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedNeverOpened();

        private DbConnectionClosedNeverOpened() 
            : base(ConnectionState.Closed, false, true)
        {
        }
    }

    internal sealed class DbConnectionClosedPreviouslyOpened 
        : DbConnectionClosed
    {
        // Closed Connection, Has Previously Been Opened
        internal static readonly DbConnectionInternal SingletonInstance = new DbConnectionClosedPreviouslyOpened();

        private DbConnectionClosedPreviouslyOpened() 
            : base(ConnectionState.Closed, true, true)
        {
        }

        internal override bool TryReplaceConnection(DbConnection                               outerConnection
                                                  , DbConnectionFactory                        connectionFactory
                                                  , TaskCompletionSource<DbConnectionInternal> retry
                                                  , DbConnectionOptions                        userOptions)
        {
            return TryOpenConnection(outerConnection, connectionFactory, retry, userOptions);
        }
    }
}
