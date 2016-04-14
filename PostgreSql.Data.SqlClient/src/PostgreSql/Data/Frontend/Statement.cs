// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using PostgreSql.Data.SqlClient;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Statement
        : IDisposable
    {
        private const char STATEMENT = 'S';
        private const char PORTAL    = 'P'; 

        private Connection            _connection;
        private string                _statementText;
        private string                _parsedStatementText;
        private bool                  _hasRows;
        private string                _tag;
        private string                _parseName;
        private string                _portalName;
        private bool                  _allRowsFetched;
        private int                   _recordsAffected;
        private RowDescriptor         _rowDescriptor;
        private Queue<DataRecord>     _rows;
        private StatementState        _state;
        private PgParameterCollection _parameters;
        private CommandType           _commandType;
        private List<int>             _parameterIndices;
        private int                   _fetchSize;

        internal bool           HasRows         => _hasRows;
        internal string         Tag             => _tag;
        internal int            RecordsAffected => _recordsAffected;
        internal RowDescriptor  RowDescriptor   => _rowDescriptor;
        internal StatementState State           => _state;

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
                    if (IsPrepared || IsRunning || IsCancelled)
                    {
                        Close();
                    }

                    _statementText = value;

                    if (CommandType == CommandType.StoredProcedure)
                    {
                        _parsedStatementText = _parsedStatementText.ToStoredProcedureCall(_parameters);
                    }
                    else
                    {
                        _parsedStatementText = _statementText.ParseCommandText(_parameters, ref _parameterIndices);
                    }
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
                    if (HasMoreRows || (_rows != null && _rows.Count > 0))
                    {
                        throw new InvalidOperationException("Fetch size cannot be changed while fetching rows.");
                    }
                    _fetchSize = value;
                    _rows      = new Queue<DataRecord>(_fetchSize);
                }
            } 
        }

        internal bool IsCancelled => _state == StatementState.Cancelled;

        internal bool IsRunning
        {
            get
            {
                return (_state == StatementState.Parsing
                     || _state == StatementState.Describing
                     || _state == StatementState.Binding
                     || _state == StatementState.Executing
                     || _state == StatementState.OnQuery
                     || HasMoreRows);
            }
        }

        internal bool IsPrepared
        {
            get
            {
                return (_state == StatementState.Parsed
                     || _state == StatementState.Described
                     || _state == StatementState.Bound
                     || _state == StatementState.Executed);
            }
        }

        internal bool HasMoreRows => _hasRows && !_allRowsFetched;

        internal Statement(Connection connection)
            : this(connection, null)
        {
        }

        internal Statement(Connection connection, string statementText)
        {
            _connection       = connection;
            _commandType      = CommandType.Text;
            _state            = StatementState.Initial;
            _recordsAffected  = -1;
            _hasRows          = false;
            _allRowsFetched   = false;
            _parameters       = PgParameterCollection.Empty;
            _parameterIndices = new List<int>();
            _rowDescriptor    = new RowDescriptor();

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
                _hasRows          = false;
                _tag              = null;
                _parseName        = null;
                _portalName       = null;
                _allRowsFetched   = false;
                _recordsAffected  = -1;
                _rowDescriptor    = null;
                _rows             = null;
                _parameters       = null;
                _parameterIndices = null;

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
            if (IsRunning)
            {
                _connection.CancelRequest();
                ChangeState(StatementState.Cancelled);
            }
        }

        internal void Prepare()
        {
            try
            {
                _connection.Lock();

                string statementName = Guid.NewGuid().ToString();
                
                _parseName  = $"PS{statementName}";
                _portalName = $"PR{statementName}";

                Parse();
                DescribeStatement();
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

                Bind();
                Execute(CommandBehavior.SingleRow);

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

        internal void ExecuteReader()
        {
            ExecuteReader(CommandBehavior.Default);
        }

        internal void ExecuteReader(CommandBehavior behavior)
        {
            try
            {
                _connection.Lock();

                ThrowIfCancelled();

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

                Bind();
                Execute(CommandBehavior.SingleResult);

                if (!_rows.IsEmpty())
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

                var message = _connection.CreateMessage(FrontendMessages.FunctionCall);

                // Function id
                message.Write(id);

                // Parameter count
                var parameterCount = (short)_parameterIndices.Count; 

                // Send parameters format code.
                message.Write(parameterCount);
                for (int i = 0; i < parameterCount; ++i)
                {
                    message.Write(_parameters[_parameterIndices[i]].TypeInfo.FormatCode);
                }

                // Send parameter values
                message.Write(parameterCount);
                for (int i = 0; i < parameterCount; ++i)
                {
                    var param = _parameters[_parameterIndices[i]];

                    message.Write(param.TypeInfo, ((param.PgValue != null) ? param.PgValue : param.Value));
                }

                // Send the format code for the function result
                message.Write((short)TypeFormat.Binary);

                // Send packet to the server
                _connection.Send(message);

                // Process response
                ReadUntilReadyForQuery();

                // Update status
                ChangeState(StatementState.Executed);
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
                ChangeState(StatementState.OnQuery);

                var message = _connection.CreateMessage(FrontendMessages.Query);

                message.WriteNullString(_statementText);

                // Send packet to the server
                _connection.Send(message);

                // Process response messages
                ReadUntilReadyForQuery();

                if (_hasRows)
                {
                    // Set allRowsFetched flag
                    _allRowsFetched = true;
                }

                // Update status
                ChangeState(StatementState.Initial);
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

        internal DataRecord FetchRow()
        {
            if (IsCancelled && _rows.IsEmpty())
            {
                return null;
            }

            if (!IsCancelled && !_allRowsFetched && _rows.IsEmpty())
            {
                Execute();  // Fetch next group of rows
            }

            DataRecord row = null;

            if (!_rows.IsEmpty())
            {
                row = _rows.Dequeue();
            }

            _hasRows = (!_allRowsFetched || !_rows.IsEmpty());

            return row;
        }

        internal void Close()
        {
            try
            {
                _connection.Lock();

                CloseStatement();

                _connection.Sync();

                _rowDescriptor.Resize(0);
            }
            catch (System.Exception)
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

                while (stmt._hasRows)
                {
                    var row = stmt.FetchRow();

                    stmtPlan.Append($"{row.GetString(0)} \r\n");
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

        private void Parse()
        {
            // Update status
            ChangeState(StatementState.Parsing);

            // Clear actual row list
            ClearRows();

            // Initialize RowDescriptor and Parameters
            _rowDescriptor.Clear();

            var message = _connection.CreateMessage(FrontendMessages.Parse);

            // Write Statement name and it's query
            message.WriteNullString(_parseName);
            message.WriteNullString(_parsedStatementText);

            // Parameter count
            var parameterCount = (short)_parameterIndices.Count;

            // Write parameter types
            message.Write(parameterCount);
            for (int i = 0; i < parameterCount; ++i)
            {
                message.Write(_parameters[_parameterIndices[i]].TypeInfo.Oid);
            }

            // Send the message
            _connection.Send(message);

            // Update status
            ChangeState(StatementState.Parsed);
        }

        private void DescribeStatement() => Describe(STATEMENT);
        private void DescribePortal()    => Describe(PORTAL);

        private void Describe(char type)
        {
            // Update status
            ChangeState(StatementState.Describing);

            var name    = ((type == STATEMENT) ? _parseName : _portalName);
            var message = _connection.CreateMessage(FrontendMessages.Describe);

            message.Write(type);
            message.WriteNullString(name);

            // Send packet to the server
            _connection.Send(message);

            // Flush pending messages
            _connection.Flush();

            // Process response
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
                HandleSqlMessage(rmessage);
            } while (!rmessage.IsRowDescription && !rmessage.IsNoData);

            // Update status
            ChangeState(StatementState.Described);
        }

        private void Bind()
        {
            // Update status
            ChangeState(StatementState.Binding);

            // Clear row data
            ClearRows();

            var message = _connection.CreateMessage(FrontendMessages.Bind);

            // Destination portal name
            message.WriteNullString(_portalName);

            // Prepared statement name
            message.WriteNullString(_parseName);

            // Parameter count
            var parameterCount = (short)_parameterIndices.Count;

            // Send parameters format code.
            message.Write(parameterCount);
            for (int i = 0; i < parameterCount; ++i)
            {
                message.Write(_parameters[_parameterIndices[i]].TypeInfo.FormatCode);
            }

            // Send parameter values
            message.Write(parameterCount);
            for (int i = 0; i <parameterCount; ++i)
            {
                var param = _parameters[_parameterIndices[i]];

                message.Write(param.TypeInfo, ((param.PgValue != null) ? param.PgValue : param.Value));
            }

            // Send column information
            var fieldCount = (short)_rowDescriptor.Count;
            message.Write(fieldCount);
            for (int i = 0; i < fieldCount; ++i)
            {
                message.Write(_rowDescriptor[i].TypeInfo.FormatCode);
            }

            // Send packet to the server
            _connection.Send(message);

            // Update status
            ChangeState(StatementState.Bound);
        }

        private void Execute()
        {
            Execute(CommandBehavior.Default);
        }

        private void Execute(CommandBehavior behavior)
        {
            // Update status
            ChangeState(StatementState.Executing);

            var message = _connection.CreateMessage(FrontendMessages.Execute);

            message.WriteNullString(_portalName);

            // Rows to retrieve ( 0 = nolimit )
            if (behavior.HasBehavior(CommandBehavior.SingleResult)
             || behavior.HasBehavior(CommandBehavior.SingleRow))
            {
                message.Write(1);
            }
            else
            {
                message.Write(_fetchSize);
            }

            // Send packet to the server
            _connection.Send(message);

            // Flush pending messages
            _connection.Flush();

            // Process response
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();

                HandleSqlMessage(rmessage);
            }
            while (!rmessage.IsCommandComplete && !rmessage.IsPortalSuspended);

            // If the command is finished and has returned rows
            // set all rows are received
            _allRowsFetched = rmessage.IsCommandComplete;

            // Update status
            ChangeState(StatementState.Executed);
        }

        private void CloseStatement()
        {
            if (_state != StatementState.Initial)
            {
                Close(STATEMENT, _parseName);

                // Clear remaing rows
                ClearRows();

                // Reset statment & portal names
                _parseName  = null;
                _portalName = null;

                // Update Status
                ChangeState(StatementState.Initial);
            }
        }

        private void ClosePortal()
        {
            if (_state == StatementState.Bound
             || _state == StatementState.Executing
             || _state == StatementState.Executed)
            {
                Close(PORTAL, _portalName);
                ChangeState(StatementState.Described);
            }
        }

        private void Close(char type, string name)
        {
            if (name != null && name.Length > 0)
            {
                var message = _connection.CreateMessage(FrontendMessages.Close);

                message.Write(type);
                message.WriteNullString(String.IsNullOrEmpty(name) ? String.Empty : name);

                // Send message to the server
                _connection.Send(message);

                // Flush pending messages
                _connection.Flush();

                // Read until CLOSE COMPLETE message is received
                MessageReader rmessage = null;

                do
                {
                    rmessage = _connection.Read();
                    HandleSqlMessage(rmessage);
                }
                while (!rmessage.IsCloseComplete);
            }

            _tag             = null;
            _recordsAffected = -1;
        }
        
        private void HandleSqlMessage(MessageReader message)
        {
            switch (message.MessageType)
            {
                case BackendMessages.DataRow:
                    _hasRows = true;
                    ProcessDataRow(message);
                    break;

                case BackendMessages.RowDescription:
                    ProcessRowDescription(message);
                    break;

                case BackendMessages.FunctionCallResponse:
                    ProcessFunctionResult(message);
                    break;

                case BackendMessages.CommandComplete:
                    ClosePortal();
                    ProcessTag(message);
                    break;

                case BackendMessages.EmptyQueryResponse:
                case BackendMessages.NoData:
                    ClosePortal();
                    ClearRows();
                    break;

                // case BackendCodes.PARAMETER_DESCRIPTION:
                // case BackendCodes.CLOSE_COMPLETE:
                // case BackendCodes.BIND_COMPLETE:
                // case BackendCodes.PARSE_COMPLETE:
                //     break;
            }
        }

        private void ProcessTag(MessageReader message)
        {
            _tag = message.ReadNullString();
            
            string[] elements = _tag.Split(' ');

            switch (elements[0])
            {
                case "FETCH":
                case "SELECT":
                    _recordsAffected = -1;
                    break;

                case "INSERT":
                    _recordsAffected = Int32.Parse(elements[2]);
                    break;

                case "UPDATE":
                case "DELETE":
                case "MOVE":
                    _recordsAffected = Int32.Parse(elements[1]);
                    break;
            }
        }

        private void ProcessFunctionResult(MessageReader message)
        {
            for (int i = 0; i < _parameters.Count; ++i)
            {
                var p = _parameters[i];

                if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                {
                    p.Value = message.ReadValue(p.TypeInfo, message.ReadInt32());
                }
            }
        }

        private void ProcessRowDescription(MessageReader message)
        {
            int count = message.ReadInt16();

            _rowDescriptor.Resize(count);

            for (int i = 0; i < count; ++i)
            {
                var name         = message.ReadNullString();
                var tableOid     = message.ReadInt32();
                var columnid     = message.ReadInt16();
                var typeOid      = message.ReadInt32();
                var typeSize     = message.ReadInt16();
                var typeModifier = message.ReadInt32();
                var format       = message.ReadInt16();
                var typeInfo     = TypeInfoProvider.GetBaseTypeInfo(typeOid);

                if (typeInfo == null)
                {
                    typeInfo = _connection.TypeInfoProvider.GetTypeInfo(typeOid);
                }

                _rowDescriptor.Add(new FieldDescriptor(name, tableOid, columnid, typeOid, typeSize, typeModifier, typeInfo));
            }
        }

        private void ProcessDataRow(MessageReader message)
        {
            var count  = message.ReadInt16();
            var values = new object[count];

            for (int i = 0; i < count; ++i)
            {
                values[i] = message.ReadValue(_rowDescriptor[i].TypeInfo, message.ReadInt32());
            }

            _rows.Enqueue(new DataRecord(_rowDescriptor, values));
        }

        private void ClearRows()
        {
            _rows.Clear();

            _hasRows        = false;
            _allRowsFetched = false;
        }

        private void ReadUntilReadyForQuery()
        {
            MessageReader message = null;

            do
            {
                message = _connection.Read();

                HandleSqlMessage(message);
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

        // private void ProcessParameterDescription(PgInputPacket packet)
        // {
        //     int oid   = 0;
        //     int count = packet.ReadInt16();

        //     _parameters.Clear();
        //     _parameters.Capacity = count;

        //     for (int i = 0; i < count; i++)
        //     {
        //         oid = packet.ReadInt32();

        //         _parameters.Add(new PgParameter(_database.ServerConfiguration.DataTypes.SingleOrDefault(x => x.Oid == oid)));
        //     }
        // }
    }
}
