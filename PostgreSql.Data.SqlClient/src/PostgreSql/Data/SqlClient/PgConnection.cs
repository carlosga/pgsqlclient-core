// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Data;
using System.Data.Common;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgConnection
        : DbConnection
    {
        public event PgInfoMessageEventHandler           InfoMessage;
        public event PgNotificationEventHandler          Notification;
        public event RemoteCertificateValidationCallback UserCertificateValidation;
        public event LocalCertificateSelectionCallback   UserCertificateSelection;

        private PgConnectionInternal _innerConnection;
        private ConnectionState      _state;
        private string               _connectionString;

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (IsClosed)
                {
                    _connectionString = value;
                }
            }
        }

        public override string          Database                 => _innerConnection?.Database;
        public override string          DataSource               => _innerConnection?.DataSource;
        public override string          ServerVersion            => _innerConnection?.ServerVersion;
        public override int             ConnectionTimeout        => (_innerConnection?.ConnectionTimeout ?? 15);
        public          int             PacketSize               => (_innerConnection?.PacketSize ?? 8192);
        public          bool            MultipleActiveResultSets => (_innerConnection?.MultipleActiveResultSets ?? false);
        public          string          SearchPath               => (_innerConnection?.SearchPath);
        public          int             FetchSize                => (_innerConnection?.FetchSize ?? 200);
        public override ConnectionState State                    => _state;

        internal PgConnectionInternal InnerConnection => _innerConnection;

        internal bool IsClosed      => (_state == ConnectionState.Closed);
        internal bool IsOpen        => (_state == ConnectionState.Open);
        internal bool IsConnecting  => (_state == ConnectionState.Connecting);

        public PgConnection()
            : this(null)
        {
        }

        public PgConnection(string connectionString)
            : base()
        {
            _state           = ConnectionState.Closed;
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
                        // release any managed resources
                        Close();
                    }
                    finally
                    {
                        // Cleanup
                        _innerConnection  = null;
                        _connectionString = null;
                    }

                    base.Dispose(disposing);
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
                throw new InvalidOperationException("Connection String is not initialized.");
            }
            if (!IsClosed)
            {
                throw new InvalidOperationException("Connection already open, or is broken.");
            }

            try
            {
                ChangeState(ConnectionState.Connecting);

                var connectionOptions = new ConnectionOptions(_connectionString);

                // Open connection
                if (connectionOptions.Pooling)
                {
                    _innerConnection = PgPoolManager.Instance.GetPool(_connectionString).CheckOut();
                }
                else
                {
                    _innerConnection = new PgConnectionInternal(connectionOptions);
                }

                if (_innerConnection.Encrypt)
                {
                    // Add SSL callback handlers
                    _innerConnection.Connection.UserCertificateValidation = new RemoteCertificateValidationCallback(OnUserCertificateValidation);
                    _innerConnection.Connection.UserCertificateSelection  = new LocalCertificateSelectionCallback(OnUserCertificateSelection);
                }

                // Add Info message event handler
                _innerConnection.Connection.InfoMessage = new InfoMessageCallback(OnInfoMessage);

                // Add notification event handler
                _innerConnection.Connection.Notification = new NotificationCallback(OnNotification);

                // Connect
                _innerConnection.Open(this);

                // Set connection state to Open
                ChangeState(ConnectionState.Open);
            }
            catch (Exception)
            {
                ChangeState(ConnectionState.Broken);
                throw;
            }
        }

        public override void Close()
        {
            if (!IsOpen)
            {
                return;
            }

            try
            {
                _innerConnection.Close();
            }
            catch
            {
            }
            finally
            {
                ChangeState(ConnectionState.Closed);
            }
        }

        public new PgTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted);

        public new PgTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }
            if (_innerConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
            }

            return _innerConnection.BeginTransaction(isolationLevel, null);
        }

        public PgTransaction BeginTransaction(string transactionName) => BeginTransaction(IsolationLevel.ReadCommitted, transactionName);

        public PgTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }
            if (_innerConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
            }
            if (String.IsNullOrEmpty(transactionName))
            {
                throw new InvalidOperationException("Invalid transaction or invalid name for a point at which to save within the transaction.");
            }

            return _innerConnection.BeginTransaction(isolationLevel, transactionName);
        }

        public override void ChangeDatabase(string database)
        {
            if (_state == ConnectionState.Closed)
            {
                throw new InvalidOperationException("ChangeDatabase requires an open and available Connection.");
            }

            if (database == null || database.Trim().Length == 0)
            {
                throw new InvalidOperationException("Database name is not valid.");
            }

            try
            {
               _innerConnection.ChangeDatabase(database);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                ChangeState(ConnectionState.Broken);
                throw new PgException("Cannot change database.");
            }
        }

        public new PgCommand CreateCommand() => new PgCommand(String.Empty, this, _innerConnection?.ActiveTransaction);

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

        private void ChangeState(ConnectionState newState)
        {
            var oldState = _state;

            // Set the new state
            _state = newState;

            // Emit the StateChange event
            OnStateChange(new StateChangeEventArgs(oldState, _state));
        }
    }
}
