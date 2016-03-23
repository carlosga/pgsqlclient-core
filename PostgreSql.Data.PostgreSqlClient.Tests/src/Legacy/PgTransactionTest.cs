// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using System;
using System.Data;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    [TestFixture]
    [Ignore("Needs configuration")]
    public class PgTransactionTest
        : PgBaseTest
    {
        [Test]
        public void BeginTransactionTest()
        {
            Console.WriteLine("\r\nStarting transaction");
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                transaction.Rollback();   
            }
        }

        [Test]
        public void BeginTransactionReadCommittedTest()
        {
            Console.WriteLine("\r\nStarting transaction - ReadCommitted");
            using (PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                transaction.Rollback();                
            }
        }

        [Test]
        public void BeginTransactionSerializableTest()
        {
            Console.WriteLine("\r\nStarting transaction - Serializable");
            using (PgTransaction transaction = Connection.BeginTransaction(IsolationLevel.Serializable))
            {
                transaction.Rollback();   
            }
        }

        [Test]
        public void CommitTest()
        {
            Console.WriteLine("\r\nTestin transaction Commit");
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                transaction.Commit();
                transaction.Dispose();
            }
        }

        [Test]
        public void RollbackTest()
        {
            Console.WriteLine("\r\nTestin transaction Rollback");
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                transaction.Rollback();
                transaction.Dispose();
            }
        }
    }
}
