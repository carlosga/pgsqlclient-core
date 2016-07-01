// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Xunit;
using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Threading;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class ConnectionPoolTest
    {
        // [Fact]
        public void ConnectionPool_Nwnd9()
        {
            var connBuilder   = new PgConnectionStringBuilder(DataTestClass.PostgreSql9_Northwind);
            var sourceBuilder = new DataSourceBuilder(connBuilder.DataSource);
            sourceBuilder.Protocol = null;

            connBuilder.DataSource = sourceBuilder.ToString();
            connBuilder.Pooling    = true;
            
            RunDataTestForSingleConnString(connBuilder.ConnectionString);
        }

        private static void RunDataTestForSingleConnString(string tcpConnectionString)
        {
            BasicConnectionPoolingTest(tcpConnectionString);
            ClearAllPoolsTest(tcpConnectionString);
            ReclaimEmancipatedOnOpenTest(tcpConnectionString);
        }

        /// <summary>
        /// Tests that using the same connection string results in the same pool\internal connection and a different string results in a different pool\internal connection
        /// </summary>
        /// <param name="connectionString"></param>
        private static void BasicConnectionPoolingTest(string connectionString)
        {
            var connection = new PgConnection(connectionString);
            connection.Open();
            var internalConnection = new InternalConnectionWrapper(connection);
            var connectionPool     = new ConnectionPoolWrapper(connection);
            connection.Close();

            var connection2 = new PgConnection(connectionString);
            connection2.Open();
            Assert.True(internalConnection.IsInternalConnectionOf(connection2), "New connection does not use same internal connection");
            Assert.True(connectionPool.ContainsConnection(connection2), "New connection is in a different pool");
            connection2.Close();

            var connection3 = new PgConnection(connectionString + ";App=PgConnectionPoolUnitTest;");
            connection3.Open();
            Assert.False(internalConnection.IsInternalConnectionOf(connection3), "Connection with different connection string uses same internal connection");
            Assert.False(connectionPool.ContainsConnection(connection3), "Connection with different connection string uses same connection pool");
            connection3.Close();

            connectionPool.Cleanup();

            var connection4 = new PgConnection(connectionString);
            connection4.Open();
            Assert.True(internalConnection.IsInternalConnectionOf(connection4), "New connection does not use same internal connection");
            Assert.True(connectionPool.ContainsConnection(connection4), "New connection is in a different pool");
            connection4.Close();
        }

        /// <summary>
        /// Tests if clearing all of the pools does actually remove the pools
        /// </summary>
        /// <param name="connectionString"></param>
        private static void ClearAllPoolsTest(string connectionString)
        {
            PgConnection.ClearAllPools();
            Assert.True(0 == ConnectionPoolWrapper.AllConnectionPools().Length, "Pools exist after clearing all pools");

            var connection = new PgConnection(connectionString);
            connection.Open();
            ConnectionPoolWrapper pool = new ConnectionPoolWrapper(connection);
            connection.Close();
            ConnectionPoolWrapper[] allPools = ConnectionPoolWrapper.AllConnectionPools();
            DataTestClass.AssertEqualsWithDescription(1, allPools.Length, "Incorrect number of pools exist.");
            Assert.True(allPools[0].Equals(pool), "Saved pool is not in the list of all pools");
            DataTestClass.AssertEqualsWithDescription(1, pool.ConnectionCount, "Saved pool has incorrect number of connections");

            PgConnection.ClearAllPools();
            Assert.True(0 == ConnectionPoolWrapper.AllConnectionPools().Length, "Pools exist after clearing all pools");
            DataTestClass.AssertEqualsWithDescription(0, pool.ConnectionCount, "Saved pool has incorrect number of connections.");
        }

        /// <summary>
        /// Checks if an 'emancipated' internal connection is reclaimed when a new connection is opened AND we hit max pool size
        /// NOTE: 'emancipated' means that the internal connection's PgConnection has fallen out of scope and has no references, but was not explicitly disposed\closed
        /// </summary>
        /// <param name="connectionString"></param>
        private static void ReclaimEmancipatedOnOpenTest(string connectionString)
        {
            string newConnectionString = connectionString + ";Max Pool Size=1";
            PgConnection.ClearAllPools();

            InternalConnectionWrapper internalConnection = CreateEmancipatedConnection(newConnectionString);
            ConnectionPoolWrapper connectionPool = internalConnection.ConnectionPool;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            DataTestClass.AssertEqualsWithDescription(1, connectionPool.ConnectionCount, "Wrong number of connections in the pool.");
            DataTestClass.AssertEqualsWithDescription(0, connectionPool.FreeConnectionCount, "Wrong number of free connections in the pool.");

            using (PgConnection connection = new PgConnection(newConnectionString))
            {
                connection.Open();
                Assert.True(internalConnection.IsInternalConnectionOf(connection), "Connection has wrong internal connection");
                Assert.True(connectionPool.ContainsConnection(connection), "Connection is in wrong connection pool");
            }
        }

        private static void ReplacementConnectionUsesSemaphoreTest(string connectionString)
        {
            string newConnectionString = (new PgConnectionStringBuilder(connectionString) { MaxPoolSize = 2, ConnectTimeout = 5 }).ConnectionString;
            PgConnection.ClearAllPools();

            var liveConnection = new PgConnection(newConnectionString);
            var deadConnection = new PgConnection(newConnectionString);
            liveConnection.Open();
            deadConnection.Open();
            var deadConnectionInternal = new InternalConnectionWrapper(deadConnection);
            var liveConnectionInternal = new InternalConnectionWrapper(liveConnection);
            deadConnectionInternal.KillConnection();
            deadConnection.Close();
            liveConnection.Close();

            var tasks       = new Task<InternalConnectionWrapper>[3];
            var syncBarrier = new Barrier(tasks.Length);
            Func<InternalConnectionWrapper> taskFunction = (() => ReplacementConnectionUsesSemaphoreTask(newConnectionString, syncBarrier));
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew<InternalConnectionWrapper>(taskFunction);
            }

            bool taskWithLiveConnection   = false;
            bool taskWithNewConnection    = false;
            bool taskWithCorrectException = false;

            Task waitAllTask = Task.Factory.ContinueWhenAll(tasks, (completedTasks) =>
            {
                foreach (var item in completedTasks)
                {
                    if (item.Status == TaskStatus.Faulted)
                    {
                        // One task should have a timeout exception
                        if ((!taskWithCorrectException) 
                         && (item.Exception.InnerException is InvalidOperationException) 
                         && (item.Exception.InnerException.Message.StartsWith(SystemDataResourceManager.Instance.ADP_PooledOpenTimeout)))
                        {
                            taskWithCorrectException = true;
                        }
                        else if (!taskWithCorrectException)
                        {
                            // Rethrow the unknown exception
                            ExceptionDispatchInfo exceptionInfo = ExceptionDispatchInfo.Capture(item.Exception);
                            exceptionInfo.Throw();
                        }
                    }
                    else if (item.Status == TaskStatus.RanToCompletion)
                    {
                        // One task should get the live connection
                        if (item.Result.Equals(liveConnectionInternal))
                        {
                            if (!taskWithLiveConnection)
                            {
                                taskWithLiveConnection = true;
                            }
                        }
                        else if (!item.Result.Equals(deadConnectionInternal) && !taskWithNewConnection)
                        {
                            taskWithNewConnection = true;
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Task in unknown state: {0}", item.Status);
                    }
                }
            });

            waitAllTask.Wait();
            Assert.True(taskWithLiveConnection && taskWithNewConnection && taskWithCorrectException, string.Format("Tasks didn't finish as expected.\nTask with live connection: {0}\nTask with new connection: {1}\nTask with correct exception: {2}\n", taskWithLiveConnection, taskWithNewConnection, taskWithCorrectException));
        }

        private static InternalConnectionWrapper ReplacementConnectionUsesSemaphoreTask(string connectionString, Barrier syncBarrier)
        {
            InternalConnectionWrapper internalConnection = null;

            using (PgConnection connection = new PgConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    internalConnection = new InternalConnectionWrapper(connection);
                }
                catch
                {
                    syncBarrier.SignalAndWait();
                    throw;
                }

                syncBarrier.SignalAndWait();
            }

            return internalConnection;
        }

        private static InternalConnectionWrapper CreateEmancipatedConnection(string connectionString)
        {
            var connection = new PgConnection(connectionString);
            connection.Open();
            return new InternalConnectionWrapper(connection);
        }
    }
}
