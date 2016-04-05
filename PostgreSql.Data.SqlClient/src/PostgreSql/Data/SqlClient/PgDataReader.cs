// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Frontend;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace PostgreSql.Data.SqlClient
{
    public sealed class PgDataReader
        : DbDataReader, IDbColumnSchemaGenerator
    {
        private const int STARTPOS = -1;

        private static InvalidOperationException InvalidRead()
        {
             return new InvalidOperationException("Invalid attempt to read when no data is present.");            
        }

        private bool            _open;
        private int             _position;
        private int             _recordsAffected;
        private DataRecord      _row;
        private CommandBehavior _behavior;
        private PgCommand       _command;
        private PgConnection    _connection;
        private PgStatement     _statement;
        private Queue<string>   _refCursors;

        private ReadOnlyCollection<DbColumn> _metadata;

        public override object this[int i]       => GetValue(i);
        public override object this[string name] => GetValue(GetOrdinal(name));

        public override int  Depth
        {
            get
            {
                if (IsClosed)
                {
                    throw InvalidRead();
                }
                
                return 0;
            }
        }

        public override bool IsClosed        => !_open;
        public override int  RecordsAffected => _open ? _recordsAffected : -1;
        
        public override int  FieldCount
        {
            get
            {
                if (IsClosed)
                {
                    throw InvalidRead();
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
                    throw InvalidRead();
                }
                
                return _statement.HasRows;
            }
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
                    // TODO: dispose managed state (managed objects).
                    Close();

                    base.Dispose(disposing);
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposed = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PgDataReader() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        // public void Dispose()
        // {
        //     // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //     Dispose(true);
        //     // TODO: uncomment the following line if the finalizer is overridden above.
        //     // GC.SuppressFinalize(this);
        // }
        #endregion

        public ReadOnlyCollection<DbColumn> GetColumnSchema()
        {
            if (_metadata == null)
            {
                var provider = new SchemaProvider(_connection, _statement.RowDescriptor);
                _metadata = provider.GetColumnSchema();
            }

            return _metadata;
        }

        public override bool NextResult()
        {
            if (IsClosed)
            {
                throw InvalidRead();
            }

            // Reset position
            _position = STARTPOS;

            // Close the active statement
            _statement.Close();

            // Clear current row data
            _row = null;

            // Reset records affected
            _recordsAffected = -1;

            // Reset metadata information
            _metadata = null;

            // Query for next result
            if (_refCursors.Count != 0 /*&& _connection.InnerConnection.HasActiveTransaction*/)
            {
                return NextResultFromRefCursor();
            }

            return NextResultFromMars();
        }

        public override bool Read()
        {
            if ((_behavior.HasBehavior(CommandBehavior.SingleRow) && _position != STARTPOS) || !_statement.HasRows)
            {
                return false;
            }

            _position++;

            _row = _statement.FetchRow();

            return (_row != null);
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            if (IsClosed)
            {
                throw InvalidRead();
            }

            if (IsDBNull(i))
            {
                return 0;
            }

            int bytesRead  = 0;
            int realLength = length;

            if (buffer == null)
            {
                byte[] data = (byte[])GetValue(i);

                return data.Length;
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

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            if (IsClosed)
            {
                throw InvalidRead();
            }

            if (IsDBNull(i))
            {
                return 0;
            }

            if (buffer == null)
            {
                char[] data = ((string)GetValue(i)).ToCharArray();

                return data.Length;
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

        public override Boolean        GetBoolean(int i)        => GetValue<Boolean>(i);
        public override Byte           GetByte(int i)           => GetValue<Byte>(i);
        public override Char           GetChar(int i)           => GetValue<Char>(i);
        public          DateTimeOffset GetDateTimeOffset(int i) => GetValue<DateTimeOffset>(i);
        public override Decimal        GetDecimal(int i)        => GetValue<Decimal>(i);
        public override Double         GetDouble(int i)         => GetValue<Double>(i);
        public override Single         GetFloat(int i)          => GetValue<Single>(i);
        public override Int16          GetInt16(int i)          => GetValue<Int16>(i);
        public override Int32          GetInt32(int i)          => GetValue<Int32>(i);
        public override Int64          GetInt64(int i)          => GetValue<Int64>(i);
        public          TimeSpan       GetTimeSpan(int i)       => GetValue<PgTime>(i).Value;
        public override String         GetString(int i)         => GetValue<String>(i);

        public override DateTime GetDateTime(int i)
        {
            CheckNull(i);
            
            if (_row[i] is PgDate)
            {
                return ((PgDate)_row[i]).ToDateTime();
            }
            else if (_row[i] is PgTimestamp)
            {
                return ((PgTimestamp)_row[i]).Value;
            }
            return (DateTime)_row[i];
        }

        public PgBinary    GetPgBinary(int i)    => GetValue<PgBinary>(i);
        public PgBit       GetPgBit(int i)       => GetValue<PgBit>(i);
        public PgBoolean   GetPgBoolean(int i)   => GetValue<PgBoolean>(i);
        public PgBox       GetPgBox(int i)       => GetValue<PgBox>(i);
        public PgBox2D     GetPgBox2D(int i)     => GetValue<PgBox2D>(i);
        public PgBox3D     GetPgBox3D(int i)     => GetValue<PgBox3D>(i);
        public PgByte      GetPgByte(int i)      => GetValue<PgByte>(i);
        public PgCircle    GetPgCircle(int i)    => GetValue<PgCircle>(i);
        public PgDouble    GetPgDouble(int i)    => GetValue<PgDouble>(i);
        public PgInt16     GetPgInt16(int i)     => GetValue<PgInt16>(i);
        public PgInt32     GetPgInt32(int i)     => GetValue<PgInt32>(i);
        public PgInt64     GetPgInt64(int i)     => GetValue<PgInt64>(i);
        public PgDecimal   GetPgDecimal(int i)   => GetValue<PgDecimal>(i);
        public PgMoney     GetPgMoney(int i)     => GetValue<PgMoney>(i);
        public PgReal      GetPgReal(int i)      => GetValue<PgReal>(i);
        public PgDate      GetPgDate(int i)      => GetValue<PgDate>(i);
        public PgTime      GetPgTime(int i)      => GetValue<PgTime>(i);
        public PgTimestamp GetPgTimestamp(int i) => GetValue<PgTimestamp>(i);
        public PgInterval  GetPgInterval(int i)  => GetValue<PgInterval>(i);
        public PgLine      GetPgLine(int i)      => GetValue<PgLine>(i);
        public PgLSeg      GetPgLSeg(int i)      => GetValue<PgLSeg>(i);
        public PgPath      GetPgPath(int i)      => GetValue<PgPath>(i);
        public PgPoint     GetPgPoint(int i)     => GetValue<PgPoint>(i);
        public PgPoint2D   GetPgPoint2D(int i)   => GetValue<PgPoint2D>(i);
        public PgPoint3D   GetPgPoint3D(int i)   => GetValue<PgPoint3D>(i);
        public PgPolygon   GetPgPolygon(int i)   => GetValue<PgPolygon>(i);
        public PgString    GetPgString(int i)    => GetValue<PgString>(i);

        public override Guid GetGuid(int i)
        {
            throw new NotSupportedException("Guid datatype is not supported");
        }

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

        public override object GetValue(int i)
        {
            CheckPosition();
            CheckIndex(i);

            return _row[i];
        }

        public override int GetValues(object[] values)
        {
            CheckPosition();

            return _row.GetValues(values);
        }

        public override bool IsDBNull(int i)
        {
            CheckPosition();
            CheckIndex(i);

            return (_row[i] == DBNull.Value);
        }

        public override Type   GetProviderSpecificFieldType(int i)        => GetFieldType(i);
        public override object GetProviderSpecificValue(int i)            => GetValue(i);
        public override int    GetProviderSpecificValues(object[] values) => GetValues(values);

        public override IEnumerator GetEnumerator() => new PgEnumerator(this, true);

#warning Disabled for now
        // internal PgDataRecord GetDataRecord() => new PgDataRecord(_statement.RowDescriptor, _row);

        internal void Close()
        {
            if (!_open)
            {
                return;
            }
            
            try
            {
                // This will update RecordsAffected property
                UpdateRecordsAffected();

                if (_command != null && !_command.IsDisposed)
                {
                    // Set values of output parameters
                    _command.InternalSetOutputParameters();
                }

                if (_behavior.HasBehavior(CommandBehavior.CloseConnection) && _connection != null)
                {
                    _connection.Close();
                }

                _refCursors.Clear();

                _open     = false;
                _position = STARTPOS;
            }
            catch (System.Exception)
            {
                
                throw;
            }
            finally
            {
                _command         = null;
                _statement       = null;
                _connection      = null;
                _refCursors      = null;
                _row             = null;
                _recordsAffected = -1;
                _position        = STARTPOS;
                _metadata        = null;
            }
        }

        private T GetValue<T>(int i)
        {
            CheckNull(i);

            return (T)_row[i];
        }

        private void InitializeRefCursors()
        {
            // Ref cursors can be fetched only if there is an active transaction
            if (_command.CommandType           == CommandType.StoredProcedure
             && _statement.RowDescriptor.Count == 1
             && _statement.RowDescriptor[0].TypeInfo.IsRefCursor)
            {
                // Clear refcursor's queue
                _refCursors.Clear();

                // Add refcusor's names to the queue
                DataRecord row = null;

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

        private bool NextResultFromRefCursor()
        {
            _statement.StatementText = $"fetch all in \"{_refCursors.Dequeue()}\""; 
            _statement.ExecuteReader(_command.Parameters);

            return true;
        }

        private bool NextResultFromMars()
        {
            return _command.NextResult();
        }

        private void CheckIndex(int i)
        {
            if (i < 0 || i >= FieldCount)
            {
                throw InvalidRead();
            }
        }

        private void CheckPosition()
        {
            if (IsClosed)
            {
                throw InvalidRead();
            }
            if (_position == STARTPOS)
            {
                throw InvalidRead();
            }
        }

        private void CheckNull(int i)
        {
            if (IsDBNull(i))
            {
                throw new PgNullValueException("Data is Null. This method or property cannot be called on Null values.");
            }
        }

        private void UpdateRecordsAffected()
        {
            if (_command != null && !_command.IsDisposed && _command.RecordsAffected != -1)
            {
                _recordsAffected  = ((_recordsAffected == -1) ? 0 : _recordsAffected);
                _recordsAffected += _command.RecordsAffected;
            }
        }
    }
}
