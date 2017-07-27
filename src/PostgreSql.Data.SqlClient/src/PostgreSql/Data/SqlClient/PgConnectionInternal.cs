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
            var pgException = ex as PgException;
            if (pgException != null)
            {
                foreach (PgError err in pgException.Errors)
                {
                    switch (err.Code)
                    {
                        case "08006":   // connection_failure
                        case "08001": 	// sqlclient_unable_to_establish_sqlconnection
                        case "08004": 	// sqlserver_rejected_establishment_of_sqlconnection
                        case "25P03": 	// idle_in_transaction_session_timeout
                        case "40001":   // serialization_failure
                        case "53000": 	// insufficient_resources
                        case "53200": 	// out_of_memory
                        case "53300": 	// too_many_connections
                        case "55006":   // object_in_use
                        case "55P03":   // lock_not_available
                        case "57014":   // query_canceled
                        case "57P03": 	// cannot_connect_now
                        case "58000":   // system_error
                        case "58030":   // io_error
                            return true;
                    }                   
                }

                return false;
            }

            return (ex is TimeoutException);
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
        internal string              Database              => _connection?.ConnectionOptions?.InitialCatalog;
        internal string              DataSource            => _connection?.ConnectionOptions?.DataSource;
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

                    base.Dispose();
                }

                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public override void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
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

        internal override void ValidateConnectionForExecute(DbCommand command)
        {
            PgDataReader reader = null;
            if (_connectionOptions.MultipleActiveResultSets)
            {
                if (command != null)
                { // command can't have datareader already associated with it
                    reader = FindLiveReader(command as PgCommand);
                }
            }
            else
            {
                reader = FindLiveReader(null);
            }
            if (reader != null)
            {
                // if MARS is on, then a datareader associated with the command exists
                // or if MARS is off, then a datareader exists
                throw ADP.OpenReaderExists();
            }
            // else if (MARSOn && pending_data)
            // {
            //     DrainData
            // }
        }

        internal PgDataReader FindLiveReader(PgCommand command)
        {
            PgDataReader          reader              = null;
            PgReferenceCollection referenceCollection = ReferenceCollection as PgReferenceCollection;
            if (null != referenceCollection)
            {
                reader = referenceCollection.FindLiveReader(command);
            }
            return reader;
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
                if (_providerInfo.PoolGroup != null 
                 && _connection.ConnectionOptions.InitialCatalog != _connectionOptions.InitialCatalog)
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
