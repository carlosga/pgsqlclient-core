// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgDataReader
        : DbDataReader
    {
        private const int STARTPOS = -1;

        private bool            _disposed;
        private bool            _open;
        private int             _position;
        private int             _recordsAffected;
        private object[]        _row;
        private CommandBehavior _behavior;
        private PgCommand       _command;
        private PgConnection    _connection;
        private PgStatement     _statement;
        private Queue<string>   _refCursors;

        public override object this[int i]
        {
            get { return GetValue(i); }
        }

        public override object this[string name]
        {
            get { return GetValue(GetOrdinal(name)); }
        }

        internal PgDataReader(PgConnection connection, PgCommand command)
        {
            _open            = true;
            _recordsAffected = -1;
            _position        = STARTPOS;
            _refCursors      = new Queue<string>();
            _connection      = connection;
            _command         = command;
            _behavior        = _command.CommandBehavior;
            _statement       = _command.Statement;

            Initialize();
        }

        ~PgDataReader()
        {
            Dispose(false);
        }

        public override int Depth
        {
            get { return 0; }
        }

        public override bool IsClosed
        {
            get { return !_open; }
        }

        public override int RecordsAffected
        {
            get { return IsClosed ? _recordsAffected : -1; }
        }

        public override bool HasRows
        {
            get { return _command.Statement.HasRows; }
        }

        public override bool NextResult()
        {
            if (_refCursors.Count != 0 && _connection.InnerConnection.HasActiveTransaction)
            {
                return false;
            }
            
            string sql = $"fetch all in \"{_refCursors.Dequeue()}\"";

            // Reset position
            _position = STARTPOS;

            // Close the active statement
            _statement.Close();

            // Create a new statement to fetch the current refcursor
            string statementName = Guid.NewGuid().ToString();

            _statement = _connection.InnerConnection.CreateStatement($"PS{statementName}", $"PR{statementName}", sql);

            _statement.Parse();
            _statement.Describe();
            _statement.Bind();
            _statement.Execute();
                
            return true;
        }

        public override bool Read()
        {
            if ((CheckCommandBehavior(CommandBehavior.SingleRow) && _position != STARTPOS) || !_command.Statement.HasRows)
            {
                return false;
            }

            try
            {
                _position++;

                _row = _statement.FetchRow();   
                
                return (_row != null);
            }
            catch (PgClientException ex)
            {
                throw new PgException(ex);
            }
        }

        public override int FieldCount
        {
            get { return _statement?.RowDescriptor.Count ?? -1; }
        }

        public override bool GetBoolean(int i)
        {
            return Convert.ToBoolean(GetValue(i));
        }

        public override byte GetByte(int i)
        {
            return Convert.ToByte(GetValue(i));
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            int bytesRead  = 0;
            int realLength = length;

            if (buffer == null)
            {
                if (IsDBNull(i))
                {
                    return 0;
                }
                else
                {
                    byte[] data = (byte[])GetValue(i);

                    return data.Length;
                }
            }

            byte[] byteArray = (byte[])GetValue(i);

            if (length > (byteArray.Length - dataIndex))
            {
                realLength = byteArray.Length - (int)dataIndex;
            }

            Array.Copy(byteArray, (int)dataIndex, buffer, bufferIndex, realLength);

            if ((byteArray.Length - dataIndex) < length)
            {
                bytesRead = byteArray.Length - (int)dataIndex;
            }
            else
            {
                bytesRead = length;
            }

            return bytesRead;
        }

        public override char GetChar(int i)
        {
            return Convert.ToChar(GetValue(i));
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            CheckPosition();
            CheckIndex(i);

            if (buffer == null)
            {
                if (IsDBNull(i))
                {
                    return 0;
                }
                else
                {
                    char[] data = ((string)GetValue(i)).ToCharArray();

                    return data.Length;
                }
            }

            int charsRead = 0;
            int realLength = length;

            char[] charArray = ((string)GetValue(i)).ToCharArray();

            if (length > (charArray.Length - dataIndex))
            {
                realLength = charArray.Length - (int)dataIndex;
            }

            Array.Copy(charArray, (int)dataIndex, buffer, bufferIndex, realLength);

            if ((charArray.Length - dataIndex) < length)
            {
                charsRead = charArray.Length - (int)dataIndex;
            }
            else
            {
                charsRead = length;
            }

            return charsRead;
        }

        public override string GetDataTypeName(int i)
        {
            CheckIndex(i);

            return _statement.RowDescriptor[i].Type.Name;
        }

        public override DateTime GetDateTime(int i)
        {
            return Convert.ToDateTime(GetValue(i));
        }

        public override Decimal GetDecimal(int i)
        {
            return Convert.ToDecimal(GetValue(i));
        }

        public override double GetDouble(int i)
        {
            return Convert.ToDouble(GetValue(i));
        }

        public override Type GetFieldType(int i)
        {
            CheckIndex(i);

            return _statement.RowDescriptor[i].Type.SystemType;
        }

        public override float GetFloat(int i)
        {
            return Convert.ToSingle(GetValue(i));
        }

        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException("Guid datatype is not supported");
        }

        public override Int16 GetInt16(int i)
        {
            return Convert.ToInt16(GetValue(i));
        }

        public override Int32 GetInt32(int i)
        {
            return Convert.ToInt32(GetValue(i));
        }

        public override Int64 GetInt64(int i)
        {
            return Convert.ToInt64(GetValue(i));
        }

        public override String GetName(int i)
        {
            CheckIndex(i);

            return _statement.RowDescriptor[i].Name;
        }

        public override int GetOrdinal(string name)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("Reader closed");
            }

            return _statement.RowDescriptor.IndexOf(name);
        }

        public override string GetString(int i)
        {
            return Convert.ToString(GetValue(i));
        }

        public override object GetValue(int i)
        {
            CheckPosition();
            CheckIndex(i);

            return _row[i];
        }

        public override int GetValues(object[] values)
        {
            CheckPosition();

            Array.Copy(_row, values, FieldCount);

            return values.Length;
        }

        public override bool IsDBNull(int i)
        {
            CheckPosition();
            CheckIndex(i);

            return (_row[i] == DBNull.Value);
        }

        public TimeSpan GetTimeSpan(int i)
        {
            return (TimeSpan)GetValue(i);
        }

        public PgTimeSpan GetPgTimeSpan(int i)
        {
            return (PgTimeSpan)GetProviderSpecificValue(i);
        }

        public PgPoint GetPgPoint(int i)
        {
            return GetProviderSpecificValue<PgPoint>(i);
        }

        public PgBox GetPgBox(int i)
        {
            return GetProviderSpecificValue<PgBox>(i);
        }

        public PgLSeg GetPgLSeg(int i)
        {
            return GetProviderSpecificValue<PgLSeg>(i);
        }

        public PgCircle GetPgCircle(int i)
        {
            return GetProviderSpecificValue<PgCircle>(i);
        }

        public PgPath GetPgPath(int i)
        {
            return GetProviderSpecificValue<PgPath>(i);
        }

        public PgPolygon GetPgPolygon(int i)
        {
            return GetProviderSpecificValue<PgPolygon>(i);
        }

        public PgBox2D GetPgBox2D(int i)
        {
            return GetProviderSpecificValue<PgBox2D>(i);
        }

        public override Type GetProviderSpecificFieldType(int i)
        {
            return GetFieldType(i);
        }

        public override object GetProviderSpecificValue(int i)
        {
            switch (GetProviderDbType(i))
            {
                case PgDbType.Interval:
                    return new PgTimeSpan(GetTimeSpan(i));

                default:
                    return GetValue(i);
            }
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return GetValues(values);
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return new PgEnumerator(this, true);
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects).

                    try
                    {
                        Close();
                    }
                    finally
                    {
                        _command         = null;
                        _statement       = null;
                        _connection      = null;
                        _refCursors      = null;
                        _row             = null;
                        _recordsAffected = -1;
                        _position        = -1;
                    }
                }

                // release any unmanaged resources

                _disposed = true;
            }
        }

        internal PgDataRecord GetDataRecord()
        {
            return new PgDataRecord(_statement.RowDescriptor, _row);
        }

        internal void Close()
        {
            if (!_open)
            {
                return;
            }

            // This will update RecordsAffected property
            UpdateRecordsAffected();

            if (_command != null && !_command.IsDisposed)
            {
                if (_command.Statement != null)
                {
                    // Set values of output parameters
                    _command.InternalSetOutputParameters();
                }

                _command.ActiveDataReader = null;
            }

            if (_connection != null && CheckCommandBehavior(CommandBehavior.CloseConnection))
            {
                _connection.Close();
            }

            _refCursors.Clear();

            _open     = false;
            _position = STARTPOS;
        }

        private T GetProviderSpecificValue<T>(int i)
        {
            CheckPosition();
            CheckIndex(i);

            return (T)_row[i];
        }

        private void Initialize()
        {
            if (!_connection.InnerConnection.HasActiveTransaction)
            {
                return;
            }

            // Ref cursors can be fetched only if there is an active transaction
            if (_command.CommandType                   == CommandType.StoredProcedure
             && _command.Statement.RowDescriptor.Count == 1
             && _command.Statement.RowDescriptor[0].Type.IsRefCursor)
            {
                // Clear refcursor's queue
                _refCursors.Clear();

                // Add refcusor's names to the queue
                object[] row = null;

                while (_statement.HasRows)
                {
                    row = _statement.FetchRow();   

                    if (row != null)
                    {
                        _refCursors.Enqueue((string)row[0]);
                    }
                }

                // Grab information of the first refcursor
                NextResult();
            }
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= FieldCount)
            {
                throw new IndexOutOfRangeException("Could not find specified column in results.");
            }
        }

        private void CheckPosition()
        {
            if (_position == STARTPOS)
            {
                throw new InvalidOperationException("There are no data to read.");
            }
        }

        private int GetSize(int i)
        {
            return _command.Statement.RowDescriptor[i].Type.Size;
        }

        private PgDbType GetProviderDbType(int i)
        {
            return (PgDbType)_command.Statement.RowDescriptor[i].Type.DataType;
        }

        private bool IsNumeric(int i)
        {
            CheckIndex(i);

            return _command.Statement.RowDescriptor[i].Type.IsNumeric;
        }

        private bool IsBinary(int i)
        {
            CheckIndex(i);

            return _command.Statement.RowDescriptor[i].Type.IsBinary;
        }

        private bool IsAliased(int i)
        {
#warning "TODO: Implement"
            return false;
        }

        private bool IsExpression(int i)
        {
            CheckIndex(i);

            return (_command.Statement.RowDescriptor[i].TableOid == 0
                  & _command.Statement.RowDescriptor[i].ColumnId == 0);
        }

        private void UpdateRecordsAffected()
        {
            if (_command != null && !_command.IsDisposed && _command.RecordsAffected != -1)
            {
                _recordsAffected  = ((_recordsAffected == -1) ? 0 : _recordsAffected);
                _recordsAffected += _command.RecordsAffected;
            }
        }

        private bool CheckCommandBehavior(CommandBehavior behavior)
        {
            return ((behavior & _behavior) == behavior);
        }
    }
}