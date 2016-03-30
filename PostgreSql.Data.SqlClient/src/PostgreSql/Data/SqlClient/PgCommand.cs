// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgCommand
        : DbCommand
    {
        private PgConnection          _connection;
        private PgTransaction         _transaction;
        private PgParameterCollection _parameters;
        private UpdateRowSource       _updatedRowSource;
        private PgStatement           _statement;
        private WeakReference         _activeDataReader;
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
                if (_connection != null && _activeDataReader != null && _activeDataReader.IsAlive)
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
                if (_connection != null && _activeDataReader != null && _activeDataReader.IsAlive)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }

                _transaction = value as PgTransaction;
            }
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

                    base.Dispose(disposing);
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

            return InternalExecuteNonQuery();
        }

        public new PgDataReader ExecuteReader() => ExecuteReader(CommandBehavior.Default);
               
        public new PgDataReader ExecuteReader(CommandBehavior behavior) 
        {
            CheckCommand();

            InternalExecuteReader(behavior);

            _activeDataReader = new WeakReference(new PgDataReader(_connection, this));

            return _activeDataReader.Target as PgDataReader;
        }
        
        public override object ExecuteScalar()
        {
            CheckCommand();

            return InternalExecuteScalar();
        }

        public override void Prepare() 
        {
            CheckCommand();

            InternalPrepare();
        }

        protected override DbParameter CreateDbParameter() => CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
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

            _statement.Prepare(_parameters);

            if (_queryIndex == 0)
            {
                // Add the command to the internal connection prepared statements
                _connection.InnerConnection.AddPreparedCommand(this);
            }
        }

        internal int InternalExecuteNonQuery()
        {
            InternalPrepare();

            var recordsAffected = _statement.ExecuteNonQuery(_parameters);

            InternalSetOutputParameters();

            return recordsAffected;
        }

        private void InternalExecuteReader(CommandBehavior behavior)
        {
            _commandBehavior = behavior;

            InternalPrepare();

            if (_commandBehavior.HasBehavior(CommandBehavior.Default)
             || _commandBehavior.HasBehavior(CommandBehavior.SequentialAccess)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleResult)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleRow)
             || _commandBehavior.HasBehavior(CommandBehavior.CloseConnection))
            {
                _statement.ExecuteReader(_parameters);
            }
        }

        internal object InternalExecuteScalar()
        {
            InternalPrepare();

            return _statement.ExecuteScalar(_parameters);
        }

        internal bool NextResult()
        {
            if (!_connection.MultipleActiveResultSets || _queries.IsEmpty())
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

        internal void InternalClose()
        {
            try
            {
                if (_activeDataReader != null && _activeDataReader.IsAlive)
                {
                    var reader = _activeDataReader.Target as PgDataReader;
                    if (!reader.IsClosed)
                    {
                        reader.Close();
                    }
                }

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
            _activeDataReader = null;
            
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

        private void CheckCommand([System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                throw new InvalidOperationException($"{memberName} requires an open and available Connection. The connection's current state is closed.");
            }
            if (_activeDataReader != null && _activeDataReader.IsAlive)
            {
                throw new InvalidOperationException("There is already an open DataReader associated with this Command which must be closed first.");
            }

            if (_transaction == null && _connection.InnerConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException($"{memberName} requires the command to have a transaction when the connection assigned to the command is in a pending local transaction. The Transaction property of the command has not been initialized.");
            }

            if (_transaction != null && !_connection.Equals(Transaction.Connection))
            {
                throw new InvalidOperationException("The transaction is either not associated with the current connection or has been completed.");
            }

            if (_commandText == null || _commandText.Length == 0)
            {
                throw new InvalidOperationException("The command text for this Command has not been set.");
            }
        }
    }
}
