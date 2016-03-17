// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using [Fact];
using System;
using System.Data;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    public class PgTransactionTest
        : PgBaseTest
    {
        [TestMethod]
        public void BeginTransactionTest()
        {
            Console.WriteLine("\r\nStarting transaction");
            PgTransaction transaction = Connection.BeginTransaction();
            transaction.Rollback();
        }

        [TestMethod]
        public void BeginTransactionReadCommittedTest()
        {
            Console.WriteLine("\r\nStarting transaction - ReadCommitted");
            PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            transaction.Rollback();
        }

        [TestMethod]
        public void BeginTransactionSerializableTest()
        {
            Console.WriteLine("\r\nStarting transaction - Serializable");
            PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.Serializable);
            transaction.Rollback();
        }

        [TestMethod]
        public void CommitTest()
        {
            Console.WriteLine("\r\nTestin transaction Commit");
            PgTransaction transaction = Connection.BeginTransaction();
            transaction.Commit();
            transaction.Dispose();
        }

        [TestMethod]
        public void RollbackTest()
        {
            Console.WriteLine("\r\nTestin transaction Rollback");
            PgTransaction transaction = Connection.BeginTransaction();
            transaction.Rollback();
            transaction.Dispose();
        }
    }
}