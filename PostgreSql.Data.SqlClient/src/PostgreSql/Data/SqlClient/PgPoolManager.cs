// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using System.Threading;

namespace PostgreSql.Data.SqlClient
{
    internal sealed class PgPoolManager
    {
        private static readonly PgPoolManager s_instance = new PgPoolManager();

        public static PgPoolManager Instance
        {
            get { return PgPoolManager.s_instance; }
        }

        private Hashtable _pools;
        private Hashtable _handlers;
        private object    _syncObject;

        public int PoolsCount
        {
            get
            {
                if (_pools != null)
                {
                    return _pools.Count;
                }

                return 0;
            }
        }

        private Hashtable Pools
        {
            get
            {
                if (_pools == null)
                {
                    _pools = Hashtable.Synchronized(new Hashtable());
                }

                return _pools;
            }
        }

        private Hashtable Handlers
        {
            get
            {
                if (_handlers == null)
                {
                    _handlers = Hashtable.Synchronized(new Hashtable());
                }

                return _handlers;
            }
        }

        private object SyncObject
        {
            get
            {
                if (_syncObject == null)
                {
                    Interlocked.CompareExchange(ref _syncObject, new object(), null);
                }

                return _syncObject;
            }
        }

        private PgPoolManager()
        {
        }

        public PgConnectionPool GetPool(string connectionString)
        {
            PgConnectionPool pool = FindPool(connectionString);

            if (pool == null)
            {
                pool = CreatePool(connectionString);
            }

            return pool;
        }

        public PgConnectionPool FindPool(string connectionString)
        {
            PgConnectionPool pool = null;

            lock (SyncObject)
            {
                int hashCode = connectionString.GetHashCode();

                if (Pools.ContainsKey(hashCode))
                {
                    pool = (PgConnectionPool)Pools[hashCode];
                }
            }

            return pool;
        }

        public PgConnectionPool CreatePool(string connectionString)
        {
            PgConnectionPool pool = null;

            lock (SyncObject)
            {
                pool = FindPool(connectionString);

                if (pool == null)
                {
                    lock (Pools.SyncRoot)
                    {
                        int hashcode = connectionString.GetHashCode();

                        // Create an empty pool	handler
                        EmptyPoolEventHandler handler = new EmptyPoolEventHandler(OnEmptyPool);

                        Handlers.Add(hashcode, handler);

                        // Create the new connection pool
                        pool = new PgConnectionPool(connectionString);

                        Pools.Add(hashcode, pool);

                        pool.EmptyPool += handler;
                    }
                }
            }

            return pool;
        }

        public void ClearAllPools()
        {
            lock (SyncObject)
            {
                lock (Pools.SyncRoot)
                {
                    PgConnectionPool[] tempPools = new PgConnectionPool[Pools.Count];

                    Pools.Values.CopyTo(tempPools, 0);

                    foreach (PgConnectionPool pool in tempPools)
                    {
                        // Clear pool
                        pool.Clear();
                    }

                    // Clear Hashtables
                    Pools.Clear();
                    Handlers.Clear();
                }
            }
        }

        public void ClearPool(string connectionString)
        {
            lock (SyncObject)
            {
                lock (Pools.SyncRoot)
                {
                    int hashCode = connectionString.GetHashCode();

                    if (Pools.ContainsKey(hashCode))
                    {
                        PgConnectionPool pool = (PgConnectionPool)Pools[hashCode];

                        // Clear pool
                        pool.Clear();
                    }
                }
            }
        }

        public int GetPooledConnectionCount(string connectionString)
        {
            PgConnectionPool pool = FindPool(connectionString);

            return (pool != null) ? pool.Count : 0;
        }

        private void OnEmptyPool(object sender, EventArgs e)
        {
            lock (Pools.SyncRoot)
            {
                int hashCode = (int)sender;

                if (Pools.ContainsKey(hashCode))
                {
                    PgConnectionPool pool = (PgConnectionPool)Pools[hashCode];

                    lock (pool.SyncObject)
                    {
                        EmptyPoolEventHandler handler = (EmptyPoolEventHandler)Handlers[hashCode];

                        pool.EmptyPool -= handler;

                        Pools.Remove(hashCode);
                        Handlers.Remove(hashCode);

                        pool = null;
                        handler = null;
                    }
                }
            }
        }
    }
}
