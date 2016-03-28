// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgTransaction
        : DbTransaction
    {
        private PgConnection          _connection;
        private IsolationLevel        _isolationLevel;
        private PgTransactionInternal _innerTransaction;

        public override IsolationLevel IsolationLevel => _isolationLevel;
       
        protected override DbConnection DbConnection => _connection;

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
            _innerTransaction = connection.InnerConnection.Database.CreateTransaction(IsolationLevel);
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
                        if (_connection?.InnerConnection != null
                         && _connection.InnerConnection.HasActiveTransaction)
                        {
                            // Implicitly roll back if the transaction still valid.
                            Rollback();
                        }
                    }
                    finally
                    {
                        _connection       = null;
                        _innerTransaction = null; 
                        _disposed         = true;
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgTransaction() {
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

        internal void Begin(string transactionName)
        {
            _innerTransaction.Begin();
            if (transactionName != null && transactionName.Length > 0)
            {
                _innerTransaction.Save(transactionName);   
            }
        }

        private void CheckTransaction()
        {
            if (_connection?.InnerConnection != null
             && !_connection.InnerConnection.HasActiveTransaction)
            {
                throw new InvalidOperationException("This Transaction has completed; it is no longer usable.");
            }
        }
    }
}