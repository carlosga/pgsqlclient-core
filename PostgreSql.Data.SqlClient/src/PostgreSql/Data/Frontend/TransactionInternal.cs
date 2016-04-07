// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using PostgreSql.Data.SqlClient;

namespace PostgreSql.Data.Frontend
{
    internal sealed class TransactionInternal
    {
        private readonly Connection     _connection;
        private readonly IsolationLevel _isolationLevel;

        internal TransactionInternal(Connection connection, IsolationLevel isolationLevel)
        {
            _connection     = connection;
            _isolationLevel = isolationLevel;
        }

        internal void Begin()
        {
            string sql = "START TRANSACTION ISOLATION LEVEL ";

            switch (_isolationLevel)
            {
                case IsolationLevel.ReadUncommitted:
                    sql += "READ UNCOMMITTED";
                    break;

                case IsolationLevel.RepeatableRead:
                    sql += "REPEATABLE READ";
                    break;

                case IsolationLevel.Serializable:
                    sql += "SERIALIZABLE";
                    break;

                case IsolationLevel.ReadCommitted:
                default:
                    sql += "READ COMMITTED";
                    break;
            }

            using (var stmt = _connection.CreateStatement(sql))
            {
                stmt.Query();

                if (stmt.Tag != "START TRANSACTION")
                {
                    throw new PgException("A transaction is currently active. Parallel transactions are not supported.");
                }
            }
        }

        internal void Commit()
        {
            using (var stmt = _connection.CreateStatement("COMMIT TRANSACTION"))
            {
                stmt.Query();

                if (stmt.Tag != "COMMIT")
                {
                    throw new PgException("There are no transaction for commit.");
                }
            }
        }

        internal void Rollback()
        {
            using (var stmt = _connection.CreateStatement("ROLLBACK TRANSACTION"))
            {
                stmt.Query();

                if (stmt.Tag != "ROLLBACK")
                {
                    throw new PgException("There are no transaction for rollback.");
                }
            }
        }

        internal void Save(string savePointName)
        {
            if (savePointName == null || savePointName.Length == 0)
            {
                throw new ArgumentException("Invalid transaction or invalid name for a point at which to save within the transaction.");
            }

            using (var stmt = _connection.CreateStatement($"SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }

        internal void Commit(string savePointName)
        {
            if (savePointName == null || savePointName.Length == 0)
            {
                throw new ArgumentException("Invalid transaction or invalid name for a point at which to save within the transaction.");
            }

            using (var stmt = _connection.CreateStatement($"RELEASE SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }

        internal void Rollback(string savePointName)
        {
            if (savePointName == null || savePointName.Length == 0)
            {
                throw new ArgumentException("Invalid transaction or invalid name for a point at which to save within the transaction.");
            }

            using (var stmt = _connection.CreateStatement($"ROLLBACK WORK TO SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }
    }
}
