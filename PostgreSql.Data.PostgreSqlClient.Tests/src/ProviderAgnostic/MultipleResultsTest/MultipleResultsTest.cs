// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;
using System.Data.Common;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    [TestFixture]
    public class MultipleResultsTest 
        : DataTestClass
    {
        [Test]
        public static void TestMain()
        {
            Assert.True((new MultipleResultsTest()).RunTestCoreAndCompareWithBaseline());
        }

        protected override void RunDataTest()
        {
            MultipleErrorHandling(new PgConnection(PostgreSql9_Northwind));
        }

        private static void MultipleErrorHandling(DbConnection connection)
        {
            try
            {
                Console.WriteLine("MultipleErrorHandling {0}", connection.GetType().Name);
                Type expectedException = null;
                if (connection is PgConnection)
                {
                    ((PgConnection)connection).InfoMessage += delegate (object sender, PgInfoMessageEventArgs args)
                    {
                        Console.WriteLine("*** SQL CONNECTION INFO MESSAGE : {0} ****", args.Message);
                    };
                    expectedException = typeof(SqlException);
                }
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "RAISE NOTICE '0';\n" +
                        "SELECT num = 1, str = 'ABC';\n" +
                        "RAISE NOTICE '1';\n" +
                        "RAISERROR('Error 1', 15, 1);\n" +
                        "RAISE NOTICE '3';\n" +
                        "SELECT num = 2, str = 'ABC';\n" +
                        "RAISE NOTICE '4';\n" +
                        "RAISERROR('Error 2', 15, 1);\n" +
                        "RAISE NOTICE '5';\n" +
                        "SELECT num = 3, str = 'ABC';\n" +
                        "RAISE NOTICE '6';\n" +
                        "RAISERROR('Error 3', 15, 1);\n" +
                        "RAISE NOTICE '7';\n" +
                        "SELECT num = 4, str = 'ABC';\n" +
                        "RAISE NOTICE '8';\n" +
                        "RAISERROR('Error 4', 15, 1);\n" +
                        "RAISE NOTICE '9';\n" +
                        "SELECT num = 5, str = 'ABC';\n" +
                        "RAISE NOTICE '10';\n" +
                        "RAISERROR('Error 5', 15, 1);\n" +
                        "RAISE NOTICE '11';\n";

                    try
                    {
                        Console.WriteLine("**** ExecuteNonQuery *****");
                        command.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        PrintException(expectedException, e);
                    }

                    try
                    {
                        Console.WriteLine("**** ExecuteScalar ****");
                        command.ExecuteScalar();
                    }
                    catch (Exception e)
                    {
                        PrintException(expectedException, e);
                    }

                    try
                    {
                        Console.WriteLine("**** ExecuteReader ****");
                        using (DbDataReader reader = command.ExecuteReader())
                        {
                            bool moreResults = true;
                            do
                            {
                                try
                                {
                                    Console.WriteLine("NextResult");
                                    moreResults = reader.NextResult();
                                }
                                catch (Exception e)
                                {
                                    PrintException(expectedException, e);
                                }
                            } while (moreResults);
                        }
                    }
                    catch (Exception e)
                    {
                        PrintException(null, e);
                    }
                }
            }
            catch (Exception e)
            {
                PrintException(null, e);
            }
            try
            {
                connection.Dispose();
            }
            catch (Exception e)
            {
                PrintException(null, e);
            }
        }
    }
}
