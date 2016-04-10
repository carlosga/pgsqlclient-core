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
        private static readonly List<PgParameter> s_EmptyParameters = new List<PgParameter>();

        struct Query
        {
            public string            CommandText;
            public List<PgParameter> Parameters;
        }

        private PgConnection          _connection;
        private PgTransaction         _transaction;
        private PgParameterCollection _parameters;
        private UpdateRowSource       _updatedRowSource;
        private Statement             _statement;
        private WeakReference         _activeDataReader;
        private CommandBehavior       _commandBehavior;
        private CommandType           _commandType;
        private List<Query>           _queries;
        private int                   _queryIndex;
        private string                _commandText;
        private int                   _commandTimeout;
        private bool                  _designTimeVisible;

        public override string CommandText
        {
            get { return _commandText; }
            set
            {
                if (_commandText != value)
                {
                    if (_statement != null && !String.IsNullOrEmpty(_commandText) && _commandText != value)
                    {
                        InternalClose();
                    }

                    _commandText = value;

                    ParseComandText();
                }
            }
        }

        public override CommandType CommandType
        {
            get { return _commandType; }
            set
            { 
                if (_commandType != value)
                {
                    _commandType = value;
                    ParseComandText();
                } 
            }
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
                    _transaction = null;
                    InternalClose();
                    ParseComandText();
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
        internal Statement       Statement       => _statement;
        internal int             RecordsAffected => (_statement?.RecordsAffected ?? -1);
        internal bool            IsDisposed      => _disposed;

        private Query CurrentQuery => _queries[_queryIndex];

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
            _queries           = new List<Query>();
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

            ParseComandText();
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
                        _queries     = null;
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

            if (_queries == null || _queries.Count <= 1) 
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
                _statement = _connection.InnerConnection.CreateStatement(CurrentQuery.CommandText);
            }
            else
            {
                _statement.StatementText = CurrentQuery.CommandText;
            }

            if (_statement.State == StatementState.Initial)
            {
                _statement.Prepare(CurrentQuery.Parameters);
            }

            if (_queryIndex == 0)
            {
                _connection.InnerConnection.AddCommand(this);
            }
        }

        internal int InternalExecuteNonQuery()
        {
            InternalPrepare();

            var recordsAffected = _statement.ExecuteNonQuery(CurrentQuery.Parameters);

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
                _queryIndex = 0;
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
                _statement.ExecuteReader(behavior, CurrentQuery.Parameters);
            }
        }

        internal object InternalExecuteScalar()
        {
            InternalPrepare();

            return _statement.ExecuteScalar(CurrentQuery.Parameters);
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

                // Closing the prepared statement closes all his portals too.
                _statement.Close();
            }
            catch
            {
            }
            finally
            {
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

        private void ParseComandText()
        {
            _queryIndex = 0;
            _queries.Clear();

            var queries = _commandText.SplitCommandText();

            if (queries != null && queries.Count > 0)
            {
                _queries.Capacity = queries.Count;
                
                for (int i = 0; i < queries.Count; i++)
                {
                    var query = new Query();
                    var info  = queries[i].ParseCommandText();

                    if (info.Item2 != null && info.Item2.Count > 0)
                    {
                        query.Parameters = new List<PgParameter>(info.Item2.Count);
                        for (int j = 0; j < info.Item2.Count; j++)
                        {
                            query.Parameters.Add(_parameters[info.Item2[j]]);
                        }
                    }
                    else
                    {
                        query.Parameters = s_EmptyParameters;
                    }

                    if (_commandType == CommandType.StoredProcedure)
                    {
                        query.CommandText = info.Item1.ToStoredProcedureCall(_parameters);
                    }
                    else
                    {
                        query.CommandText = info.Item1;
                    }

                    _queries.Add(query);
                }
            }
        }
    }
}
