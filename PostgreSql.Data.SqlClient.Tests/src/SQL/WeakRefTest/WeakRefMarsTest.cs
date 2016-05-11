// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;
using System.Runtime.CompilerServices;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class WeakRefMarsTest
    {
        private const string COMMAND_TEXT_1      = "SELECT CustomerID, CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax from Customers";
        private const string COMMAND_TEXT_2      = "SELECT CompanyName from Customers";
        private const string COLUMN_NAME_2       = "companyname";
        private const string DATABASE_NAME       = "pubs";
        private const int    CONCURRENT_COMMANDS = 5;

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

        [Fact]
        public static void TestReaderMars()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind + ";multipleactiveresultsets=true;Max Pool Size=1";

            TestReaderMarsCase("Case 1: ExecuteReader*5 Close, ExecuteReader.", connectionString, ReaderTestType.ReaderClose, ReaderVerificationType.ExecuteReader);
            TestReaderMarsCase("Case 2: ExecuteReader*5 Dispose, ExecuteReader.", connectionString, ReaderTestType.ReaderDispose, ReaderVerificationType.ExecuteReader);
            TestReaderMarsCase("Case 3: ExecuteReader*5 GC, ExecuteReader.", connectionString, ReaderTestType.ReaderGC, ReaderVerificationType.ExecuteReader);
            TestReaderMarsCase("Case 4: ExecuteReader*5 Connection Close, ExecuteReader.", connectionString, ReaderTestType.ConnectionClose, ReaderVerificationType.ExecuteReader);
            TestReaderMarsCase("Case 5: ExecuteReader*5 GC, Connection Close, ExecuteReader.", connectionString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ExecuteReader);

            TestReaderMarsCase("Case 6: ExecuteReader*5 Close, ChangeDatabase.", connectionString, ReaderTestType.ReaderClose, ReaderVerificationType.ChangeDatabase);
            TestReaderMarsCase("Case 7: ExecuteReader*5 Dispose, ChangeDatabase.", connectionString, ReaderTestType.ReaderDispose, ReaderVerificationType.ChangeDatabase);
            TestReaderMarsCase("Case 8: ExecuteReader*5 GC, ChangeDatabase.", connectionString, ReaderTestType.ReaderGC, ReaderVerificationType.ChangeDatabase);
            TestReaderMarsCase("Case 9: ExecuteReader*5 Connection Close, ChangeDatabase.", connectionString, ReaderTestType.ConnectionClose, ReaderVerificationType.ChangeDatabase);
            TestReaderMarsCase("Case 10: ExecuteReader*5 GC, Connection Close, ChangeDatabase.", connectionString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.ChangeDatabase);

            TestReaderMarsCase("Case 11: ExecuteReader*5 Close, BeginTransaction.", connectionString, ReaderTestType.ReaderClose, ReaderVerificationType.BeginTransaction);
            TestReaderMarsCase("Case 12: ExecuteReader*5 Dispose, BeginTransaction.", connectionString, ReaderTestType.ReaderDispose, ReaderVerificationType.BeginTransaction);

            TestReaderMarsCase("Case 13: ExecuteReader*5 Connection Close, BeginTransaction.", connectionString, ReaderTestType.ConnectionClose, ReaderVerificationType.BeginTransaction);
            TestReaderMarsCase("Case 14: ExecuteReader*5 GC, Connection Close, BeginTransaction.", connectionString, ReaderTestType.ReaderGCConnectionClose, ReaderVerificationType.BeginTransaction);
        }

        [Fact]
        public static void TestTransactionSingle()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind + ";multipleactiveresultsets=true;Max Pool Size=1";

            TestTransactionSingleCase("Case 1: BeginTransaction, Rollback.", connectionString, TransactionTestType.TransactionRollback);
            TestTransactionSingleCase("Case 2: BeginTransaction, Dispose.", connectionString, TransactionTestType.TransactionDispose);
            TestTransactionSingleCase("Case 3: BeginTransaction, GC.", connectionString, TransactionTestType.TransactionGC);
            TestTransactionSingleCase("Case 4: BeginTransaction, Connection Close.", connectionString, TransactionTestType.ConnectionClose);
            TestTransactionSingleCase("Case 5: BeginTransaction, GC, Connection Close.", connectionString, TransactionTestType.TransactionGCConnectionClose);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TestReaderMarsCase(string caseName, string connectionString, ReaderTestType testType, ReaderVerificationType verificationType)
        {
            WeakReference  weak = null;
            PgCommand[]    cmd  = new PgCommand[CONCURRENT_COMMANDS];
            PgDataReader[] gch  = new PgDataReader[CONCURRENT_COMMANDS];

            using (PgConnection con = new PgConnection(connectionString))
            {
                con.Open();

                for (int i = 0; i < CONCURRENT_COMMANDS; i++)
                {
                    cmd[i] = con.CreateCommand();
                    cmd[i].CommandText = COMMAND_TEXT_1;
                    
                    if ((testType != ReaderTestType.ReaderGC) && (testType != ReaderTestType.ReaderGCConnectionClose))
                    {
                        gch[i] = cmd[i].ExecuteReader();
                    }
                    else
                    {
                        gch[i] = null;
                    }
                }

                for (int i = 0; i < CONCURRENT_COMMANDS; i++)
                {
                    switch (testType)
                    {
                        case ReaderTestType.ReaderClose:
                            gch[i].Dispose();
                            break;

                        case ReaderTestType.ReaderDispose:
                            gch[i].Dispose();
                            break;

                        case ReaderTestType.ReaderGC:
                            weak = OpenNullifyReader(cmd[i]);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Assert.False(weak.IsAlive, "Transaction is still alive on TestReaderMars: ReaderGC");
                            break;

                        case ReaderTestType.ConnectionClose:
                            GC.SuppressFinalize(gch[i]);
                            con.Close();
                            con.Open();
                            break;

                        case ReaderTestType.ReaderGCConnectionClose:
                            weak = OpenNullifyReader(cmd[i]);
                            GC.Collect();
                            GC.WaitForPendingFinalizers();
                            Assert.False(weak.IsAlive, "Transaction is still alive on TestReaderMars: ReaderGCConnectionClose");
                            con.Close();
                            con.Open();
                            break;
                    }

                    cmd[i].Dispose();
                }

                using (PgCommand verificationCmd = con.CreateCommand())
                {
                    switch (verificationType)
                    {
                        case ReaderVerificationType.ExecuteReader:
                            verificationCmd.CommandText = COMMAND_TEXT_2;
                            using (PgDataReader rdr = verificationCmd.ExecuteReader())
                            {
                                rdr.Read();
                                DataTestClass.AssertEqualsWithDescription(1, rdr.FieldCount, "Execute Reader should return expected Field count");
                                DataTestClass.AssertEqualsWithDescription(COLUMN_NAME_2, rdr.GetName(0), "Execute Reader should return expected Field name");
                            }
                            break;

                        case ReaderVerificationType.ChangeDatabase:
                            con.ChangeDatabase(DATABASE_NAME);
                            DataTestClass.AssertEqualsWithDescription(DATABASE_NAME, con.Database, "Change Database should return expected Database Name");
                            break;

                        case ReaderVerificationType.BeginTransaction:
    #warning TODO: Port to PostgreSql
                            // verificationCmd.Transaction = con.BeginTransaction();
                            // verificationCmd.CommandText = "select @@trancount";
                            // int tranCount = (int)verificationCmd.ExecuteScalar();
                            // DataTestClass.AssertEqualsWithDescription(1, tranCount, "Begin Transaction should return expected Transaction count");
                            break;
                    }
                }
            }
        }

        private static WeakReference OpenNullifyReader(PgCommand command)
        {
            var reader = command.ExecuteReader();
            var weak   = new WeakReference(reader);

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
                {
                    gch = con.BeginTransaction();
                }
                
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
                        Assert.False(weak.IsAlive, "Transaction is still alive on TestTransactionSingle: TransactionGC");
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
                        Assert.False(weak.IsAlive, "Transaction is still alive on TestTransactionSingle: TransactionGCConnectionClose");
                        con.Close();
                        con.Open();
                        break;
                }

                using (PgCommand cmd = con.CreateCommand())
                {
#warning TODO: See how to port to postgresql
                    // cmd.CommandText = "select @@trancount";
                    // int tranCount = (int)cmd.ExecuteScalar();
                    // DataTestClass.AssertEqualsWithDescription(0, tranCount, "TransactionSingle Case " + caseName + " should return expected trans count");
                }
            }
        }

        private static WeakReference OpenNullifyTransaction(PgConnection connection)
        {
            var transaction = connection.BeginTransaction();
            var weak        = new WeakReference(transaction);

            transaction = null;

            return weak;
        }
    }
}
