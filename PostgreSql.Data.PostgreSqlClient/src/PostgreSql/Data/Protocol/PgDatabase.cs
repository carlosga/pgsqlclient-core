// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol.Authentication;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Security;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgDatabase
        : IDisposable
    {
        private PgNetworkStream     _stream;
        private PgConnectionOptions _connectionOptions;
        private PgServerConfig      _serverConfiguration;
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

        internal RemoteCertificateValidationCallback UserCertificateValidationCallback
        {
            get;
            set;
        }

        internal LocalCertificateSelectionCallback UserCertificateSelectionCallback
        {
            get;
            set;
        }
        
        internal PgServerConfig      ServerConfiguration => _serverConfiguration;
        internal PgConnectionOptions ConnectionOptions   => _connectionOptions;
        internal PgTransactionStatus TransactionStatus   => _transactionStatus;
        
        internal PgDatabase(string connectionString)
        {
            _connectionOptions = new PgConnectionOptions(connectionString);
            _stream            = new PgNetworkStream();
        }

        #region IDisposable Support

        private bool _disposedValue = false; // To detect redundant calls

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgDatabase()
        // {
        //     // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //     Dispose(false);
        // }

        private void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Close();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }

        #endregion
       
        internal void Open()
        {
            try
            {
                // Reset instance data
                _authenticated       = false;
                _serverConfiguration = new PgServerConfig();

                _stream.Open(_connectionOptions.DataSource
                           , _connectionOptions.PortNumber
                           , _connectionOptions.Encrypt);
                                                                            
                // Send startup packet
                SendStartupPacket();
                
                // Read startup response
                PgInputPacket response = null;
                               
                do 
                {
                    response = Read();
                    
                    HandlePacket(response);
                } while (!response.IsReadyForQuery);
            }
            catch (IOException ex)
            {
                Close();
                
                throw new PgClientException(ex.Message);
            }
            catch (PgClientException)
            {
                Close();
                 
                throw;
            }
        }

        internal void Close()
        {
            try
            {
                _stream.Close();
            }
            catch
            {
            }
            finally
            {
                _connectionOptions   = null;
                _serverConfiguration = null;
                _transactionStatus   = PgTransactionStatus.Default;
                _handle              = -1;
                _secretKey           = -1;
                _stream              = null;
                _authenticated       = false;

                // Remove info message callback
                InfoMessage = null;

                // Remove notification callback
                Notification = null;
            }
        }

        internal PgTransactionInternal CreateTransaction(IsolationLevel isolationLevel) 
            => new PgTransactionInternal(this, isolationLevel);                        

        internal PgStatement CreateStatement() => new PgStatement(this);

        internal PgStatement CreateStatement(string stmtText) => new PgStatement(this, stmtText);

        internal PgStatement CreateStatement(string parseName, string portalName) => new PgStatement(this, parseName, portalName);

        internal PgStatement CreateStatement(string parseName, string portalName, string stmtText)
            => new PgStatement(this, parseName, portalName, stmtText);

        internal void Flush() => _stream.WritePacket(PgFrontEndCodes.FLUSH);

        internal void Sync()
        {
            if (!_authenticated)
            {
                return; 
            }          
              
            _stream.WritePacket(PgFrontEndCodes.SYNC);
            
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
            _stream.WritePacket(packet);
        }
        
        internal PgOutputPacket CreateOutputPacket(char type) => new PgOutputPacket(type, _serverConfiguration);
        
        internal PgInputPacket Read()
        {
            var packet = _stream.ReadPacket(_serverConfiguration);
            
            if (packet.Message == PgBackendCodes.READY_FOR_QUERY)
            {
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
            }
            else if (packet.Message == PgBackendCodes.ERROR_RESPONSE)
            {
                // Read the error message and trow the exception
                var ex = HandleErrorMessage(packet);

                // Perform a sync
                Sync();

                // Throw the PostgreSQL exception
                throw ex;
            }
            
            return packet;
        }
       
        internal void Send(PgOutputPacket packet) => _stream.WritePacket(packet);

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
                    // First read salt to use when encrypting the password
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
            _stream.WritePacket(authPacket);            
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

            packet.Write(PgCodes.PROTOCOL_VERSION3);
            packet.WriteNullString("user");
            packet.WriteNullString(_connectionOptions.UserID);

            if (!String.IsNullOrEmpty(_connectionOptions.Database))
            {
                packet.WriteNullString("database");
                packet.WriteNullString(_connectionOptions.Database);
            }

            // Select ISO date style
            packet.WriteNullString("DateStyle");
            packet.WriteNullString(PgCodes.DATE_STYLE);

            if (!String.IsNullOrEmpty(_connectionOptions.SearchPath))
            {
                packet.WriteNullString("search_path");
                packet.WriteNullString(_connectionOptions.SearchPath);
            }

            // Terminator
            packet.WriteByte(0);

            _stream.WritePacket(packet);
                     
#warning TODO: look if it's worth to send any of these:
            // http://www.postgresql.org/docs/current/static/protocol-flow.html#PROTOCOL-ASYNC
            //
            // At present there is a hard-wired set of parameters for which ParameterStatus will be generated, they are:
            //
            // server_version
            // server_encoding
            // client_encoding
            // application_name
            // is_superuser
            // session_authorization
            // DateStyle
            // IntervalStyle
            // TimeZone
            // integer_datetimes
            // standard_conforming_strings
            // search_path
            // 
            // (server_encoding, TimeZone, and integer_datetimes were not reported by releases before 8.0; 
            //  standard_conforming_strings was not reported by releases before 8.1;
            //  IntervalStyle was not reported by releases before 8.4; 
            //  application_name was not reported by releases before 9.0.)
            // 
            // Note that server_version, server_encoding and integer_datetimes are pseudo-parameters that cannot change after startup.
            // This set might change in the future, or even become configurable. 
            // Accordingly, a frontend should simply ignore ParameterStatus for parameters that it does not understand or care about.                                           
        }

        private void HandleParameterStatus(PgInputPacket packet) 
            => _serverConfiguration.SetValue(packet.ReadNullString(), packet.ReadNullString());            
    }
}