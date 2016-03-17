// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class PgConnectionInternal
    {
        private PgDatabase    _database;
        private PgConnection  _owner;
        private PgTransaction _activeTransaction;
        private string        _connectionString;
        private long          _created;
        private long          _lifetime;
        private bool          _pooled;

        private ConcurrentDictionary<int, WeakReference<PgCommand>> _preparedCommands;

        internal PgDatabase Database
        {
            get { return _database; }
        }

        internal long Created
        {
            get { return _created; }
            set { _created = value; }
        }

        internal bool Pooled
        {
            get { return _pooled; }
            set { _pooled = value; }
        }

        internal bool HasActiveTransaction
        {
            get { return (_activeTransaction != null && !_activeTransaction.IsUpdated); }
        }

        internal long Lifetime
        {
            get { return _lifetime; }
            set { _lifetime = value; }
        }

        internal string ServerVersion
        {
            get { return _database?.ParameterStatus?["server_version"]; }
        }

        internal PgConnectionInternal(string connectionString)
        {
            _connectionString = connectionString;
            _database         = new PgDatabase(connectionString);
            _preparedCommands = new ConcurrentDictionary<int, WeakReference<PgCommand>>();
            _created          = 0;
            _lifetime         = 0;
        }

        internal void Open(PgConnection owner)
        {
            try
            {
                // Connect
                _database.Open();

                // Grab Data Types Oid's from the database if requested
                FetchDatabaseOids();

                // Update owner
                _owner = owner;
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }

        internal void Close()
        {
            try
            {
                // Dispose Active commands
                ClosePreparedCommands();

                // Rollback active transaction
                DisposeActiveTransaction();

                // Close connection permanently or send it back to the pool
                if (_pooled)
                {
                    PgPoolManager.Instance.GetPool(_connectionString).CheckIn(this);
                }
                else
                {
                    _database.Dispose();
                }
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
            finally
            {
                _owner             = null;
                _activeTransaction = null;
                _database          = null;
                _preparedCommands  = null;
                _connectionString  = null;
                _created           = 0;
                _lifetime          = 0;
                _pooled            = false;
            }
        }

        internal PgTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return BeginTransaction(isolationLevel, null);
        }

        internal PgTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
        {           
            if (_activeTransaction != null)
            {
                throw new InvalidOperationException("A transaction is currently active. Parallel transactions are not supported.");
            }

            try
            {
                _activeTransaction = new PgTransaction(_owner, isolationLevel);                
            }
            catch (PgClientException ex)
            {
                DisposeActiveTransaction();

                throw new PgException(ex);
            }

            return _activeTransaction;
        }

        internal PgCommand CreateCommand()
        {
            return new PgCommand(String.Empty, _owner, _activeTransaction);
        }

        internal void DisposeActiveTransaction()
        {
            // Rollback active transation
            if (HasActiveTransaction)
            {
                _activeTransaction.Dispose();
                _activeTransaction = null;
            }
        }

        internal PgStatement CreateStatement()
        {
            return _database.CreateStatement();
        }

        internal PgStatement CreateStatement(string stmtText)
        {
            return _database.CreateStatement(stmtText);
        }

        internal PgStatement CreateStatement(string parseName, string portalName)
        {
            return _database.CreateStatement(parseName, portalName);
        }

        internal PgStatement CreateStatement(string parseName, string portalName, string stmtText)
        {
            return _database.CreateStatement(parseName, portalName, stmtText);
        }

        internal void ClosePreparedCommands()
        {
            foreach (var commandRef in _preparedCommands)
            {
                PgCommand command = null;

                if (commandRef.Value != null && commandRef.Value.TryGetTarget(out command))
                {
                    command.InternalClose();
                }
            }

            _preparedCommands.Clear();
        }

        internal void AddPreparedCommand(PgCommand command)
        {
            _preparedCommands[command.GetHashCode()] = new WeakReference<PgCommand>(command);
        }

        internal void RemovePreparedCommand(PgCommand command)
        {
            WeakReference<PgCommand> removed = null;

            _preparedCommands.TryRemove(command.GetHashCode(), out removed);
        }

        internal bool Verify()
        {
            bool isValid = true;

            try
            {
                // Try to send a Sync message to the PostgreSQL Server
                _database.Sync();
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }

        internal void FetchDatabaseOids()
        {
            if (!_database.Options.UseDatabaseOids)
            {
                return;
            }

            string sql = "SELECT oid FROM pg_type WHERE typname=@typeName";

            if (_owner != null)
            {
                using (PgCommand command = new PgCommand(sql, _owner))
                {
                    command.Parameters.Add(new PgParameter("@typeName", PgDbType.VarChar));

                    // After the connection gets established we should update the Data Types collection oids
                    foreach (PgType type in Database.DataTypes)
                    {
                        command.Parameters["@typeName"].Value = type.Name;

                        object realOid = command.ExecuteScalar();

                        if (realOid != null && Convert.ToInt32(realOid) != type.Oid)
                        {
                            type.Oid = Convert.ToInt32(realOid);
                        }
                    }
                }
            }
        }
    }
}