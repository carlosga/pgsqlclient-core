// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
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
        private Statement             _statement;
        private WeakReference         _activeDataReader;
        private CommandBehavior       _commandBehavior;
        private CommandType           _commandType;
        private List<string>          _commands;
        private int                   _commandIndex;
        private string                _commandText;
        private int                   _commandTimeout;
        private bool                  _designTimeVisible;
        private int                   _fetchSize;

        public override string CommandText
        {
            get { return _commandText ?? String.Empty; }
            set
            {
                if (_commandText != value)
                {
                    if (_statement != null && !String.IsNullOrEmpty(_commandText) && _commandText != value)
                    {
                        InternalClose();
                    }

                    _commandText = value;
                    _commands    = _commandText.SplitCommandText();
                }
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

        public new PgParameterCollection Parameters => _parameters;

        public override UpdateRowSource UpdatedRowSource
        {
            get { return _updatedRowSource; }
            set { _updatedRowSource = value; }
        }

        public new PgConnection Connection
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
                    InternalClose();
                    _connection  = value;
                    _transaction = null;
                }
            }
        }

        public new PgTransaction Transaction
        {
            get { return _transaction; }
            set
            {
                if (_connection != null && _activeDataReader != null && _activeDataReader.IsAlive)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }
                
                if (value != _transaction)
                {
                    _transaction = value;
                }
            }
        }

        public int FetchSize
        {
            get { return _fetchSize; }
            set
            {
                if (_fetchSize < 0)
                {
                    throw new ArgumentException("The property value assigned is less than 0.");
                }
                if (_connection != null && _activeDataReader != null && _activeDataReader.IsAlive)
                {
                    throw new InvalidOperationException("There is already an open DataReader associated with this Connection which must be closed first.");
                }
                _fetchSize = value; 
            }
        }

        protected override DbConnection DbConnection
        {
            get { return Connection; }
            set { Connection = value as PgConnection; }
        }

        protected override DbTransaction DbTransaction
        {
            get { return Transaction; }
            set { Transaction = value as PgTransaction; }
        }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        internal CommandBehavior CommandBehavior => _commandBehavior;
        internal Statement       Statement       => _statement;
        internal int             RecordsAffected => (_statement?.RecordsAffected ?? -1);
        internal bool            IsDisposed      => _disposed;

        private string CurrentCommand => _commands[_commandIndex];

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
            _commands          = new List<string>();
            _fetchSize         = 200;
        }

        public PgCommand(string commandText)
            : this(commandText, null, null)
        {
        }

        public PgCommand(string commandText, PgConnection connection)
            : this(commandText, connection, null)
        {
        }

        public PgCommand(string commandText, PgConnection connection, PgTransaction transaction)
            : this()
        {
            CommandText = commandText;
            Connection  = connection;
            Transaction = transaction;
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
                        _connection  = null;
                        _transaction = null;
                        _parameters  = null;
                        _commandText = null;
                        _commands    = null;
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
            if (_statement != null && _statement.IsRunning)
            {
                _statement.Cancel();
            }
        }

        public new PgParameter CreateParameter() => new PgParameter();

        public override int ExecuteNonQuery()
        {
            CheckCommand();

            if (_commands.Count == 1) 
            {
                return InternalExecuteNonQuery();
            }
            else
            {
                return InternalExecuteNonQueryMars();
            }
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
            if (_statement == null)
            {
                _statement = _connection.InnerConnection.CreateStatement();
                _statement.Parameters = _parameters; 
            }

            _statement.CommandType   = _commandType;
            _statement.StatementText = CurrentCommand;
            _statement.FetchSize     = _fetchSize;

            if (_statement.State == StatementState.Initial)
            {
                _statement.Prepare();
            }

            if (_commandIndex == 0)
            {
                _connection.InnerConnection.AddCommand(this);
            }
        }

        internal int InternalExecuteNonQuery()
        {
            InternalPrepare();

            var recordsAffected = _statement.ExecuteNonQuery();

            InternalSetOutputParameters();

            return recordsAffected;
        }
        
        private int InternalExecuteNonQueryMars()
        {
            int totalAffected = -1;
            var errors        = new List<PgError>();

            try
            {
                do
                {
                    try
                    {
                        var affected   = InternalExecuteNonQuery();
                        totalAffected += ((affected != -1) ? affected : 0);
                    }
                    catch (PgException pgex)
                    {
                        errors.AddRange(pgex.Errors);
                    }
                } while(PrepareNextMarsCommandText());

                if (errors.Count > 0)
                {
                    throw new PgException(errors[0].Message, errors);
                }
            }
            finally
            {
                _commandIndex = 0;
            }

            return totalAffected;
        }

        private void InternalExecuteReader(CommandBehavior behavior)
        {
            _commandBehavior = behavior;

            InternalPrepare();

            if (behavior.HasBehavior(CommandBehavior.Default)
             || behavior.HasBehavior(CommandBehavior.SequentialAccess)
             || behavior.HasBehavior(CommandBehavior.SingleResult)
             || behavior.HasBehavior(CommandBehavior.SingleRow)
             || behavior.HasBehavior(CommandBehavior.CloseConnection))
            {
                _statement.ExecuteReader(behavior);
            }
        }

        internal object InternalExecuteScalar()
        {
            InternalPrepare();
            return _statement.ExecuteScalar();
        }

        internal bool NextResult()
        {
            if (!PrepareNextMarsCommandText())
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

                _connection.InnerConnection.RemoveCommand(this);
                _statement.Dispose();
            }
            catch
            {
            }
            finally
            {
                _statement        = null;
                _activeDataReader = null;
                _commandIndex     = 0;
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

            for (int i = 0; i < _parameters.Count; ++i)
            {
                if (_parameters[i].Direction != ParameterDirection.Input)
                {
                    _parameters[i].Value = row[i];
                }
            }
        }

        private bool PrepareNextMarsCommandText()
        {
            if (!_connection.MultipleActiveResultSets)
            {
                return false;
            }

            // Try to advance to the next query
            if ((_commandIndex + 1) >= _commands.Count)
            {
                return false;
            }

            ++_commandIndex;

            return true;
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
