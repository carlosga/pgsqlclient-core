// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Frontend;
using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.NetworkInformation;
using System.Buffers;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgDataReader
        : DbDataReader, IDbColumnSchemaGenerator
    {
        private const int STARTPOS = -1;

        private bool             _open;
        private int              _position;
        private int              _recordsAffected;
        private DataRow          _row;
        private CommandBehavior  _behavior;
        private PgCommand        _command;
        private PgConnection     _connection;
        private Statement        _statement;
        private Statement        _refCursor;
        private Queue<Statement> _refCursors;

        private ReadOnlyCollection<DbColumn> _metadata;

        public override object this[int i]       => GetValue(i);
        public override object this[string name] => GetValue(name);

        public override int  Depth
        {
            get
            {
                if (IsClosed)
                {
                    throw ADP.DataReaderClosed();
                }
                return 0;
            }
        }

        public override bool IsClosed        => !_open;
        public override int  RecordsAffected => _open ? _recordsAffected : -1;

        public override int FieldCount
        {
            get
            {
                if (IsClosed)
                {
                    throw ADP.DataReaderClosed();
                }
                return _statement?.RowDescriptor.Count ?? 0;
            }
        }

        public override bool HasRows
        {
            get
            {
                if (IsClosed)
                {
                    throw ADP.DataReaderClosed();
                }
                return _statement.HasRows;
            }
        }

        internal PgCommand Command => _command;

        internal PgDataReader(PgConnection connection, PgCommand command)
        {
            _open            = true;
            _recordsAffected = -1;
            _position        = STARTPOS;
            _connection      = connection;
            _command         = command;
            _behavior        = _command.CommandBehavior;
            _statement       = _command.Statement;
            _row             = new Frontend.DataRow();

            _connection.AddWeakReference(this, PgReferenceCollection.DataReaderTag);

            InitializeRefCursors();
        }

        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    InternalClose();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
        #endregion

        public ReadOnlyCollection<DbColumn> GetColumnSchema()
        {
            if (_metadata == null)
            {
                var descriptor = _refCursor?.RowDescriptor ?? _statement.RowDescriptor;
                var provider   = new DbColumnSchemaGenerator(_statement.Connection, descriptor);

                _metadata = provider.GetColumnSchema();
            }

            return _metadata;
        }

        public override bool NextResult()
        {
            if (IsClosed)
            {
                throw ADP.DataReaderClosed();
            }

            // Throw exception if the statement has been cancelled
            if (_refCursor != null)
            {
                _refCursor.ThrowIfCancelled();
            }
            else
            {
                _statement.ThrowIfCancelled();
            }

            // Reset position
            _position = STARTPOS;

            // Reset current row data
            _row?.Reset();

            // Reset records affected
            _recordsAffected = -1;

            // Reset metadata information
            _metadata = null;

            // Close the current ref cursor
            _refCursor?.CloseStatement();

            // Query for next result
            if (_refCursors != null && _refCursors.Count != 0)
            {
                return NextResultFromRefCursor();
            }

            // Close the active statement
            _statement.CloseStatement();

            return _command.NextResult(); 
        }

        public override bool Read()
        {
            if (IsClosed)
            {
                throw ADP.DataReaderClosed();
            }
            
            if (!_statement.HasRows)
            {
                return false;
            }

            _position++;

            return (_refCursor != null) ? _row.ReadFrom(_refCursor) : _row.ReadFrom(_statement);
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            CheckDataIsReady();

            return _row.GetBytes(i, dataIndex, buffer, bufferIndex, length);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            CheckDataIsReady();
            return _row.GetChars(i, dataIndex, buffer, bufferIndex, length);
        }

        public override bool GetBoolean(int i)
        {
            CheckDataIsReady();
            return _row.GetBoolean(i);
        }

        public override byte GetByte(int i)
        {
            CheckDataIsReady();
            return _row.GetByte(i);
        }

        public override char GetChar(int i)
        {
            CheckDataIsReady();
            return _row.GetChar(i);
        }

        public override DateTime GetDateTime(int i)
        {
            CheckDataIsReady();
            return _row.GetDateTime(i);
        }

        public DateTimeOffset GetDateTimeOffset(int i)
        {
            CheckDataIsReady();
            return _row.GetDateTimeOffset(i);
        }

        public override decimal GetDecimal(int i)
        {
            CheckDataIsReady();
            return _row.GetDecimal(i);
        }

        public override double GetDouble(int i)
        {
            CheckDataIsReady();
            return _row.GetDouble(i);
        }

        public override float GetFloat(int i)
        {
            CheckDataIsReady();
            return _row.GetFloat(i);
        }

        public override Guid GetGuid(int i)
        {
            CheckDataIsReady();
            return _row.GetGuid(i);
        }

        public IPAddress GetIPAddress(int i)
        {
            CheckDataIsReady();
            return _row.GetIPAddress(i);
        }

        public override short GetInt16(int i)
        {
            CheckDataIsReady();
            return _row.GetInt16(i);
        }

        public override int GetInt32(int i)
        {
            CheckDataIsReady();
            return _row.GetInt32(i);
        }

        public override long GetInt64(int i)
        {
            CheckDataIsReady();
            return _row.GetInt64(i);
        }

        public PhysicalAddress GetMacAddress(int i)
        {
            CheckDataIsReady();
            return _row.GetMacAddress(i);
        }

        public override string GetString(int i)
        {
            CheckDataIsReady();
            return _row.GetString(i);
        }

        public TimeSpan GetTimeSpan(int i)
        {
            CheckDataIsReady();
            return _row.GetTimeSpan(i);
        }

        public PgBinary    GetPgBinary(int i)    => GetFieldValue<PgBinary>(i);
        public PgBit       GetPgBit(int i)       => GetFieldValue<PgBit>(i);
        public PgBoolean   GetPgBoolean(int i)   => GetFieldValue<PgBoolean>(i);
        public PgBox       GetPgBox(int i)       => GetFieldValue<PgBox>(i);
        public PgBox2D     GetPgBox2D(int i)     => GetFieldValue<PgBox2D>(i);
        public PgBox3D     GetPgBox3D(int i)     => GetFieldValue<PgBox3D>(i);
        public PgByte      GetPgByte(int i)      => GetFieldValue<PgByte>(i);
        public PgCircle    GetPgCircle(int i)    => GetFieldValue<PgCircle>(i);
        public PgDouble    GetPgDouble(int i)    => GetFieldValue<PgDouble>(i);
        public PgInt16     GetPgInt16(int i)     => GetFieldValue<PgInt16>(i);
        public PgInt32     GetPgInt32(int i)     => GetFieldValue<PgInt32>(i);
        public PgInt64     GetPgInt64(int i)     => GetFieldValue<PgInt64>(i);
        public PgNumeric   GetPgNumeric(int i)   => GetFieldValue<PgNumeric>(i);
        public PgMoney     GetPgMoney(int i)     => GetFieldValue<PgMoney>(i);
        public PgReal      GetPgReal(int i)      => GetFieldValue<PgReal>(i);
        public PgDate      GetPgDate(int i)      => GetFieldValue<PgDate>(i);
        public PgTime      GetPgTime(int i)      => GetFieldValue<PgTime>(i);
        public PgTimestamp GetPgTimestamp(int i) => GetFieldValue<PgTimestamp>(i);
        public PgInterval  GetPgInterval(int i)  => GetFieldValue<PgInterval>(i);
        public PgLine      GetPgLine(int i)      => GetFieldValue<PgLine>(i);
        public PgLSeg      GetPgLSeg(int i)      => GetFieldValue<PgLSeg>(i);
        public PgPath      GetPgPath(int i)      => GetFieldValue<PgPath>(i);
        public PgPoint     GetPgPoint(int i)     => GetFieldValue<PgPoint>(i);
        public PgPoint2D   GetPgPoint2D(int i)   => GetFieldValue<PgPoint2D>(i);
        public PgPoint3D   GetPgPoint3D(int i)   => GetFieldValue<PgPoint3D>(i);
        public PgPolygon   GetPgPolygon(int i)   => GetFieldValue<PgPolygon>(i);

        public override string GetDataTypeName(int i)
        {
            CheckIndex(i);
            return _statement.RowDescriptor[i].TypeInfo.Name;
        }

        public override Type GetFieldType(int i)
        {
            CheckIndex(i);
            return _statement.RowDescriptor[i].TypeInfo.SystemType;
        }

        public override string GetName(int i)
        {
            CheckIndex(i);
            return _statement.RowDescriptor[i].Name;
        }

        public override int GetOrdinal(string name)
        {
            CheckDataIsReady();
            return _statement.RowDescriptor.IndexOf(name);
        }

        public override object GetValue(int i)
        {
            CheckDataIsReady();
            return _row[i];
        }

        public override T GetFieldValue<T>(int i)
        {
            CheckDataIsReady();
            return _row.GetFieldValue<T>(i);
        }

        public override int GetValues(object[] values)
        {
            CheckDataIsReady();
            return _row.GetValues(values);
        }

        public override bool IsDBNull(int i)
        {
            CheckDataIsReady();
            return _row.IsDBNull(i);
        }

        public override Type GetProviderSpecificFieldType(int i)
        {
            CheckIndex(i);
            return _statement.RowDescriptor[i].TypeInfo.PgType;
        }

        public override object GetProviderSpecificValue(int i)
        {
            CheckDataIsReady();
            return _row.GetProviderSpecificValue(i);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            CheckDataIsReady();
            return _row.GetProviderSpecificValues(values);
        }

        public override IEnumerator GetEnumerator() => new PgEnumerator(this, true);

        // internal PgDataRecord GetDataRecord() => new PgDataRecord(_row);

        internal PgDataRecord GetDataRecord() => throw ADP.NotSupported();

        internal void CloseReaderFromConnection()
        {
            InternalClose(true);
        }
        
        internal void CloseReaderFromCommand()
        {
            InternalClose();
        }

        private void InternalClose(bool fromConnection = false)
        {
            if (!_open || _disposed) 
            {
                return;
            }

            try
            {
                // This will update RecordsAffected property
                UpdateRecordsAffected();

                // Reset current row
                _row?.Reset();

                // Clear ref cursors
                _refCursor?.CloseStatement();
                _refCursors?.Clear();

                // Reset state and position
                _open     = false;
                _position = STARTPOS;

                // Remove the weak reference hold by the connection
                _connection.RemoveWeakReference(this);

                // Check if the connection should be closed
                if (!fromConnection && _behavior.HasBehavior(CommandBehavior.CloseConnection))
                {
                    _connection?.Close();
                }
            }
            catch
            {
            }
            finally
            {
                _refCursor       = null;
                _refCursors      = null;
                _row             = null;
                _metadata        = null;
                _statement       = null;
                _command         = null;
                _connection      = null;
                _recordsAffected = -1;
                _position        = STARTPOS;
            }
        }

        private object GetValue(string name)
        {
            CheckDataIsReady();
            return _row[name];
        }

        private void InitializeRefCursors()
        {
            // Ref cursors can be fetched only if there is an active transaction
            if (_command.CommandType == CommandType.StoredProcedure && _statement.HasPendingRefCursors)
            {
                if (_refCursors == null)
                {
                    _refCursors = new Queue<Statement>();
                }
                else
                {
                    _refCursors.Clear();
                }

                // Add refcusor's names to the queue
                object[] row        = null;
                var      connection = _statement.Connection;

                while ((row = _statement.FetchRow()) != null)
                {
                    _refCursors.Enqueue(connection.CreateStatement($"fetch all in \"{row[0]}\""));

                    Frontend.DataRow.ReturnBuffer(ref row);
                }

                // Grab information of the first refcursor
                NextResultFromRefCursor();
            }
        }

        private bool NextResultFromRefCursor()
        {
            _refCursor = _refCursors.Dequeue();
            _refCursor.Query();

            return true;
        }

        private void UpdateRecordsAffected()
        {
            if (_command != null && !_command.IsDisposed && _command.RecordsAffected != -1)
            {
                _recordsAffected += ((_recordsAffected == -1) ? 1 : _command.RecordsAffected);
            }
        }

        private void CheckDataIsReady()
        {
            if (IsClosed)
            {
                throw ADP.DataReaderClosed();
            }
            else if (_position == STARTPOS)
            {
                throw ADP.InvalidRead();
            }
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= FieldCount)
            {
                throw ADP.InvalidRead();
            }
        }
    }
}
