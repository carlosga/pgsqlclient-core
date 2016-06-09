// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using PostgreSql.Data.SqlClient.Tests.SystemDataInternals;
using System.Threading;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class InternalConnectionWrapper
    {
        private static Dictionary<string, string> s_killByTPgConnectionStrings = new Dictionary<string, string>();
        private static ReaderWriterLockSlim s_killByTPgConnectionStringsLock = new ReaderWriterLockSlim();

        private object _internalConnection = null;

        /// <summary>
        /// Gets the internal connection associated with the given PgConnection
        /// </summary>
        /// <param name="connection">Live outer connection to grab the inner connection from</param>
        /// <param name="supportKillByTSql">If true then we will query the server for this connection's SPID details (to be used in the KillConnectionByTSql method)</param>
        public InternalConnectionWrapper(PgConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            _internalConnection = connection.GetInternalConnection();
            ConnectionString    = connection.ConnectionString;
        }

        /// <summary>
        /// Gets the connection pool this internal connection is in
        /// </summary>
        public ConnectionPoolWrapper ConnectionPool => new ConnectionPoolWrapper(_internalConnection, ConnectionString); 

        /// <summary>
        /// Is this internal connection associated with the given PgConnection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public bool IsInternalConnectionOf(PgConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException("connection");
            }

            return (_internalConnection == connection.GetInternalConnection());
        }

        /// <summary>
        /// The connection string used to create this connection
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// True if the connection is still alive, otherwise false
        /// NOTE: Do NOT use this on a connection that is currently in use (There is a Debug.Assert and it will always return true)
        /// NOTE: If the connection is dead, it will be marked as 'broken'
        /// </summary>
        public bool IsConnectionAlive()
        {
            return ConnectionHelper.IsConnectionAlive(_internalConnection);
        }
        public void KillConnection()
        {
            #warning TODO: Look if it can be implemented
            // object tdsParser = ConnectionHelper.GetParser(_internalConnection);
            // object stateObject = TdsParserHelper.GetStateObject(tdsParser);
            // object sessionHandle = TdsParserStateObjectHelper.GetSessionHandle(stateObject);

            // Assembly systemDotData = Assembly.Load(new AssemblyName(typeof(SqlConnection).GetTypeInfo().Assembly.FullName));
            // Type sniHandleType = systemDotData.GetType("System.Data.SqlClient.SNI.SNIHandle");
            // MethodInfo killConn = sniHandleType.GetMethod("KillConnection");

            // if (killConn != null)
            // {
            //     killConn.Invoke(sessionHandle, null);
            // }
            // else
            // {
            //     throw new InvalidOperationException("Error: Could not find SNI KillConnection test hook. This operation is only supported in debug builds.");
            // }
            // // Ensure kill occurs outside of check connection window
            // Thread.Sleep(100);
        }
        
        /// <summary>
        /// Converts a connection string for a format which is appropriate to kill another connection with (i.e. non-pooled, no transactions)
        /// </summary>
        /// <param name="connectionString">Base connection string to convert</param>
        /// <returns>The converted connection string</returns>
        private static string CreateKillByTPgConnectionString(string connectionString)
        {
            var builder = new PgConnectionStringBuilder(connectionString);
            // Avoid tampering with the connection pool
            builder.Pooling = false;
            return builder.ConnectionString;
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            bool areEquals = false;

            InternalConnectionWrapper objAsWrapper = obj as InternalConnectionWrapper;
            if ((objAsWrapper != null) && (objAsWrapper._internalConnection == _internalConnection))
            {
                areEquals = true;
            }

            return areEquals;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return _internalConnection.GetHashCode();
        }
    }
}
