// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgNetworkChannel
        : IDisposable
    {
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

        private Socket        _socket;
        private NetworkStream _networkStream;
        private SslStream     _secureStream;
        private Stream        _stream;
        private byte[]        _buffer;

        internal PgNetworkChannel()
        {
            _buffer = new byte[8];
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
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgNetworkStream() {
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
        
        internal void Open(string host, int port, bool secureChannel, int packetSize)
        {
            try
            {
                Connect(host, port, packetSize);

                if (secureChannel)
                {
                    bool secured = OpenSecureChannel(host);
                    if (!secured)
                    {
                        throw new PgClientException("Cannot open a secure connection against PostgreSQL server.");
                    }

                    _stream = _secureStream;
                }
                else
                {
                    _stream = _networkStream;
                }
            }
            catch (Exception)
            {
                Detach();

                throw new PgClientException("A network-related or instance-specific error occurred while establishing a connection to PostgreSQL."
                                          + " The server was not found or was not accessible."
                                          + " Verify that the server name is correct and that PostgreSQL is configured to allow remote connections.");
            }
        }
        
        internal void Close()
        {
            if (_stream == null)
            {
                return;
            }
            
            try
            {
                // Notify the server that we are closing the connection.
                WritePacket(PgFrontEndCodes.TERMINATE);
            }
            catch
            {
            }
            finally
            {
                // Close socket, network stream, ...
                Detach();                
            }
        }

        internal PgInputPacket ReadPacket(SessionData sessionData)
        {
            char type = (char)_stream.ReadByte();

            if (type == PgBackendCodes.EMPTY_QUERY_RESPONSE)
            {
                return null;
            }

            int    received = 0;
            int    length   = ReadInt32() - 4;
            byte[] buffer   = new byte[length];

            while (received < length)
            {
                received += _stream.Read(buffer, received, length - received);
            }

            return new PgInputPacket(type, buffer, sessionData);
        }

        internal void WritePacket(char type)
        {
            _stream.WriteByte((byte)type);
            Write(4);
        }

        internal void WritePacket(PgOutputPacket packet)
        {
            packet.WriteTo(_stream);
        }

        private int ReadInt32()
        {
            _stream.Read(_buffer, 0, 4);

            return (_buffer[3])
                 | (_buffer[2] <<  8)
                 | (_buffer[1] << 16)
                 | (_buffer[0] << 24);
        }

        private void Write(int value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value);

            _stream.Write(_buffer, 0, 4);
        }

        private void Connect(string host, int port, int packetSize)
        {
            var remoteAddress = Task.Run<IPAddress>(async () => {
                return await GetIPAddressAsync(host, AddressFamily.InterNetwork);
            });

            var remoteEP = new IPEndPoint(remoteAddress.Result, port);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set Receive Buffer size.
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, packetSize);

            // Set Send Buffer size.
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, packetSize);

            // Disables the Nagle algorithm.
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

            // Connect to the host
            _socket.Connect(remoteEP);

            // Set the nework stream
            _networkStream = new NetworkStream(_socket, true);
        }

        private bool OpenSecureChannel(string host)
        {
            bool request = RequestSecureChannel();

            if (request)
            {
                try
                {
                    _secureStream = new SslStream(_networkStream
                                                , false
                                                , UserCertificateValidation
                                                , UserCertificateSelection
                                                , EncryptionPolicy.RequireEncryption);

                    _secureStream.AuthenticateAsClientAsync(host, null, SslProtocols.Tls11 | SslProtocols.Tls12, true);

                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        private async Task<IPAddress> GetIPAddressAsync(string dataSource, AddressFamily addressFamily)
        {
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(dataSource).ConfigureAwait(false);

            return addresses.FirstOrDefault(a => a.AddressFamily == addressFamily) ?? addresses[0];
        }

        internal bool RequestSecureChannel()
        {
            Write(PgCodes.SSL_REQUEST);

            return ((char)_stream.ReadByte() == 'S');
        }

        private void Detach()
        {
            if (_stream != null)
            {
                _stream.Dispose();
            }
            if (_secureStream != null)
            {
                try
                {
                    _secureStream.Dispose();
                }
                catch
                {
                }
            }
            if (_networkStream != null)
            {
                _networkStream.Dispose();
            }
            if (_socket != null)
            {
                _socket.Dispose();
            }

            _stream         = null;
            _secureStream   = null;
            _networkStream  = null;
            _socket         = null;
            
            UserCertificateValidation = null;
            UserCertificateSelection  = null;            
        }
    }
}
