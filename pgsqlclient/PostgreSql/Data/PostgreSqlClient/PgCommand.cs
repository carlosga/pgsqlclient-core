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
        private static string GetStmtName()
        {
            return Guid.NewGuid().GetHashCode().ToString();
        }

        private PgConnection          _connection;
        private PgTransaction         _transaction;
        private PgParameterCollection _parameters;
        private UpdateRowSource       _updatedRowSource;
        private PgStatement           _statement;
        private PgDataReader          _activeDataReader;
        private CommandBehavior       _commandBehavior;
        private CommandType           _commandType;
        private List<string>          _namedParameters;
        private bool                  _disposed;
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

        protected override DbParameterCollection DbParameterCollection
        {
            get { return _parameters; }
        }

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

        internal CommandBehavior CommandBehavior
        {
            get { return _commandBehavior; }
        }

        internal PgStatement Statement
        {
            get { return _statement; }
        }

        internal int RecordsAffected
        {
            get
            {
                if (_statement != null)
                {
                    return _statement.RecordsAffected;
                }
                return -1;
            }
        }

        internal bool IsDisposed
        {
            get { return _disposed; }
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

        public override void Cancel()
        {
            // 50.2.7. Canceling Requests in Progress

            // During the processing of a query, the frontend might request cancellation of the query. The cancel request is not sent directly on the open connection 
            // to the backend for reasons of implementation efficiency: we don't want to have the backend constantly checking for new input from the frontend during 
            // query processing. Cancel requests should be relatively infrequent, so we make them slightly cumbersome in order to avoid a penalty in the normal case.

            // To issue a cancel request, the frontend opens a new connection to the server and sends a CancelRequest message, 
            // rather than the StartupMessage message that would ordinarily be sent across a new connection.
            // The server will process this request and then close the connection. For security reasons, no direct reply is made to the cancel request message.

            // A CancelRequest message will be ignored unless it contains the same key data (PID and secret key) passed to the frontend during connection start-up. 
            // If the request matches the PID and secret key for a currently executing backend, the processing of the current query is aborted. 
            // (In the existing implementation, this is done by sending a special signal to the backend process that is processing the query.)

            // The cancellation signal might or might not have any effect — for example, if it arrives after the backend has finished processing the query,
            // then it will have no effect. If the cancellation is effective, it results in the current command being terminated early with an error message.

            // The upshot of all this is that for reasons of both security and efficiency, the frontend has no direct way to tell whether a cancel request has succeeded. 
            // It must continue to wait for the backend to respond to the query. Issuing a cancel simply improves the odds that the current query will finish soon, 
            // and improves the odds that it will fail with an error message instead of succeeding.

            // Since the cancel request is sent across a new connection to the server and not across the regular frontend/backend communication link,
            // it is possible for the cancel request to be issued by any process, not just the frontend whose query is to be canceled. 
            // This might provide additional flexibility when building multiple-process applications. It also introduces a security risk, 
            // in that unauthorized persons might try to cancel queries. 
            // The security risk is addressed by requiring a dynamically generated secret key to be supplied in cancel requests.            

            throw new NotSupportedException();
        }
        
        public new PgParameter CreateParameter() => new PgParameter();

        public override int ExecuteNonQuery()
        {
            CheckCommand();

            InternalPrepare();
            InternalExecute();
            
            InternalSetOutputParameters();

            return _statement.RecordsAffected;            
        }

        public new PgDataReader ExecuteReader(CommandBehavior behavior)
        {
            return InternalExecuteReader(behavior);
        }
        
        public override object ExecuteScalar()
        {
            CheckCommand();

            InternalPrepare();
            InternalExecute();
            
            return _statement.ExecuteScalar();            
        }

        public override void Prepare()
        {
            CheckCommand();

            InternalPrepare();
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        InternalClose();
                    }
                    catch
                    {
                    }
                    finally
                    {
                        _namedParameters?.Clear();

                        _connection       = null;
                        _transaction      = null;
                        _parameters       = null;
                        _namedParameters  = null;
                        _commandText      = null;
                    }
                }

                // release any unmanaged resources
                _disposed = true;
            }
        }

        protected override DbParameter CreateDbParameter() => CreateParameter();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return InternalExecuteReader(behavior);
        }

        internal void InternalPrepare()
        {
            try
            {
                if (_statement        == null
                 || _statement.Status == PgStatementStatus.Initial
                 || _statement.Status == PgStatementStatus.Error)
                {
                    string sql = _commandText;

                    if (_commandType == CommandType.StoredProcedure)
                    {
                        sql = BuildStoredProcedureSql(sql);
                    }

                    string statementName = GetStmtName();
                    string prepareName   = $"PS{statementName}";
                    string portalName    = $"PR{statementName}";
                    string stmtText      = ParseNamedParameters(sql);

                    _statement = _connection.InnerConnection.CreateStatement(prepareName, portalName, stmtText);

                    // Parse statement
                    _statement.Parse();
                    
                    // Describe statement
                    _statement.Describe();
                    
                    // Add the command to the internal connection prepared statements
                    _connection.InnerConnection.AddPreparedCommand(this);
                }
                else
                {
                    // Close existent portal
                    _statement.ClosePortal();
                }
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }

        private PgDataReader InternalExecuteReader(CommandBehavior behavior)
        {
            CheckCommand();

            _commandBehavior = behavior;

            InternalPrepare();

            if (_commandBehavior.HasBehavior(CommandBehavior.Default)
             || _commandBehavior.HasBehavior(CommandBehavior.SequentialAccess)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleResult)
             || _commandBehavior.HasBehavior(CommandBehavior.SingleRow)
             || _commandBehavior.HasBehavior(CommandBehavior.CloseConnection))
            {
                InternalExecute();
            }

            return _activeDataReader = new PgDataReader(_connection, this);
        }

        internal void InternalExecute()
        {
            try
            {
                // Set parameter values
                SetParameterValues();

                // Bind Statement
                _statement.Bind();
                                
                // Execute Statement
                _statement.Execute();
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
                _statement        = null;
                _activeDataReader = null;
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

        private string BuildStoredProcedureSql(string commandText)
        {
            if (commandText.Trim().ToLower().StartsWith("select "))
            {
                return commandText;
            }

            var paramsText = new StringBuilder();

            // Append the stored proc parameter name
            paramsText.Append(commandText);
            paramsText.Append("(");

            for (int i = 0; i < _parameters.Count; ++i)
            {
                var parameter = _parameters[i];

                if (parameter.Direction == ParameterDirection.Input
                 || parameter.Direction == ParameterDirection.InputOutput)
                {
                    // Append parameter name to parameter list
                    paramsText.Append(Parameters[i].ParameterName);

                    if (i != Parameters.Count - 1)
                    {
                        paramsText.Append(",");
                    }
                }
            }

            paramsText.Append(")");
            paramsText.Replace(",)", ")");

            return $"SELECT * FROM {paramsText.ToString()}";
        }

        private string ParseNamedParameters(string sql)
        {
            var builder      = new StringBuilder();
            var paramBuilder = new StringBuilder();
            var inCommas     = false;
            var inParam      = false;
            int paramIndex   = 0;

            _namedParameters.Clear();

            if (sql.IndexOf('@') == -1)
            {
                return sql;
            }
            
            char sym = '\0';

            for (int i = 0; i < sql.Length; ++i)
            {
                sym = sql[i];

                if (inParam)
                {
                    if (Char.IsLetterOrDigit(sym) || sym == '_' || sym == '$')
                    {
                        paramBuilder.Append(sym);
                    }
                    else
                    {
                        _namedParameters.Add(paramBuilder.ToString());
                        paramBuilder.Length = 0;
                        builder.AppendFormat("${0}", ++paramIndex);
                        builder.Append(sym);
                        inParam = false;
                    }
                }
                else
                {
                    if (sym == '\'' || sym == '\"')
                    {
                        inCommas = !inCommas;
                    }
                    else if (!inCommas && sym == '@')
                    {
                        inParam = true;
                        paramBuilder.Append(sym);
                        continue;
                    }

                    builder.Append(sym);
                }
            }

            if (inParam)
            {
                _namedParameters.Add(paramBuilder.ToString());
                builder.AppendFormat("${0}", ++paramIndex);
            }

            return builder.ToString();
        }

        private void SetParameterValues()
        {
            if (_parameters != null && _parameters.Count > 0)
            {
                return;
            }

            int index = 0;

            for (int i = 0; i < _statement.Parameters.Count; ++i)
            {
                index = i;
                
                if (_namedParameters.Count > 0)
                {
                    index = _parameters.IndexOf(_namedParameters[i]);
                }

                if (_parameters[index].Direction == ParameterDirection.Input
                 || _parameters[index].Direction == ParameterDirection.InputOutput)
                {
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
}