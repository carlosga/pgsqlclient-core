// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Buffers;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Transport
        : IDisposable
    {
        // SSL Request code
        private const int SslRequestHi = 1234;
        private const int SslRequestLo = 5679;
        private const int SslRequest   = (SslRequestHi << 16) | SslRequestLo;

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
        private Stream        _reader;
        private Stream        _writer;
        private byte[]        _buffer;
        private int           _packetSize;

        internal int PacketSize => _packetSize;

        internal Transport()
        {
            _buffer = ArrayPool<byte>.Shared.Rent(4);
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Detach();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _packetSize = 0;
                _disposed   = true;

                ArrayPool<byte>.Shared.Return(_buffer, true);
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        internal void Open(string host, int port, int connectTimeout, int packetSize, bool secureChannel)
        {
            try
            {
                Connect(host, port, connectTimeout, packetSize);

                if (secureChannel)
                {
                    if (!TryOpenSecureChannel(host))
                    {
                        throw new PgException("Cannot open a secure connection against PostgreSQL server.");
                    }

                    _reader = new BufferedStream(_secureStream, packetSize);
                    _writer = _secureStream;
                }
                else
                {
                    _reader = new BufferedStream(_networkStream, packetSize);
                    _writer = _networkStream;
                }

                _packetSize = packetSize;
            }
            catch (PgException)
            {
                Detach();
                throw;
            }
            catch
            {
                Detach();
                throw new PgException("A network-related or instance-specific error occurred while establishing a connection to PostgreSQL."
                                    + " The server was not found or was not accessible."
                                    + " Verify that the server name is correct and that PostgreSQL is configured to allow remote connections.");
            }
        }

        internal void Close()
        {
            try
            {
                // Notify the server that we are closing the connection.
                WriteFrame(FrontendMessages.Terminate);
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

        internal bool IsTransportAlive(bool throwOnException = false)
        {
            try
            {
                return _socket.Connected && !_socket.Poll(0, SelectMode.SelectError);
            }
            catch
            {
                if (throwOnException)
                {
                    throw;
                }
                return false;
            }
        }

        internal byte ReadByte()
        {
            _reader.Read(_buffer, 0, 1);
            return _buffer[0];
        }

        internal unsafe int ReadInt32()
        {
            _reader.Read(_buffer, 0, 4);

            fixed (byte* pbuffer = &_buffer[0])
            {
                return ((*(pbuffer + 3) & 0xFF)
                      | (*(pbuffer + 2) & 0xFF) <<  8
                      | (*(pbuffer + 1) & 0xFF) << 16
                      | (*(pbuffer    ) & 0xFF) << 24) - 4;
            }
        }

        internal int ReadFrame(ref byte[] frame, int offset = 0)
        {
            int count = ReadInt32();
            int read  = 0;
            int total = 0;
            if ((offset + count) > frame.Length)
            {
                Array.Resize<byte>(ref frame, (offset + count) * 2);
            }
            do
            {
                read = _reader.Read(frame, offset, count);
                if (read == 0)
                {
                    break;
                }
                offset += read;
                total  += read;
                count  -= read;
            } while (count > 0);
            return total;
        }

        internal unsafe void WriteInt32(int value)
        {
            fixed (byte* pbuffer = _buffer)
            {
                *(pbuffer)     = (byte)((value >> 24) & 0xFF);
                *(pbuffer + 1) = (byte)((value >> 16) & 0xFF);
                *(pbuffer + 2) = (byte)((value >>  8) & 0xFF);
                *(pbuffer + 3) = (byte)((value      ) & 0xFF);
            }

            _writer.Write(_buffer, 0, 4);
        }

        internal void WriteFrame(byte[] frame, int offset, int length)
        {
            _writer.Write(frame, offset, length);
        }

        internal void WriteFrame(byte type)
        {
            _writer.WriteByte(type);
            WriteInt32(4);
        }

        private void Connect(string host, int port, int connectTimeout, int packetSize)
        {
            var tokenSource     = new CancellationTokenSource();
            var cancelToken     = tokenSource.Token;
            var remoteAddresses = Dns.GetHostAddressesAsync(host).GetAwaiter().GetResult();

            var task = Task.Factory.StartNew<Socket>(() => {
                // Try to connect on each IP address until one succeeds
                for (int i = 0; i < remoteAddresses.Length; ++i)
                {
                    Socket socket = null;

                    if (remoteAddresses[i].AddressFamily == AddressFamily.InterNetwork      // Address for IP version 4.
                     || remoteAddresses[i].AddressFamily == AddressFamily.InterNetworkV6)   // Address for IP version 6.
                    {
                        socket = TryConnect(remoteAddresses[i], port, packetSize, cancelToken);
                    }

                    cancelToken.ThrowIfCancellationRequested();

                    if (socket != null)
                    {
                        return socket;
                    }
                }

                return null;
            }, cancelToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

            if (!task.Wait(connectTimeout * 1000, cancelToken))
            {
                tokenSource.Cancel();
                throw new PgException("Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.");
            }
            else
            {
                var socket = task.Result;
                if (socket == null || socket.Poll(0, SelectMode.SelectError))
                {
                    throw new PgException($"No valid IP addresses found for the given host {host}:{port}.");
                }
                _socket = socket;
            }

            tokenSource.Dispose();

            // Set the nework stream
            _networkStream = new NetworkStream(_socket, true);
        }

        private Socket TryConnect(IPAddress address, int port, int packetSize, CancellationToken cancelToken)
        {
            IPEndPoint remoteEP = new IPEndPoint(address, port);
            Socket     socket   = null;

            try
            {
                socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Set Receive Buffer size.
                socket.ReceiveBufferSize = packetSize;

                // Set Send Buffer size.
                socket.SendBufferSize = packetSize;

                // Nagle algorithm
                socket.NoDelay = true;

                // Before attempt to connect check if cancellation has been requested
                cancelToken.ThrowIfCancellationRequested();

                // Connect to the host
                socket.Connect(remoteEP);
            }
            catch
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
                socket = null;
            }

            return socket;
        }

        private bool TryOpenSecureChannel(string host)
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

        private bool RequestSecureChannel()
        {
            WriteInt32(SslRequest);

            return (_reader.ReadByte() == (byte)'S');
        }

        private void Detach()
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }
            if (_writer != null)
            {
                _writer.Dispose();
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

            _reader         = null;
            _writer         = null;
            _secureStream   = null;
            _networkStream  = null;
            _socket         = null;

            UserCertificateValidation = null;
            UserCertificateSelection  = null;
        }
    }
}
