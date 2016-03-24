// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol.Authentication;
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
        private SessionData         _sessionData;
        private int                 _handle;
        private int                 _secretKey;
        private PgTransactionStatus _transactionStatus;
        private bool                _authenticated;
        
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
        
        internal SessionData         ServerConfiguration => _sessionData;
        internal PgConnectionOptions ConnectionOptions   => _connectionOptions;
        internal PgTransactionStatus TransactionStatus   => _transactionStatus;
        
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
                _authenticated = false;
                _sessionData   = new SessionData();

                // Wire up SSL callbacks
                if (_connectionOptions.Encrypt)
                {
                    _channel.UserCertificateValidation = UserCertificateValidation;
                    _channel.UserCertificateSelection  = UserCertificateSelection;                    
                }
                
                // Open the channel
                _channel.Open(_connectionOptions.DataSource
                            , _connectionOptions.PortNumber
                            , _connectionOptions.Encrypt);
                        
                // Send startup packet
                SendStartupPacket();
            }
            catch (Exception)
            {
                Close();
                 
                throw;
            }
            finally
            {
                ReleaseLock();
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
                _authenticated     = false;

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
            if (!_authenticated)
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
            
            switch (packet.Message)
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
    
                case PgBackendCodes.ERROR_RESPONSE:               
                    // Read the error message and trow the exception
                    var ex = HandleErrorMessage(packet);

                    // Perform a sync
                    Sync();

                    // Throw the PostgreSQL exception
                    throw ex;
            }
            
            return packet;
        }
       
        internal void Send(PgOutputPacket packet) => _channel.WritePacket(packet);

        private void HandlePacket(PgInputPacket packet)
        {
            switch (packet.Message)
            {
                case PgBackendCodes.AUTHENTICATION:
                    HandleAuthPacket(packet);
                    break;

                case PgBackendCodes.BACKEND_KEY_DATA:
                    _handle    = packet.ReadInt32();
                    _secretKey = packet.ReadInt32();
                    break;

                case PgBackendCodes.NOTICE_RESPONSE:
                    // Read the notice message and raise an InfoMessage event
                    InfoMessage?.Invoke(HandleErrorMessage(packet));
                    break;

                case PgBackendCodes.NOTIFICATION_RESPONSE:
                    HandleNotificationMessage(packet);
                    break;

                case PgBackendCodes.PARAMETER_STATUS:
                    HandleParameterStatus(packet);
                    break;
            }
        }

        private void HandleAuthPacket(PgInputPacket packet)
        {
            // Authentication response
            int authType   = packet.ReadInt32();
            var authPacket = CreateOutputPacket(PgFrontEndCodes.PASSWORD_MESSAGE);

            switch (authType)
            {
                case PgCodes.AUTH_OK:
                    // Authentication successful
                    _authenticated = true;
                    return;

                case PgCodes.AUTH_CLEARTEXT_PASSWORD:
                    authPacket.WriteNullString(_connectionOptions.Password);
                    break;
                   
                case PgCodes.AUTH_MD5_PASSWORD:
                    // Read salt used when encrypting the password
                    var salt = packet.ReadBytes(4);
                    var hash = MD5Authentication.EncryptPassword(salt, _connectionOptions.UserID, _connectionOptions.Password);
                    authPacket.WriteNullString(hash);             
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
            
            // Send the packet to the server
            _channel.WritePacket(authPacket);            
        }

        private PgClientException HandleErrorMessage(PgInputPacket packet)
        {
            char   type  = ' ';
            string value = String.Empty;
            var    error = new PgClientError();

            while (type != PgErrorCodes.END)
            {
                type  = packet.ReadChar();
                value = packet.ReadNullString();
                
                switch (type)
                {
                    case PgErrorCodes.SEVERITY:
                        error.Severity = value;
                        break;

                    case PgErrorCodes.CODE:
                        error.Code = value;
                        break;

                    case PgErrorCodes.MESSAGE:
                        error.Message = value;
                        break;

                    case PgErrorCodes.DETAIL:
                        error.Detail = value;
                        break;

                    case PgErrorCodes.HINT:
                        error.Hint = value;
                        break;

                    case PgErrorCodes.POSITION:
                        error.Position = value;
                        break;

                    case PgErrorCodes.WHERE:
                        error.Where = value;
                        break;

                    case PgErrorCodes.FILE:
                        error.File = value;
                        break;

                    case PgErrorCodes.LINE:
                        error.Line = Convert.ToInt32(value);
                        break;

                    case PgErrorCodes.ROUTINE:
                        error.Routine = value;
                        break;
                }
            }

            return new PgClientException(error.Message, error);
        }

        private void HandleNotificationMessage(PgInputPacket packet)
        {
            var processId  = packet.ReadInt32();
            var condition  = packet.ReadNullString();
            var additional = packet.ReadNullString();

            Notification?.Invoke(processId, condition, additional);
        }
        
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
        }

        private void HandleParameterStatus(PgInputPacket packet) 
            => _sessionData.SetValue(packet.ReadNullString(), packet.ReadNullString());            
    }
}
