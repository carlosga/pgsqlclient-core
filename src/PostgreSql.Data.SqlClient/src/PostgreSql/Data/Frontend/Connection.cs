// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Net.Security;
using System.Threading;
using PostgreSql.Data.Frontend.Sasl;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Connection
        : IDisposable
    {
        // Cancel request code
        private const int CancelRequestHi   = 1234;
        private const int CancelRequestLo   = 5678;
        private const int CancelRequestCode = (CancelRequestHi << 16) | CancelRequestLo;

        // Protocol version 3.0
        private const int ProtocolVersion3Major = 3;
        private const int ProtocolVersion3Minor = 0;
        private const int ProtocolVersion3      = (ProtocolVersion3Major << 16) | ProtocolVersion3Minor;

        private DbConnectionOptions _connectionOptions;
        private TransactionState    _transactionState;
        private SessionData         _sessionData;
        private int                 _processId;
        private int                 _secretKey;
        private bool                _open;
        private Transport           _transport;
        private MessageReader       _reader;
        private ISaslMechanism      _saslAuthenticator;
        private SyncMode            _syncMode;

        internal NotificationCallback Notification
        {
            get;
            set;
        }

        internal InfoMessageCallback InfoMessage
        {
            get;
            set;
        }

        internal RemoteCertificateValidationCallback UserCertificateValidation
        {
            get;
            set;
        }

        internal LocalCertificateSelectionCallback UserCertificateSelection
        {
            get;
            set;
        }

        internal SessionData         SessionData       => _sessionData;
        internal TransactionState    TransactionState  => _transactionState;
        internal DbConnectionOptions ConnectionOptions => _connectionOptions;

        private SemaphoreSlim _activeSemaphore;
        private SemaphoreSlim LazyEnsureActiveSemaphoreInitialized()
        {
            return LazyInitializer.EnsureInitialized(ref _activeSemaphore, () => new SemaphoreSlim(1, 1));
        }
        private SemaphoreSlim _cancelRequestSemaphore;
        private SemaphoreSlim LazyEnsureCancelRequestSemaphoreInitialized()
        {
            return LazyInitializer.EnsureInitialized(ref _cancelRequestSemaphore, () => new SemaphoreSlim(1, 1));
        }

        internal Connection(DbConnectionOptions connectionOptions)
        {
            _connectionOptions = connectionOptions;
            _transport         = new Transport();
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Close();
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        internal void Lock()
        {
            SemaphoreSlim sem = LazyEnsureActiveSemaphoreInitialized();
            sem.Wait();
        }

        internal void ReleaseLock()
        {
            SemaphoreSlim sem = LazyEnsureActiveSemaphoreInitialized();
            sem.Release();
        }

        internal void Open()
        {
            try
            {
                Lock();
                OpenInternal();
                ReleaseLock();
            }
            catch
            {
                ReleaseLock();
                Close();

                throw;
            }
        }

        internal void Close()
        {
            if (!_open)
            {
                return;
            }
            
            try
            {
                Lock();

                _transport?.Close();
                _reader?.Dispose();

                TypeInfoProviderCache.Release(_connectionOptions);
            }
            catch
            {
            }
            finally
            {
                ReleaseLock();

                _activeSemaphore?.Dispose();
                _cancelRequestSemaphore?.Dispose();

                _connectionOptions      = null;
                _sessionData            = null;
                _transport              = null;
                _reader                 = null;
                _processId              = -1;
                _secretKey              = -1;
                _open                   = false;
                _activeSemaphore        = null;
                _cancelRequestSemaphore = null;
                _saslAuthenticator      = null;
                _transactionState       = TransactionState.Default;
                _syncMode               = SyncMode.None;

                // Callback cleanup
                ReleaseCallbacks();
            }
        }

        internal void ChangeDatabase(string database)
        {
            try
            {
                Lock();

                // Update the current database name
                _connectionOptions.ChangeDatabase(database);

                // Close current transport
                _transport.Close();

                // Release the type info provider
                TypeInfoProviderCache.Release(_connectionOptions);

                // Reset the current session data
                _sessionData = null;

                // Reopen against the new database
                OpenInternal();

                ReleaseLock();
            }
            catch
            {
                ReleaseLock();

                Close();

                throw;
            }
        }

        internal Transaction CreateTransaction(IsolationLevel isolationLevel) => new Transaction(this, isolationLevel);

        internal Statement CreateStatement()                => new Statement(this);
        internal Statement CreateStatement(string stmtText) => new Statement(this, stmtText);

        internal void CancelRequest()
        {
            SemaphoreSlim activeSem = LazyEnsureActiveSemaphoreInitialized();
            if (activeSem.CurrentCount == 0)
            {
                // No pending requests
                return;
            }
            SemaphoreSlim cancelSem = LazyEnsureCancelRequestSemaphoreInitialized();
            if (cancelSem.CurrentCount == 1)
            {
                // A cancelation request is already in progress
                return;
            }

            Connection connection = null;

            try
            {
                cancelSem.Wait();

                connection = new Connection(_connectionOptions);
                connection.Open();

                using (var message = new MessageWriter(FrontendMessages.Untyped, _sessionData))
                {
                    message.Write(CancelRequestCode);
                    message.Write(_processId);
                    message.Write(_secretKey);

                    message.WriteTo(_transport);
                }
            }
            finally
            {
                connection?.Dispose();
                cancelSem.Release();
            }
        }

        internal void Sync()
        {
            _transport.WriteFrame(FrontendMessages.Sync);
            ReadUntilReadyForQuery();
        }

        internal MessageReader Read()
        {
            _reader.ReadFrom(_transport);

            switch (_reader.MessageType)
            {
            case BackendMessages.ReadyForQuery:
                _transactionState = (TransactionState)_reader.ReadByte();
                break;

            case BackendMessages.NotificationResponse:
                HandleNotificationMessage(_reader);
                break;

            case BackendMessages.NoticeResponse:
            case BackendMessages.ErrorResponse:
                HandleErrorMessage(_reader);
                break;
            }

            return _reader;
        }

        internal void Send(MessageWriter message, SyncMode syncMode = SyncMode.None) 
        {
            _syncMode = syncMode;
            message.WriteTo(_transport, syncMode);
        }

        internal bool IsConnectionAlive(bool throwOnException = false) 
            => ((_transport == null) ? false : _transport.IsTransportAlive(throwOnException)); 

        private void OpenInternal()
        {
            // Reset instance data
            _open        = false;
            _sessionData = new SessionData(_connectionOptions, TypeInfoProviderCache.GetOrAdd(_connectionOptions));
            _reader      = new MessageReader(_sessionData);

            // Wire up SSL callbacks
            if (_connectionOptions.Encrypt)
            {
                _transport.UserCertificateValidation = UserCertificateValidation;
                _transport.UserCertificateSelection  = UserCertificateSelection;
            }

            // Open the channel
            _transport.Open(_connectionOptions.DataSource
                          , _connectionOptions.PortNumber
                          , _connectionOptions.ConnectTimeout
                          , _connectionOptions.PacketSize
                          , _connectionOptions.Encrypt);

            // Send startup message
            SendStartupMessage();

            // Set the connection as open
            _open = true;
        }

        private void SendStartupMessage()
        {
            // Send Startup message
            using (var message = new MessageWriter(FrontendMessages.Untyped, _sessionData))
            {
                // http://www.postgresql.org/docs/9.5/static/runtime-config-client.html

                // user name
                message.Write(ProtocolVersion3);
                message.WriteNullString("user");
                message.WriteNullString(_connectionOptions.UserID);

                // database
                if (!string.IsNullOrEmpty(_connectionOptions.InitialCatalog))
                {
                    message.WriteNullString("database");
                    message.WriteNullString(_connectionOptions.InitialCatalog);
                }

                // select ISO date style
                message.WriteNullString("DateStyle");
                message.WriteNullString(PgDate.DateStyle);

                // search_path
                if (!string.IsNullOrEmpty(_connectionOptions.SearchPath))
                {
                    message.WriteNullString("search_path");
                    message.WriteNullString(_connectionOptions.SearchPath);
                }

                // application_name
                if (!string.IsNullOrEmpty(_connectionOptions.ApplicationName))
                {
                    message.WriteNullString("application_name");
                    message.WriteNullString(_connectionOptions.ApplicationName);
                }

                // statement_timeout (milliseconds)
                if (_connectionOptions.CommandTimeout > 0)
                {
                    message.WriteNullString("statement_timeout");
                    message.WriteNullString($"{_connectionOptions.CommandTimeout}s");
                }

                // lock_timeout (milliseconds)
                if (_connectionOptions.LockTimeout > 0)
                {
                    message.WriteNullString("lock_timeout");
                    message.WriteNullString(_connectionOptions.LockTimeout.ToString(TypeInfoProvider.InvariantCulture));
                }

                // default_transaction_read_only
                if (_connectionOptions.DefaultTransactionReadOnly)
                {
                    message.WriteNullString("default_transaction_read_only");
                    message.WriteNullString("on");
                }

                // default_tablespace
                if (!string.IsNullOrEmpty(_connectionOptions.DefaultTablespace))
                {
                    message.WriteNullString("default_tablespace");
                    message.WriteNullString(_connectionOptions.DefaultTablespace);
                }

                // Terminator
                message.WriteByte(0);

                message.WriteTo(_transport);
            }

            // Process responses
            ReadUntilReadyForQuery();
        }

        private void ReadUntilReadyForQuery()
        {
            MessageReader message = null;

            do
            {
                message = Read();
                HandleMessage(message);
            } while (!message.IsReadyForQuery);
        }

        private void HandleMessage(MessageReader message)
        {
            switch (message.MessageType)
            {
            case BackendMessages.Authentication:
                HandleAuthMessage(message);
                break;

            case BackendMessages.BackendKeyData:
                _processId = message.ReadInt32();
                _secretKey = message.ReadInt32();
                break;

            case BackendMessages.ParameterStatus:
                HandleParameterStatus(message);
                break;
            }
        }

        private void HandleAuthMessage(MessageReader message)
        {
            // Authentication response
            var authType = (AuthenticationStage)message.ReadInt32();

            switch (authType)
            {
            case AuthenticationStage.Done:
                // Authentication successful
                return;

            case AuthenticationStage.MD5:
                // Read salt used when encrypting the password
                AuthenticationMD5(message.ReadBytes(4));
                break;

            case AuthenticationStage.AuthenticationSASL:
                AuthenticationSASL(message);
                break;

            case AuthenticationStage.SASLContinue:
                SASLContinue(message);
                break;

            case AuthenticationStage.AuthenticationSASLFinal:
                AuthenticationSASLFinal(message);
                break;

            default:
                throw ADP.NotSupported();
            }
        }

        private void AuthenticationMD5(byte[] salt)
        {
            using (var response = new MessageWriter(FrontendMessages.PasswordMessage, _sessionData))
            {
                var hash = MD5Authentication.EncryptPassword(salt, _connectionOptions.UserID, _connectionOptions.Password);

                response.WriteNullString(hash);
                response.WriteTo(_transport);
            }
        }

        private void AuthenticationSASL(MessageReader message)
        {
            using (var response = new MessageWriter(FrontendMessages.SASLInitialresponse, _sessionData))
            {
                var saslMechanism = message.ReadNullString();   // Name of a SASL authentication mechanism.

                _saslAuthenticator = SaslScram.Create(saslMechanism, _sessionData.ClientEncoding);

                var buffer = _saslAuthenticator.Auth();

                response.WriteNullString(saslMechanism);           
                response.Write(buffer.Length);
                response.Write(buffer);

                response.WriteTo(_transport);            
            }
        }

        private void SASLContinue(MessageReader message)
        {
            using (var response = new MessageWriter(FrontendMessages.SASLResponse, _sessionData))
            {
                var challenge = message.ReadToEnd();
                // var channelBinding = _transport.GetChannelBinding();

                var buffer = _saslAuthenticator.Challenge(challenge, _connectionOptions.Password);

                if (buffer == null)
                {
                    throw new PgException("Authentication failed");
                }

                response.Write(buffer);

                response.WriteTo(_transport);
            }
        }

        private void AuthenticationSASLFinal(MessageReader message)
        {
            bool valid =_saslAuthenticator.Verify(message.ReadToEnd());
            _saslAuthenticator = null;
            if (!valid)
            {
                throw new PgException("Authentication failed");
            }
        }

        private void HandleErrorMessage(MessageReader message)
        {
            byte   type     = 255;
            string value    = string.Empty;
            string severity = null;
            string emessage = null;
            string code     = null;
            string detail   = null;
            string hint     = null;
            string where    = null;
            string position = null;
            string file     = null;
            int    line     = 0;
            string routine  = null;

            while (type != ErrorMessageParts.End)
            {
                type  = message.ReadByte();
                value = message.ReadNullString();

                switch (type)
                {
                case ErrorMessageParts.Severity:
                    severity = value;
                    break;

                case ErrorMessageParts.Code:
                    code = value;
                    break;

                case ErrorMessageParts.Message:
                    emessage = value;
                    break;

                case ErrorMessageParts.Detail:
                    detail = value;
                    break;

                case ErrorMessageParts.Hint:
                    hint = value;
                    break;

                case ErrorMessageParts.Position:
                    position = value;
                    break;

                case ErrorMessageParts.Where:
                    where = value;
                    break;

                case ErrorMessageParts.File:
                    file = value;
                    break;

                case ErrorMessageParts.Line:
                    line = Convert.ToInt32(value);
                    break;

                case ErrorMessageParts.Routine:
                    routine = value;
                    break;
                }
            }

            var error     = new PgError(severity, emessage, code, detail, hint, where, position, file, line, routine);
            var exception = new PgException(error.Message, error);

            InfoMessage?.Invoke(exception);

            if (error.Severity == ErrorSeverity.Error
             || error.Severity == ErrorSeverity.Fatal
             || error.Severity == ErrorSeverity.Panic)
            {
                if (_syncMode == SyncMode.Sync || _syncMode == SyncMode.SyncAndFlush)
                {
                    ReadUntilReadyForQuery();
                }

                throw exception;
            }
        }

        private void HandleNotificationMessage(MessageReader message)
        {
            var processId  = message.ReadInt32();
            var condition  = message.ReadNullString();
            var additional = message.ReadNullString();

            Notification?.Invoke(processId, condition, additional);
        }

        private void HandleParameterStatus(MessageReader message)
            => _sessionData.SetValue(message.ReadNullString(), message.ReadNullString());
            
        private void ReleaseCallbacks()
        {
            UserCertificateValidation = null;
            UserCertificateSelection  = null;
            InfoMessage               = null;
            Notification              = null;
        }
    }
}
