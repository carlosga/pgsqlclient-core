// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System.Collections;
using System.Globalization;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public static class ExceptionTest
    {
        // data value and server consts
        private const string badServer               = "NotAServer";
        private const string sqlsvrBadConn           = "A network-related or instance-specific error occurred while establishing a connection to PostgreSQL. The server was not found or was not accessible. Verify that the server name is correct and that PostgreSQL is configured to allow remote connections.";
        private const string logonFailedErrorMessage = "password authentication failed for user \"{0}\"";
        private const string execReaderFailedMessage = "ExecuteReader requires an open and available Connection. The connection's current state is Closed.";
        private const string warningNoiseMessage     = "The full-text search condition contained noise word(s).";
        private const string warningInfoMessage      = "Test of info messages";
        private const string orderIdQuery            = "select orderid from orders where orderid < 10250";

        [Fact]
        public static void WarningTest()
        {
            var connectionString = DataTestClass.PostgreSql_Northwind;
            var hitWarnings      = false;

            Action<object, PgInfoMessageEventArgs> warningCallback =
                (object sender, PgInfoMessageEventArgs imevent) =>
                {
                    for (int i = 0; i < imevent.Errors.Count; i++)
                    {
                        Assert.True(imevent.Errors[i].Message.Contains(warningInfoMessage), "FAILED: WarningTest Callback did not contain correct message.");
                    }
                    
                    hitWarnings = true;
                };

            var handler = new PgInfoMessageEventHandler(warningCallback);
            using (var connection  = new PgConnection(connectionString + "pooling=false"))
            {
                connection.InfoMessage += handler;
                connection.Open();

                PgCommand cmd = new PgCommand(string.Format("SELECT RAISE_NOTICE('{0}')", warningInfoMessage), connection);
                cmd.ExecuteNonQuery();

                connection.InfoMessage -= handler;
                cmd.ExecuteNonQuery();
            }

            Assert.True(hitWarnings, "FAILED: Should have received warnings from this query");
        }

        [Fact]
        public static void ExceptionTests()
        {
            var connectionString = DataTestClass.PostgreSql_Northwind;
            var builder          = new PgConnectionStringBuilder(connectionString);

            // tests improper server name thrown from constructor of tdsparser
            var badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), sqlsvrBadConn);

            // tests incorrect password
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { Password = string.Empty };
            var errorMessage = string.Format(logonFailedErrorMessage, badBuilder.UserID);
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex));
            
            // tests incorrect database name
            badBuilder   = new PgConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "NotADatabase" };
            // errorMessage = string.Format("Cannot open database \"{0}\" requested by the login. The login failed.", badBuilder.InitialCatalog);
            errorMessage = string.Format("database \"{0}\" does not exist", badBuilder.InitialCatalog);
            PgException firstAttemptException = VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex));
            
            // Verify that the same error results in a different instance of an exception, but with the same data
