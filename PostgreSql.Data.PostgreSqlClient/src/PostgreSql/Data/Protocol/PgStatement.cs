// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgStatement
        : IDisposable
    {
        private PgDatabase        _database;
        private string            _statementText;
        private bool              _hasRows;
        private string            _tag;
        private string            _parseName;
        private string            _portalName;
        private bool              _allRowsFetched;
        private PgRowDescriptor   _rowDescriptor;
        private Queue<object[]>   _rows;
        private List<PgParameter> _parameters;
        private PgParameter       _outParameter;
        private int               _recordsAffected;
        private PgStatementStatus _status;

        internal bool               HasRows         => _hasRows;
        internal string             Tag             => _tag;
        internal int                RecordsAffected => _recordsAffected;
        internal PgRowDescriptor    RowDescriptor   => _rowDescriptor;
        internal IList<PgParameter> Parameters      => _parameters;
        internal PgStatementStatus  Status          => _status;
        
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
                return (_status == PgStatementStatus.Parsed 
                     || _status == PgStatementStatus.Described
                     || _status == PgStatementStatus.Binded
                     || _status == PgStatementStatus.Executed);                
            }
        }

        internal PgStatement(PgDatabase database)
            : this(database, null)
        {
        }

        internal PgStatement(PgDatabase database, string stmtText)
        {
            _database        = database;
            _status          = PgStatementStatus.Initial;
            _statementText   = stmtText;
            _recordsAffected = -1;
            _hasRows         = false;
            _allRowsFetched  = false;
            _outParameter    = new PgParameter();
            _parameters      = new List<PgParameter>();
            _rowDescriptor   = new PgRowDescriptor();
            _rows            = new Queue<object[]>(_database.ConnectionOptions.FetchSize);
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
        
        internal void Prepare()
        {
            try
            {
                if (_status != PgStatementStatus.Initial)
                {
                    Close();
                }

                _database.Lock();
                                
                string statementName = Guid.NewGuid().GetHashCode().ToString();
                
                _parseName  = $"PS{statementName}";
                _portalName = $"PR{statementName}";
                
                Parse();
                Describe();
            }
            catch
            {
                // Update status
                _status = PgStatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _database.ReleaseLock();
            }
        }
        
        internal int ExecuteNonQuery()
        {
            if (_status == PgStatementStatus.Initial)
            {
                Prepare();
            }
            
            try
            {
                _database.Lock();
                
                Bind();
                Execute(0);
                
                ClosePortal();
                
                return _recordsAffected;
            }
            catch
            {
                // Update status
                _status = PgStatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _database.ReleaseLock();                
            }
        }
               
        internal void ExecuteReader()
        {
            if (_status == PgStatementStatus.Initial)
            {
                Prepare();
            }
            
            try
            {
                _database.Lock();
                
                Bind();
                Execute();
            }
            catch
            {
                // Update status
                _status = PgStatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _database.ReleaseLock();
            }
        }

        internal object ExecuteScalar()
        {
            if (_status == PgStatementStatus.Initial)
            {
                Prepare();
            }
            
            try
            {
                _database.Lock();
                
                Bind();
                Execute(1);
                
                object value = null;              
                  
                if (!_rows.IsEmpty())
                {
                    value = _rows.Dequeue()[0];
                }
                
                ClosePortal();
                
                return value;                
            }
            catch
            {
                // Update status
                _status = PgStatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _database.ReleaseLock();
            }
        }                

        internal void ExecuteFunction(int id)
        {
            try
            {
                _database.Lock();
                
                // Update status
                _status = PgStatementStatus.Executing;

                var packet = _database.CreateOutputPacket(PgFrontEndCodes.FUNCTION_CALL);

                // Function id
                packet.Write(id);

                // Send parameters format code.
                packet.Write((short)_parameters.Count);

                for (int i = 0; i < _parameters.Count; i++)
                {
                    packet.Write((short)_parameters[i].DataType.Format);
                }

                // Send parameter values
                packet.Write((short)_parameters.Count);

                for (int i = 0; i < _parameters.Count; i++)
                {
                    packet.Write(_parameters[i]);
                }

                // Send the format code for the function result
                packet.Write(PgCodes.BINARY_FORMAT);

                // Send packet to the server
                _database.Send(packet);

                // Receive response
                PgInputPacket response = null;

                do
                {
                    response = _database.Read();
                    
                    HandleSqlMessage(response);
                }
                while (!response.IsReadyForQuery);

                // Update status
                _status = PgStatementStatus.Executed;
            }
            catch
            {
                // Update status
                _status = PgStatementStatus.Broken;
                // Throw exception
                throw;
            }
            finally
            {
                _database.ReleaseLock();
            }
        }

        internal void Query()
        {
            try
            {
                _database.Lock();
                
                // Update Status
                _status = PgStatementStatus.OnQuery;

                var packet = _database.CreateOutputPacket(PgFrontEndCodes.QUERY);

                packet.WriteNullString(_statementText);

                // Send packet to the server
                _database.Send(packet);

                // Receive response
                PgInputPacket response = null;

                do
                {
                    response = _database.Read();
                    
                    HandleSqlMessage(response);
                }
                while (!response.IsReadyForQuery);

                if (_hasRows)
                {
                    // Set allRowsFetched flag
                    _allRowsFetched = true;
                }               

                // Update status
                _status = PgStatementStatus.Initial;
            }
            catch
            {
                _status = PgStatementStatus.Broken;
                throw;
            }
            finally
            {
                _database.ReleaseLock();   
            }           
        }

        internal object[] FetchRow()
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
                _database.Lock();
                
                ClosePortal();
                CloseStatement();                   
            }
            catch (System.Exception)
            {                
                throw;
            }
            finally
            {
                _database.ReleaseLock();
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

            using (var stmt = _database.CreateStatement(stmtText))
            {
                stmt.Query();

                while (stmt._hasRows)
                {
                    object[] row = stmt.FetchRow();

                    stmtPlan.Append($"{row[0]} \r\n");
                }
            }

            return stmtPlan.ToString();
        }

        private void Parse()
        {
            // Update status
            _status = PgStatementStatus.Parsing;

            // Clear actual row list
            ClearRows();

            // Initialize RowDescriptor and Parameters
            _rowDescriptor.Clear();
            _parameters.Clear();

            var packet = _database.CreateOutputPacket(PgFrontEndCodes.PARSE);

            packet.WriteNullString(_parseName);
            packet.WriteNullString(_statementText);
            packet.Write((short)0);
            
            // Send packet to the server
            _database.Send(packet);

            // Update status
            _status = PgStatementStatus.Parsed;
        }

        private void Describe()       => Describe('S');
        private void DescribePortal() => Describe('P');

        private void Describe(char stmtType)
        {
            // Update status
            _status = PgStatementStatus.Describing;

            var name   = ((stmtType == 'S') ? _parseName : _portalName);
            var packet = _database.CreateOutputPacket(PgFrontEndCodes.DESCRIBE);

            packet.Write(stmtType);
            packet.WriteNullString(name);

            // Send packet to the server
            _database.Send(packet);

            // Flush pending messages
            _database.Flush();

            // Receive Describe response
            PgInputPacket response = null;
            
            do
            {
                response = _database.Read();
                HandleSqlMessage(response);                    
            } while (!response.IsRowDescription && !response.IsNoData);

            DescribeParameters();                

            // Update status
            _status = PgStatementStatus.Described;
        }
        
        private void Bind()
        {
            // Update status
            _status = PgStatementStatus.Binding;

            // Clear row data
            ClearRows();

            var packet = _database.CreateOutputPacket(PgFrontEndCodes.BIND);

            // Destination portal name
            packet.WriteNullString(_portalName);

            // Prepared statement name
            packet.WriteNullString(_parseName);

            // Send parameters format code.
            packet.Write((short)_parameters.Count);
            for (int i = 0; i < _parameters.Count; i++)
            {
                packet.Write((short)_parameters[i].DataType.Format);
            }

            // Send parameter values
            packet.Write((short)_parameters.Count);
            for (int i = 0; i < _parameters.Count; i++)
            {
                packet.Write(_parameters[i]);
            }

            // Send column information
            packet.Write((short)_rowDescriptor.Count);
            for (int i = 0; i < _rowDescriptor.Count; i++)
            {
                packet.Write((short)_rowDescriptor[i].Type.Format);
            }

            // Send packet to the server
            _database.Send(packet);

            // Update status
            _status = PgStatementStatus.Binded;
        }

        private void Execute()
        {
            Execute(_database.ConnectionOptions.FetchSize);
        }

        private void Execute(int fetchSize)
        {
            // Update status
            _status = PgStatementStatus.Executing;

            var packet = _database.CreateOutputPacket(PgFrontEndCodes.EXECUTE);

            packet.WriteNullString(_portalName);
            packet.Write(fetchSize);	// Rows to retrieve ( 0 = nolimit )

            // Send packet to the server
            _database.Send(packet);

            // Flush pending messages
            _database.Flush();

            // Receive response
            PgInputPacket response = null;

            do
            {
                response = _database.Read();
                
                HandleSqlMessage(response);
            }
            while (!response.IsReadyForQuery && !response.IsCommandComplete && !response.IsPortalSuspended);

            // If the command is finished and has returned rows
            // set all rows are received
            if ((response.IsReadyForQuery || response.IsCommandComplete) && _hasRows)
            {
                _allRowsFetched = true;
            }

            // If all rows are received or the command doesn't return
            // rows perform a Sync.
            if (!_hasRows || _allRowsFetched)
            {
                _database.Sync();
            }

            // Update status
            _status = PgStatementStatus.Executed;
        }
        
        private void DescribeParameters()
        {
#warning TODO : Rewrite
            // Review if there are some parameter with a domain as a Data Type
            foreach (PgParameter parameter in _parameters.Where(x => x.DataType == null))
            {
                string sql = $"select typbasetype from pg_type where oid = {parameter.DataTypeOid} and typtype = 'd'";
                
                // It's a non supported data type or a domain data type
                using (var stmt = new PgStatement(_database, sql))
                {
                    stmt.Query();

                    if (!stmt._hasRows)
                    {
                        throw new PgClientException("Unsupported data type");
                    }

                    var row         = stmt.FetchRow();
                    int baseTypeOid = Convert.ToInt32(row[0]);
                    var dataType    = _database.ServerConfiguration.DataTypes.SingleOrDefault(x => x.Oid == baseTypeOid);

                    if (dataType == null)
                    {
                        throw new PgClientException("Unsupported data type");
                    }

                    // Try to add the data type to the list of supported data types
                    parameter.DataType = dataType;
                }
            }            
        }

        private void CloseStatement()
        {
            if (_status == PgStatementStatus.Parsed 
             || _status == PgStatementStatus.Described)
            {
                Close('S', _parseName);

                // Clear parameters
                _parameters.Clear();

                // Update Status
                _status = PgStatementStatus.Initial;
                
                // Reset names
                _parseName  = null;
                _portalName = null;                
            }
        }
        
        private void ClosePortal()
        {
            if (_status == PgStatementStatus.Binded
             || _status == PgStatementStatus.Executed)
            {
                Close('P', _portalName);
                
                // Update Status
                _status = PgStatementStatus.Described;
            }
        }   

        private void Close(char type, string name)
        {
            try
            {
                if (name != null && name.Length > 0)
                {
                    var packet = _database.CreateOutputPacket(PgFrontEndCodes.CLOSE);

                    packet.Write(type);
                    packet.WriteNullString(String.IsNullOrEmpty(name) ? String.Empty : name);

                    // Send packet to the server
                    _database.Send(packet);

                    // Sync server and client
                    _database.Flush();

                    // Read until CLOSE COMPLETE message is received
                    PgInputPacket response = null;

                    do
                    {
                        response = _database.Read();
                        HandleSqlMessage(response);
                    }
                    while (!response.IsCloseComplete);
                }
                
                // Cleanup
                ClearRows();
                
                _tag             = null;          
                _recordsAffected = -1;      
            }
            catch
            {
                // Update Status
                _status = PgStatementStatus.Broken;

                // Throw exception
                throw;
            }
        }

        private void HandleSqlMessage(PgInputPacket packet)
        {
            switch (packet.Message)
            {
                case PgBackendCodes.DATAROW:
                    _hasRows = true;
                    ProcessDataRow(packet);
                    break;

                case PgBackendCodes.ROW_DESCRIPTION:
                    ProcessRowDescription(packet);
                    break;

                case PgBackendCodes.FUNCTION_CALL_RESPONSE:
                    ProcessFunctionResult(packet);
                    break;

                case PgBackendCodes.COMMAND_COMPLETE:
                    ClosePortal();
                    ProcessTag(packet);
                    break;

                case PgBackendCodes.EMPTY_QUERY_RESPONSE:
                case PgBackendCodes.NODATA:
                    ClearRows();
                    break;

                case PgBackendCodes.PARAMETER_DESCRIPTION:
                    ProcessParameterDescription(packet);
                    break;
                
                // case PgBackendCodes.CLOSE_COMPLETE:
                // case PgBackendCodes.BIND_COMPLETE:
                // case PgBackendCodes.PARSE_COMPLETE:
                //     break;
            }
        }

        private void ProcessTag(PgInputPacket packet)
        {
            _tag = packet.ReadNullString();
            
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

        private void ProcessFunctionResult(PgInputPacket packet)
        {
            _outParameter.Value = packet.ReadValue(_outParameter.DataType, packet.ReadInt32());
        }

        private void ProcessRowDescription(PgInputPacket packet)
        {
            int count = packet.ReadInt16();

            _rowDescriptor.Resize(count);

            for (int i = 0; i < count; i++)
            {
                var name         = packet.ReadNullString();
                var tableOid     = packet.ReadInt32();
                var columnid     = packet.ReadInt16();
                var typeOid      = packet.ReadInt32();
                var typeSize     = packet.ReadInt16();
                var typeModifier = packet.ReadInt32();
                var format       = (PgTypeFormat)packet.ReadInt16();
                var type         = _database.ServerConfiguration.DataTypes.SingleOrDefault(x => x.Oid == typeOid);

                _rowDescriptor.Add(new PgFieldDescriptor(name, tableOid, columnid, typeOid, typeSize, typeModifier, format, type));
            }
        }

        private void ProcessParameterDescription(PgInputPacket packet)
        {
            int oid   = 0;
            int count = packet.ReadInt16();

            _parameters.Clear();
            _parameters.Capacity = count;

            for (int i = 0; i < count; i++)
            {
                oid = packet.ReadInt32();

                _parameters.Add(new PgParameter(_database.ServerConfiguration.DataTypes.SingleOrDefault(x => x.Oid == oid)));
            }
        }

        private void ProcessDataRow(PgInputPacket packet)
        {
            var count  = packet.ReadInt16();
            var values = new object[count];

            for (int i = 0; i < values.Length; i++)
            {
                int length = packet.ReadInt32();

                if (length == -1)
                {
                    values[i] = DBNull.Value;
                }
                else
                {
                    var descriptor = _rowDescriptor[i];
                    var formatCode = descriptor.Type.Format;

                    if (_status == PgStatementStatus.OnQuery)
                    {
                        formatCode = descriptor.Format;
                    }

                    values[i] = packet.ReadFormattedValue(descriptor.Type, formatCode, length);
                }
            }

            _rows.Enqueue(values);
        }
        
        private void ClearRows()
        {
            _rows.Clear();

            _hasRows        = false;
            _allRowsFetched = false;
        }        
    }
}
