// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;

namespace PostgreSql.Data.SqlClient.Tests.SystemDataInternals
{
    internal static class ConnectionPoolHelper
    {
        private static Assembly s_systemDotData = Assembly.Load(new AssemblyName(typeof(PgConnection).GetTypeInfo().Assembly.FullName));
        private static Type s_dbConnectionPool = s_systemDotData.GetType("System.Data.ProviderBase.DbConnectionPool");
        private static Type s_dbConnectionPoolGroup = s_systemDotData.GetType("System.Data.ProviderBase.DbConnectionPoolGroup");
        private static Type s_dbConnectionPoolIdentity = s_systemDotData.GetType("System.Data.ProviderBase.DbConnectionPoolIdentity");
        private static Type s_dbConnectionFactory = s_systemDotData.GetType("System.Data.ProviderBase.DbConnectionFactory");
        private static Type s_sqlConnectionFactory = s_systemDotData.GetType("PostgreSql.Data.SqlClient.PgConnectionFactory");
        private static Type s_dbConnectionPoolKey = s_systemDotData.GetType("System.Data.Common.DbConnectionPoolKey");
        private static Type s_dictStringPoolGroup = typeof(Dictionary<,>).MakeGenericType(s_dbConnectionPoolKey, s_dbConnectionPoolGroup);
        private static Type s_dictPoolIdentityPool = typeof(ConcurrentDictionary<,>).MakeGenericType(s_dbConnectionPoolIdentity, s_dbConnectionPool);
        private static PropertyInfo s_dbConnectionPoolCount = s_dbConnectionPool.GetProperty("Count", BindingFlags.Instance | BindingFlags.NonPublic);
        private static PropertyInfo s_dictStringPoolGroupGetKeys = s_dictStringPoolGroup.GetProperty("Keys");
        private static PropertyInfo s_dictPoolIdentityPoolValues = s_dictPoolIdentityPool.GetProperty("Values");
        private static FieldInfo s_dbConnectionFactoryPoolGroupList = s_dbConnectionFactory.GetField("_connectionPoolGroups", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo s_dbConnectionPoolGroupPoolCollection = s_dbConnectionPoolGroup.GetField("_poolCollection", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo s_sqlConnectionFactorySingleton = s_sqlConnectionFactory.GetField("SingletonInstance", BindingFlags.Static | BindingFlags.Public);
        private static FieldInfo s_dbConnectionPoolStackOld = s_dbConnectionPool.GetField("_stackOld", BindingFlags.Instance | BindingFlags.NonPublic);
        private static FieldInfo s_dbConnectionPoolStackNew = s_dbConnectionPool.GetField("_stackNew", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo s_dbConnectionPoolCleanup = s_dbConnectionPool.GetMethod("CleanupCallback", BindingFlags.Instance | BindingFlags.NonPublic);
        private static MethodInfo s_dictStringPoolGroupTryGetValue = s_dictStringPoolGroup.GetMethod("TryGetValue");

        public static int CountFreeConnections(object pool)
        {
            VerifyObjectIsPool(pool);

            ICollection oldStack = (ICollection)s_dbConnectionPoolStackOld.GetValue(pool);
            ICollection newStack = (ICollection)s_dbConnectionPoolStackNew.GetValue(pool);

            return (oldStack.Count + newStack.Count);
        }

        /// <summary>
        /// Finds all connection pools
        /// </summary>
        /// <returns></returns>
        public static List<Tuple<object, object>> AllConnectionPools()
        {
            var connectionPools    = new List<Tuple<object, object>>();
            var factorySingleton   = s_sqlConnectionFactorySingleton.GetValue(null);
            var AllPoolGroups      = s_dbConnectionFactoryPoolGroupList.GetValue(factorySingleton);
            var connectionPoolKeys = (ICollection)s_dictStringPoolGroupGetKeys.GetValue(AllPoolGroups, null);
            
            foreach (var item in connectionPoolKeys)
            {
                object[] args = new object[] { item, null };
                
                s_dictStringPoolGroupTryGetValue.Invoke(AllPoolGroups, args);
                if (args[1] != null)
                {
                    var poolCollection = s_dbConnectionPoolGroupPoolCollection.GetValue(args[1]);
                    var poolList       = (IEnumerable)(s_dictPoolIdentityPoolValues.GetValue(poolCollection));
                    foreach (object pool in poolList)
                    {
                        connectionPools.Add(new Tuple<object, object>(pool, item));
                    }
                }
            }

            return connectionPools;
        }

        /// <summary>
        /// Finds a connection pool based on a connection string
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static object ConnectionPoolFromString(string connectionString)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException("connectionString");
            }

            object pool             = null;
            object factorySingleton = s_sqlConnectionFactorySingleton.GetValue(null);
            object allPoolGroups    = s_dbConnectionFactoryPoolGroupList.GetValue(factorySingleton);
            var    args             = new object[] { connectionString, null };
            bool   found            = (bool)s_dictStringPoolGroupTryGetValue.Invoke(allPoolGroups, args);
            
            if ((found) && (args[1] != null))
            {
                ICollection poolList = (ICollection)s_dictPoolIdentityPoolValues.GetValue(args[1]);
                if (poolList.Count == 1)
                {
                    poolList.Cast<object>().First();
                }
                else if (poolList.Count > 1)
                {
                    throw new NotSupportedException("Using multiple identities with SSPI is not supported");
                }
            }

            return pool;
        }

        /// <summary>
        /// Causes the cleanup timer code in the connection pool to be invoked
        /// </summary>
        /// <param name="obj">A connection pool object</param>
        internal static void CleanConnectionPool(object pool)
        {
            VerifyObjectIsPool(pool);
            s_dbConnectionPoolCleanup.Invoke(pool, new object[] { null });
        }

        /// <summary>
        /// Counts the number of connections in a connection pool
        /// </summary>
        /// <param name="pool">Pool to count connections in</param>
        /// <returns></returns>
        internal static int CountConnectionsInPool(object pool)
        {
            VerifyObjectIsPool(pool);
            return (int)s_dbConnectionPoolCount.GetValue(pool, null);
        }

        private static void VerifyObjectIsPool(object pool)
        {
            if (pool == null)
            {
                throw new ArgumentNullException("pool");
            }
            if (!s_dbConnectionPool.IsInstanceOfType(pool))
            {
                throw new ArgumentException("Object provided was not a DbConnectionPool", "pool");
            }
        }
    }
}
