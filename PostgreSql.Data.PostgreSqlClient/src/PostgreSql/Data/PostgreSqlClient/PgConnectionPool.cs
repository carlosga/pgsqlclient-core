// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Protocol;
using System;
using System.Collections;
using System.Threading;

namespace PostgreSql.Data.PostgreSqlClient
{
    internal sealed class PgConnectionPool
    {
        private enum MoveType
        {
            LockedToUnlocked,
            UnlockedToLocked
        }

        public event EmptyPoolEventHandler EmptyPool;

        private PgConnectionOptions _options;
        private ArrayList           _locked;
        private ArrayList           _unlocked;
        private Thread              _cleanUpThread;
        private string              _connectionString;
        private bool                _isRunning;
        private long                _lifeTime;
        private object              _syncObject;

        public object SyncObject
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

        public int Count
        {
            get
            {
                lock (_unlocked.SyncRoot)
                {
                    return _unlocked.Count + _locked.Count;
                }
            }
        }

        public bool HasUnlocked
        {
            get { return _unlocked.Count > 0; }
        }

        public PgConnectionPool(string connectionString)
        {
            _connectionString = connectionString;
            _options          = new PgConnectionOptions(connectionString);
            _lifeTime         = _options.ConnectionLifeTime * TimeSpan.TicksPerSecond;

            if (_options.MaxPoolSize == 0)
            {
                _locked   = ArrayList.Synchronized(new ArrayList());
                _unlocked = ArrayList.Synchronized(new ArrayList());
            }
            else
            {
                _locked   = ArrayList.Synchronized(new ArrayList(_options.MaxPoolSize));
                _unlocked = ArrayList.Synchronized(new ArrayList(_options.MaxPoolSize));
            }

            // If a	minimun	number of connections is requested initialize the pool
            Initialize();

            // Start the cleanup thread	only if	needed
            if (_lifeTime != 0)
            {
                _isRunning = true;

                _cleanUpThread      = new Thread(new ThreadStart(RunCleanup));
                _cleanUpThread.Name = "Cleanup Thread";
                _cleanUpThread.Start();
                _cleanUpThread.IsBackground = true;
            }
        }

        public void CheckIn(PgConnectionInternal connection)
        {
            connection.Created = System.DateTime.Now.Ticks;

            MoveConnection(connection, MoveType.LockedToUnlocked);
        }

        public PgConnectionInternal CheckOut()
        {
            PgConnectionInternal newConnection = null;

            lock (SyncObject)
            {
                // 1. Try to Get a connection from the unlocked connection list.
                newConnection = GetConnection();
                if (newConnection != null)
                {
                    return newConnection;
                }

                // 2. Check if we have reached the max number of allowed connections
                CheckMaxPoolSize();

                // 3. Try to Get a connection from the unlocked connection list.
                newConnection = GetConnection();
                if (newConnection != null)
                {
                    return newConnection;
                }

                // 4. In any other case create a new connection
                newConnection = Create();

                // Set connection pooling settings to the new connection
                newConnection.Lifetime = _options.ConnectionLifeTime;
                newConnection.Pooled   = true;

                // Added to	the	locked connections list.
                _locked.Add(newConnection);
            }

            return newConnection;
        }

        public void Clear()
        {
            lock (SyncObject)
            {
                // Stop	cleanup	thread
                if (_cleanUpThread != null)
                {
#warning TODO: Check how to abort the thread on .NET Core 
                    // _cleanUpThread.Abort();
                    _cleanUpThread.Join();
                }

                // Close all unlocked connections
                PgConnectionInternal[] list = (PgConnectionInternal[])_unlocked.ToArray(typeof(PgConnectionInternal));

                foreach (PgConnectionInternal connection in list)
                {
                    connection.Close();
                }

                // Close all locked	connections
                list = (PgConnectionInternal[])_locked.ToArray(typeof(PgConnectionInternal));

                foreach (PgConnectionInternal connection in list)
                {
                    connection.Close();
                }

                // Clear lists
                _unlocked.Clear();
                _locked.Clear();

                // Raise EmptyPool event
                if (EmptyPool != null)
                {
                    EmptyPool(_connectionString.GetHashCode(), null);
                }

                // Reset fields
                _unlocked         = null;
                _locked           = null;
                _connectionString = null;
                _cleanUpThread    = null;
                EmptyPool         = null;
            }
        }

        private void Initialize()
        {
            lock (SyncObject)
            {
                for (int i = 0; i < _options.MinPoolSize; i++)
                {
                    _unlocked.Add(Create());
                }
            }
        }

