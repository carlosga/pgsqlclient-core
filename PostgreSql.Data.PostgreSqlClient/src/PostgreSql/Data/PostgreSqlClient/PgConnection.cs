// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Data;
using System.Data.Common;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgConnection
        : DbConnection
    {
        public event PgInfoMessageEventHandler           InfoMessage;
        public event PgNotificationEventHandler          Notification;
        public event RemoteCertificateValidationCallback UserCertificateValidation;
        public event LocalCertificateSelectionCallback   UserCertificateSelection;

        private PgConnectionInternal _innerConnection;
        private PgConnectionOptions  _connectionOptions;
        private ConnectionState      _state;
        private bool                 _disposed;
        private string               _connectionString;

        public override string ConnectionString
        {
            get { return _connectionString; }
            set
            {
                if (IsClosed)
                {
                    _connectionOptions = new PgConnectionOptions(value);
                    _connectionString  = value;
                } 
            }
        }

        public override string          Database                 => _connectionOptions?.Database;
        public override string          DataSource               => _connectionOptions?.DataSource;
        public override string          ServerVersion            => _innerConnection?.ServerVersion;
        public override ConnectionState State                    => _state;
        public override int             ConnectionTimeout        => (_connectionOptions?.ConnectionTimeout ?? 15);
        public          int             PacketSize               => (_connectionOptions?.PacketSize ?? 8192);
        public          bool            MultipleActiveResultSets => (_connectionOptions?.MultipleActiveResultSets ?? false);
        public          string          SearchPath               => (_connectionOptions?.SearchPath);
        public          int             FetchSize                => (_connectionOptions?.FetchSize ?? 200);

        internal PgConnectionInternal InnerConnection
        {
            get { return _innerConnection; }
            // set { _innerConnection = value; }
        }

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

        public new PgTransaction BeginTransaction() => BeginTransaction(IsolationLevel.ReadCommitted, null);
        
        public new PgTransaction BeginTransaction(IsolationLevel isolationLevel) => BeginTransaction(isolationLevel, null);
        
        public PgTransaction BeginTransaction(string transactionName) => BeginTransaction(IsolationLevel.ReadCommitted, transactionName);

        public PgTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
        {
            // if (String.IsNullOrEmpty(transactionName))
            // {
            //     throw new ArgumentException("No transaction name was be specified.");
            // }
            
            if (IsClosed)
            {
                throw new InvalidOperationException("BeginTransaction requires an open and available Connection.");
            }

            return _innerConnection.BeginTransaction(isolationLevel, transactionName);
        }

        public override void ChangeDatabase(string db)
        {
            throw new NotImplementedException();
        }

        public new PgCommand CreateCommand() => _innerConnection.CreateCommand();
        
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

                // Open connection
                if (_connectionOptions.Pooling)
                {
                    _innerConnection = PgPoolManager.Instance.GetPool(_connectionString).CheckOut();
                }
                else
                {
                    _innerConnection = new PgConnectionInternal(_connectionString);
                }

                if (_connectionOptions.Encrypt)
                {
                    // Add SSL callback handlers
                    _innerConnection.Database.UserCertificateValidationCallback = new RemoteCertificateValidationCallback(OnUserCertificateValidation);
                    _innerConnection.Database.UserCertificateSelectionCallback  = new LocalCertificateSelectionCallback(OnUserCertificateSelection);
                }

                // Add Info message event handler
                _innerConnection.Database.InfoMessage = new InfoMessageCallback(OnInfoMessage);

                // Add notification event handler
                _innerConnection.Database.Notification = new NotificationCallback(OnNotification);

                // Connect
                _innerConnection.Open(this);

                // Set connection state to Open
                ChangeState(ConnectionState.Open);
            }
            catch (PgClientException ex)
            {
                ChangeState(ConnectionState.Broken);
                throw new PgException(ex);
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
                // Cleanup
                _innerConnection   = null;
                _connectionString  = null;
                _connectionOptions = null;
                
                ChangeState(ConnectionState.Closed);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        // release any managed resources
                        Close();
                    }
                    finally
                    {
                    }
                }

                // release any unmanaged resources
                _disposed = true;
            }
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand() => CreateCommand();

        private void OnInfoMessage(PgClientException ex) => InfoMessage?.Invoke(this, new PgInfoMessageEventArgs(ex));

        private void OnNotification(int processId, string condition, string aditional)
        {
            Notification?.Invoke(this, new PgNotificationEventArgs(processId, condition, aditional));
        }

        private bool OnUserCertificateValidation(object          sender
                                               , X509Certificate certificate
                                               , X509Chain       chain
                                               , SslPolicyErrors sslPolicyErrors)
        {
            if (UserCertificateValidation != null)
            {
                return UserCertificateValidation(this, certificate, chain, sslPolicyErrors);
            }

            return false;
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