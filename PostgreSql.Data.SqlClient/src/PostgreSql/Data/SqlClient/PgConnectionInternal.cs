// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgConnectionInternal
    {
        private PgDatabase    _database;
        private PgConnection  _owner;
        private WeakReference _activeTransaction;
        private string        _connectionString;
        private long          _created;
        private long          _lifetime;
        private bool          _pooled;

        private ConcurrentDictionary<int, WeakReference<PgCommand>> _preparedCommands;

        internal string        ServerVersion     => _database.SessionData.ServerVersion;
        internal PgDatabase    Database          => _database;
        internal PgTransaction ActiveTransaction => _activeTransaction?.Target as PgTransaction;

        internal bool HasActiveTransaction
        {
            get 
            {
                 return (_activeTransaction != null 
                      && _activeTransaction.IsAlive
                      && _database.TransactionStatus != PgTransactionStatus.Default); 
            }
        }

        internal long Lifetime
        {
            get { return _lifetime; }
            set { _lifetime = value; }
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
                    _database.ReleaseCallbacks();
                    
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

        internal PgTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
        {
            var transaction = new PgTransaction(_owner, isolationLevel);
            transaction.Begin(transactionName);

            _activeTransaction = new WeakReference(transaction);

            return transaction;
        }

        internal PgCommand CreateCommand() => new PgCommand(String.Empty, _owner, ActiveTransaction);

        internal void DisposeActiveTransaction()
        {
            if (HasActiveTransaction)
            {
                ActiveTransaction.Dispose();
                _activeTransaction = null;
            }
        }

        internal PgStatement CreateStatement() => _database.CreateStatement();

        internal PgStatement CreateStatement(string stmtText) => _database.CreateStatement(stmtText);

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
                // Try to send a Sync message
                _database.Sync();
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
