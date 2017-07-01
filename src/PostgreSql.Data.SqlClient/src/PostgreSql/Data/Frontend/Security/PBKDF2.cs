// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// --------------------------------------------------------------------------------------------------
// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using System.Diagnostics;
using System;
using System.Data.Common;
using System.Security.Cryptography;

namespace PostgreSql.Data.Frontend.Security
{
    /// <summary>
    /// Based on CoreFX Rfc2898DeriveBytes ( netstandard 1.6 ) SHA-1 only implementation
    /// </summary>
    internal sealed class PBKDF2
        : IDisposable
    {
        private const int MinimumSaltSize = 8;

        private readonly byte[] _password;
        private byte[]          _salt;
        private uint            _iterations;
        private HMACSHA256      _hmac;

        private byte[] _buffer;
        private uint   _block;
        private int    _startIndex;
        private int    _endIndex;
        private int    _blockSize;
        
        internal PBKDF2(byte[] password, byte[] salt, int iterations)
        {
            if (salt == null)
            {
                throw ADP.ArgumentNull(nameof(salt));
            }
            if (salt.Length < MinimumSaltSize)
            {
                throw ADP.Argument(nameof(salt));
            }
            if (iterations <= 0)
            {
                throw ADP.ArgumentOutOfRange(nameof(iterations));
            }
            if (password == null)
            {
                throw ADP.ArgumentNull(nameof(password));
            }

            _salt       = CryptographicBuffer.Clone(salt);
            _iterations = (uint)iterations;
            _password   = CryptographicBuffer.Clone(password);
            _hmac       = new HMACSHA256(_password);
            _blockSize  = _hmac.HashSize >> 3;

            Initialize();
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _hmac?.Dispose();
                    _hmac = null;
                    if (_buffer != null)
                    {
                        Array.Clear(_buffer, 0, _buffer.Length);
                    }
                    if (_password != null)
                    {
                        Array.Clear(_password, 0, _password.Length);
                    }
                    if (_salt != null)
                    {
                        Array.Clear(_salt, 0, _salt.Length);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        #endregion        

        internal byte[] GetBytes(int cb)
        {
            if (cb <= 0)
            {
                throw ADP.ArgumentOutOfRange(nameof(cb));
            }

            byte[] password = new byte[cb];

            int offset = 0;
            int size   = _endIndex - _startIndex;

            if (size > 0)
            {
                if (cb >= size)
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, size);
                    _startIndex = 0;
                    _endIndex   = 0;
                    offset     += size;
                }
                else
                {
                    Buffer.BlockCopy(_buffer, _startIndex, password, 0, cb);
                    _startIndex += cb;
                    return password;
                }
            }

            Debug.Assert(_startIndex == 0 && _endIndex == 0, "Invalid start or end index in the internal buffer.");

            while (offset < cb)
            {
                byte[] T_block = Hi();
                int remainder = cb - offset;
                if (remainder > _blockSize)
                {
                    Buffer.BlockCopy(T_block, 0, password, offset, _blockSize);
                    offset += _blockSize;
                }
                else
                {
                    Buffer.BlockCopy(T_block, 0, password, offset, remainder);
                    offset += remainder;
                    Buffer.BlockCopy(T_block, remainder, _buffer, _startIndex, _blockSize - remainder);
                    _endIndex += (_blockSize - remainder);
                    return password;
                }
            }
            
            return password;
        }

        private void Initialize()
        {
            if (_buffer != null)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
            }
            _buffer     = new byte[_blockSize];
            _block      = 1;
            _startIndex = 0;
            _endIndex   = 0;
        }

        // This function is defined as follows:
        // Hi (S, i) = HMAC(S || i) | HMAC2(S || i) | ... | HMAC(iterations) (S || i) 
        // where i is the block number.
        private unsafe byte[] Hi()
        {
            byte[] temp = new byte[_salt.Length + sizeof(uint)];
            Buffer.BlockCopy(_salt, 0, temp, 0, _salt.Length);

            fixed (byte* pbuffer = &temp[_salt.Length])
            {
                *(pbuffer)     = (byte)((_block >> 24) & 0xFF);
                *(pbuffer + 1) = (byte)((_block >> 16) & 0xFF);
                *(pbuffer + 2) = (byte)((_block >>  8) & 0xFF);
                *(pbuffer + 3) = (byte)((_block      ) & 0xFF);
            }

            temp = _hmac.ComputeHash(temp);
            
            byte[] ret = temp;
            for (int i = 2; i <= _iterations; i++)
            {
                temp = _hmac.ComputeHash(temp);
                ret  = CryptographicBuffer.Xor(ret, temp);
            }

            _block++;

            return ret;
        }
    }
}
