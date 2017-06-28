// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using PostgreSql.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Buffers;

namespace PostgreSql.Data.Frontend
{
    internal sealed partial class MessageReader
        : ITypeReader
    {
        private byte        _messageType;
        private byte[]      _buffer;
        private int         _position;
        private int         _length;
        private int         _capacity;
        private byte        _pendingMessage;
        private SessionData _sessionData;

        internal byte MessageType       => _messageType;
        internal int  Length            => _length;
        internal int  Position          => _position;
        internal bool IsRowDescription  => (_messageType == BackendMessages.RowDescription);
        internal bool IsEmptyQuery      => (_messageType == BackendMessages.EmptyQueryResponse);
        internal bool IsNoData          => (_messageType == BackendMessages.NoData);
        internal bool IsPortalSuspended => (_messageType == BackendMessages.PortalSuspended);
        internal bool IsCommandComplete => (_messageType == BackendMessages.CommandComplete);
        internal bool IsCloseComplete   => (_messageType == BackendMessages.CloseComplete);
        internal bool IsReadyForQuery   => (_messageType == BackendMessages.ReadyForQuery);

        internal MessageReader(SessionData sessionData)
        {
            _sessionData = sessionData;
            _capacity    = sessionData.ConnectionOptions.PacketSize;
            _buffer      = ArrayPool<byte>.Shared.Rent(_capacity);
        }

        internal void Clear()
        {
            _messageType    = 0;
            _position       = 0;
            _length         = 0;
            _capacity       = 0;
            _pendingMessage = 0;
            _sessionData    = null;

            ArrayPool<byte>.Shared.Return(_buffer, true);
        }

        internal byte[] ReadBytes(int count)
        {
            byte[] buffer = new byte[count];

            Buffer.BlockCopy(_buffer, _position, buffer, 0, count);

            _position += count;

            return buffer;
        }

        internal string ReadNullString()
        {
            int start = _position;

            while (_position < _length && _buffer[_position] != 0) 
            { 
                ++_position;
            }

            int count = _position - start;

            if (_position < _buffer.Length)
            {
                ++_position;
            }

            return (count == 0) ? string.Empty : _sessionData.ClientEncoding.GetString(_buffer, start, count);
        }

        internal void ReadFrom(Transport transport)
        {
            _position = 0;

            if (_pendingMessage != 0)
            {
                _messageType    = _pendingMessage;
                _pendingMessage = 0;

                if (_buffer.Length > (_capacity * 2))
                {
                    PooledBuffer.Resize(ref _buffer, _capacity);
                    // Array.Resize<byte>(ref _buffer, _capacity);
                }
            }
            else
            {
                _messageType = transport.ReadByte();
            }

            if (_messageType == BackendMessages.DataRow)
            {
                _pendingMessage = _messageType;
                _length         = 0;

                do
                {
                    _length        += transport.ReadFrame(ref _buffer, _length);
                    _pendingMessage = transport.ReadByte();
                } while (_pendingMessage == _messageType);
            }
            else
            {
                _length = transport.ReadFrame(ref _buffer);
            }
        }

        internal object ReadValue(TypeInfo typeInfo)
        {
            return ReadValue(typeInfo, ReadInt32());
        }

        private object ReadValue(TypeInfo typeInfo, int length)
        {
            if (length == -1)
            {
                return DBNull.Value;
            }

            Debug.Assert((_position + length) <= _length);

            switch (typeInfo.PgDbType)
            {
            case PgDbType.Void:
                return DBNull.Value;

            case PgDbType.Bytea:
                return ReadBytes(length);

            case PgDbType.Char:
                return ReadString(length).TrimEnd();

            case PgDbType.VarChar:
            case PgDbType.Text:
                return ReadString(length);

            case PgDbType.Boolean:
                return ReadBoolean();

            case PgDbType.Byte:
                return ReadByte();

            case PgDbType.Numeric:
                return ReadNumeric();

            case PgDbType.Money:
                return ReadMoney();

            case PgDbType.Real:
                return ReadSingle();

            case PgDbType.Double:
                return ReadDouble();

            case PgDbType.SmallInt:
                return ReadInt16();

            case PgDbType.Integer:
                return ReadInt32();

            case PgDbType.BigInt:
                return ReadInt64();

            case PgDbType.Interval:
                return ReadInterval();

            case PgDbType.Date:
                return ReadDate();

            case PgDbType.Time:
                return ReadTime();

            case PgDbType.Timestamp:
                return ReadTimestamp();

            case PgDbType.TimeTZ:
                return ReadTimeWithTZ();

            case PgDbType.TimestampTZ:
                return ReadTimestampWithTZ();

            case PgDbType.Point:
                return ReadPoint();

            case PgDbType.Circle:
                return ReadCircle();

            case PgDbType.Line:
                return ReadLine();

            case PgDbType.LSeg:
                return ReadLSeg();

            case PgDbType.Polygon:
                return ReadPolygon();

            case PgDbType.Path:
                return ReadPath();

            case PgDbType.Box:
            case PgDbType.Box2D:
            case PgDbType.Box3D:
                return ReadBox();

            case PgDbType.Uuid:
                return ReadUuid();

            case PgDbType.IPAddress:
                return ReadIPAddress();

            case PgDbType.MacAddress:
                return ReadMacAddress();

            case PgDbType.Array:
                return ReadArray(typeInfo, length);

            case PgDbType.Vector:
                return ReadVector(typeInfo, length);

            case PgDbType.Composite:
                return ReadComposite(typeInfo, length);

            default:
                return ReadBytes(length);
            }
        }

        private char ReadChar()    => (char)_buffer[_position++];
        private bool ReadBoolean() => (ReadByte() == 1);

        private string ReadString(int count)
        {
            var data = _sessionData.ClientEncoding.GetString(_buffer, _position, count);

            _position += count;

            return data;
        }
    }
}
