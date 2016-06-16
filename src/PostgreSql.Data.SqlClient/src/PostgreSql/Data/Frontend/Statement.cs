// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Statement
        : IDisposable
    {
        private static readonly byte   STATEMENT     = (byte)'S';
        private static readonly byte   PORTAL        = (byte)'P';
        private static readonly char[] TAG_SEPARATOR = new char[] { ' ' }; 

        private Connection            _connection;
        private string                _statementText;
        private string                _parsedStatementText;
        private string                _tag;
        private string                _parseName;
        private string                _portalName;
        private int                   _fetchSize;
        private int                   _recordsAffected;
        private bool                  _hasRows;
        private RowDescriptor         _rowDescriptor;
        private Queue<object[]>       _rows;
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
        private MessageWriter         _functionMessage;

        internal bool           HasRows         => _hasRows;
        internal string         Tag             => _tag;
        internal int            RecordsAffected => _recordsAffected;
        internal RowDescriptor  RowDescriptor   => _rowDescriptor;
        internal StatementState State           => _state;

        internal bool IsPrepared  => _state == StatementState.Prepared || IsExecuting || IsSuspended;
        internal bool IsExecuting => _state == StatementState.Executing;
        internal bool IsSuspended => _state == StatementState.Suspended;
        internal bool IsCancelled => _state == StatementState.Cancelled;

        internal Connection Connection => _connection;

        internal CommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        internal string StatementText
        {
            get { return _statementText; }
            set 
            {
                if (_statementText != value)
                {
                    _statementText       = value;
                    _parsedStatementText = null;
                }
            }
        }

        internal PgParameterCollection Parameters
        {
            get { return _parameters; }
            set { _parameters = value; }
        }

        internal int FetchSize
        {
            get { return _fetchSize; }
            set
            {
                if (_fetchSize != value)
                {
                    if (IsExecuting || (_rows != null && _rows.Count > 0))
                    {
                        throw new InvalidOperationException("Fetch size cannot be changed while fetching rows.");
                    }
                    _fetchSize = value;
                    _rows      = new Queue<object[]>(_fetchSize);
                }
            } 
        }

        internal Statement(Connection connection)
            : this(connection, null)
        {
        }

        internal Statement(Connection connection, string statementText)
        {
            _connection       = connection;
            _commandType      = CommandType.Text;
            _state            = StatementState.Default;
            _recordsAffected  = -1;
            _parameters       = PgParameterCollection.Empty;
            _parameterIndices = new List<int>();
            _rowDescriptor    = new RowDescriptor();
            _hasRows          = false;
            _parseMessage     = new MessageWriter(FrontendMessages.Parse        , _connection.SessionData);
            _describeMessage  = new MessageWriter(FrontendMessages.Describe     , _connection.SessionData);
            _bindMessage      = new MessageWriter(FrontendMessages.Bind         , _connection.SessionData);
            _executeMessage   = new MessageWriter(FrontendMessages.Execute      , _connection.SessionData);
            _closeMessage     = new MessageWriter(FrontendMessages.Close        , _connection.SessionData);
            _queryMessage     = new MessageWriter(FrontendMessages.Query        , _connection.SessionData);
            _functionMessage  = new MessageWriter(FrontendMessages.FunctionCall , _connection.SessionData);;

            StatementText     = statementText;
            FetchSize         = 200;
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
                _connection       = null;
                _statementText    = null; 
                _tag              = null;
                _parseName        = null;
                _portalName       = null;
                _rowDescriptor    = null;
                _rows             = null;
                _parameters       = null;
                _parameterIndices = null;
                _hasRows          = false;
                _recordsAffected  = -1;
                _parseMessage     = null;
                _describeMessage  = null;
                _bindMessage      = null;
                _executeMessage   = null;
                _closeMessage     = null;
                _queryMessage     = null;
                _functionMessage  = null;

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgStatement() {
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

        internal void Cancel()
        {
            if (IsExecuting)
            {
                _connection.CancelRequest();
                ChangeState(StatementState.Cancelled);
            }
        }

        internal void Prepare()
        {
            try
            {
                Close();

                _connection.Lock();

                string statementName = Guid.NewGuid().ToString();

                _parseName  = $"PS{statementName}";
                _portalName = $"PR{statementName}";

                Parse();
                DescribeStatement();

                ChangeState(StatementState.Prepared);
            }
            catch
            {
                throw;
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
                InternalSetOutputParameters();

                return _recordsAffected;
            }
            catch
            {
                throw;
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
            catch
            {
                throw;
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
            catch
            {
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void ExecuteFunction(int id)
        {
            try
            {
                _connection.Lock();

                ThrowIfCancelled();

                ChangeState(StatementState.Executing);

                _functionMessage.Clear();

                // Function id
                _functionMessage.Write(id);

                // Send parameters format code.
                _functionMessage.Write(65537);

                // Send parameter values
                _functionMessage.Write((short)_parameterIndices.Count);
                for (int i = 0; i < _parameterIndices.Count; ++i)
                {
                    var param = _parameters[_parameterIndices[i]];

                    _functionMessage.Write(param.TypeInfo, ((param.PgValue != null) ? param.PgValue : param.Value));
                }

                // Send the format code for the function result
                _functionMessage.Write((short)TypeFormat.Binary);

                // Send message
                _connection.Send(_functionMessage);

                // Process response
                ReadUntilReadyForQuery();

                // Update status
                ChangeState(StatementState.Prepared);
            }
            catch
            {
                throw;
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

                _queryMessage.Clear();

                _queryMessage.WriteNullString(_statementText);

                // Send message
                _connection.Send(_queryMessage);

                // Process response
                ReadUntilReadyForQuery();

                // Update status
                ChangeState(StatementState.Default);
            }
            catch
            {
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal object[] FetchRow()
        {
            if (IsCancelled)
            {
                if (_rows.Count == 0)
                {
                    return null;
                }
                return _rows.Dequeue();
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

        internal void Close()
        {
            if (_state == StatementState.Default)
            {
                return;
            }

            try
            {
                _connection.Lock();

                // Close current statement
                Close(STATEMENT);

                // Sync state
                _connection.Sync();
                ReadUntilReadyForQuery();

                // Reset has rows flag
                _hasRows = false;

                // Clear remaing rows
                _rows.Clear();

                // Reset statement and portal names
                _parseName  = null;
                _portalName = null;

                // Reset the row descriptor
                _rowDescriptor.Clear();

                // Reset statement parameters
                _parameters = null;

                // Reset command type
                _commandType = CommandType.Text;

                // Clear messages
                _parseMessage.Clear();
                _describeMessage.Clear();
                _bindMessage.Clear();
                _executeMessage.Clear();
                _closeMessage.Clear();
                _queryMessage.Clear();
                _functionMessage.Clear();

                // Update Status
                ChangeState(StatementState.Default);
            }
            catch
            {
                throw;
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

            using (var stmt = _connection.CreateStatement(stmtText))
            {
                stmt.Query();

                while (_rows.Count > 0)
                {
                    var row = stmt.FetchRow();

                    stmtPlan.Append($"{row[0]} \r\n");
                }
            }

            return stmtPlan.ToString();
        }

        internal void ThrowIfCancelled()
        {
            if (IsCancelled)
            {
                throw new PgException("Operation cancelled by user.");
            }
        }

        internal void InternalSetOutputParameters()
        {
            if (_commandType != CommandType.StoredProcedure || _parameters.Count == 0 || _rows.Count == 0)
            {
                return;
            }
            
            var row   = FetchRow();
            var index = -1;

            for (int i = 0; i < _parameters.Count; ++i)
            {
                if (_parameters[i].Direction != ParameterDirection.Input)
                {
                    _parameters[i].Value = row[++index];
                }
            }
        }

        private void Parse()
        {
            _parseMessage.Clear();

            // Reset has rows flag
            _hasRows = false;

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
            else
            {
                _parsedStatementText = _statementText.ParseCommandText(_parameters, ref _parameterIndices);
            }

            // Write Statement name and it's query
            _parseMessage.WriteNullString(_parseName);
            _parseMessage.WriteNullString(_parsedStatementText);

            // Write parameter types
            _parseMessage.Write((short)_parameterIndices.Count);
            for (int i = 0; i < _parameterIndices.Count; ++i)
            {
                _parseMessage.Write(_parameters[_parameterIndices[i]].TypeInfo.Oid);
            }

            // Send the message
            _connection.Send(_parseMessage);
        }

        private void DescribeStatement() => Describe(STATEMENT);
        private void DescribePortal()    => Describe(PORTAL);

        private void Describe(byte type)
        {
            _describeMessage.Clear();

            _describeMessage.WriteByte(type);
            _describeMessage.WriteNullString(((type == STATEMENT) ? _parseName : _portalName));

            _connection.Send(_describeMessage);
            _connection.Flush();

            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
                HandleMessage(rmessage);
            } while (!rmessage.IsRowDescription && !rmessage.IsNoData);
        }

        private void Bind()
        {
            if (IsSuspended)
            {
                ClosePortal();
            }

            _bindMessage.Clear();

            // Destination portal name
            _bindMessage.WriteNullString(_portalName);

            // Prepared statement name
            _bindMessage.WriteNullString(_parseName);

            // Parameter format code.
            _bindMessage.Write(65537);

            // Parameter value
            _bindMessage.Write((short)_parameterIndices.Count);
            for (int i = 0; i < _parameterIndices.Count; ++i)
            {
                var param = _parameters[_parameterIndices[i]];
                _bindMessage.Write(param.TypeInfo, ((param.PgValue != null) ? param.PgValue : param.Value));
            }

            // Column information
            _bindMessage.Write(65537);

            // Send message
            _connection.Send(_bindMessage);
        }

        private void Execute() => Execute(CommandBehavior.Default);

        private void Execute(CommandBehavior behavior)
        {
            _executeMessage.Clear();
            _executeMessage.WriteNullString(_portalName);

            // Rows to retrieve ( 0 = nolimit )
            if (behavior.HasBehavior(CommandBehavior.SingleResult)
             || behavior.HasBehavior(CommandBehavior.SingleRow))
            {
                _executeMessage.Write(1);
            }
            else
            {
                _executeMessage.Write(_fetchSize);
            }

            _connection.Send(_executeMessage);
            _connection.Flush();

            // Process response
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
                HandleMessage(rmessage);
            } while (!rmessage.IsCommandComplete && !rmessage.IsPortalSuspended && !rmessage.IsEmptyQuery);

            if (rmessage.IsCommandComplete 
             || rmessage.IsEmptyQuery
             || behavior.HasBehavior(CommandBehavior.SingleResult) 
             || behavior.HasBehavior(CommandBehavior.SingleRow))
            {
                ClosePortal();
            }
        }

        private void ClosePortal()
        {
            Close(PORTAL);
            ChangeState(StatementState.Prepared);
        }

        private void Close(byte type)
        {
            var name = ((type == STATEMENT) ? _parseName : _portalName);

            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            _closeMessage.Clear();
            _closeMessage.WriteByte(type);
            _closeMessage.WriteNullString(name);

            _connection.Send(_closeMessage);
            _connection.Flush();

            // Read until CLOSE COMPLETE message is received
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
            } while (!rmessage.IsCloseComplete);

            _tag             = null;
            _recordsAffected = -1;
        }

        private void HandleMessage(MessageReader message)
        {
            switch (message.MessageType)
            {
            // case BackendMessages.ParameterDescription:
            //     ProcessParameterDescription(message);
            //     break;

            case BackendMessages.RowDescription:
                ProcessRowDescription(message);
                break;

            case BackendMessages.DataRow:
                ProcessDataRow(message);
                break;

            case BackendMessages.FunctionCallResponse:
                ProcessFunctionResult(message);
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

            string[] elements = _tag.Split(TAG_SEPARATOR);

            switch (elements[0])
            {
            case "FETCH":
            case "SELECT":
                _recordsAffected = -1;
                break;

            case "INSERT":
                _recordsAffected = int.Parse(elements[2]);
                break;

            case "UPDATE":
            case "DELETE":
            case "MOVE":
            case "COPY":
                _recordsAffected = int.Parse(elements[1]);
                break;
            }
        }

        private void ProcessFunctionResult(MessageReader message)
        {
            for (int i = 0; i < _parameterIndices.Count; ++i)
            {
                var p = _parameters[_parameterIndices[i]];

                if (p.Direction != ParameterDirection.Input)
                {
                    p.Value = message.ReadValue(p.TypeInfo, message.ReadInt32());
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
                var typeInfo     = TypeInfoProvider.GetTypeInfo(typeOid);

                if (typeInfo == null)
                {
                    typeInfo = _connection.SessionData.TypeInfoProvider.GetCompositeTypeInfo(typeOid);
                }

                _rowDescriptor[i] = new FieldDescriptor(name, tableOid, columnid, typeOid, typeSize, typeModifier, typeInfo);
            }
        }

        // private void ProcessParameterDescription(MessageReader message)
        // {
        //     int oid   = 0;
        //     int count = message.ReadInt16();

        //     if (count != _parameterIndices.Count)
        //     {
        //         throw ADP.PrepareParametersCount(count, _parameterIndices.Count);
        //     }

        //     PgParameter parameter = null;

        //     for (int i = 0; i < count; ++i)
        //     {
        //         oid                = message.ReadInt32();
        //         parameter          = _parameters[_parameterIndices[i]]; 
        //         parameter.TypeInfo = TypeInfoProvider.GetTypeInfo(oid);

        //         if (parameter.TypeInfo == null)
        //         {
        //             parameter.TypeInfo = _connection.SessionData.TypeInfoProvider.GetCompositeTypeInfo(oid);

        //             if (parameter.TypeInfo == null)
        //             {
        //                 throw ADP.PrepareParametersUnknownDataType(oid);
        //             }
        //         }
        //     }
        // }

        private void ProcessDataRow(MessageReader message)
        {
            while (message.Position < message.Length)
            {
                var values = new object[message.ReadInt16()];
                for (int i = 0; i < values.Length; ++i)
                {
                    values[i] = message.ReadValue(_rowDescriptor[i].TypeInfo, message.ReadInt32());
                }
                _rows.Enqueue(values);
            }
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
            if (!IsCancelled)
            {
                _state = newState;
            }
        }
    }
}