        private PgConnectionInternal Create()
        {
            var connection = new PgConnectionInternal(_connectionString);

            connection.Pooled  = true;
            connection.Created = DateTime.Now.Ticks;

            return connection;
        }

        private PgConnectionInternal GetConnection()
        {
            PgConnectionInternal result = null;
            long check = -1;

            lock (_unlocked.SyncRoot)
            {
                PgConnectionInternal[] connections = (PgConnectionInternal[])_unlocked.ToArray(typeof(PgConnectionInternal));

                for (int i = connections.Length - 1; i >= 0; i--)
                {
                    if (connections[i].Verify())
                    {
                        if (_lifeTime != 0)
                        {
                            long now    = DateTime.Now.Ticks;
                            long expire = connections[i].Created + _lifeTime;

                            if (now >= expire)
                            {
                                if (CheckMinPoolSize())
                                {
                                    _unlocked.Remove(connections[i]);
                                    Expire(connections[i]);
                                }
                            }
                            else
                            {
                                if (expire > check)
                                {
                                    check = expire;
                                    result = connections[i];
                                }
                            }
                        }
                        else
                        {
                            result = connections[i];
                            break;
                        }
                    }
                    else
                    {
                        _unlocked.Remove(connections[i]);
                        Expire(connections[i]);
                    }
                }

                if (result != null)
                {
                    MoveConnection(result, MoveType.UnlockedToLocked);
                }
            }

            return result;
        }

        private bool CheckMinPoolSize()
        {
            return !(_options.MinPoolSize > 0 && Count == _options.MinPoolSize);
        }

        private void CheckMaxPoolSize()
        {
            if (_options.MaxPoolSize > 0 && Count >= _options.MaxPoolSize)
            {
                long timeout = _options.ConnectionTimeout * TimeSpan.TicksPerSecond;
                long start = DateTime.Now.Ticks;

                /*
                 Loop brakes without errors in next situations:
                    1. connection was returned from locked to unlocked by calling CheckIn in other thread (HasUnlocked = true)
                    2. connection was moved from locked to unlocked (by Checkin) and then cleaned (removed from unlocked by Cleanup)
                */
                while (true)
                {
                    if (Count >= _options.MaxPoolSize && HasUnlocked == false)
                    {
                        if ((DateTime.Now.Ticks - start) > timeout)
                        {
                            throw new Exception("Timeout exceeded.");
                        }

                        Thread.Sleep(100);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void RunCleanup()
        {
            int interval = Convert.ToInt32(TimeSpan.FromTicks(_lifeTime).TotalMilliseconds);

            if (interval > 60000)
            {
                interval = 60000;
            }

            try
            {
                while (_isRunning)
                {
                    Thread.Sleep(interval);

                    Cleanup();

                    if (Count == 0)
                    {
                        lock (SyncObject)
                        {
                            // Empty pool
                            if (EmptyPool != null)
                            {
                                EmptyPool(_connectionString.GetHashCode(), null);
                            }

                            // Stop	running
                            _isRunning = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
                _isRunning = false;
            }
        }

        private void Expire(PgConnectionInternal connection)
        {
            try
            {
                if (connection.Verify())
                {
                    connection.Close();
                }
            }
            catch (Exception)
            {
                // Do not raise an exception as the connection could be invalid due to several reasons
                // ( network problems, server shutdown, ... )
            }
        }

        private void Cleanup()
        {
            lock (_unlocked.SyncRoot)
            {
                if (_unlocked.Count > 0 && _lifeTime != 0)
                {
                    PgConnectionInternal[] list = (PgConnectionInternal[])_unlocked.ToArray(typeof(PgConnectionInternal));

                    foreach (PgConnectionInternal connection in list)
                    {
                        long now = DateTime.Now.Ticks;
                        long expire = connection.Created + _lifeTime;

                        if (now >= expire)
                        {
                            if (CheckMinPoolSize())
                            {
                                _unlocked.Remove(connection);
                                Expire(connection);
                            }
                        }
                    }
                }
            }
        }

        private void MoveConnection(PgConnectionInternal connection, MoveType moveType)
        {
            if (null == connection)
            {
                return;
            }

            lock (_unlocked.SyncRoot)
            {
                switch (moveType)
                {
                    case MoveType.LockedToUnlocked:
                        _locked.Remove(connection);
                        _unlocked.Add(connection);
                        break;

                    case MoveType.UnlockedToLocked:
                        _unlocked.Remove(connection);
                        _locked.Add(connection);
                        break;
                }
            }
        }
    }
}