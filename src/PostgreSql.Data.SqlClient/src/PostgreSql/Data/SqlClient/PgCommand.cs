// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

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
            get => _commandText ?? string.Empty;
            set
            {
                if (_commandText != value)
                {
                    if (_statement != null && !string.IsNullOrEmpty(_commandText))
                    {
                        Unprepare();
                    }

                    _commandText = value;
                }
            }
        }

        public override CommandType CommandType
        {
            get => _commandType;
            set => _commandType = value;
        }

        public override int CommandTimeout
        {
            get => _commandTimeout;
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidCommandTimeout(value);
                }

                _commandTimeout = value;
            }
        }

        public override bool DesignTimeVisible
        {
            get => _designTimeVisible;
            set => _designTimeVisible = value;
        }

        public new PgParameterCollection Parameters => _parameters;

        public override UpdateRowSource UpdatedRowSource
        {
            get => _updatedRowSource;
            set => _updatedRowSource = value;
        }

        public new PgConnection Connection
        {
            get => _connection;
            set
            {
                if (HasLiveReader)
                {
                    throw ADP.OpenReaderExists();
                }

                if (_connection != value)
                {
                    if (_connection != null)
                    {
                        InternalClose();
                    }
                    _connection  = value;
                    _transaction = null;
                }
            }
        }

        public new PgTransaction Transaction
        {
            get => _transaction;
            set
            {
                if (HasLiveReader)
                {
                    throw ADP.OpenReaderExists();
                }
                
                if (value != _transaction)
                {
                    _transaction = value;
                }
            }
        }

        public int FetchSize
        {
            get => _fetchSize;
            set
            {
                if (value < 0)
                {
                    throw ADP.InvalidFetchSize(value);
                }
                if (HasLiveReader)
                {
                    throw ADP.OpenReaderExists();
                }
                _fetchSize = value; 
            }
        }

        protected override DbConnection DbConnection
        {
            get => _connection;
            set => Connection = value as PgConnection;
        }

        protected override DbTransaction DbTransaction
        {
            get => _transaction;
            set => Transaction = value as PgTransaction;
        }

        protected override DbParameterCollection DbParameterCollection => _parameters;

        internal CommandBehavior CommandBehavior => _commandBehavior;
        internal Statement       Statement       => _statement;
        internal int             RecordsAffected => (_statement?.RecordsAffected ?? -1);
        internal bool            IsDisposed      => _disposed;

        private bool HasLiveReader => (FindLiveReader() != null);

        public PgCommand()
            : base()
        {
            _commandText       = string.Empty;
            _commandType       = CommandType.Text;
            _commandTimeout    = 30;
            _updatedRowSource  = UpdateRowSource.Both;
            _commandBehavior   = CommandBehavior.Default;
            _designTimeVisible = false;
            _parameters        = new PgParameterCollection();
            _commands          = new List<string>(1);
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
                    InternalClose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed     = true;
                _connection   = null;
                _transaction  = null;
                _parameters   = null;
                _commandText  = null;
                _commands     = null;
                _commandIndex = -1;
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
            if (_statement != null && _statement.IsExecuting)
            {
                _statement.Cancel();
            }
        }

        public new PgParameter CreateParameter() => new PgParameter();

        public override int ExecuteNonQuery()
        {
            CheckCommand();

            if (!_connection.MultipleActiveResultSets)
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

            return new PgDataReader(_connection, this);
        }

        public override object ExecuteScalar()
        {
            CheckCommand();

            if (!_connection.MultipleActiveResultSets)
            {
                return InternalExecuteScalar();
            }
            else
            {
                return InternalExecuteScalarMars();
            }
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

        internal bool NextResult()
        {
            if (!NextCommandText())
            {
                return false;
            }

            InternalExecuteReader(_commandBehavior);

            return true;
        }

        internal void CloseCommandFromConnection()
        {
            InternalClose();
        }

        internal void Unprepare()
        {
            if (_disposed)
            {
                return;
            }
            try
            {
                var reader = FindLiveReader();

                reader?.CloseReaderFromCommand();
                _statement?.Dispose();
                _commands?.Clear();
            }
            catch
            {
            }
            finally
            {
                _statement    = null;
                _commandIndex = 0;
            }            
        }

        internal void InternalClose()
        {
            Unprepare();
            _connection?.RemoveWeakReference(this);            
        }

        private void InternalPrepare()
        {
            var internalConnection = _connection.InnerConnection as PgConnectionInternal; 

            if (_statement == null)
            {
                _statement = internalConnection.CreateStatement();
            }
            else if (_statement.IsPrepared)
            {
                return;
            }

            _commands.Clear();

            if (_connection.MultipleActiveResultSets)
            {
                _commandText.SplitCommandText(ref _commands);
            }
            else 
            {
                _commands.Add(_commandText);
            }

            _statement.Parameters    = _parameters;
            _statement.CommandType   = _commandType;
            _statement.StatementText = _commands[_commandIndex];
            _statement.FetchSize     = _fetchSize;

            _statement.Prepare();
            _connection.AddWeakReference(this, PgReferenceCollection.CommandTag);
        }

        private int InternalExecuteNonQuery()
        {
            InternalPrepare();

            return _statement.ExecuteNonQuery();
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

        private object InternalExecuteScalar()
        {
            InternalPrepare();
            return _statement.ExecuteScalar();
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
                    finally
                    {
                        _statement.Close();
                    }
                } while (NextCommandText());

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

        private object InternalExecuteScalarMars()
        {
            var errors = new List<PgError>();

            try
            {
                do
                {
                    try
                    {
                        var result = InternalExecuteScalar();
                        if (result != null && errors.Count == 0)
                        {
                            return result;
                        }
                    }
                    catch (PgException pgex)
                    {
                        errors.AddRange(pgex.Errors);
                    }
                    finally
                    {
                        _statement.Close();
                    }
                } while (NextCommandText());

                if (errors.Count > 0)
                {
                    throw new PgException(errors[0].Message, errors);
                }
            }
            finally
            {
                _commandIndex = 0;
            }

            return null;
        }

        private bool NextCommandText()
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

        private PgDataReader FindLiveReader()
        {
            var innerConnection = _connection?.InnerConnection as PgConnectionInternal;
            
            if (innerConnection?.ReferenceCollection != null)
            {
                var references = innerConnection.ReferenceCollection as PgReferenceCollection;

                return references?.FindLiveReader(this);
            }

            return null;
        }

        private void CheckCommand([System.Runtime.CompilerServices.CallerMemberName] string memberName = null)
        {
            if (_connection == null)
            {
                throw ADP.ConnectionRequired(memberName);
            }
            if (_connection.State != ConnectionState.Open)
            {
                throw ADP.OpenConnectionRequired(memberName, _connection.State);
            }
            if (HasLiveReader)
            {
                throw ADP.OpenReaderExists();
            }

            if (_transaction == null && _connection.HasActiveTransaction)
            {
                throw ADP.TransactionRequired(memberName);
            }
            if (_transaction != null && !_connection.Equals(Transaction.Connection))
            {
                throw ADP.TransactionConnectionMismatch();
            }

            if (string.IsNullOrEmpty(_commandText))
            {
                throw ADP.CommandTextRequired(memberName);
            }
        }
    }
}
