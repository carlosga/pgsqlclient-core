// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using Xunit;
using System;
using System.Data;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    public class PgConnectionTest
        : PgBaseTest
    {
        [Fact]
        public void BeginTransactionTest()
        {
            using (var transaction = Connection.BeginTransaction())
            {
                transaction.Rollback();
            }
        }

        [Fact]
        public void BeginTransactionReadCommittedTest()
        {
            using (var transaction = Connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                transaction.Rollback();
            }
        }

        [Fact]
        public void BeginTransactionSerializableTest()
        {
            using (var transaction = Connection.BeginTransaction(IsolationLevel.Serializable))
            {
                transaction.Rollback();
            }
        }

        [Fact]
        public void DatabaseTest()
        {
            Console.WriteLine("Actual database : {0}", Connection.Database);
        }

        [Fact]
        public void DataSourceTest()
        {
            Console.WriteLine("Actual server : {0}", Connection.DataSource);
        }

        [Fact]
        public void ConnectionTimeOutTest()
        {
            Console.WriteLine("Actual connection timeout : {0}", Connection.ConnectionTimeout);
        }

        [Fact]
        public void ServerVersionTest()
        {
            Console.WriteLine("PostgreSQL Server version : {0}", Connection.ServerVersion);
        }

        [Fact]
        public void PacketSizeTest()
        {
            Console.WriteLine("Actual opacket size : {0}", Connection.PacketSize);
        }

        [Fact]
        public void CreateCommandTest()
        {
            using (var command = Connection.CreateCommand()) { }
        }
    }
}