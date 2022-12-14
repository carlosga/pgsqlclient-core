// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System;
using PostgreSql.Data.SqlClient.Tests.SystemDataInternals;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class ConnectionPoolWrapper
    {
        private object _connectionPool = null;

        /// <summary>
        /// The number of connections in this connection pool (free + non-free; including transaction pools)
        /// </summary>
        public int ConnectionCount => ConnectionPoolHelper.CountConnectionsInPool(_connectionPool); 

        /// <summary>
        /// Counts the number of free connection in the pool (excluding any transaction pools)
        /// </summary>
        public int FreeConnectionCount => ConnectionPoolHelper.CountFreeConnections(_connectionPool);

        /// <summary>
        /// The connection string associated with this connection pool
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Finds the connection pool for the given connection
        /// NOTE: This only works for connections that are currently open
        /// </summary>
        /// <param name="connection"></param>
        public ConnectionPoolWrapper(PgConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _connectionPool = ConnectionHelper.GetConnectionPool(connection.GetInternalConnection());
            ConnectionString = connection.ConnectionString;

            if (_connectionPool == null)
            {
                throw new ArgumentException("Provided connection does not have a connection pool", "connection");
            }
        }

        /// <summary>
        /// Finds the connection pool for the given connection string
        /// </summary>
        /// <param name="connectionString"></param>
        public ConnectionPoolWrapper(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            ConnectionString = connectionString;
            _connectionPool = ConnectionPoolHelper.ConnectionPoolFromString(connectionString);

            if (_connectionPool == null)
            {
                throw new ArgumentException("No pool exists for the provided connection string", "connectionString");
            }
        }

        /// <summary>
        /// Finds the connection pool for the given internal connection
        /// </summary>
        /// <param name="internalConnection"></param>
        internal ConnectionPoolWrapper(object internalConnection, string connectionString)
        {
            _connectionPool = ConnectionHelper.GetConnectionPool(internalConnection);
            ConnectionString = connectionString;

            if (_connectionPool == null)
            {
                throw new ArgumentException("Provided internal connection does not have a connection pool", "internalConnection");
            }
        }

        private ConnectionPoolWrapper()
        { 
        }

        /// <summary>
        /// Creates a list of all connection pools
        /// </summary>
        /// <returns></returns>
        public static ConnectionPoolWrapper[] AllConnectionPools()
        {
            return (from t in ConnectionPoolHelper.AllConnectionPools()
                    select new ConnectionPoolWrapper() { _connectionPool = t.Item1 }).ToArray();
        }

        /// <summary>
        /// Invokes the cleanup timer code
        /// </summary>
        public void Cleanup()
        {
            ConnectionPoolHelper.CleanConnectionPool(_connectionPool);
        }

        /// <summary>
        /// Checks if the PgConnection specified has an internal connection that belongs to this pool
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool ContainsConnection(PgConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return (_connectionPool == ConnectionHelper.GetConnectionPool(connection.GetInternalConnection()));
        }

        /// <summary>
        /// Checks if this wrapper wraps the same pool as another wrapper
        /// </summary>
        /// <param name="obj">Wrapper to check against</param>
        /// <returns>True if <paramref name="obj"/> is a ConnectionPoolWrapper that points to the same pool as this wrapper, otherwise false</returns>
        public override bool Equals(object obj)
        {
            var objAsPool = obj as ConnectionPoolWrapper;
            return ((objAsPool != null) && (objAsPool._connectionPool == _connectionPool));
        }

        public override int GetHashCode()
        {
            return _connectionPool.GetHashCode();
        }
    }
}
