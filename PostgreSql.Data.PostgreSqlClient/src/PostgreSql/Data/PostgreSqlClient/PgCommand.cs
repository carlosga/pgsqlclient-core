// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgCommand
        : DbCommand
    {
        private PgConnection          _connection;
        private PgTransaction         _transaction;
        private PgParameterCollection _parameters;
        private UpdateRowSource       _updatedRowSource;
        private PgStatement           _statement;
        private PgDataReader          _activeDataReader;
        private CommandBehavior       _commandBehavior;
        private CommandType           _commandType;
        private List<string>          _namedParameters;
        private List<string>          _queries;
        private int                   _queryIndex;
        private string                _commandText;
        private int                   _commandTimeout;
        private bool                  _designTimeVisible;

        public override string CommandText
        {
            get { return _commandText; }
            set
            {
                if (_statement != null && !String.IsNullOrEmpty(_commandText) && _commandText != value)
                {
                    InternalClose();
                }

                _commandText = value;
            }
        }

        public override CommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }

        public override int CommandTimeout
        {
            get { return _commandTimeout; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("The property value assigned is less than 0.");
                }

                _commandTimeout = value;
            }
        }

        public override bool DesignTimeVisible
        {
            get { return _designTimeVisible; }
            set { _designTimeVisible = value; }
        }
        
        public new PgParameterCollection Parameters
        {
            get { return _parameters; }
        }        

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _updatedRowSource; }
            set { _updatedRowSource = value; }
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
            set
            {
                if (_connection != null && ActiveDataReader != null)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }

                if (_connection != value)
                {
                    if (_transaction != null)
                    {
                        _transaction = null;
                    }

                    InternalClose();
                }

                _connection = value as PgConnection;
            }
        }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        protected override DbTransaction DbTransaction
        {
            get { return _transaction; }
            set
            {
                if (_connection != null && ActiveDataReader != null)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }

                _transaction = value as PgTransaction;
            }
        }

        internal PgDataReader ActiveDataReader
        {
            get { return _activeDataReader; }
            set { _activeDataReader = value; }
        }

        internal CommandBehavior CommandBehavior => _commandBehavior;
        internal PgStatement     Statement       => _statement;
        internal int             RecordsAffected => (_statement?.RecordsAffected ?? -1);
        internal bool            IsDisposed      => _disposed;

        internal string CurrentCommandText
        {
            get { return ((_queries != null) ? _queries[_queryIndex] : _commandText); }
        }

        public PgCommand()
            : base()
        {
            _commandText       = String.Empty;
            _commandType       = CommandType.Text;
            _commandTimeout    = 30;
            _updatedRowSource  = UpdateRowSource.Both;
            _commandBehavior   = CommandBehavior.Default;
            _designTimeVisible = false;
            _parameters        = new PgParameterCollection();
            _namedParameters   = new List<string>();
        }

        public PgCommand(string cmdText)
            : this(cmdText, null, null)
        {
        }

        public PgCommand(string cmdText, PgConnection connection)
            : this(cmdText, connection, null)
        {
        }

        public PgCommand(string cmdText, PgConnection connection, PgTransaction transaction)
            : this()
        {
            _commandText = cmdText;
            _connection  = connection;
            _transaction = transaction;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    try
                    {
                        InternalClose();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _connection       = null;
                        _transaction      = null;
                        _parameters       = null;
                        _namedParameters  = null;
                        _commandText      = null;
                    }                    
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgCommand() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        // public void Dispose()
        // {
        //     // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //     Dispose(true);
        //     // TODO: uncomment the following line if the finalizer is overridden above.
        //     // GC.SuppressFinalize(this);
        // }
        #endregion
        
        public override void Cancel()
        {
            throw new NotSupportedException();
        }
        
        public new PgParameter CreateParameter() => new PgParameter();

        public override int ExecuteNonQuery()
        {
            CheckCommand();

            try
            {
                return InternalExecuteNonQuery();                
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }                        
        }

        public new PgDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
               
        public new PgDataReader ExecuteReader(CommandBehavior behavior) 
        {
            CheckCommand();

            try
            {
                return InternalExecuteReader(behavior);
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }
        
        public override object ExecuteScalar()
        {
            CheckCommand();

            try
            {
                return InternalExecuteScalar();
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }

        public override void Prepare() 
        {
            CheckCommand();

            try
            {
                InternalPrepare();
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }            
        } 

        protected override DbParameter CreateDbParameter() => CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return InternalExecuteReader(behavior);
        }

        internal void InternalPrepare()
        {
            if (_connection.MultipleActiveResultSets && _queries == null)
            {
                _queries = _commandText.SplitQueries();
            }
            
            string stmtText = CurrentCommandText.ParseNamedParameters(ref _namedParameters);

            if (_commandType == CommandType.StoredProcedure)
            {
                stmtText = stmtText.ToStoredProcedureCall(_parameters);
            }
            
            if (_queryIndex == 0)
            {
                _statement = _connection.InnerConnection.CreateStatement(stmtText);                        
            }
            else
            {
                _statement.StatementText = stmtText;    
            }

            _statement.Prepare();
                                
            if (_queryIndex == 0)
            {
                // Add the command to the internal connection prepared statements
                _connection.InnerConnection.AddPreparedCommand(this);                        
            }                
        }

        internal int InternalExecuteNonQuery()
        {
            InternalPrepare();
            
            SetParameterValues();
            
            var recordsAffected = _statement.ExecuteNonQuery();
            
            InternalSetOutputParameters();
            
            return recordsAffected;
        }

        private PgDataReader InternalExecuteReader(CommandBehavior behavior)
        {
            _commandBehavior = behavior;

            InternalPrepare();

            if (_commandBehavior.HasBehavior(CommandBehavior.Default)
             || _commandBehavior.HasBehavior(CommandBehavior.SequentialAccess)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleResult)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleRow)
             || _commandBehavior.HasBehavior(CommandBehavior.CloseConnection))
            {
                SetParameterValues();
                
                _statement.ExecuteReader();
            }

            return _activeDataReader = new PgDataReader(_connection, this);
        }
        
        internal object InternalExecuteScalar()
        {
            InternalPrepare();
                            
            SetParameterValues();
            
            return _statement.ExecuteScalar();
        }

        internal bool NextResult()
        {
            try
            {
                if (!_connection.MultipleActiveResultSets)
                {
                    return false;
                }
                
                // Try to advance to the next query
                ++_queryIndex;                
                if (_queryIndex >= _queries.Count)
                {
                    return false;
                }

                InternalExecuteReader(_commandBehavior);
                
                return true;
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }

        internal void InternalClose()
        {
            try
            {
                _connection.InnerConnection.RemovePreparedCommand(this);
                
                // Closing the prepared statement closes all his portals too.
                _statement.Close();
            }
            catch
            {
            }
            finally
            {
                _namedParameters?.Clear();
                
                _statement        = null;
                _activeDataReader = null;
                _queries          = null;
                _queryIndex       = 0;
           }
        }

        internal void InternalSetOutputParameters()
        {
            if (CommandType != CommandType.StoredProcedure || _parameters.Count == 0 || !_statement.HasRows)
            {
                return;
            }
            
            var row = _statement.FetchRow();   

            if (!row.IsEmpty())
            {
                for (int i = 0; i < _parameters.Count; ++i)
                {
                    if (_parameters[i].Direction != ParameterDirection.Input)
                    {
                        _parameters[i].Value = row[i];
                    }
                }
            }
        }

        private void CheckCommand()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException("Connection must valid and open");
            }
            if (ActiveDataReader != null)
            {
                throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
            }

            if (_transaction == null && _connection.InnerConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException("Execute requires the Command object to have a Transaction object when the Connection object assigned to the command is in a pending local transaction.  The Transaction property of the Command has not been initialized.");
            }

            if (_transaction != null && !_connection.Equals(Transaction.Connection))
            {
                throw new InvalidOperationException("Command Connection is not equal to Transaction Connection");
            }

            if (_commandText == null || _commandText.Length == 0)
            {
                throw new InvalidOperationException("The command text for this Command has not been set.");
            }
        }

        private void SetParameterValues()
        {
            if (_parameters == null && _parameters.Count == 0)
            {
                return;
            }

            int index = 0;

            for (int i = 0; i < _statement.Parameters.Count; ++i)
            {
                if (_parameters[index].Direction == ParameterDirection.Output
                 || _parameters[index].Direction == ParameterDirection.ReturnValue)
                {
                    continue;
                }
                
                index = i;
                
                if (_namedParameters.Count > 0)
                {
                    index = _parameters.IndexOf(_namedParameters[i]);
                }

                if (_parameters[index].Value == DBNull.Value)
                {
                    _statement.Parameters[i].Value = null;
                }
                else
                {
                    _statement.Parameters[i].Value = _parameters[index].Value;
                }
            }
        }
    }
}
