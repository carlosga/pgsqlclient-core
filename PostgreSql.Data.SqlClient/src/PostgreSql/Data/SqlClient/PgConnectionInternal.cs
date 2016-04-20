// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Linq;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgConnectionInternal
        : DbConnectionInternal, IDisposable
    {
        private DbConnectionOptions               _connectionOptions;
        private Connection                        _connection;
        private WeakReference                     _activeTransaction;
        private DbConnectionPoolIdentity          _identity;
        private DbConnectionPoolGroupProviderInfo _providerInfo;

        internal PgConnection        OwningConnection  => (PgConnection)Owner;
        internal Connection          Connection        => _connection;
        internal PgTransaction       ActiveTransaction => _activeTransaction?.Target as PgTransaction;
        internal string              Database          => _connection?.Database;
        internal string              DataSource        => _connection?.DataSource;
        internal DbConnectionOptions ConnectionOptions => _connectionOptions;

        internal override string ServerVersion => _connection.SessionData.ServerVersion;

        internal bool HasActiveTransaction
        {
            get
            {
                 return (_activeTransaction != null 
                      && _activeTransaction.IsAlive
                      && _connection.TransactionState != TransactionState.Default); 
            }
        }

        internal PgConnectionInternal(DbConnectionOptions connectionOptions)
            : this(DbConnectionPoolIdentity.NoIdentity, null, connectionOptions)
        {
        }

        internal PgConnectionInternal(DbConnectionPoolIdentity          identity
                                    , DbConnectionPoolGroupProviderInfo providerInfo
                                    , DbConnectionOptions               connectionOptions)
            : base()
        {
            _connectionOptions = connectionOptions;
            _connection        = new Connection(_connectionOptions);
            _identity          = identity;
            _providerInfo      = providerInfo;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    // Close();

                    base.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgConnection() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        // public void Dispose()
        // {
        //     // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //     Dispose(true);
        //     // TODO: uncomment the following line if the finalizer is overridden above.
        //     // GC.SuppressFinalize(this);
        // }
        #endregion

        protected override void Activate()
        {
        }
        protected override void Deactivate()
        {
        }

        internal void Open(PgConnection owner)
        {
            // Connect
            _connection.Open();
        }

        protected override void PrepareForCloseConnection()
        {
            // Dispose Active commands
            CloseCommands();

            // Rollback active transaction
            DisposeActiveTransaction();
        }

        internal override void ChangeDatabase(string database)
        {
            _connection.ChangeDatabase(database);
        }

        internal override DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var transaction = new PgTransaction(OwningConnection, isolationLevel);
            transaction.Begin();

            _activeTransaction = new WeakReference(transaction);

            return transaction;
        }

        internal void DisposeActiveTransaction()
        {
            if (HasActiveTransaction)
            {
                ActiveTransaction.Dispose();
                _activeTransaction = null;
            }
        }

        internal Statement CreateStatement() => _connection.CreateStatement();

        internal Statement CreateStatement(string stmtText) => _connection.CreateStatement(stmtText);

        internal void CloseCommands()
        {
#warning TODO: Use PgReferenceCollection
            // foreach (var commandRef in _commands)
            // {
            //     PgCommand command = null;

            //     if (commandRef.Value != null && commandRef.Value.TryGetTarget(out command))
            //     {
            //         command.InternalClose();
            //     }
            // }
            
            // _commands.Clear();
        }

        internal void AddCommand(PgCommand command)
        {
#warning TODO: Use PgReferenceCollection
            // _commands[command.GetHashCode()] = new WeakReference<PgCommand>(command);
        }

        internal void RemoveCommand(PgCommand command)
        {
#warning TODO: Use PgReferenceCollection
            // WeakReference<PgCommand> removed = null;
            // _commands.TryRemove(command.GetHashCode(), out removed);
        }

        internal bool Verify()
        {
            bool isValid = true;

            try
            {
                // Try to send a Sync message
                _connection.Sync();
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new PgReferenceCollection();
        }

        internal override bool TryReplaceConnection(DbConnection                               outerConnection
                                                  , DbConnectionFactory                        connectionFactory
                                                  , TaskCompletionSource<DbConnectionInternal> retry
                                                  , DbConnectionOptions                        userOptions)
        {
            return base.TryOpenConnectionInternal(outerConnection, connectionFactory, retry, userOptions);
        }
    }
}
