// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;
using System.Diagnostics;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgConnection
        : DbConnection
    {
        public event PgInfoMessageEventHandler           InfoMessage;
        public event PgNotificationEventHandler          Notification;
        public event RemoteCertificateValidationCallback UserCertificateValidation;
        public event LocalCertificateSelectionCallback   UserCertificateSelection;

        private DbConnectionInternal  _innerConnection;
        private DbConnectionOptions   _userConnectionOptions;
        private DbConnectionPoolGroup _poolGroup;
        private string                _connectionString;
        private int                   _closeCount;
        private bool                  _applyTransientFaultHandling;

        public static void ClearAllPools()
        {
            PgConnectionFactory.SingletonInstance.ClearAllPools();
        }

        public static void ClearPool(PgConnection connection)
        {
            ADP.CheckArgumentNull(connection, nameof(connection));

            var connectionOptions = connection.ConnectionOptions;
            if (connectionOptions != null)
            {
                PgConnectionFactory.SingletonInstance.ClearPool(connection);
            }
        }
        
        public override string ConnectionString
        {
            get { return InternalGetConnectionString(); }
            set
            {
                InternalSetConnectionString(value);
                _connectionString = value;
            }
        }

        public override ConnectionState State           => _innerConnection.State;
        public override string ServerVersion            => _innerConnection?.ServerVersion;
        public override string Database                 => _userConnectionOptions?.InitialCatalog;
        public override string DataSource               => _userConnectionOptions?.DataSource;
        public override int    ConnectionTimeout        => (_userConnectionOptions?.ConnectTimeout ?? DbConnectionStringDefaults.ConnectTimeout);
        public          int    PacketSize               => (_userConnectionOptions?.PacketSize ?? DbConnectionStringDefaults.PacketSize);
        public          bool   MultipleActiveResultSets => (_userConnectionOptions?.MultipleActiveResultSets ?? DbConnectionStringDefaults.MultipleActiveResultSets);
        public          string SearchPath               => (_userConnectionOptions?.SearchPath);

        internal DbConnectionFactory  ConnectionFactory     => PgConnectionFactory.SingletonInstance;
        internal DbConnectionInternal InnerConnection       => _innerConnection;
        internal DbConnectionOptions  ConnectionOptions     => ((_poolGroup != null) ? _poolGroup.ConnectionOptions : null);
        internal DbConnectionOptions  UserConnectionOptions => _userConnectionOptions;

        internal bool ApplyTransientFaultHandling => _applyTransientFaultHandling;

        internal bool ForceNewConnection { get; set; }

        internal DbConnectionPoolGroup PoolGroup
        { 
            get { return _poolGroup; }
            set { _poolGroup = value; }
        }

        public PgConnection()
            : this(null)
        {
        }

        public PgConnection(string connectionString)
            : base()
        {
            _innerConnection = DbConnectionClosedNeverOpened.SingletonInstance;
            ConnectionString = connectionString ?? String.Empty;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    try
                    {
                        // This will call the Close method
                        base.Dispose(disposing);
                    }
                    finally
                    {
                        _innerConnection       = null;
                        _connectionString      = null;
                        _userConnectionOptions = null;
                        _poolGroup             = null;
                    }
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

        public override void Open()
        {
            if (String.IsNullOrEmpty(_connectionString))
            {
                throw ADP.NoConnectionString();
            }

            if (!TryOpen(null))
            {
                throw new InvalidOperationException("Cannot open a new connection");
            }
        }

        public override void Close()
        {
            _innerConnection.CloseConnection(this, ConnectionFactory);
        }

        public new PgTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted);

        public new PgTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return _innerConnection.BeginTransaction(isolationLevel) as PgTransaction;
        }

        public PgTransaction BeginTransaction(string transactionName) => BeginTransaction(IsolationLevel.ReadCommitted, transactionName);

        public PgTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
        {
            if (String.IsNullOrEmpty(transactionName))
            {
                throw ADP.NullEmptyTransactionName();
            }

            PgTransaction transaction = _innerConnection.BeginTransaction(isolationLevel) as PgTransaction;
            if (transaction != null)
            {
                transaction.Save(transactionName);
            }

            return transaction;
        }

        public override void ChangeDatabase(string database)
        {
            _innerConnection.ChangeDatabase(database);
        }

        public new PgCommand CreateCommand()
        {
            var internalConnection = _innerConnection as PgConnectionInternal;
            return new PgCommand(String.Empty, this, internalConnection?.ActiveTransaction);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand() => CreateCommand();

        private void OnInfoMessage(PgException ex) => InfoMessage?.Invoke(this, new PgInfoMessageEventArgs(ex));

        private void OnNotification(int processId, string condition, string aditional)
        {
            Notification?.Invoke(this, new PgNotificationEventArgs(processId, condition, aditional));
        }

        private bool OnUserCertificateValidation(object          sender
                                               , X509Certificate certificate
                                               , X509Chain       chain
                                               , SslPolicyErrors sslPolicyErrors)
        {
            return UserCertificateValidation?.Invoke(this, certificate, chain, sslPolicyErrors) ?? false;
        }

        private X509Certificate OnUserCertificateSelection(object                    sender
                                                         , string                    targetHost            
                                                         , X509CertificateCollection localCertificates
                                                         , X509Certificate           remoteCertificate
                                                         , string[]                  acceptableIssuers)
        {
            return UserCertificateSelection?.Invoke(this, targetHost, localCertificates, remoteCertificate, acceptableIssuers);
        }

        internal void PermissionDemand()
        {
            Debug.Assert(DbConnectionClosedConnecting.SingletonInstance == _innerConnection, "not connecting");
            DbConnectionPoolGroup poolGroup         = PoolGroup;
            DbConnectionOptions   connectionOptions = ConnectionOptions;
            if (connectionOptions == null || connectionOptions.IsEmpty)
            {
                throw ADP.NoConnectionString();
            }
            DbConnectionOptions userConnectionOptions = UserConnectionOptions;
            Debug.Assert(userConnectionOptions != null, "null UserConnectionOptions");
        }

        internal void SetInnerConnectionEvent(DbConnectionInternal to)
        {
            ConnectionState originalState = _innerConnection.State & ConnectionState.Open;
            ConnectionState currentState  = to.State & ConnectionState.Open;
            if (originalState != currentState && ConnectionState.Closed == currentState)
            {
                unchecked { _closeCount++; }
            }
            _innerConnection = to;
            if (ConnectionState.Closed == originalState && ConnectionState.Open == currentState)
            {
                OnStateChange(DbConnectionInternal.StateChangeOpen);
            }
            else if (ConnectionState.Open == originalState && ConnectionState.Closed == currentState)
            {
                OnStateChange(DbConnectionInternal.StateChangeClosed);
            }
            else
            {
                Debug.Assert(false, "unexpected state switch");
                if (originalState != currentState)
                {
                    OnStateChange(new StateChangeEventArgs(originalState, currentState));
                }
            }
        }

        internal void AddWeakReference(object value, int tag)
        {
            InnerConnection.AddWeakReference(value, tag);
        }

        internal void NotifyWeakReference(int message)
        {
            InnerConnection.NotifyWeakReference(message);
        }

        internal void RemoveWeakReference(object value)
        {
            InnerConnection.RemoveWeakReference(value);
        }

        internal bool SetInnerConnectionFrom(DbConnectionInternal to, DbConnectionInternal from)
        {
            return (Interlocked.CompareExchange<DbConnectionInternal>(ref _innerConnection, to, from) == from);
        }

        internal void SetInnerConnectionTo(DbConnectionInternal to)
        {
            _innerConnection = to;
        }

        private bool TryOpen(TaskCompletionSource<DbConnectionInternal> retry)
        {
            var connectionOptions        = ConnectionOptions;
            _applyTransientFaultHandling = (retry == null && connectionOptions != null && connectionOptions.ConnectRetryCount > 0);

#warning TODO : Wire up SSL Callbacks
            // if (_innerConnection.Encrypt)
            // {
            //     // Add SSL callback handlers
            //     _innerConnection.Connection.UserCertificateValidation = new RemoteCertificateValidationCallback(OnUserCertificateValidation);
            //     _innerConnection.Connection.UserCertificateSelection  = new LocalCertificateSelectionCallback(OnUserCertificateSelection);
            // }

            if (ForceNewConnection)
            {
                if (!InnerConnection.TryReplaceConnection(this, ConnectionFactory, retry, UserConnectionOptions))
                {
                    return false;
                }
            }
            else
            {
                if (!InnerConnection.TryOpenConnection(this, ConnectionFactory, retry, UserConnectionOptions))
                {
                    return false;
                }
            }

            // does not require GC.KeepAlive(this) because of OnStateChange

            var innerConnection = InnerConnection as PgConnectionInternal;
            Debug.Assert(innerConnection != null           , "Invalid connection state.");
            Debug.Assert(innerConnection.Connection != null, "Frontend connection cannot be null.");

            if (!innerConnection.ConnectionOptions.Pooling)
            {
                // For non-pooled connections, we need to make sure that the finalizer does actually run to avoid leaking SNI handles
                // GC.ReRegisterForFinalize(this);
            }

            // if (StatisticsEnabled)
            // {
            //     ADP.TimerCurrent(out _statistics._openTimestamp);
            //     tdsInnerConnection.Parser.Statistics = _statistics;
            // }
            // else
            // {
            //     tdsInnerConnection.Parser.Statistics = null;
            //     _statistics = null; // in case of previous Open/Close/reset_CollectStats sequence
            // }

            return true;
        }

        private string InternalGetConnectionString()
        {
            bool hidePassword      = InnerConnection.ShouldHidePassword;
            var  connectionOptions = _userConnectionOptions;
            return ((connectionOptions != null) ? connectionOptions.UsersConnectionString(hidePassword) : String.Empty);
        }

        private void InternalSetConnectionString(string connectionString)
        {
            DbConnectionPoolKey   key                = new PgConnectionPoolKey(connectionString);
            DbConnectionOptions   connectionOptions  = null;
            DbConnectionPoolGroup poolGroup          = ConnectionFactory.GetConnectionPoolGroup(key, null, ref connectionOptions);
            DbConnectionInternal  connectionInternal = InnerConnection;
            bool flag = connectionInternal.AllowSetConnectionString;
            if (flag)
            {
                flag = SetInnerConnectionFrom(DbConnectionClosedBusy.SingletonInstance, connectionInternal);
                if (flag)
                {
                    _userConnectionOptions = connectionOptions;
                    _poolGroup             = poolGroup;
                    _innerConnection       = DbConnectionClosedNeverOpened.SingletonInstance;
                }
            }
            if (!flag)
            {
                throw ADP.OpenConnectionPropertySet(nameof(ConnectionString), connectionInternal.State);
            }
        }
    }
}
