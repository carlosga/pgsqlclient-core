// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Globalization;
using System;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    [TestFixture]
    public static class ExceptionTest
    {
        // data value and server consts
        private const string badServer = "NotAServer";
        private const string sqlsvrBadConn = "A network-related or instance-specific error occurred while establishing a connection to SQL Server. The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server is configured to allow remote connections.";
        private const string logonFailedErrorMessage = "Login failed for user '{0}'.";
        private const string execReaderFailedMessage = "ExecuteReader requires an open and available Connection. The connection's current state is closed.";
        private const string warningNoiseMessage = "The full-text search condition contained noise word(s).";
        private const string warningInfoMessage = "Test of info messages";
        private const string orderIdQuery = "select orderid from orders where orderid < 10250";

        [Test]
        public static void WarningTest()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind;

            Action<object, PgInfoMessageEventArgs> warningCallback =
                (object sender, PgInfoMessageEventArgs imevent) =>
                {
                    for (int i = 0; i < imevent.Errors.Count; i++)
                    {
                        Assert.True(imevent.Errors[i].Message.Contains(warningInfoMessage), "FAILED: WarningTest Callback did not contain correct message.");
                    }
                };

            PgInfoMessageEventHandler handler = new PgInfoMessageEventHandler(warningCallback);
            using (PgConnection PgConnection  = new PgConnection(connectionString + ";pooling=false;"))
            {
                PgConnection.InfoMessage += handler;
                PgConnection.Open();

                PgCommand cmd = new PgCommand(string.Format("PRINT N'{0}'", warningInfoMessage), PgConnection);
                cmd.ExecuteNonQuery();

                PgConnection.InfoMessage -= handler;
                cmd.ExecuteNonQuery();
            }
        }

        [Test]
        public static void WarningsBeforeRowsTest()
        {
            var connectionString = DataTestClass.PostgreSql9_Northwind;
            var hitWarnings      = false;
            int iteration        = 0;
            
            Action<object, PgInfoMessageEventArgs> warningCallback =
                (object sender, PgInfoMessageEventArgs imevent) =>
                {
                    for (int i = 0; i < imevent.Errors.Count; i++)
                    {
                        Assert.True(imevent.Errors[i].Message.Contains(warningNoiseMessage), "FAILED: WarningsBeforeRowsTest Callback did not contain correct message. Failed in loop iteration: " + iteration);
                    }
                    hitWarnings = true;
                };

            var handler      = new PgInfoMessageEventHandler(warningCallback);
            var PgConnection = new PgConnection(connectionString);
            PgConnection.InfoMessage += handler;
            PgConnection.Open();
            foreach (string orderClause in new string[] { "", " order by FirstName" })
            {
                foreach (bool messagesOnErrors in new bool[] { true, false })
                {
                    iteration++;

#warning TODO: WTF !
                    // PgConnection.FireInfoMessageEventOnUserErrors = messagesOnErrors;

                    // These queries should return warnings because AND here is a noise word.
                    PgCommand cmd = new PgCommand("select FirstName from Northwind.dbo.Employees where contains(FirstName, '\"Anne AND\"')" + orderClause, PgConnection);
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        Assert.True(reader.HasRows, "FAILED: PgDataReader.HasRows is not correct (should be TRUE)");

                        bool receivedRows = false;
                        while (reader.Read())
                        {
                            receivedRows = true;
                        }
                        Assert.True(receivedRows, "FAILED: Should have received rows from this query.");
                        Assert.True(hitWarnings, "FAILED: Should have received warnings from this query");
                    }
                    hitWarnings = false;

                    cmd.CommandText = "select FirstName from Northwind.dbo.Employees where contains(FirstName, '\"NotARealPerson AND\"')" + orderClause;
                    using (PgDataReader reader = cmd.ExecuteReader())
                    {
                        Assert.False(reader.HasRows, "FAILED: PgDataReader.HasRows is not correct (should be FALSE)");

                        bool receivedRows = false;
                        while (reader.Read())
                        {
                            receivedRows = true;
                        }
                        Assert.False(receivedRows, "FAILED: Should have NOT received rows from this query.");
                        Assert.True(hitWarnings, "FAILED: Should have received warnings from this query");
                    }
                }
            }
            PgConnection.Close();
        }

        private static bool CheckThatExceptionsAreDistinctButHaveSameData(PgException e1, PgException e2)
        {
            Assert.True(e1 != e2, "FAILED: verification of exception cloning in subsequent connection attempts");

            Assert.False((e1 == null) || (e2 == null), "FAILED: One of exceptions is null, another is not");

            bool equal = (e1.Message == e2.Message) && (e1.HelpLink == e2.HelpLink) && (e1.InnerException == e2.InnerException)
                && (e1.Source == e2.Source) && (e1.Data.Count == e2.Data.Count) && (e1.Errors == e2.Errors);
            IDictionaryEnumerator enum1 = e1.Data.GetEnumerator();
            IDictionaryEnumerator enum2 = e2.Data.GetEnumerator();
            while (equal)
            {
                if (!enum1.MoveNext())
                    break;
                enum2.MoveNext();
                equal = (enum1.Key == enum2.Key) && (enum2.Value == enum2.Value);
            }

            Assert.True(equal, string.Format("FAILED: exceptions do not contain the same data (besides call stack):\nFirst: {0}\nSecond: {1}\n", e1, e2));

            return true;
        }

        [Test]
        public static void ExceptionTests()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind;
            PgConnectionStringBuilder builder = new PgConnectionStringBuilder(connectionString);

            // tests improper server name thrown from constructor of tdsparser
            PgConnectionStringBuilder badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };

            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), sqlsvrBadConn, VerifyException);

            // tests incorrect password - thrown from the adapter
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { Password = string.Empty };
            string errorMessage = string.Format(CultureInfo.InvariantCulture, logonFailedErrorMessage, badBuilder.UserID);
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex, 1, 18456, 1, 14));

            // tests incorrect database name - exception thrown from adapter
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { InitialCatalog = "NotADatabase" };
            errorMessage = string.Format(CultureInfo.InvariantCulture, "Cannot open database \"{0}\" requested by the login. The login failed.", badBuilder.InitialCatalog);
            PgException firstAttemptException = VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex, 2, 4060, 1, 11));

            // Verify that the same error results in a different instance of an exception, but with the same data
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => CheckThatExceptionsAreDistinctButHaveSameData(firstAttemptException, ex));

            // tests incorrect user name - exception thrown from adapter
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { UserID = "NotAUser" };
            errorMessage = string.Format(CultureInfo.InvariantCulture, logonFailedErrorMessage, badBuilder.UserID);
            VerifyConnectionFailure<PgException>(() => GenerateConnectionException(badBuilder.ConnectionString), errorMessage, (ex) => VerifyException(ex, 1, 18456, 1, 14));
        }

        [Test]
        public static void VariousExceptionTests()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind;

            PgConnectionStringBuilder builder = new PgConnectionStringBuilder(connectionString);

            // Test 1 - A
            PgConnectionStringBuilder badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };
            using (var PgConnection = new PgConnection(badBuilder.ConnectionString))
            {
                using (PgCommand command = PgConnection.CreateCommand())
                {
                    command.CommandText = orderIdQuery;
                    VerifyConnectionFailure<InvalidOperationException>(() => command.ExecuteReader(), execReaderFailedMessage);
                }
            }

            // Test 1 - B
            badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { Password = string.Empty };
            using (var PgConnection = new PgConnection(badBuilder.ConnectionString))
            {
                string errorMessage = string.Format(CultureInfo.InvariantCulture, logonFailedErrorMessage, badBuilder.UserID);
                VerifyConnectionFailure<PgException>(() => PgConnection.Open(), errorMessage, (ex) => VerifyException(ex, 1, 18456, 1, 14));
            }
        }

        [Test]
        public static void IndependentConnectionExceptionTest()
        {
            string connectionString = DataTestClass.PostgreSql9_Northwind;

            PgConnectionStringBuilder builder = new PgConnectionStringBuilder(connectionString);

            PgConnectionStringBuilder badBuilder = new PgConnectionStringBuilder(builder.ConnectionString) { DataSource = badServer, ConnectTimeout = 1 };
            using (var PgConnection = new PgConnection(badBuilder.ConnectionString))
            {
                // Test 1
                VerifyConnectionFailure<PgException>(() => PgConnection.Open(), sqlsvrBadConn, VerifyException);

                // Test 2
                using (PgCommand command = new PgCommand(orderIdQuery, PgConnection))
                {
                    VerifyConnectionFailure<InvalidOperationException>(() => command.ExecuteReader(), execReaderFailedMessage);
                }
            }
        }

        private static void GenerateConnectionException(string connectionString)
        {
            using (PgConnection PgConnection = new PgConnection(connectionString))
            {
                PgConnection.Open();
                using (PgCommand command = PgConnection.CreateCommand())
                {
                    command.CommandText = orderIdQuery;
                    command.ExecuteReader();
                }
            }
        }

        private static TException VerifyConnectionFailure<TException>(TestDelegate connectAction, string expectedExceptionMessage, Func<TException, bool> exVerifier) where TException : Exception
        {
            TException ex = Assert.Throws<TException>(connectAction);
            Assert.True(ex.Message.Contains(expectedExceptionMessage), string.Format("FAILED: PgException did not contain expected error message. Actual message: {0}", ex.Message));
            Assert.True(exVerifier(ex), "FAILED: Exception verifier failed on the exception.");

            return ex;
        }

        private static TException VerifyConnectionFailure<TException>(TestDelegate connectAction, string expectedExceptionMessage) where TException : Exception
        {
            return VerifyConnectionFailure<TException>(connectAction, expectedExceptionMessage, (ex) => true);
        }

        private static bool VerifyException(PgException exception)
        {
            VerifyException(exception, 1);
            return true;
        }

        private static bool VerifyException(PgException exception, int count, int? errorNumber = null, int? errorState = null, int? severity = null)
        {
            // Verify that there are the correct number of errors in the exception
            Assert.True(exception.Errors.Count == count, string.Format("FAILED: Incorrect number of errors. Expected: {0}. Actual: {1}.", count, exception.Errors.Count));

            // Ensure that all errors have an error-level severity
            for (int i = 0; i < count; i++)
            {
                Assert.True(!exception.Errors[i].Code.StartsWith("01"), "FAILED: verification of Exception!  Exception contains a warning!");
            }

            // Check the properties of the exception populated by the server are correct
#warning TODO: Modify PgException ??
            // if (severity != null)
            // {
            //     Assert.True(severity == exception.Severity, string.Format("FAILED: Severity of exception is incorrect. Expected: {0}. Actual: {1}.", severity, exception.Number));
            // }

            // if (errorState.HasValue)
            // {
            //     Assert.True(errorState.Value == exception.State, string.Format("FAILED: Error state of exception is incorrect. Expected: {0}. Actual: {1}.", errorState.Value, exception.State));
            // }

            // if (severity.HasValue)
            // {
            //     Assert.True(severity.Value == exception.Class, string.Format("FAILED: Severity of exception is incorrect. Expected: {0}. Actual: {1}.", severity.Value, exception.Class));
            // }

            // if ((errorNumber.HasValue) && (errorState.HasValue) && (severity.HasValue))
            // {
            //     string detailsText = string.Format("Error Number:{0},State:{1},Class:{2}", errorNumber.Value, errorState.Value, severity.Value);
            //     Assert.True(exception.ToString().Contains(detailsText), string.Format("FAILED: PgException.ToString does not contain the error number, state and severity information"));
            // }

            // verify that the this[] function on the collection works, as well as the All function
            PgError[] errors = new PgError[exception.Errors.Count];
            exception.Errors.CopyTo(errors, 0);
            Assert.True((errors[0].Message).Equals(exception.Errors[0].Message), string.Format("FAILED: verification of Exception! ErrorCollection indexer/CopyTo resulted in incorrect value."));

            return true;
        }
    }
}
