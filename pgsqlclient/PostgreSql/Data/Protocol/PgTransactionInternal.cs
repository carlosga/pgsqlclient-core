// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using System.Threading.Tasks;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgTransactionInternal
    {
        private readonly PgDatabase     _database;
        private readonly IsolationLevel _isolationLevel;

        internal PgTransactionInternal(PgDatabase database, IsolationLevel isolationLevel)
        {
            _database       = database;
            _isolationLevel = isolationLevel;
        }

        internal void Begin()
        {
            string sql = "START TRANSACTION ISOLATION LEVEL ";

            switch (_isolationLevel)
            {
                case IsolationLevel.ReadUncommitted:
                    throw new NotSupportedException("Read uncommitted transaction isolation is not supported");

                case IsolationLevel.RepeatableRead:
                    throw new NotSupportedException("Repeatable read transaction isolation is not supported");

                case IsolationLevel.Serializable:
                    sql += "SERIALIZABLE";
                    break;

                case IsolationLevel.ReadCommitted:
                default:
                    sql += "READ COMMITTED";
                    break;
            }

            using (var stmt = _database.CreateStatement(sql))
            {
                Task.Run(async () => await stmt.QueryAsync().ConfigureAwait(false)).Wait();

                if (stmt.Tag != "START TRANSACTION")
                {
                    throw new PgClientException("A transaction is currently active. Parallel transactions are not supported.");
                }
            }
        }

        internal void Commit()
        {
            using (var stmt = _database.CreateStatement("COMMIT TRANSACTION"))
            {
                Task.Run(async () => await stmt.QueryAsync());

                if (stmt.Tag != "COMMIT")
                {
                    throw new PgClientException("There are no transaction for commit.");
                }
            }
        }

        internal void Rollback()
        {
            using (var stmt = _database.CreateStatement("ROLLBACK TRANSACTION"))
            {
                Task.Run(async () => await stmt.QueryAsync().ConfigureAwait(false)).Wait();

                if (stmt.Tag != "ROLLBACK")
                {
                    throw new PgClientException("There are no transaction for rollback.");
                }
            }
        }

        internal void Save(string savePointName)
        {
            if (savePointName == null)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }
            else if (savePointName.Length == 0)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }

            using (var stmt = _database.CreateStatement($"SAVEPOINT {savePointName}"))
            {
                Task.Run(async () => await stmt.QueryAsync().ConfigureAwait(false)).Wait();
            }
        }

        internal void Commit(string savePointName)
        {
            if (savePointName == null)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }
            else if (savePointName.Length == 0)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }

            using (var stmt = _database.CreateStatement($"RELEASE SAVEPOINT {savePointName}"))
            {
                Task.Run(async () => await stmt.QueryAsync().ConfigureAwait(false)).Wait();
            }
        }

        internal void Rollback(string savePointName)
        {
            if (savePointName == null)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }
            else if (savePointName.Length == 0)
            {
                throw new ArgumentException("No transaction name was be specified.");
            }

            using (var stmt = _database.CreateStatement($"ROLLBACK WORK TO SAVEPOINT {savePointName}"))
            {
                Task.Run(async () => await stmt.QueryAsync().ConfigureAwait(false)).Wait();
            }
        }
    }
}