#warning TODO: port ??
            // VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => CheckThatExceptionsAreDistinctButHaveSameData(firstAttemptException, ex));

            // tests incorrect user name - exception thrown from adapter
            badBuilder   = new PgConnectionStringBuilder(builder.ConnectionString) { UserID = "NotAUser" };
            errorMessage = string.Format(CultureInfo.InvariantCulture, logonFailedErrorMessage, badBuilder.UserID);
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex));
        }

        [Fact]
        public static void VariousExceptionTests()
        {
            var connectionString = DataTestClass.PostgreSql_Northwind;
            var builder          = new PgConnectionStringBuilder(connectionString);

            // Test 1 - A
            var badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };
            using (var connection = new PgConnection(badBuilder.ConnectionString))
            {
                using (PgCommand command = connection.CreateCommand())
                {
                    command.CommandText = orderIdQuery;
                    VerifyConnectionFailure<InvalidOperationException>(() => command.ExecuteReader(), execReaderFailedMessage);
                }
            }

            // Test 1 - B
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { Password = string.Empty };
            using (var connection = new PgConnection(badBuilder.ConnectionString))
            {                
                string errorMessage = string.Format(logonFailedErrorMessage, badBuilder.UserID);
                VerifyConnectionFailure<PgException>(() => connection.Open(), errorMessage, (ex) => VerifyException(ex));
            }
        }

        [Fact]
        public static void IndependentConnectionExceptionTest()
        {
            var connectionString = DataTestClass.PostgreSql_Northwind;
            var builder          = new PgConnectionStringBuilder(connectionString);
            var badBuilder       = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };
            
            using (var connection = new PgConnection(badBuilder.ConnectionString))
            {
                // Test 1
                VerifyConnectionFailure<PgException>(() => connection.Open(), sqlsvrBadConn);

                // Test 2
                using (var command = new PgCommand(orderIdQuery, connection))
                {
                    VerifyConnectionFailure<InvalidOperationException>(() => command.ExecuteReader(), execReaderFailedMessage);
                }
            }
        }

        private static bool CheckThatExceptionsAreDistinctButHaveSameData(PgException e1, PgException e2)
        {
            Assert.True(e1 != e2, "FAILED: verification of exception cloning in subsequent connection attempts");

            Assert.False((e1 == null) || (e2 == null), "FAILED: One of exceptions is null, another is not");

            bool equal = (e1.Message        == e2.Message) 
                      && (e1.HelpLink       == e2.HelpLink) 
                      && (e1.InnerException == e2.InnerException)
                      && (e1.Source         == e2.Source) 
                      && (e1.Data.Count     == e2.Data.Count) 
                      && (e1.Errors         == e2.Errors);
                
            IDictionaryEnumerator enum1 = e1.Data.GetEnumerator();
            IDictionaryEnumerator enum2 = e2.Data.GetEnumerator();
            
            while (equal)
            {
                if (!enum1.MoveNext())
                {
                    break;
                }
                    
                enum2.MoveNext();
                equal = (enum1.Key == enum2.Key) && (enum2.Value == enum2.Value);
            }

            Assert.True(equal, string.Format("FAILED: exceptions do not contain the same data (besides call stack):\nFirst: {0}\nSecond: {1}\n", e1, e2));

            return true;
        }

        private static void GenerateConnectionException(string connectionString)
        {
            using (var connection = new PgConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = orderIdQuery;
                    command.ExecuteReader();
                }
            }
        }

        private static TException VerifyConnectionFailure<TException>(Action connectAction, string expectedExceptionMessage, Func<TException, bool> exVerifier) where TException : Exception
        {
            TException ex = Assert.Throws<TException>(connectAction);

            Assert.True(ex.Message.Contains(expectedExceptionMessage), string.Format("FAILED: PgException did not contain expected error message. Actual message: {0}", ex.Message));
            Assert.True(exVerifier(ex), "FAILED: Exception verifier failed on the exception.");

            return ex;
        }

        private static TException VerifyConnectionFailure<TException>(Action connectAction, string expectedExceptionMessage) where TException : Exception
        {
            return VerifyConnectionFailure<TException>(connectAction, expectedExceptionMessage, (ex) => true);
        }

        private static bool VerifyException(PgException exception)
        {
            VerifyException(exception, 1);
            return true;
        }

        private static bool VerifyException(PgException exception, int count)
        {
            // Verify that there are the correct number of errors in the exception
            Assert.True(exception.Errors.Count == count, string.Format("FAILED: Incorrect number of errors. Expected: {0}. Actual: {1}.", count, exception.Errors.Count));

            // Ensure that all errors have an error-level severity
            for (int i = 0; i < count; i++)
            {
                Assert.True(!exception.Errors[i].Code.StartsWith("01"), "FAILED: verification of Exception!  Exception contains a warning!");
            }

            // verify that the this[] function on the collection works, as well as the All function
            PgError[] errors = new PgError[exception.Errors.Count];
            exception.Errors.CopyTo(errors, 0);
            Assert.True((errors[0].Message).Equals(exception.Errors[0].Message), string.Format("FAILED: verification of Exception! ErrorCollection indexer/CopyTo resulted in incorrect value."));

            return true;
        }
    }
}
