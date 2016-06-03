// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgConnectionInternal
        : DbConnectionInternal, IDisposable
    {
        private static bool IsTransient(Exception ex)
        {
            return false;
        }
        
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
            : base()
        {
            if (string.IsNullOrEmpty(connectionOptions.DataSource))
            {
                throw ADP.InvalidOperation("Cannot open a connection without specifying a data source or server.");
            }

            _connectionOptions           = connectionOptions;
            _identity                    = identity;
            _providerInfo                = providerInfo;
            _userConnectionOptions       = userConnectionOptions;
            _applyTransientFaultHandling = applyTransientFaultHandling;
            _connection                  = new Connection(userConnectionOptions);

            if (userConnectionOptions != null)
            {
                _connection.Notification              = userConnectionOptions.Notification; 
                _connection.InfoMessage               = userConnectionOptions.InfoMessage;
                _connection.UserCertificateValidation = userConnectionOptions.UserCertificateValidation;
                _connection.UserCertificateSelection  = UserConnectionOptions.UserCertificateSelection;
            }

            RetryOperation operation = new RetryOperation(_connectionOptions.ConnectRetryCount
                                                        , _connectionOptions.ConnectRetryInterval * 1000
                                                        , _applyTransientFaultHandling);

            operation.Execute(() => _connection.Open(), (ex) => IsTransient(ex));
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // guard against multiple concurrent dispose calls
                    var connection = Interlocked.Exchange(ref _connection, null);
                    if (connection != null)
                    {
                        connection.Close();
                    }

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
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        internal override void ChangeDatabase(string database)
        {
            if (string.IsNullOrEmpty(database))
            {
                throw ADP.EmptyDatabaseName();
            }
            _connection.ChangeDatabase(database);
        }

        internal override DbTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            var transaction = new PgTransaction(OwningConnection, _connection.CreateTransaction(isolationLevel));

            transaction.Begin();

            _activeTransaction = new WeakReference(transaction);

            return transaction;
        }

        internal Statement CreateStatement()                => _connection.CreateStatement();
        internal Statement CreateStatement(string stmtText) => _connection.CreateStatement(stmtText);

        internal override bool IsConnectionAlive(bool throwOnException = false)
        {
            try
            {
                return _connection.IsConnectionAlive(throwOnException);
            }
            catch
            {
                if (throwOnException)
                {
                    throw;
                }
                return false;
            }
        }

        internal override bool TryReplaceConnection(DbConnection                               outerConnection
                                                  , DbConnectionFactory                        connectionFactory
                                                  , TaskCompletionSource<DbConnectionInternal> retry
                                                  , DbConnectionOptions                        userOptions)
        {
            return base.TryOpenConnectionInternal(outerConnection, connectionFactory, retry, userOptions);
        }

        protected override void Activate()
        {
        }

        protected override void Deactivate()
        {
            try
            {
                if (HasActiveTransaction)
                {
                    ActiveTransaction.Dispose();
                    _activeTransaction = null;
                }
                var referenceCollection = ReferenceCollection as PgReferenceCollection;
                if (referenceCollection != null)
                {
                    referenceCollection.Deactivate();
                }
                // This should be true only if PgConnection.ChangeDatabase has been called with a different database name
                // In that case restore the connection to the original database only for pooled connections.
                if (_providerInfo.PoolGroup != null && _connection.Database != _connectionOptions.InitialCatalog)
                {
                    _connection.ChangeDatabase(_connectionOptions.InitialCatalog);
                }
                _connection.InfoMessage               = null;
                _connection.Notification              = null;
                _connection.UserCertificateSelection  = null;
                _connection.UserCertificateValidation = null;
            }
            catch (Exception e)
            {
                if (!ADP.IsCatchableExceptionType(e))
                {
                    throw;
                }

                // if an exception occurred, the inner connection will be
                // marked as unusable and destroyed upon returning to the
                // pool
                DoomThisConnection();
            }
        }

        protected override DbReferenceCollection CreateReferenceCollection()
        {
            return new PgReferenceCollection();
        }
    }
}
