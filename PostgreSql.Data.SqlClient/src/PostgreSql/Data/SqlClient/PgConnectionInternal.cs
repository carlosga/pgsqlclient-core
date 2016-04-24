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
        private Connection                        _connection;
        private DbConnectionOptions               _connectionOptions;
        private WeakReference                     _activeTransaction;
        private DbConnectionPoolIdentity          _identity;
        private DbConnectionPoolGroupProviderInfo _providerInfo;
        private DbConnectionOptions               _userConnectionOptions;
        private bool                              _applyTransientFaultHandling;

        internal PgConnection        OwningConnection      => (PgConnection)Owner;
        internal Connection          Connection            => _connection;
        internal PgTransaction       ActiveTransaction     => _activeTransaction?.Target as PgTransaction;
        internal string              Database              => _connection?.Database;
        internal string              DataSource            => _connection?.DataSource;
        internal DbConnectionOptions ConnectionOptions     => _connectionOptions;
        internal DbConnectionOptions UserConnectionOptions => _userConnectionOptions;
        internal bool ApplyTransientFaultHandling          => _applyTransientFaultHandling;

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

        internal PgConnectionInternal(DbConnectionPoolIdentity          identity
                                    , DbConnectionOptions               connectionOptions
                                    , DbConnectionPoolGroupProviderInfo providerInfo
                                    , DbConnectionOptions               userConnectionOptions       = null
                                    , bool                              applyTransientFaultHandling = false) 
            : base() // : base(connectionOptions)
        {
            _connectionOptions           = connectionOptions;
            _identity                    = identity;
            _providerInfo                = providerInfo;
            _userConnectionOptions       = userConnectionOptions;
            _applyTransientFaultHandling = applyTransientFaultHandling;
            _connection                  = new Connection(_connectionOptions);

            _connection.Open();
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

        protected override void PrepareForCloseConnection()
        {
            // Rollback active transaction
            if (HasActiveTransaction)
            {
                ActiveTransaction.Dispose();
                _activeTransaction = null;
            }
        }

        internal override void ChangeDatabase(string database)
        {
            if (database == null || database.Trim().Length == 0)
            {
                throw ADP.EmptyDatabaseName();
            }
            _connection.ChangeDatabase(database);
        }

        internal override DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
            }

            var transaction = new PgTransaction(OwningConnection, isolationLevel);

            transaction.Begin();

            _activeTransaction = new WeakReference(transaction);

            return transaction;
        }

        internal Statement CreateStatement() => _connection.CreateStatement();

        internal Statement CreateStatement(string stmtText) => _connection.CreateStatement(stmtText);

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

        internal override bool TryReplaceConnection(DbConnection                               outerConnection
                                                  , DbConnectionFactory                        connectionFactory
                                                  , TaskCompletionSource<DbConnectionInternal> retry
                                                  , DbConnectionOptions                        userOptions)
        {
            return base.TryOpenConnectionInternal(outerConnection, connectionFactory, retry, userOptions);
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            Console.WriteLine("PgReferenceCollection");
            return new PgReferenceCollection();
        }
    }
}
