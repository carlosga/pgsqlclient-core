// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;
using System.Data.Common;
using System;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    [TestFixture]
    [Ignore("Not ported yet")]
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
                        Console.WriteLine($"*** SQL CONNECTION INFO MESSAGE : {args.Message} ****");
                    };
                    expectedException = typeof(PgException);
                }
                connection.Open();

                using (DbCommand command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT raise_notice('0');\n" +
                        "SELECT 1 as num, 'ABC' as str;\n" +
                        "SELECT raise_notice('1');\n" +
                        "SELECT raise_exception('Error 1');\n" +
                        "SELECT raise_notice('3');\n" +
                        "SELECT 2 as num, 'ABC' as str;\n" +
                        "SELECT raise_notice('4');\n" +
                        "SELECT raise_exception('Error 2');\n" +
                        "SELECT raise_notice('5');\n" +
                        "SELECT 3 as num, 'ABC' as str;\n" +
                        "SELECT raise_notice('6');\n" +
                        "SELECT raise_exception('Error 3');\n" +
                        "SELECT raise_notice('7');\n" +
                        "SELECT 4 as num, 'ABC' as str;\n" +
                        "SELECT raise_notice('8');\n" +
                        "SELECT raise_exception('Error 4');\n" +
                        "SELECT raise_notice('9');\n" +
                        "SELECT 5 as num, 'ABC' as str;\n" +
                        "SELECT raise_notice('10');\n" +
                        "SELECT raise_exception('Error 5');\n" +
                        "SELECT raise_notice('11');\n";

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
