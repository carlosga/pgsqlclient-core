// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using System;
using System.Data;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    [TestFixture]
    [Ignore("Needs configuration")]
    public class PgConnectionTest
        : PgBaseTest
    {
        [Test]
        public void BeginTransactionTest()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                transaction.Rollback();
            }
        }

        [Test]
        public void BeginTransactionReadCommittedTest()
        {
            using (var transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                transaction.Rollback();
            }
        }

        [Test]
        public void BeginTransactionSerializableTest()
        {
            using (var transaction = Connection.BeginTransaction(IsolationLevel.Serializable))
            {
                transaction.Rollback();
            }
        }

        [Test]
        public void DatabaseTest()
        {
            Console.WriteLine("Actual database : {0}", Connection.Database);
        }

        [Test]
        public void DataSourceTest()
        {
            Console.WriteLine("Actual server : {0}", Connection.DataSource);
        }

        [Test]
        public void ConnectionTimeOutTest()
        {
            Console.WriteLine("Actual connection timeout : {0}", Connection.ConnectionTimeout);
        }

        [Test]
        public void ServerVersionTest()
        {
            Console.WriteLine("PostgreSQL Server version : {0}", Connection.ServerVersion);
        }

        [Test]
        public void PacketSizeTest()
        {
            Console.WriteLine("Actual opacket size : {0}", Connection.PacketSize);
        }

        [Test]
        public void CreateCommandTest()
        {
            using (var command = Connection.CreateCommand()) { }
        }
    }
}
