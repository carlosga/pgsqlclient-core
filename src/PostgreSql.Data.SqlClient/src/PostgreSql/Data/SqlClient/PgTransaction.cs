// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgTransaction
        : DbTransaction
    {
        private PgConnection   _connection;
        private IsolationLevel _isolationLevel;
        private Transaction    _innerTransaction;

        public override IsolationLevel IsolationLevel => _isolationLevel;
        public new      PgConnection   Connection     => _connection;

        protected override DbConnection DbConnection => _connection;

        internal PgTransaction(PgConnection connection, Transaction innerTransaction)
        {
            _connection       = connection;
            _innerTransaction = innerTransaction;
            _isolationLevel   = innerTransaction.IsolationLevel;
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    try
                    {
                        var internalConnection = _connection?.InnerConnection as PgConnectionInternal; 
                        if (internalConnection != null && internalConnection.HasActiveTransaction)
                        {
                            // Implicitly roll back if the transaction still valid.
                            if (_innerTransaction != null)
                            {
                                _innerTransaction.Rollback();
                            }
                        }
                    }
                    finally
                    {
                        _connection       = null;
                        _innerTransaction = null; 
                        _disposed         = true;
                    }
                }

                _disposed = true;
            }
        }
        #endregion
        
        public override void Commit()
        {
            CheckTransaction();

            _innerTransaction.Commit();
        }

        public override void Rollback()
        {
            CheckTransaction();

            _innerTransaction.Rollback();
        }

        public void Save(string savePointName)
        {
            if (string.IsNullOrEmpty(savePointName))
            {
                throw ADP.NullEmptyTransactionName();
            }
            CheckTransaction();

            _innerTransaction.Save(savePointName);
        }

        public void Commit(string savePointName)
        {
            if (string.IsNullOrEmpty(savePointName))
            {
                throw ADP.NullEmptyTransactionName();
            }
            CheckTransaction();

            _innerTransaction.Commit(savePointName);
        }

        public void Rollback(string savePointName)
        {
            if (string.IsNullOrEmpty(savePointName))
            {
                throw ADP.NullEmptyTransactionName();
            }
            CheckTransaction();

            _innerTransaction.Rollback(savePointName);
        }

        internal void Begin()
        {
            _innerTransaction.Begin();
        }

        private void CheckTransaction()
        {
            var internalConnection = _connection?.InnerConnection as PgConnectionInternal; 
            if (internalConnection != null && !internalConnection.HasActiveTransaction)
            {
                throw ADP.TransactionZombied(this);
            }
            internalConnection.ValidateConnectionForExecute(null);
        }
    }
}
