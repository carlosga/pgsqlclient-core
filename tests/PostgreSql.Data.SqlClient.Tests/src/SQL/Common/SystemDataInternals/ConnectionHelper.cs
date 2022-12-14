// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;
using System.Diagnostics;
using System;

namespace PostgreSql.Data.SqlClient.Tests.SystemDataInternals
{
    internal static class ConnectionHelper
    {
        private static Assembly     s_systemDotData                         = Assembly.Load(new AssemblyName(typeof(PgConnection).GetTypeInfo().Assembly.FullName));
        private static Type         s_sqlConnection                         = s_systemDotData.GetType("PostgreSql.Data.SqlClient.PgConnection");
        private static Type         s_sqlInternalConnection                 = s_systemDotData.GetType("PostgreSql.Data.SqlClient.PgConnectionInternal");
        private static Type         s_dbConnectionInternal                  = s_systemDotData.GetType("System.Data.ProviderBase.DbConnectionInternal");
        private static PropertyInfo s_sqlConnectionInternalConnection       = s_sqlConnection.GetProperty("InnerConnection", BindingFlags.Instance | BindingFlags.NonPublic);
        private static PropertyInfo s_dbConnectionInternalPool              = s_dbConnectionInternal.GetProperty("Pool", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo   s_dbConnectionInternalIsConnectionAlive = s_dbConnectionInternal.GetMethod("IsConnectionAlive", BindingFlags.Instance | BindingFlags.NonPublic);

        public static object GetConnectionPool(object internalConnection)
        {
            VerifyObjectIsInternalConnection(internalConnection);
            return s_dbConnectionInternalPool.GetValue(internalConnection, null);
        }

        public static object GetInternalConnection(this PgConnection connection)
        {
            object internalConnection = s_sqlConnectionInternalConnection.GetValue(connection, null);
            Debug.Assert(((internalConnection != null) && (s_dbConnectionInternal.IsInstanceOfType(internalConnection))), "Connection provided has an invalid internal connection");
            return internalConnection;
        }

        public static bool IsConnectionAlive(object internalConnection)
        {
            VerifyObjectIsInternalConnection(internalConnection);
            return (bool)s_dbConnectionInternalIsConnectionAlive.Invoke(internalConnection, new object[] { false });
        }

        private static void VerifyObjectIsInternalConnection(object internalConnection)
        {
            if (internalConnection == null)
            {
                throw new ArgumentNullException("internalConnection");
            }
            if (!s_dbConnectionInternal.IsInstanceOfType(internalConnection))
            {
                throw new ArgumentException("Object provided was not a DbConnectionInternal", "internalConnection");
            }
        }
    }
}
