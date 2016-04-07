// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostgreSql.Data.SqlClient;
using PostgreSql.Data.PgTypes;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Statement
        : IDisposable
    {
        // Statements and Portals
        private const char STATEMENT = 'S';
        private const char PORTAL    = 'P'; 

        private Connection        _connection;
        private string            _statementText;
        private bool              _hasRows;
        private string            _tag;
        private string            _parseName;
        private string            _portalName;
        private bool              _allRowsFetched;
        private int               _recordsAffected;
        private RowDescriptor     _rowDescriptor;
        private Queue<DataRecord> _rows;
        private PgParameter       _outParameter;
        private StatementStatus   _status;

        internal bool            HasRows         => _hasRows;
        internal string          Tag             => _tag;
        internal int             RecordsAffected => _recordsAffected;
        internal RowDescriptor   RowDescriptor   => _rowDescriptor;
        internal StatementStatus Status          => _status;

        internal string StatementText
        {
            get { return _statementText; }
            set 
            {
                if (_statementText != value)
                {
                    Close();
                    _statementText = value;
                }
            }
        }

        internal bool IsPrepared
        {
            get
            {
                return (_status == StatementStatus.Parsed
                     || _status == StatementStatus.Described
                     || _status == StatementStatus.Bound
                     || _status == StatementStatus.Executed);
            }
        }

        internal Statement(Connection connection)
            : this(connection, null)
        {
        }

        internal Statement(Connection connection, string stmtText)
        {
            _connection      = connection;
            _status          = StatementStatus.Initial;
            _statementText   = stmtText;
            _recordsAffected = -1;
            _hasRows         = false;
            _allRowsFetched  = false;
            _outParameter    = new PgParameter();
            _rowDescriptor   = new RowDescriptor();
            _rows            = new Queue<DataRecord>(_connection.ConnectionOptions.FetchSize);
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

        internal void Prepare(PgParameterCollection parameters)
        {
            try
            {
                if (_status != StatementStatus.Initial)
                {
                    Close();
                }

                _connection.Lock();

                string statementName = Guid.NewGuid().ToString();
                
                _parseName  = $"PS{statementName}";
                _portalName = $"PR{statementName}";

                Parse(parameters);
                DescribeStatement();
            }
            catch
            {
                // Update status
                _status = StatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal int ExecuteNonQuery(PgParameterCollection parameters)
        {
            if (_status == StatementStatus.Initial)
            {
                Prepare(parameters);
            }

            try
            {
                _connection.Lock();

                Bind(parameters);
                Execute(1);
                ClosePortal();

                return _recordsAffected;
            }
            catch
            {
                // Update status
                _status = StatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void ExecuteReader(PgParameterCollection parameters)
        {
            if (_status == StatementStatus.Initial)
            {
                Prepare(parameters);
            }

            try
            {
                _connection.Lock();

                Bind(parameters);
                Execute();
            }
            catch
            {
                // Update status
                _status = StatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal object ExecuteScalar(PgParameterCollection parameters)
        {
            if (_status == StatementStatus.Initial)
            {
                Prepare(parameters);
            }

            try
            {
                _connection.Lock();

                Bind(parameters);
                Execute(1);
                ClosePortal();

                object value = null;
                  
                if (!_rows.IsEmpty())
                {
                    value = _rows.Dequeue()[0];
                }

                return value;
            }
            catch
            {
                // Update status
                _status = StatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal void ExecuteFunction(int id, PgParameterCollection parameters)
        {
            try
            {
                _connection.Lock();

                // Update status
                _status = StatementStatus.Executing;

                var message = _connection.CreateMessage(FrontendMessages.FunctionCall);

                // Function id
                message.Write(id);

                // Send parameters format code.
                message.Write((short)parameters.Count);

                for (int i = 0; i < parameters.Count; i++)
                {
                    message.Write((short)parameters[i].TypeInfo.Format);
                }

                // Send parameter values
                message.Write((short)parameters.Count);

                for (int i = 0; i < parameters.Count; i++)
                {
                    var parameter = parameters[i];

                    if (parameter.Value         == System.DBNull.Value 
                     || parameter.Value         == null 
                     || parameter.TypeInfo.Size == 0 /* Void */)
                    {
                        // -1 indicates a NULL argument value
                        message.Write(-1);
                    }
                    else
                    {
                        message.Write(parameter.TypeInfo
                                    , ((parameter.PgValue != null) ? parameter.PgValue : parameter.Value));
                    }
                }

                // Send the format code for the function result
                message.Write((short)TypeFormat.Binary);

                // Send packet to the server
                _connection.Send(message);

                // Process response messages
                ReadUntilReadyForQuery();

                // Update status
                _status = StatementStatus.Executed;
            }
            catch
            {
                // Update status
                _status = StatementStatus.Broken;
                // Throw exception
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
                _status = StatementStatus.OnQuery;

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
                _status = StatementStatus.Initial;
            }
            catch
            {
                _status = StatementStatus.Broken;
                throw;
            }
            finally
            {
                _connection.ReleaseLock();
            }
        }

        internal DataRecord FetchRow()
        {
            if (!_allRowsFetched && _rows.IsEmpty())
            {
                // Retrieve next group of rows
                Execute();
            }

            if (!_rows.IsEmpty())
            {
                return _rows.Dequeue();
            }

            return null;
        }

        internal void Close()
        {
            try
            {
                _connection.Lock();

                ClosePortal();
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

        private void Parse(PgParameterCollection parameters)
        {
            // Update status
            _status = StatementStatus.Parsing;

            // Clear actual row list
            ClearRows();

            // Initialize RowDescriptor and Parameters
            _rowDescriptor.Clear();

            var message = _connection.CreateMessage(FrontendMessages.Parse);

            message.WriteNullString(_parseName);
            message.WriteNullString(_statementText);
            message.Write((short)parameters.Count);
            for (int i = 0; i < parameters.Count; i++)
            {
                message.Write(parameters[i].TypeInfo.Oid);
            }

            // Send the message
            _connection.Send(message);

            // Update status
            _status = StatementStatus.Parsed;
        }

        private void DescribeStatement() => Describe(STATEMENT);
        private void DescribePortal()    => Describe(PORTAL);

        private void Describe(char type)
        {
            // Update status
            _status = StatementStatus.Describing;

            var name    = ((type == STATEMENT) ? _parseName : _portalName);
            var message = _connection.CreateMessage(FrontendMessages.Describe);

            message.Write(type);
            message.WriteNullString(name);

            // Send packet to the server
            _connection.Send(message);

            // Flush pending messages
            _connection.Flush();

            // Receive Describe response
            MessageReader rmessage = null;

            do
            {
                rmessage = _connection.Read();
                HandleSqlMessage(rmessage);
            } while (!rmessage.IsRowDescription && !rmessage.IsNoData);

            // Update status
            _status = StatementStatus.Described;
        }
        
        private void Bind(PgParameterCollection parameters)
        {
            // Update status
            _status = StatementStatus.Binding;

            // Clear row data
            ClearRows();

            var message = _connection.CreateMessage(FrontendMessages.Bind);

            // Destination portal name
            message.WriteNullString(_portalName);

            // Prepared statement name
            message.WriteNullString(_parseName);

            // Send parameters format code.
            message.Write((short)parameters.Count);
            for (int i = 0; i < parameters.Count; i++)
            {
                message.Write((short)parameters[i].TypeInfo.Format);
            }

            // Send parameter values
            message.Write((short)parameters.Count);
            for (int i = 0; i < parameters.Count; i++)
            {
                var parameter = parameters[i];

                if (parameter.Value         == System.DBNull.Value 
                 || parameter.Value         == null 
                 || parameter.TypeInfo.Size == 0 /* Void */)
                {
                    // -1 indicates a NULL argument value
                    message.Write(-1);
                }
                else
                {
                    message.Write(parameter.TypeInfo
                                , ((parameter.PgValue != null) ? parameter.PgValue : parameter.Value));
                }
            }

            // Send column information
            message.Write((short)_rowDescriptor.Count);
            for (int i = 0; i < _rowDescriptor.Count; i++)
            {
                message.Write((short)_rowDescriptor[i].TypeInfo.Format);
            }

            // Send packet to the server
            _connection.Send(message);

            // Update status
            _status = StatementStatus.Bound;
        }

        private void Execute()
        {
            Execute(_connection.ConnectionOptions.FetchSize);
        }

        private void Execute(int fetchSize)
        {
            // Update status
            _status = StatementStatus.Executing;

            var message = _connection.CreateMessage(FrontendMessages.Execute);

            message.WriteNullString(_portalName);
            message.Write(fetchSize);	// Rows to retrieve ( 0 = nolimit )

            // Send packet to the server
            _connection.Send(message);

            // Flush pending messages
            _connection.Flush();

            // Receive response
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

            // If all rows are received or the command doesn't return
            // rows perform a Sync.
            if (_allRowsFetched)
            {
                ClosePortal();
            }

            // Update status
            _status = StatementStatus.Executed;
        }

        private void CloseStatement()
        {
            if (_status == StatementStatus.Parsed 
             || _status == StatementStatus.Described)
            {
                Close(STATEMENT, _parseName);

                // Clear remaing rows
                ClearRows();

                // Update Status
                _status = StatementStatus.Initial;

                // Reset names
                _parseName  = null;
                _portalName = null;
            }
        }

        private void ClosePortal()
        {
            if (_status == StatementStatus.Bound
             || _status == StatementStatus.Executing
             || _status == StatementStatus.Executed)
            {
                Close(PORTAL, _portalName);

                // Update Status
                _status = StatementStatus.Described;
            }
        }

        private void Close(char type, string name)
        {
            try
            {
                if (name != null && name.Length > 0)
                {
                    var message = _connection.CreateMessage(FrontendMessages.Close);

                    message.Write(type);
                    message.WriteNullString(String.IsNullOrEmpty(name) ? String.Empty : name);

                    // Send packet to the server
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
            catch
            {
                // Update Status
                _status = StatementStatus.Broken;

                // Throw exception
                throw;
            }
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
            _outParameter.Value = message.ReadValue(_outParameter.TypeInfo, message.ReadInt32());
        }

        private void ProcessRowDescription(MessageReader message)
        {
            int count = message.ReadInt16();

            _rowDescriptor.Resize(count);

            for (int i = 0; i < count; i++)
            {
                var name         = message.ReadNullString();
                var tableOid     = message.ReadInt32();
                var columnid     = message.ReadInt16();
                var typeOid      = message.ReadInt32();
                var typeSize     = message.ReadInt16();
                var typeModifier = message.ReadInt32();
                var format       = message.ReadInt16();
                var typeInfo     = PgTypeInfoProvider.Types[typeOid];

                _rowDescriptor.Add(new FieldDescriptor(name, tableOid, columnid, typeOid, typeSize, typeModifier, typeInfo));
            }
        }

        private void ProcessDataRow(MessageReader message)
        {
            var values = new object[message.ReadInt16()];

            for (int i = 0; i < values.Length; i++)
            {
                int length = message.ReadInt32();
                var field  = _rowDescriptor[i];

                if (length == -1 || field.TypeInfo.PgDbType == PgDbType.Void)
                {
                    values[i] = DBNull.Value;
                }
                else
                {
                    values[i] = message.ReadValue(field.TypeInfo, length);
                }
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
