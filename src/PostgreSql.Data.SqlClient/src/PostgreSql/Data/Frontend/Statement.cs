// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.Common;
using System.Buffers;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Statement
        : IDisposable
    {
        private static readonly byte STATEMENT = (byte)'S';
        private static readonly byte PORTAL    = (byte)'P';

        private Connection            _connection;
        private string                _statementText;
        private string                _parsedStatementText;
        private string                _tag;
        private string                _portalName;
        private string                _statementName;
        private int                   _fetchSize;
        private int                   _recordsAffected;
        private bool                  _hasRows;
        private StatementState        _state;
        private PgParameterCollection _parameters;
        private CommandType           _commandType;
        private List<int>             _parameterIndices;
        private MessageWriter         _parseMessage;
        private MessageWriter         _describeMessage;
        private MessageWriter         _bindMessage;
        private MessageWriter         _executeMessage;
        private MessageWriter         _closeMessage;
        private MessageWriter         _queryMessage;
        private RowDescriptor         _rowDescriptor;
        private Queue<object[]>       _rows;

        internal bool           HasRows         => _hasRows;
        internal string         Tag             => _tag;
        internal int            RecordsAffected => _recordsAffected;
        internal RowDescriptor  RowDescriptor   => _rowDescriptor;

        internal bool IsPrepared   => _state == StatementState.Prepared || IsExecuting || IsSuspended;
        internal bool IsExecuting  => _state == StatementState.Executing || IsSuspended;
        internal bool IsSuspended  => _state == StatementState.Suspended;
        internal bool IsCancelling => _state == StatementState.Cancelling;
        internal bool IsCancelled  => _state == StatementState.Cancelled;

        internal Connection Connection => _connection;

        internal CommandType CommandType
        {
            get => _commandType;
            set => _commandType = value;
        }

        internal string StatementText
        {
            get => _statementText;
            set 
            {
                if (_statementText != value)
                {
                    _statementText       = value;
                    _parsedStatementText = null;
                }
            }
        }

        internal int FetchSize
        {
            get => _fetchSize;
            set
            {
                if (_fetchSize != value)
                {
                    if (IsExecuting || (_rows != null && _rows.Count > 0))
                    {
                        throw ADP.InvalidOperation("Fetch size cannot be changed while fetching rows.");
                    }
                    _fetchSize = value;
                    _rows      = new Queue<object[]>(_fetchSize);
                }
            } 
        }

        internal PgParameterCollection Parameters
        {
            get => _parameters;
            set => _parameters = value;
        }

        internal bool HasPendingRefCursors => (_rowDescriptor.Count == 1 && _rowDescriptor[0].TypeInfo.IsRefCursor);

        internal Statement(Connection connection)
            : this(connection, null)
        {
        }

        internal Statement(Connection connection, string statementText)
        {
            _connection       = connection;
            _commandType      = CommandType.Text;
            _state            = StatementState.Default;
            _parameters       = PgParameterCollection.Empty;
            _statementText    = statementText;
            _recordsAffected  = -1;
            _hasRows          = false;
            _rowDescriptor    = new RowDescriptor();
            _parseMessage     = new MessageWriter(FrontendMessages.Parse   , _connection.SessionData);
            _describeMessage  = new MessageWriter(FrontendMessages.Describe, _connection.SessionData);
            _bindMessage      = new MessageWriter(FrontendMessages.Bind    , _connection.SessionData);
            _executeMessage   = new MessageWriter(FrontendMessages.Execute , _connection.SessionData);
            _closeMessage     = new MessageWriter(FrontendMessages.Close   , _connection.SessionData);
            _queryMessage     = new MessageWriter(FrontendMessages.Query   , _connection.SessionData);
            _statementName    = Guid.NewGuid().ToString();
            _portalName       = Guid.NewGuid().ToString();

            FetchSize = 200;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseStatement();
                    ClearRows();

                    _rowDescriptor?.Clear();
                    _parameterIndices?.Clear();
                    _parseMessage?.Dispose();
                    _describeMessage?.Dispose();
                    _bindMessage?.Dispose();
                    _executeMessage?.Dispose();
                    _closeMessage?.Dispose();
                    _queryMessage?.Dispose();
                }

                _connection          = null;
                _statementText       = null; 
                _parsedStatementText = null;
                _tag                 = null;
                _portalName          = null;
                _statementName       = null;
                _parameters          = null;
                _parameterIndices    = null;
                _parseMessage        = null;
                _describeMessage     = null;
                _bindMessage         = null;
                _executeMessage      = null;
                _closeMessage        = null;
                _queryMessage        = null;
                _rowDescriptor       = null;
                _rows                = null;
                _hasRows             = false;
                _recordsAffected     = -1;
                _fetchSize           = 0;

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        internal void Prepare()
        {
            try
            {
                CloseStatement();

                _connection.Lock();

                Parse();
                DescribeStatement();

                ChangeState(StatementState.Prepared);
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal int ExecuteNonQuery()
        {
            try
            {
                _connection.Lock();

                ThrowIfCancelled();

                ChangeState(StatementState.Executing);

                Bind();
                Execute(CommandBehavior.SingleRow);
                SetOutputParameters();

                return _recordsAffected;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void ExecuteReader() => ExecuteReader(CommandBehavior.Default);

        internal void ExecuteReader(CommandBehavior behavior)
        {
            try
            {
                _connection.Lock();

                ThrowIfCancelled();

                ChangeState(StatementState.Executing);

                Bind();
                Execute(behavior);
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal object ExecuteScalar()
        {
            try
            {
                _connection.Lock();

                ThrowIfCancelled();

                ChangeState(StatementState.Executing);

                Bind();
                Execute(CommandBehavior.SingleResult);

                if (_rows.Count > 0)
                {
                    return _rows.Dequeue()[0];
                }

                return null;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void Query()
        {
            try
            {
                _connection.Lock();

                // Update Status
                ChangeState(StatementState.Executing);

                _queryMessage.Reset();
                _queryMessage.WriteNullString(_statementText);

                // Send message
                _connection.Send(_queryMessage);

                // Process response
                ReadUntilReadyForQuery();

                // Update status
                ChangeState(StatementState.Default);
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void Cancel()
        {
            if (IsExecuting)
            {
                ChangeState(StatementState.Cancelling);
                _connection.CancelRequest();
                CloseStatement();
                ChangeState(StatementState.Cancelled);
            }
        }

        internal object[] FetchRow()
        {
            if (IsCancelled)
            {
                return ((_rows.Count == 0) ? null : _rows.Dequeue());
            }

            if (IsSuspended && _rows.Count == 0)
            {
                Execute();  // Fetch next group of rows
            }

            if (_rows.Count != 0)
            {
                return _rows.Dequeue();
            }

            return null;
        }
        
        internal void CloseStatement()
        {
            if (_state == StatementState.Default)
            {
                return;
            }

            try
            {
                _connection.Lock();

                // Set values of output parameters
                SetOutputParameters();

                // Close current statement
                _closeMessage.Reset();
                _closeMessage.WriteByte(STATEMENT);
                _closeMessage.WriteNullString(_statementName);

                _connection.Send(_closeMessage, SyncMode.SyncAndFlush);

                // Read until IS READY FOR QUERY message is received
                ReadUntilReadyForQuery();

                // Reset records affected and statement tag
                _recordsAffected = -1;
                _tag             = null;

                // Reset the row descriptor
                _rowDescriptor.Clear();

                // Reset statement and portal names
                _statementName = null;
                _portalName    = null;

                // Clear remaing rows
                ClearRows();

                // Reset statement parameters
                _parameters       = null;
                _parameterIndices = null;

                // Reset command type
                _commandType = CommandType.Text;

                // Reset messages
                _parseMessage.Reset();
                _describeMessage.Reset();
                _bindMessage.Reset();
                _executeMessage.Reset();
                _closeMessage.Reset();
                _queryMessage.Reset();

                // Update Status
                ChangeState(StatementState.Default);
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal string GetPlan(bool verbose)
        {
            var stmtPlan = new StringBuilder();
            var stmtText = "EXPLAIN ANALYZE ";

            if (verbose)
            {
                stmtText += "VERBOSE ";
            }

            ThrowIfCancelled();

            var dataRow = new DataRow();

            using (var stmt = _connection.CreateStatement(stmtText))
            {
                stmt.Query();

                while (_rows.Count > 0)
                {
                    dataRow.ReadFrom(stmt);                                       
                    stmtPlan.Append($"{dataRow.GetValue(0)} \r\n");
                    dataRow.Reset();
                }
            }

            return stmtPlan.ToString();
        }

        internal void ThrowIfCancelled()
        {
            if (IsCancelling || IsCancelled)
            {
                throw new PgException("Operation cancelled by user.");
            }
        }

        internal void SetOutputParameters()
        {
            if (_commandType != CommandType.StoredProcedure || _parameters.Count == 0 || _rows.Count == 0)
            {
                return;
            }

            var dataRow = new DataRow();
            int index   = -1;

            dataRow.ReadFrom(this);
            for (int i = 0; i < _parameters.Count; ++i)
            {
                if (_parameters[i].Direction != ParameterDirection.Input)
                {
                    _parameters[i].Value = dataRow.GetValue(++index);
                }
            }
            dataRow.Reset();
        }

        private void Parse()
        {
            _parseMessage.Reset();

            // Clear row data
            _rows.Clear();

            // Reset row descriptor information
            _rowDescriptor.Clear();

            // Parse statement text
            if (_commandType == CommandType.StoredProcedure)
            {
                _parsedStatementText = _statementText.ToStoredProcedureCall(_parameters)
                                                     .ParseCommandText(_parameters, ref _parameterIndices);
            }
            else if (_parameters != null && _parameters.Count > 0)
            {
                _parsedStatementText = _statementText.ParseCommandText(_parameters, ref _parameterIndices);
            }
            else
            {
                if (_parameterIndices != null &&_parameterIndices.Count > 0)
                {
                    _parameterIndices.Clear();
                }
                _parsedStatementText = _statementText;
            }

            // Write Statement name and it's query
            _parseMessage.WriteNullString(_statementName);
            _parseMessage.WriteNullString(_parsedStatementText);

            // Write parameter types
            _parseMessage.Write((short)((_parameterIndices == null) ? 0 : _parameterIndices.Count));
            if (_parameterIndices != null)
            {
                for (int i = 0; i < _parameterIndices.Count; ++i)
                {
                    var parameter = _parameters[_parameterIndices[i]];
                    if (!parameter.IsTypeSet && parameter.TypeInfo?.Oid == Oid.Unknown) 
                    {
                        _parseMessage.Write(0);
                    }
                    else
                    {
                        _parseMessage.Write(parameter.TypeInfo?.Oid ?? 0);
                    }
                }
            }

            // Send the message
            _connection.Send(_parseMessage);
        }

        private void DescribeStatement() => Describe(STATEMENT);
        private void DescribePortal()    => Describe(PORTAL);

        private void Describe(byte type)
        {
            _describeMessage.Reset();

            _describeMessage.WriteByte(type);
            _describeMessage.WriteNullString(((type == STATEMENT) ? _statementName : _portalName));

            _connection.Send(_describeMessage, SyncMode.SyncAndFlush);

            ReadUntilReadyForQuery();
        }

        private void Bind()
        {
            if (IsSuspended)
            {
                ClosePortal();
            }

            _bindMessage.Reset();

            // Destination portal name
            _bindMessage.WriteNullString(_portalName);

            // Prepared statement name
            _bindMessage.WriteNullString(_statementName);

            // Parameter format code.
            _bindMessage.Write(65537);

            // Parameter value
            _bindMessage.Write((short)((_parameterIndices == null) ? 0 : _parameterIndices.Count));
            if (_parameterIndices != null)
            {
                for (int i = 0; i < _parameterIndices.Count; ++i)
                {
                    var param = _parameters[_parameterIndices[i]];
                    _bindMessage.Write(param.TypeInfo, ((param.PgValue != null) ? param.PgValue : param.Value));
                }
            }

            // Column information
            _bindMessage.Write(65537);

            // Send message
            _connection.Send(_bindMessage);
        }

        private void Execute() => Execute(CommandBehavior.Default);

        private void Execute(CommandBehavior behavior)
        {
            _executeMessage.Reset();
            _executeMessage.WriteNullString(_portalName);

            // Rows to retrieve ( 0 = nolimit )
            if (behavior.HasBehavior(CommandBehavior.SingleResult)
             || behavior.HasBehavior(CommandBehavior.SingleRow))
            {
                _executeMessage.Write(1);
            }
            else if (_connection.TransactionState == TransactionState.Active)
            {
                _executeMessage.Write(_fetchSize);
            }
            else
            {
                _executeMessage.Write(0);
            }

            ThrowIfCancelled();

            var mode = (HasPendingRefCursors ? SyncMode.Flush : SyncMode.SyncAndFlush);

            _connection.Send(_executeMessage, mode);

            // Process response
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
                HandleMessage(rmessage);
            } while (!rmessage.IsCommandComplete && !rmessage.IsPortalSuspended && !rmessage.IsEmptyQuery);

            var closePortal = (rmessage.IsCommandComplete || rmessage.IsEmptyQuery || rmessage.IsNoData);

            if (mode == SyncMode.SyncAndFlush)
            {
                ReadUntilReadyForQuery();
            }

            if (closePortal
             || behavior.HasBehavior(CommandBehavior.SingleResult) 
             || behavior.HasBehavior(CommandBehavior.SingleRow))
            {
                ClosePortal();
            }            
        }

        private void ClosePortal()
        {
            _closeMessage.Reset();
            _closeMessage.WriteByte(PORTAL);
            _closeMessage.WriteNullString(_portalName);

            _connection.Send(_closeMessage, SyncMode.Flush);

            // Read until CLOSE COMPLETE message is received
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
            } while (!rmessage.IsCloseComplete);

            ChangeState(StatementState.Prepared);
        }

        private void HandleMessage(MessageReader message)
        {
            switch (message.MessageType)
            {
            case BackendMessages.DataRow:
                ProcessDataRow(message);
                break;

            case BackendMessages.RowDescription:
                ProcessRowDescription(message);
                break;
            
            case BackendMessages.ParameterDescription:        
                ProcessParameterDescription(message);
                break;

            case BackendMessages.PortalSuspended:
                ChangeState(StatementState.Suspended);
                break;

            case BackendMessages.CommandComplete:
                ProcessTag(message);
                break;

            case BackendMessages.EmptyQueryResponse:
            case BackendMessages.NoData:
                _rows.Clear();
                _hasRows = false;
                break;

            // case BackendMessages.CopyData:
            //     Console.WriteLine("CopyData");
            //     break;

            // case BackendMessages.CopyDone:
            //     Console.WriteLine("CopyDone");
            //     break;

            // case BackendMessages.CopyBothResponse:
            //     Console.WriteLine("CopyBothResponse");
            //     break;

            // case BackendMessages.CopyInResponse:
            //     Console.WriteLine("CopyInResponse");
            //     break;

            // case BackendMessages.CopyOutResponse:
            //     Console.WriteLine("CopyOutResponse");
            //     break;
            }
        }

        private void ProcessTag(MessageReader message)
        {
            _tag = message.ReadNullString();

            _recordsAffected = -1;                

            if (_tag.StartsWith("INSERT", StringComparison.Ordinal))
            {                
                _recordsAffected = int.Parse(_tag.Substring(_tag.LastIndexOf(' ')));
            }
            else if (_tag.StartsWith("DELETE", StringComparison.Ordinal)
                  || _tag.StartsWith("UPDATE", StringComparison.Ordinal)
                  || _tag.StartsWith("MOVE", StringComparison.Ordinal)
                  || _tag.StartsWith("COPY", StringComparison.Ordinal))
            {
                var index = _tag.IndexOf(' ');
                if (index != -1)
                {
                    _recordsAffected = int.Parse(_tag.Substring(index));
                }
            }
        }

        private void ProcessParameterDescription(MessageReader message)
        {
            var provider = _connection.SessionData.TypeInfoProvider;
            int count    = message.ReadInt16();

            if (count != ((_parameterIndices?.Count ?? 0)))
            {
                throw ADP.PrepareParametersCount(count, _parameterIndices.Count);
            }

            for (int i = 0; i < count; ++i)
            {
                var oid       = message.ReadInt32();
                var parameter = _parameters[_parameterIndices[i]]; 
                var typeInfo  = parameter.TypeInfo;

                if (typeInfo == null || oid != parameter.TypeInfo.Oid)
                {
                    parameter.TypeInfo = provider.GetTypeInfo(oid);
                }
            }
        }

        private void ProcessRowDescription(MessageReader message)
        {
            int count = message.ReadInt16();

            _rowDescriptor.Allocate(count);

            for (int i = 0; i < count; ++i)
            {
                var name         = message.ReadNullString();
                var tableOid     = message.ReadInt32();
                var columnid     = message.ReadInt16();
                var typeOid      = message.ReadInt32();
                var typeSize     = message.ReadInt16();
                var typeModifier = message.ReadInt32();
                var format       = message.ReadInt16();
                var typeInfo     = _connection.SessionData.TypeInfoProvider.GetTypeInfo(typeOid);

                _rowDescriptor[i] = new FieldDescriptor(name, tableOid, columnid, typeOid, typeSize, typeModifier, typeInfo);
            }
        }

        private void ProcessDataRow(MessageReader message)
        {
            var count  = message.ReadInt16();
            var values = DataRow.RentBuffer(count);
            for (int i = 0; i < count; ++i)
            {
                 values[i] = message.ReadValue(_rowDescriptor[i].TypeInfo);
            }
             _rows.Enqueue(values);
            _hasRows = true;
        }

        private void ReadUntilReadyForQuery()
        {
            MessageReader message = null;

            do
            {
                message = _connection.Read();
                HandleMessage(message);
            }
            while (!message.IsReadyForQuery);
        }

        private void ChangeState(StatementState newState)
        {
            if (!IsCancelling && !IsCancelled)
            {
                _state = newState;
            }
        }

        private void ClearRows()
        {
            if (_rows.Count > 0)
            {
                while (_rows.Count > 0)
                {
                    var row = _rows.Dequeue();
                    DataRow.ReturnBuffer(ref row);
                }
                _rows.Clear();
            }
            _hasRows = false;
        }
    }
}
