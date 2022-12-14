// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System.Data;
using System.Data.Common;

namespace PostgreSql.Data.Frontend
{
    internal sealed class Transaction
    {
        private readonly Connection     _connection;
        private readonly IsolationLevel _isolationLevel;

        internal IsolationLevel IsolationLevel => _isolationLevel; 

        internal Transaction(Connection connection, IsolationLevel isolationLevel)
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
                case IsolationLevel.Snapshot:
                    sql += "REPEATABLE READ";
                    break;

                case IsolationLevel.Serializable:
                    sql += "SERIALIZABLE";
                    break;

                case IsolationLevel.ReadCommitted:
                case IsolationLevel.Unspecified:
                    sql += "READ COMMITTED";
                    break;

                default:
                    throw ADP.InvalidIsolationLevel(_isolationLevel);
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
            using (var stmt = _connection.CreateStatement($"SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }

        internal void Commit(string savePointName)
        {
            using (var stmt = _connection.CreateStatement($"RELEASE SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }

        internal void Rollback(string savePointName)
        {
            using (var stmt = _connection.CreateStatement($"ROLLBACK WORK TO SAVEPOINT {savePointName}"))
            {
                stmt.Query();
            }
        }
    }
}
