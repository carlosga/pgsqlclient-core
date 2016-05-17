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
        private byte          _pendingMessage;
        private int           _packetSize;

        internal Transport()
        {
            _buffer = new byte[4];
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
                _buffer         = null;
                _pendingMessage = 0;
                _packetSize     = 0;

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

        internal void Open(string host, int port, int connectTimeout, int packetSize, bool secureChannel)
        {
            try
            {
                Connect(host, port, connectTimeout, packetSize);

                if (secureChannel)
                {
                    if (!OpenSecureChannel(host))
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
            catch (Exception)
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
                WriteMessage(FrontendMessages.Terminate);
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
                return _socket.Connected && _socket.Poll(1000, SelectMode.SelectRead);
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

        internal MessageReader ReadMessage(SessionData sessionData)
        {
            if (_pendingMessage != 0)
            {
                var mtype = _pendingMessage;

                _pendingMessage = 0;

                return new MessageReader(mtype, ReadFrame(), sessionData);
            }

            byte type = (byte)_reader.ReadByte();

            if (type == BackendMessages.DataRow)
            {
                var frame = new byte[_packetSize];
                var count = 0;

                while (type == BackendMessages.DataRow)
                {
                    count += ReadFrame(ref frame, count, ReadFrameLength());
                    type   = (byte)_reader.ReadByte();
                }

                if (frame.Length > count)
                {
                    Array.Resize<byte>(ref frame, count);
                }

                _pendingMessage = type;

                return new MessageReader(BackendMessages.DataRow, frame, sessionData);
            }

            _pendingMessage = 0;

            return new MessageReader(type, ReadFrame(), sessionData);
        }
        
        private unsafe int ReadFrameLength()
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

        private byte[] ReadFrame()
        {
            int    count = ReadFrameLength();
            byte[] frame = new byte[count];

            ReadFrame(ref frame, 0, count);

            return frame;
        }

        private int ReadFrame(ref byte[] frame, int offset, int count)
        {
            if (count == 0)
            {
                return 0;
            }
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

        internal void WriteMessage(byte type)
        {
            _writer.WriteByte(type);
            Write(4);
        }

        internal void WriteMessage(MessageWriter message)
        {
            message.WriteTo(_writer);
        }

        private void Write(int value)
        {
            _buffer[0] = (byte)((value >> 24) & 0xFF);
            _buffer[1] = (byte)((value >> 16) & 0xFF);
            _buffer[2] = (byte)((value >>  8) & 0xFF);
            _buffer[3] = (byte)((value      ) & 0xFF);

            _writer.Write(_buffer, 0, 4);
        }

        private void Connect(string host, int port, int connectTimeout, int packetSize)
        {
            // Obtain the IP addresses for the specified host
            var task = Dns.GetHostAddressesAsync(host);
            task.Wait();

            var remoteAddresses = task.Result;

            // Try to connect on each IP address until one succeeds
            for (int i = 0; i < remoteAddresses.Length; ++i)
            {
                if (remoteAddresses[i].AddressFamily == AddressFamily.InterNetwork      // Address for IP version 4.
                 || remoteAddresses[i].AddressFamily == AddressFamily.InterNetworkV6)   // Address for IP version 6.
                {
                    _socket = TryConnect(remoteAddresses[i], port, connectTimeout, packetSize);
                }

                if (_socket != null)
                {
                    break;
                }
            }

            if (_socket == null)
            {
                throw new PgException($"No valid IP addresses found for the given host {host}:{port}.");
            }

            // Set the nework stream
            _networkStream = new NetworkStream(_socket, true);
        }

        private Socket TryConnect(IPAddress address, int port, int connectTimeout, int packetSize)
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

                // Connect to the host
                var complete = new ManualResetEvent(false);
                var args     = new SocketAsyncEventArgs { RemoteEndPoint = remoteEP, UserToken = complete };

                args.Completed += (object sender, SocketAsyncEventArgs saeargs) =>
                {
                    var mre = saeargs.UserToken as ManualResetEvent;
                    mre.Set();
                };

                var result = socket.ConnectAsync(args);

                complete.WaitOne(connectTimeout * 1000);

                if (!result || !socket.Connected || args.SocketError != SocketError.Success)
                {
                    complete.Reset();
                    Socket.CancelConnectAsync(args);
                    complete.WaitOne();

                    throw new PgException("Timeout expired. The timeout period elapsed prior to completion of the operation or the server is not responding.");
                }
            }
            catch
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Dispose();
                socket = null;
            }

            return socket;
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

        internal bool RequestSecureChannel()
        {
            Write(SslRequest);

            return ((char)_reader.ReadByte() == 'S');
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
