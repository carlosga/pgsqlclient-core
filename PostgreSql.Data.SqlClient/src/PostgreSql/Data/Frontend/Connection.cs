// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Authentication;
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

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

        private Transport         _transport;
        private ConnectionOptions _connectionOptions;
        private TransactionStatus _transactionStatus;
        private SessionData       _sessionData;
        private int               _handle;
        private int               _secretKey;
        private bool              _open;

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

        internal SessionData       SessionData       => _sessionData;
        internal ConnectionOptions ConnectionOptions => _connectionOptions;
        internal TransactionStatus TransactionStatus => _transactionStatus;

        private SemaphoreSlim _asyncActiveSemaphore;
        internal SemaphoreSlim LazyEnsureAsyncActiveSemaphoreInitialized()
        {
            // Lazily-initialize _asyncActiveSemaphore.  As we're never accessing the SemaphoreSlim's
            // WaitHandle, we don't need to worry about Disposing it.
            return LazyInitializer.EnsureInitialized(ref _asyncActiveSemaphore, () => new SemaphoreSlim(1, 1));
        }

        internal Connection(string connectionString)
        {
            _connectionOptions = new ConnectionOptions(connectionString);
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
                    // TODO: dispose managed state (managed objects).
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgDatabase() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        internal void Lock()
        {
            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            sem.Wait();
        }

        internal void ReleaseLock()
        {
            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            sem.Release();
        }

        internal void Open()
        {
            try
            {
                Lock();

                // Reset instance data
                _open        = false;
                _sessionData = new SessionData();

                // Wire up SSL callbacks
                if (_connectionOptions.Encrypt)
                {
                    _transport.UserCertificateValidation = UserCertificateValidation;
                    _transport.UserCertificateSelection  = UserCertificateSelection;
                }

                // Open the channel
                _transport.Open(_connectionOptions.DataSource
                            , _connectionOptions.PortNumber
                            , _connectionOptions.ConnectionTimeout
                            , _connectionOptions.PacketSize
                            , _connectionOptions.Encrypt);

                // Send startup message
                SendStartupMessage();

                // Release lock
                ReleaseLock();
            }
            catch (Exception)
            {
                ReleaseLock();

                Close();

                throw;
            }
        }

        internal void Close()
        {
            try
            {
                Lock();

                _transport.Close();
            }
            catch
            {
            }
            finally
            {
                _connectionOptions = null;
                _sessionData       = null;
                _transactionStatus = TransactionStatus.Default;
                _handle            = -1;
                _secretKey         = -1;
                _transport           = null;
                _open              = false;

                // Callback cleanup
                InfoMessage               = null;
                Notification              = null;
                UserCertificateValidation = null;
                UserCertificateSelection  = null;

                ReleaseLock();
            }
        }

        internal void ReleaseCallbacks()
        {
            UserCertificateValidation = null;
            UserCertificateSelection  = null;
            InfoMessage               = null;
            Notification              = null;
        }

        internal TransactionInternal CreateTransaction(IsolationLevel isolationLevel)
            => new TransactionInternal(this, isolationLevel);

        internal Statement CreateStatement() => new Statement(this);

        internal Statement CreateStatement(string stmtText) => new Statement(this, stmtText);

        internal void Flush() => _transport.WriteMessage(FrontendMessages.Flush);

        internal void Sync()
        {
            if (!_open)
            {
                return;
            }

            _transport.WriteMessage(FrontendMessages.Sync);

            ReadUntilReadyForQuery();
        }

        internal void CancelRequest()
        {
            var message = CreateMessage(FrontendMessages.Untyped);

            message.Write(CancelRequestCode);
            message.Write(_handle);
            message.Write(_secretKey);

            _transport.WriteMessage(message);
        }

        internal MessageWriter CreateMessage(char type) => new MessageWriter(type, _sessionData);

        internal MessageReader Read()
        {
            var message = _transport.ReadMessage(_sessionData);

            switch (message.MessageType)
            {
                case BackendMessages.ReadyForQuery:
                    switch (message.ReadChar())
                    {
                        case 'T':
                            _transactionStatus = TransactionStatus.Active;
                            break;

                        case 'E':
                            _transactionStatus = TransactionStatus.Broken;
                            break;

                        case 'I':
                        default:
                            _transactionStatus = TransactionStatus.Default;
                            break;
                    }
                    break;

                case BackendMessages.NotificationResponse:
                    HandleNotificationMessage(message);
                    break;

                case BackendMessages.NoticeResponse:
                case BackendMessages.ErrorResponse:
                    HandleErrorMessage(message);
                    break;
            }

            return message;
        }

        internal void Send(MessageWriter message) => _transport.WriteMessage(message);

        private void SendStartupMessage()
        {
            // Send Startup message
            var message = CreateMessage(FrontendMessages.Untyped);

            // user name
            message.Write(ProtocolVersion3);
            message.WriteNullString("user");
            message.WriteNullString(_connectionOptions.UserID);

            // database
            if (!String.IsNullOrEmpty(_connectionOptions.Database))
            {
                message.WriteNullString("database");
                message.WriteNullString(_connectionOptions.Database);
            }

            // select ISO date style
            message.WriteNullString("DateStyle");
            message.WriteNullString(PgDate.DateStyle);

            // search path
            if (!String.IsNullOrEmpty(_connectionOptions.SearchPath))
            {
                message.WriteNullString("search_path");
                message.WriteNullString(_connectionOptions.SearchPath);
            }

            // Terminator
            message.WriteByte(0);

            _transport.WriteMessage(message);

            // Process responses
            ReadUntilReadyForQuery();

            _open = true;
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

        private void SendClearTextPasswordAuthentication()
        {
            var authPacket = CreateMessage(FrontendMessages.PasswordMessage);

            authPacket.WriteNullString(_connectionOptions.Password);

            _transport.WriteMessage(authPacket);
        }

        private void SendPasswordAuthentication(byte[] salt)
        {
            var authMsg = CreateMessage(FrontendMessages.PasswordMessage);
            var hash    = MD5Authentication.EncryptPassword(salt, _connectionOptions.UserID, _connectionOptions.Password);
            
            authMsg.WriteNullString(hash);

            Send(authMsg);
        }

        private void HandleMessage(MessageReader message)
        {
            switch (message.MessageType)
            {
                case BackendMessages.Authentication:
                    HandleAuthMessage(message);
                    break;

                case BackendMessages.BackendKeyData:
                    _handle    = message.ReadInt32();
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

                case AuthenticationStage.ClearText:
                    SendClearTextPasswordAuthentication();
                    break;

                case AuthenticationStage.MD5:
                    // Read salt used when encrypting the password
                    SendPasswordAuthentication(message.ReadBytes(4));
                   break;

                default:
                    throw new NotSupportedException();

#warning TODO: Review & implement if needed
                // case AuthenticationStage.Kerberosv4:      // Kerberos V4 authentication is required
                // case AuthenticationStage.Kerberosv4:      // Kerberos V5 authentication is required
                // case AuthenticationStage.GSS:
                //     // The frontend must now initiate a GSSAPI negotiation.
                //     // The frontend will send a PasswordMessage with the first part of the GSSAPI data stream in response to this.
                //     // If further messages are needed, the server will respond with AuthenticationGSSContinue.
                //     throw new NotSupportedException();
                //     break;

                // case AuthenticationStage.SSPI:
                //     // The frontend must now initiate a SSPI negotiation.
                //     // The frontend will send a PasswordMessage with the first part of the SSPI data stream in response to this.
                //     // If further messages are needed, the server will respond with AuthenticationGSSContinue.
                //     throw new NotSupportedException();
                //     break;

                // case AuthenticationStage.GSSContinue:
                //     // This message contains the response data from the previous step of GSSAPI or SSPI negotiation
                //     // (AuthenticationGSS, AuthenticationSSPI or a previous AuthenticationGSSContinue).
                //     // If the GSSAPI or SSPI data in this message indicates more data is needed to complete the authentication,
                //     // the frontend must send that data as another PasswordMessage.
                //     // If GSSAPI or SSPI authentication is completed by this message, the server will next send AuthenticationOk
                //     // to indicate successful authentication or ErrorResponse to indicate failure.
                //     throw new NotSupportedException();
                //     break;
            }
        }

        private void HandleErrorMessage(MessageReader message)
        {
            char   type     = ' ';
            string value    = String.Empty;
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
                type  = message.ReadChar();
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
                Sync();

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

        // internal void GetDatabaseTypeInfo()
        // {
        //     // if (!_database.ConnectionOptions.UseDatabaseOids)
        //     // {
        //     //     return;
        //     // }

        //     string sql = "SELECT oid FROM pg_type WHERE typname=$1";

        //     using (var statement = CreateStatement(sql))
        //     {
        //         // Set parameter type info
        //         s_typeInfoParams[0].TypeInfo = _sessionData.DataTypes.Single(x => x.Name == "varchar");

        //         // Prepare statement execution
        //         statement.Prepare(s_typeInfoParams);

        //         // Grab real oids
        //         foreach (var type in _sessionData.DataTypes)
        //         {
        //             s_typeInfoParams[0].Value = type.Name;

        //             int? realOid = (int?)statement.ExecuteScalar(s_typeInfoParams);

        //             if (realOid != null && realOid.Value != type.Oid)
        //             {
        //                 type.Oid = realOid.Value;
        //             }
        //         }
        //     }
        // }
    }
}