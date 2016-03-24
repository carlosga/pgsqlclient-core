// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    [TestFixture]
    public static class WeakRefTest
    {
        private const string COMMAND_TEXT_1 = "SELECT au_id, au_lname, au_fname, phone, address, city, state, zip, contract from authors";
        private const string COMMAND_TEXT_2 = "SELECT au_lname from authors";
        private const string COLUMN_NAME_2  = "au_lname";
        private const string DATABASE_NAME  = "northwind";

        private enum ReaderTestType
        {
            ReaderClose,
            ReaderDispose,
            ReaderGC,
            ConnectionClose,
            ReaderGCConnectionClose,
        }

        private enum ReaderVerificationType
        {
            ExecuteReader,
            ChangeDatabase,
            BeginTransaction,
            EnlistDistributedTransaction,
        }

        private enum TransactionTestType
        {
            TransactionRollback,
            TransactionDispose,
            TransactionGC,
            ConnectionClose,
            TransactionGCConnectionClose,
        }

        [Test]
        public static void TestReaderNonMars()
        {
            string connString = DataTestClass.PostgreSql9_Pubs + "Max Pool Size=1";

            TestReaderNonMarsCase("Case 1: ExecuteReader, Close, ExecuteReader.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case 2: ExecuteReader, Dispose, ExecuteReader.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case 3: ExecuteReader, GC, ExecuteReader.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case 4: ExecuteReader, Connection Close, ExecuteReader.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.ExecuteReader);
            TestReaderNonMarsCase("Case 5: ExecuteReader, GC, Connection Close, ExecuteReader.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ExecuteReader);

            TestReaderNonMarsCase("Case 6: ExecuteReader, Close, ChangeDatabase.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case 7: ExecuteReader, Dispose, ChangeDatabase.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case 8: ExecuteReader, GC, ChangeDatabase.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case 9: ExecuteReader, Connection Close, ChangeDatabase.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.ChangeDatabase);
            TestReaderNonMarsCase("Case 10: ExecuteReader, GC, Connection Close, ChangeDatabase.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ChangeDatabase);

            TestReaderNonMarsCase("Case 11: ExecuteReader, Close, BeginTransaction.", connString, ReaderTestType.ReaderClose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 12: ExecuteReader, Dispose, BeginTransaction.", connString, ReaderTestType.ReaderDispose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 13: ExecuteReader, GC, BeginTransaction.", connString, ReaderTestType.ReaderGC, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 14: ExecuteReader, Connection Close, BeginTransaction.", connString, ReaderTestType.ConnectionClose, ReaderVerificationType.BeginTransaction);
            TestReaderNonMarsCase("Case 15: ExecuteReader, GC, Connection Close, BeginTransaction.", connString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.BeginTransaction);
        }

        [Test]
        public static void TestTransactionSingle()
        {
            string connString = DataTestClass.PostgreSql9_Pubs + "Max Pool Size=1";

            TestTransactionSingleCase("Case 1: BeginTransaction, Rollback.", connString, TransactionTestType.TransactionRollback);
            TestTransactionSingleCase("Case 2: BeginTransaction, Dispose.", connString, TransactionTestType.TransactionDispose);
            TestTransactionSingleCase("Case 3: BeginTransaction, GC.", connString, TransactionTestType.TransactionGC);
            TestTransactionSingleCase("Case 4: BeginTransaction, Connection Close.", connString, TransactionTestType.ConnectionClose);
            TestTransactionSingleCase("Case 5: BeginTransaction, GC, Connection Close.", connString, TransactionTestType.TransactionGCConnectionClose);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestReaderNonMarsCase(string caseName, string connectionString, ReaderTestType testType, ReaderVerificationType verificationType)
        {
            WeakReference weak = null;

            using (PgConnection con = new PgConnection(connectionString))
            {
                con.Open();

                using (PgCommand cmd = con.CreateCommand())
                {
                    cmd.CommandText = COMMAND_TEXT_1;

                    PgDataReader gch = null;
                    if ((testType != ReaderTestType.ReaderGC) && (testType != ReaderTestType.ReaderGCConnectionClose))
                        gch = cmd.ExecuteReader();

                    switch (testType)
                    {
                        case ReaderTestType.ReaderClose:
                            gch.Dispose();
                            break;
                        case ReaderTestType.ReaderDispose:
                            gch.Dispose();
                            break;
                        case ReaderTestType.ReaderGC:
                            weak = OpenNullifyReader(cmd);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Assert.False(weak.IsAlive, "Reader is still alive!");
                            break;

                        case ReaderTestType.ConnectionClose:
                            GC.SuppressFinalize(gch);
                            con.Close();
                            con.Open();
                            break;

                        case ReaderTestType.ReaderGCConnectionClose:
                            weak = OpenNullifyReader(cmd);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            Assert.False(weak.IsAlive, "Reader is still alive!");
                            con.Close();
                            con.Open();
                            break;
                    }

                    switch (verificationType)
                    {
                        case ReaderVerificationType.ExecuteReader:
                            cmd.CommandText = COMMAND_TEXT_2;
                            using (PgDataReader rdr = cmd.ExecuteReader())
                            {
                                rdr.Read();
                                Assert.AreEqual(rdr.FieldCount, 1);
                                Assert.AreEqual(rdr.GetName(0), COLUMN_NAME_2);
                            }
                            break;

                        case ReaderVerificationType.ChangeDatabase:
                            con.ChangeDatabase(DATABASE_NAME);
                            Assert.AreEqual(con.Database, DATABASE_NAME);
                            break;

                        case ReaderVerificationType.BeginTransaction:
#warning TODO: See how to port to postgresql
                            // cmd.Transaction = con.BeginTransaction();
                            // cmd.CommandText = "select @@trancount";
                            // int tranCount = (int)cmd.ExecuteScalar();
                            // Assert.AreEqual(tranCount, 1);
                            break;
                    }
                }
            }
        }

        private static WeakReference OpenNullifyReader(PgCommand cmd)
        {
            PgDataReader reader = cmd.ExecuteReader();
            WeakReference weak = new WeakReference(reader);
            reader = null;
            return weak;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestTransactionSingleCase(string caseName, string connectionString, TransactionTestType testType)
        {
            WeakReference weak = null;

            using (PgConnection con = new PgConnection(connectionString))
            {
                con.Open();

                PgTransaction gch = null;
                if ((testType != TransactionTestType.TransactionGC) && (testType != TransactionTestType.TransactionGCConnectionClose))
                    gch = con.BeginTransaction();

                switch (testType)
                {
                    case TransactionTestType.TransactionRollback:
                        gch.Rollback();
                        break;

                    case TransactionTestType.TransactionDispose:
                        gch.Dispose();
                        break;

                    case TransactionTestType.TransactionGC:
                        weak = OpenNullifyTransaction(con);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        Assert.False(weak.IsAlive, "Transaction is still alive!");
                        break;
                    case TransactionTestType.ConnectionClose:
                        GC.SuppressFinalize(gch);
                        con.Close();
                        con.Open();
                        break;

                    case TransactionTestType.TransactionGCConnectionClose:
                        weak = OpenNullifyTransaction(con);
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        Assert.False(weak.IsAlive, "Transaction is still alive!");
                        con.Close();
                        con.Open();
                        break;
                }

                using (PgCommand cmd = con.CreateCommand())
                {
#warning TODO: See how to port to postgresql
                    // cmd.CommandText = "select @@trancount";
                    // int tranCount = (int)cmd.ExecuteScalar();
                    // Assert.AreEqual(tranCount, 0);
                }
            }
        }

        private static WeakReference OpenNullifyTransaction(PgConnection connection)
        {
            PgTransaction transaction = connection.BeginTransaction();
            WeakReference weak = new WeakReference(transaction);
            transaction = null;
            return weak;
        }
    }
}
