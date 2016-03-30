// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Authentication;
using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgDatabase
        : IDisposable
    {
        private PgNetworkChannel    _channel;
        private PgConnectionOptions _connectionOptions;
        private PgTransactionStatus _transactionStatus;
        private SessionData         _sessionData;
        private int                 _handle;
        private int                 _secretKey;
        private bool                _open;

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
        internal PgConnectionOptions ConnectionOptions => _connectionOptions;
        internal PgTransactionStatus TransactionStatus => _transactionStatus;

        private SemaphoreSlim _asyncActiveSemaphore;
        internal SemaphoreSlim LazyEnsureAsyncActiveSemaphoreInitialized()
        {
            // Lazily-initialize _asyncActiveSemaphore.  As we're never accessing the SemaphoreSlim's
            // WaitHandle, we don't need to worry about Disposing it.
            return LazyInitializer.EnsureInitialized(ref _asyncActiveSemaphore, () => new SemaphoreSlim(1, 1));
        }

        internal PgDatabase(string connectionString)
        {
            _connectionOptions = new PgConnectionOptions(connectionString);
            _channel           = new PgNetworkChannel();
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
                    _channel.UserCertificateValidation = UserCertificateValidation;
                    _channel.UserCertificateSelection  = UserCertificateSelection;
                }

                // Open the channel
                _channel.Open(_connectionOptions.DataSource
                            , _connectionOptions.PortNumber
                            , _connectionOptions.ConnectionTimeout
                            , _connectionOptions.PacketSize
                            , _connectionOptions.Encrypt);

                // Send startup packet
                SendStartupPacket();

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

                _channel.Close();
            }
            catch
            {
            }
            finally
            {
                _connectionOptions = null;
                _sessionData       = null;
                _transactionStatus = PgTransactionStatus.Default;
                _handle            = -1;
                _secretKey         = -1;
                _channel           = null;
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

        internal PgTransactionInternal CreateTransaction(IsolationLevel isolationLevel)
            => new PgTransactionInternal(this, isolationLevel);

        internal PgStatement CreateStatement() => new PgStatement(this);

        internal PgStatement CreateStatement(string stmtText) => new PgStatement(this, stmtText);

        internal void Flush() => _channel.WritePacket(PgFrontEndCodes.FLUSH);

        internal void Sync()
        {
            if (!_open)
            {
                return;
            }
            
            _channel.WritePacket(PgFrontEndCodes.SYNC);

            PgInputPacket response = null;

            do
            {
                response = Read();
                
                HandlePacket(response);
            } while (!response.IsReadyForQuery);
        }

        internal void CancelRequest()
        {
            var packet = CreateOutputPacket(PgFrontEndCodes.UNTYPED);

            packet.Write(16);
            packet.Write(PgCodes.CANCEL_REQUEST);
            packet.Write(_handle);
            packet.Write(_secretKey);

            // Send packet to the server
            _channel.WritePacket(packet);
        }

        internal PgOutputPacket CreateOutputPacket(char type) => new PgOutputPacket(type, _sessionData);

        internal PgInputPacket Read()
        {
            var packet = _channel.ReadPacket(_sessionData);

            switch (packet.PacketType)
            {
                case PgBackendCodes.READY_FOR_QUERY:
                    switch (packet.ReadChar())
                    {
                        case 'T':
                            _transactionStatus = PgTransactionStatus.Active;
                            break;

                        case 'E':
                            _transactionStatus = PgTransactionStatus.Broken;
                            break;

                        case 'I':
                        default:
                            _transactionStatus = PgTransactionStatus.Default;
                            break;
                    }
                    break;

                case PgBackendCodes.NOTIFICATION_RESPONSE:
                    Console.WriteLine("PgBackendCodes.NOTIFICATION_RESPONSE");
                    HandleNotificationMessage(packet);
                    break;

                case PgBackendCodes.NOTICE_RESPONSE:
                case PgBackendCodes.ERROR_RESPONSE:
                    HandleErrorMessage(packet);
                    break;
            }

            return packet;
        }

        internal void Send(PgOutputPacket packet) => _channel.WritePacket(packet);

        private void SendStartupPacket()
        {
            // Send Startup message
            var packet = CreateOutputPacket(PgFrontEndCodes.UNTYPED);

            // user name
            packet.Write(PgCodes.PROTOCOL_VERSION3);
            packet.WriteNullString("user");
            packet.WriteNullString(_connectionOptions.UserID);

            // database
            if (!String.IsNullOrEmpty(_connectionOptions.Database))
            {
                packet.WriteNullString("database");
                packet.WriteNullString(_connectionOptions.Database);
            }

            // select ISO date style
            packet.WriteNullString("DateStyle");
            packet.WriteNullString(PgCodes.DATE_STYLE);

            // search path
            if (!String.IsNullOrEmpty(_connectionOptions.SearchPath))
            {
                packet.WriteNullString("search_path");
                packet.WriteNullString(_connectionOptions.SearchPath);
            }

            // Terminator
            packet.WriteByte(0);

            _channel.WritePacket(packet);

            // Read startup response
            PgInputPacket response = null;

            do
            {
                response = Read();

                HandlePacket(response);
            } while (!response.IsReadyForQuery);
            
            _open = true;
        }

        private void SendClearTextPasswordAuthentication(PgInputPacket packet)
        {
            var authPacket = CreateOutputPacket(PgFrontEndCodes.PASSWORD_MESSAGE);

            authPacket.WriteNullString(_connectionOptions.Password);

            _channel.WritePacket(authPacket);
        }

        private void SendPasswordAuthentication(PgInputPacket packet)
        {
            var authPacket = CreateOutputPacket(PgFrontEndCodes.PASSWORD_MESSAGE);

            var salt = packet.ReadBytes(4);
            var hash = MD5Authentication.EncryptPassword(salt, _connectionOptions.UserID, _connectionOptions.Password);
            authPacket.WriteNullString(hash);

            _channel.WritePacket(authPacket);
        }

        private void HandlePacket(PgInputPacket packet)
        {
            switch (packet.PacketType)
            {
                case PgBackendCodes.AUTHENTICATION:
                    HandleAuthPacket(packet);
                    break;

                case PgBackendCodes.BACKEND_KEY_DATA:
                    _handle    = packet.ReadInt32();
                    _secretKey = packet.ReadInt32();
                    break;

                case PgBackendCodes.PARAMETER_STATUS:
                    HandleParameterStatus(packet);
                    break;
            }
        }

        private void HandleAuthPacket(PgInputPacket packet)
        {
            // Authentication response
            int authType = packet.ReadInt32();

            switch (authType)
            {
                case PgCodes.AUTH_OK:
                    // Authentication successful
                    return;

                case PgCodes.AUTH_CLEARTEXT_PASSWORD:
                    SendClearTextPasswordAuthentication(packet);
                    break;

                case PgCodes.AUTH_MD5_PASSWORD:
                    // Read salt used when encrypting the password
                    SendPasswordAuthentication(packet);
                   break;

                default:
                    throw new NotSupportedException();

#warning TODO: Review & implement if needed
                // case PgCodes.AUTH_KERBEROS_V4:      // Kerberos V4 authentication is required
                // case PgCodes.AUTH_KERBEROS_V5:      // Kerberos V5 authentication is required
                // case PgCodes.AuthenticationGSS:
                //     // The frontend must now initiate a GSSAPI negotiation.
                //     // The frontend will send a PasswordMessage with the first part of the GSSAPI data stream in response to this.
                //     // If further messages are needed, the server will respond with AuthenticationGSSContinue.
                //     throw new NotSupportedException();
                //     break;

                // case PgCodes.AuthenticationSSPI:
                //     // The frontend must now initiate a SSPI negotiation.
                //     // The frontend will send a PasswordMessage with the first part of the SSPI data stream in response to this.
                //     // If further messages are needed, the server will respond with AuthenticationGSSContinue.
                //     throw new NotSupportedException();
                //     break;

                // case PgCodes.AuthenticationGSSContinue:
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

        private void HandleErrorMessage(PgInputPacket packet)
        {
            char   type     = ' ';
            string value    = String.Empty;
            string severity = null;
            string message  = null;
            string code     = null;
            string detail   = null;
            string hint     = null;
            string where    = null;
            string position = null;
            string file     = null;
            int    line     = 0;
            string routine  = null;

            while (type != PgErrorCodes.END)
            {
                type  = packet.ReadChar();
                value = packet.ReadNullString();

                switch (type)
                {
                    case PgErrorCodes.SEVERITY:
                        severity = value;
                        break;

                    case PgErrorCodes.CODE:
                        code = value;
                        break;

                    case PgErrorCodes.MESSAGE:
                        message = value;
                        break;

                    case PgErrorCodes.DETAIL:
                        detail = value;
                        break;

                    case PgErrorCodes.HINT:
                        hint = value;
                        break;

                    case PgErrorCodes.POSITION:
                        position = value;
                        break;

                    case PgErrorCodes.WHERE:
                        where = value;
                        break;

                    case PgErrorCodes.FILE:
                        file = value;
                        break;

                    case PgErrorCodes.LINE:
                        line = Convert.ToInt32(value);
                        break;

                    case PgErrorCodes.ROUTINE:
                        routine = value;
                        break;
                }
            }
            
            var error = new PgError(severity
                                  , message
                                  , code
                                  , detail
                                  , hint
                                  , where
                                  , position
                                  , file
                                  , line
                                  , routine);

            var exception = new PgException(error.Message, error);
            
            InfoMessage?.Invoke(exception);
            
            if (error.Severity == PgCodes.ERROR_SEVERITY
             || error.Severity == PgCodes.FATAL_SEVERITY
             || error.Severity == PgCodes.PANIC_SEVERITY)
            {
                Sync();

                throw exception;
            }
        }

        private void HandleNotificationMessage(PgInputPacket packet)
        {
            var processId  = packet.ReadInt32();
            var condition  = packet.ReadNullString();
            var additional = packet.ReadNullString();

            Notification?.Invoke(processId, condition, additional);
        }

        private void HandleParameterStatus(PgInputPacket packet)
            => _sessionData.SetValue(packet.ReadNullString(), packet.ReadNullString());

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
