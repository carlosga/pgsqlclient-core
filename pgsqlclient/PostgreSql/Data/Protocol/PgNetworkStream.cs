// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgNetworkStream
        : IDisposable
    {
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

        private static readonly byte[] s_buffer = new byte[0];

        private Socket        _socket;
        private NetworkStream _networkStream;
        private SslStream     _secureStream;
        private Stream        _stream;
        private byte[]        _buffer;        

        private SemaphoreSlim _asyncActiveSemaphore;

        internal SemaphoreSlim LazyEnsureAsyncActiveSemaphoreInitialized()
        {
            // Lazily-initialize _asyncActiveSemaphore.  As we're never accessing the SemaphoreSlim's
            // WaitHandle, we don't need to worry about Disposing it.
            return LazyInitializer.EnsureInitialized(ref _asyncActiveSemaphore, () => new SemaphoreSlim(1, 1));
        }
                
        internal PgNetworkStream()
        {
            _buffer = new byte[8];
        }

        internal Task OpenAsync(string host, int portNumber, bool secureConnection)
        {
            return OpenAsync(host, portNumber, secureConnection, CancellationToken.None);
        }

        internal async Task OpenAsync(string            host
                                    , int               portNumber
                                    , bool              secureConnection
                                    , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }
                            
            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            await sem.WaitAsync().ConfigureAwait(false);
                        
            try
            {
                await ConnectAsync(host, portNumber, cancellationToken).ConfigureAwait(false);

                if (secureConnection)
                {
                    bool secured = await OpenSecureChannelAsync(host).ConfigureAwait(false);
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
            catch (Exception ex)
            {
                Detach();

                return Task.FromException(ex);
            }
            finally
            {
                sem.Release();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
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

        internal Task CloseAsync()
        {
            return CloseAsync(CancellationToken.None);
        }

        internal async Task CloseAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            await sem.WaitAsync().ConfigureAwait(false);

            try 
            {
                // Notify the server that we are closing the connection.
                await WritePacketAsync(PgFrontEndCodes.TERMINATE, cancellationToken).ConfigureAwait(false);

                // Close socket, network stream, ...
                Detach();
            }
            finally
            {
                sem.Release();
            }
        }

        internal Task<PgInputPacket> ReadPacketAsync(PgServerConfig serverConfig)
        {
            return ReadPacketAsync(serverConfig, CancellationToken.None);
        }
        
        internal async Task<PgInputPacket> ReadPacketAsync(PgServerConfig    serverConfig
                                                         , CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled<PgInputPacket>(cancellationToken);   
            }
             
            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            await sem.WaitAsync().ConfigureAwait(false);

            try
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
                    received += await _stream.ReadAsync(buffer, received, length - received, cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return Task.FromCanceled<PgInputPacket>(cancellationToken);
                    }
                }

                return new PgInputPacket(type, buffer, serverConfig);
            }
            finally
            {
                sem.Release();
            }            
        }

        internal Task WritePacketAsync(char type)
        {
            return WritePacketAsync(type, CancellationToken.None);
        }

        internal Task WritePacketAsync(char type, CancellationToken cancellationToken)
        {
            return WritePacketAsync(type, s_buffer, cancellationToken);
        }

        internal Task WritePacketAsync(char type, PgOutputPacket packet)
        {
            return WritePacketAsync(type, packet.ToArray(), CancellationToken.None);
        }

        internal async Task WritePacketAsync(char type, PgOutputPacket packet, CancellationToken cancellationToken)
        {
           await WritePacketAsync(type, packet.ToArray(), cancellationToken).ConfigureAwait(false);
        }

        internal async Task WritePacketAsync(char type, byte[] buffer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            SemaphoreSlim sem = LazyEnsureAsyncActiveSemaphoreInitialized();
            await sem.WaitAsync().ConfigureAwait(false);

            try
            {
                // Write packet Type
                _stream.WriteByte((byte)type);

                // Write packet length
                Write(((buffer == null) ? 4 : buffer.Length + 4));

                // Write packet contents
                if (buffer != null && buffer.Length > 0)
                {
                    await _stream.WriteAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
                }
            }
            finally
            {
                sem.Release();
            }
        }

        private int ReadInt32()
        {
            _stream.Read(_buffer, 0, 4);

            return (_buffer[3])
                 | (_buffer[2] <<  8)
                 | (_buffer[1] << 16)
                 | (_buffer[0] << 24);
        }

        public void Write(int value)
        {
            _buffer[0] = (byte)(value >> 24);
            _buffer[1] = (byte)(value >> 16);
            _buffer[2] = (byte)(value >> 8);
            _buffer[3] = (byte)(value);

            _stream.Write(_buffer, 0, 4);
        }

        private async Task ConnectAsync(string host, int portNumber, CancellationToken cancellationToken)
        {
            var remoteAddress = await GetIPAddressAsync(host, AddressFamily.InterNetwork).ConfigureAwait(false);
            var remoteEP      = new IPEndPoint(remoteAddress, portNumber);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            // Set Receive Buffer size.
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, 8192);

            // Set Send Buffer size.
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, 8192);

            // Disables the Nagle algorithm for send coalescing.
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, 1);

            await _socket.ConnectAsync(remoteEP, cancellationToken).ConfigureAwait(false);
           
            if (!cancellationToken.IsCancellationRequested) 
            {
                // Set the nework stream
                _networkStream = new NetworkStream(_socket, true);
            }
        }

        private async Task<bool> OpenSecureChannelAsync(string host)
        {
            bool request = RequestSecureChannel();

            if (request)
            {
                _secureStream = new SslStream(_networkStream
                                             , false
                                             , UserCertificateValidationCallback
                                             , UserCertificateSelectionCallback);

                await _secureStream.AuthenticateAsClientAsync(host);

                return true;
            }

            return false;
        }

        internal bool RequestSecureChannel()
        {
            Write(PgCodes.SSL_REQUEST);

            return (ReadChar() == 'S');
        }

        private async Task<IPAddress> GetIPAddressAsync(string dataSource, AddressFamily addressFamily)
        {
            IPAddress[] addresses = await Dns.GetHostAddressesAsync(dataSource).ConfigureAwait(false);

            return addresses.FirstOrDefault(a => a.AddressFamily == addressFamily) ?? addresses[0];
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
        }
    }
}