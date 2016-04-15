// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgConnectionInternal
    {
        private Connection    _connection;
        private PgConnection  _owner;
        private WeakReference _activeTransaction;
        private long          _created;
        private long          _lifetime;
        private bool          _pooled;

        private ConcurrentDictionary<int, WeakReference<PgCommand>> _commands;

        internal Connection    Connection               => _connection;
        internal PgTransaction ActiveTransaction        => _activeTransaction?.Target as PgTransaction;
        internal string        ServerVersion            => _connection.SessionData.ServerVersion;
        internal string        Database                 => _connection?.Database;
        internal string        DataSource               => _connection?.DataSource;
        internal int           ConnectionTimeout        => (_connection?.ConnectionTimeout ?? 15);
        internal int           PacketSize               => (_connection?.PacketSize ?? 8192);
        internal bool          MultipleActiveResultSets => (_connection?.MultipleActiveResultSets ?? false);
        internal string        SearchPath               => (_connection?.SearchPath);
        internal bool          Pooling                  => (_connection?.Pooling ?? false);
        internal bool          Encrypt                  => (_connection?.Encrypt ?? false);

        internal bool HasActiveTransaction
        {
            get
            {
                 return (_activeTransaction != null 
                      && _activeTransaction.IsAlive
                      && _connection.TransactionState != TransactionState.Default); 
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

        internal PgConnectionInternal(ConnectionOptions connectionOptions)
        {
            _connection = new Connection(connectionOptions);
            _commands   = new ConcurrentDictionary<int, WeakReference<PgCommand>>();
            _created    = 0;
            _lifetime   = 0;
        }

        internal void Open(PgConnection owner)
        {
            // Connect
            _connection.Open();

            // Update owner
            _owner = owner;
        }

        internal void Close()
        {
            try
            {
                // Dispose Active commands
                CloseCommands();

                // Rollback active transaction
                DisposeActiveTransaction();

                // Close connection permanently or send it back to the pool
                if (_pooled)
                {
                    _connection.ReleaseCallbacks();

                    PgPoolManager.Instance.GetPool(_connection.ConnectionString).CheckIn(this);
                }
                else
                {
                    _connection.Dispose();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _owner             = null;
                _activeTransaction = null;
                _connection        = null;
                _commands  = null;
                _created           = 0;
                _lifetime          = 0;
                _pooled            = false;
            }
        }
        
        internal void ChangeDatabase(string database)
        {
            _connection.ChangeDatabase(database);
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

        internal Statement CreateStatement() => _connection.CreateStatement();

        internal Statement CreateStatement(string stmtText) => _connection.CreateStatement(stmtText);

        internal void CloseCommands()
        {
            foreach (var commandRef in _commands)
            {
                PgCommand command = null;

                if (commandRef.Value != null && commandRef.Value.TryGetTarget(out command))
                {
                    command.InternalClose();
                }
            }
            
            _commands.Clear();
        }

        internal void AddCommand(PgCommand command)
        {
            _commands[command.GetHashCode()] = new WeakReference<PgCommand>(command);
        }

        internal void RemoveCommand(PgCommand command)
        {
            WeakReference<PgCommand> removed = null;

            _commands.TryRemove(command.GetHashCode(), out removed);
        }

        internal bool Verify()
        {
            bool isValid = true;

            try
            {
                // Try to send a Sync message
                _connection.Sync();
            }
            catch
            {
                isValid = false;
            }

            return isValid;
        }
    }
}
