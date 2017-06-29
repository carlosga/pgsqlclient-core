// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class MultipleResultsTest 
        : DataTestClass
    {
        [Fact]
        public void TestMain()
        {
            Assert.True(RunTestCoreAndCompareWithBaseline());
        }

        protected override void RunDataTest()
        {
            MultipleErrorHandling(new PgConnection(PostgreSql_Northwind + "multipleactiveresultsets=true;"));
        }

        private static void MultipleErrorHandling(PgConnection connection)
        {
            try
            {
                Console.WriteLine("MultipleErrorHandling {0}", connection.GetType().Name);
                Type expectedException = typeof(PgException);

                connection.InfoMessage += delegate (object sender, PgInfoMessageEventArgs args)
                {
                    Console.WriteLine($"*** SQL CONNECTION INFO MESSAGE : {args.Message} ****");
                };

                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT raise_notice('0');"
                      + "SELECT 1 as num, 'ABC' as str;"
                      + "SELECT raise_notice('1');"
                      + "SELECT raise_error('Error 1');"
                      + "SELECT raise_notice('3');"
                      + "SELECT 2 as num, 'ABC' as str;"
                      + "SELECT raise_notice('4');"
                      + "SELECT raise_error('Error 2');"
                      + "SELECT raise_notice('5');"
                      + "SELECT 3 as num, 'ABC' as str;"
                      + "SELECT raise_notice('6');"
                      + "SELECT raise_error('Error 3');"
                      + "SELECT raise_notice('7');"
                      + "SELECT 4 as num, 'ABC' as str;"
                      + "SELECT raise_notice('8');"
                      + "SELECT raise_error('Error 4');" 
                      + "SELECT raise_notice('9');"
                      + "SELECT 5 as num, 'ABC' as str;"
                      + "SELECT raise_notice('10');"
                      + "SELECT raise_error('Error 5');"
                      + "SELECT raise_notice('11');";

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
                        using (var reader = command.ExecuteReader())
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
