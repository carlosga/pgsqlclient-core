// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgTransaction
        : DbTransaction
    {
        private PgConnection            _connection;
        private IsolationLevel          _isolationLevel;
        private PgTransactionInternal   _innerTransaction;
        private bool                    _disposed;
        private bool                    _isUpdated;

        public override IsolationLevel IsolationLevel
        {
            get { return _isolationLevel; }
        }

        protected override DbConnection DbConnection
        {
            get { return _connection; }
        }

        internal bool IsUpdated
        {
            get { return _isUpdated; }
        }

        private PgTransaction()
            : this(null)
        {
        }

        internal PgTransaction(PgConnection connection)
            : this(connection, IsolationLevel.ReadCommitted)
        {
        }

        internal PgTransaction(PgConnection connection, IsolationLevel isolationLevel)
        {
            _connection       = connection;
            _isolationLevel   = isolationLevel;
            _innerTransaction = connection.InnerConnection.Database.BeginTransaction(IsolationLevel);
        }

        ~PgTransaction()
        {
            Dispose(false);
        }

        public override void Commit()
        {
            CheckTransaction();

            _innerTransaction.Commit();

            _isUpdated = true;
        }

        public override void Rollback()
        {
            CheckTransaction();

            _innerTransaction.Rollback();

            _isUpdated = true;
        }

        public void Save(string savePointName)
        {
            CheckTransaction();

            _innerTransaction.Save(savePointName);
        }

        public void Commit(string savePointName)
        {
            CheckTransaction();

            _innerTransaction.Commit(savePointName);
        }

        public void Rollback(string savePointName)
        {
            CheckTransaction();

            _innerTransaction.Rollback(savePointName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null && !_isUpdated)
                    {
                        try
                        {
                            // Implicitly roll back if the transaction still valid.
                            Rollback();
                        }
                        finally
                        {
                            _connection       = null;
                            _innerTransaction = null;
                            _disposed         = true;
                            _isUpdated        = true;
                        }
                    }
                }
            }
        }

        private void CheckTransaction()
        {
            if (_isUpdated)
            {
                throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
            }
        }
    }
}