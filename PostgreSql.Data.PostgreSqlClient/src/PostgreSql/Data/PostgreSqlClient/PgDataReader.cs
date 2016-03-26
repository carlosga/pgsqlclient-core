// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Protocol;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace PostgreSql.Data.PostgreSqlClient
{
    public sealed class PgDataReader
        : DbDataReader, IDbColumnSchemaGenerator
    {
        private const int STARTPOS = -1;

        private bool            _open;
        private int             _position;
        private int             _recordsAffected;
        private PgDataRecord    _row;
        private CommandBehavior _behavior;
        private PgCommand       _command;
        private PgConnection    _connection;
        private PgStatement     _statement;
        private Queue<string>   _refCursors;
        
        private ReadOnlyCollection<DbColumn> _metadata;

        public override object this[int i]       => GetValue(i);
        public override object this[string name] => GetValue(name);

        public override int  Depth           => 0;
        public override bool IsClosed        => !_open;
        public override int  RecordsAffected => !IsClosed ? _recordsAffected : -1;
        public override int  FieldCount      => !IsClosed ? _row.FieldCount : -1;
                
        public override bool HasRows
        {
            get
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("Invalid attempt to read when no data is present.");
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
                        _position        = STARTPOS;
                        _metadata        = null;
                    }
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
                _metadata = BuildQuerySchema();
            }
            
            return _metadata;
        }

        public override bool NextResult()
        {
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
                
        public override bool GetBoolean(int i) 
        {
            CheckPosition();
            
            return _row.GetBoolean(i);
        }
        
        public override byte GetByte(int i)
        {
            CheckPosition();
            
            return _row.GetByte(i);            
        }

        public override long GetBytes(int i, long dataIndex, byte[] buffer, int bufferIndex, int length)
        {
            CheckPosition();
            
            return _row.GetBytes(i, dataIndex, buffer, bufferIndex, length);
        }

        public override char GetChar(int i)
        {
            CheckPosition();
            
            return _row.GetChar(i);
        }

        public override long GetChars(int i, long dataIndex, char[] buffer, int bufferIndex, int length)
        {
            CheckPosition();
            
            return _row.GetChars(i, dataIndex, buffer, bufferIndex, length);
        }

        public override string GetDataTypeName(int i)
        {
            return _row.GetDataTypeName(i);
        }

        public override DateTime GetDateTime(int i)
        {
            CheckPosition();
            
            return _row.GetDateTime(i);            
        } 
        
        public override Decimal GetDecimal(int i)
        {
            CheckPosition();
         
            return _row.GetDecimal(i);   
        }
        
        public override double GetDouble(int i)
        {
            CheckPosition();
         
            return _row.GetDouble(i);   
        }

        public override Type GetFieldType(int i)
        {
            return _row.GetFieldType(i);
        }

        public override float GetFloat(int i)
        {
            CheckPosition();
            
            return _row.GetFloat(i);   
        }
        
        public override Guid GetGuid(int i)
        {
            CheckPosition();
            
            return _row.GetGuid(i);
        }

        public override Int16 GetInt16(int i)
        {
            CheckPosition();
            
            return _row.GetInt16(i);
        }
        
        public override Int32 GetInt32(int i)
        {
            CheckPosition();
            
            return _row.GetInt32(i);            
        }
        
        public override Int64 GetInt64(int i)
        {
            CheckPosition();
            
            return _row.GetInt64(i);            
        }

        public override String GetName(int i)
        {
            return _row.GetName(i);
        }

        public override int GetOrdinal(string name)
        {
            if (IsClosed)
            {
                throw new InvalidOperationException("Reader closed");
            }

            return _row.GetOrdinal(name);
        }

        public override string GetString(int i)
        {
            CheckPosition();
            
            return _row.GetString(i); 
        }

        public override object GetValue(int i)
        {
            CheckPosition();

            return _row.GetValue(i);
        }

        public object GetValue(string name)
        {
            CheckPosition();

            return _row.GetValue(name);
        }

        public override int GetValues(object[] values)
        {
            CheckPosition();

            return _row.GetValues(values);
        }

        public override bool IsDBNull(int i)
        {
            CheckPosition();

            return _row.IsDBNull(i);
        }

        public TimeSpan GetTimeSpan(int i)
        {
            CheckPosition();
            
            return _row.GetPgTimeSpan(i).Value;
        }
        
        public PgTimeSpan GetPgTimeSpan(int i)
        {
            CheckPosition();
         
            return _row.GetPgTimeSpan(i);
        }
        
        public PgPoint GetPgPoint(int i)
        {
            CheckPosition();
         
            return _row.GetPgPoint(i);
        }
        
        public PgBox GetPgBox(int i)
        {
            CheckPosition();
         
            return _row.GetPgBox(i);
        }
        
        public PgLSeg GetPgLSeg(int i)
        {
            CheckPosition();
         
            return _row.GetPgLSeg(i);
        }
        
        public PgCircle GetPgCircle(int i)
        {
            CheckPosition();
         
            return _row.GetPgCircle(i);
        }
        
        public PgPath GetPgPath(int i)
        {
            CheckPosition();
         
            return _row.GetPgPath(i);
        }
        
        public PgPolygon GetPgPolygon(int i)
        {
            CheckPosition();
         
            return _row.GetPgPolygon(i);
        }
                
        public PgBox2D GetPgBox2D(int i)
        {
            CheckPosition();
         
            return _row.GetPgBox2D(i);
        }
        
        public override Type GetProviderSpecificFieldType(int i)
        {
            return _row.GetFieldType(i);
        }
        
        public override object GetProviderSpecificValue(int i)            => GetValue(i);
        public override int    GetProviderSpecificValues(object[] values) => GetValues(values);

        public override IEnumerator GetEnumerator() => new PgEnumerator(this, true);

        internal PgDataRecord GetDataRecord() => _row;

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
                // Set values of output parameters
                _command.InternalSetOutputParameters();
                _command.ActiveDataReader = null;
            }

            if (_behavior.HasBehavior(CommandBehavior.CloseConnection) && _connection != null)
            {
                _connection.Close();
            }

            _refCursors.Clear();

            _open     = false;
            _position = STARTPOS;
        }

        private void InitializeRefCursors()
        {
            // Ref cursors can be fetched only if there is an active transaction
            if (_command.CommandType           == CommandType.StoredProcedure
             && _statement.RowDescriptor.Count == 1
             && _statement.RowDescriptor[0].Type.IsRefCursor)
            {
                // Clear refcursor's queue
                _refCursors.Clear();

                // Add refcusor's names to the queue
                PgDataRecord row = null;

                while (_statement.HasRows)
                {
                    row = _statement.FetchRow();   

                    if (row != null)
                    {
                        _refCursors.Enqueue(row.GetString(0));
                    }
                }

                // Grab information of the first refcursor
                NextResult();
            }
        }
        
        private bool NextResultFromRefCursor()
        {
            _statement.StatementText = $"fetch all in \"{_refCursors.Dequeue()}\""; 
            _statement.ExecuteReader();
                
            return true;
        }
        
        private bool NextResultFromMars()
        {
            return _command.NextResult();
        }

        private void CheckPosition()
        {
            if (_position == STARTPOS)
            {
                throw new InvalidOperationException("Invalid attempt to read when no data is present.");
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
                
        private ReadOnlyCollection<DbColumn> BuildQuerySchema()
        {
            var rowDescriptor = _statement.RowDescriptor;            
            var columns       = new DbColumn[rowDescriptor.Count];
            
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new PgDbColumn(rowDescriptor[i]);
            }

            // if (this.schemaTable == null)
            // {
            //     int    tableCount   = 0;
            //     string currentTable = String.Empty;

            //     this.schemaTable = this.GetSchemaTableStructure();

            //     this.schemaTable.BeginLoadData();

            //     PgCommand columnsCmd = new PgCommand(this.GetColumnsSql(), this.connection);
                
            //     columnsCmd.Parameters.Add("@OidNumber", PgDbType.Int4);
            //     columnsCmd.Parameters.Add("@OidTable", PgDbType.Int4);
                                
            //     PgCommand primaryKeyCmd	= new PgCommand(this.GetPrimaryKeysSql(), this.connection);

            //     primaryKeyCmd.Parameters.Add("@OidTable", PgDbType.Int4);
                
            //     for (int i = 0; i < this.statement.RowDescriptor.Fields.Length; i++)
            //     {
            //         object[] columnInfo	= null;
            //         Array	 pKeyInfo	= null;

            //         // Execute commands
            //         columnsCmd.Parameters[0].Value = this.statement.RowDescriptor.Fields[i].OidNumber;
            //         columnsCmd.Parameters[1].Value = this.statement.RowDescriptor.Fields[i].OidTable;

            //         primaryKeyCmd.Parameters[0].Value = this.statement.RowDescriptor.Fields[i].OidTable;

            //         columnsCmd.InternalPrepare(); // First time it will prepare the command, next times it will close the open portal
            //         columnsCmd.InternalExecute();

            //         primaryKeyCmd.InternalPrepare(); // First time it will prepare the command, next times it will close the open portal
            //         primaryKeyCmd.InternalExecute();

            //         // Get Column Information
            //         if (columnsCmd.Statement.Rows != null && columnsCmd.Statement.Rows.Length > 0)
            //         {
            //             columnInfo = (object[])columnsCmd.Statement.Rows[0];
            //         }

            //         // Get Primary Key Info
            //         if (primaryKeyCmd.Statement.Rows != null && primaryKeyCmd.Statement.Rows.Length > 0)
            //         {
            //             object[] temp = (object[])primaryKeyCmd.Statement.Rows[0];
            //             pKeyInfo = (Array)temp[0];
            //         }

            //         // Add row information
            //         DataRow schemaRow = this.schemaTable.NewRow();

            //         schemaRow["ColumnName"]	   = this.GetName(i);
            //         schemaRow["ColumnOrdinal"] = (i + 1);
            //         schemaRow["ColumnSize"]    = this.GetSize(i);

            //         if (this.IsNumeric(i))
            //         {
            //             schemaRow["NumericPrecision"] = this.GetNumericPrecision(i);
            //             schemaRow["NumericScale"]	  = this.GetNumericScale(i);
            //         }
            //         else
            //         {
            //             schemaRow["NumericPrecision"] = DBNull.Value;
            //             schemaRow["NumericScale"]	  = DBNull.Value;
            //         }

            //         schemaRow["DataType"]		 = this.GetFieldType(i);
            //         schemaRow["ProviderType"]    = this.GetProviderDbType(i);
            //         schemaRow["IsLong"]			 = this.IsLong(i);
            //         schemaRow["IsRowVersion"]	 = this.GetName(i).CaseInsensitiveCompare("oid");
            //         schemaRow["IsUnique"]		 = false;
            //         schemaRow["IsAliased"]		 = this.IsAliased(i);
            //         schemaRow["IsExpression"]    = this.IsExpression(i);
            //         schemaRow["BaseCatalogName"] = System.DBNull.Value;

            //         if (columnInfo != null)
            //         {
            //             schemaRow["BaseSchemaName"]	 = columnInfo[0].ToString();
            //             schemaRow["BaseTableName"]	 = columnInfo[1].ToString();
            //             schemaRow["BaseColumnName"]	 = columnInfo[2].ToString();
            //             schemaRow["IsReadOnly"]		 = (bool)columnInfo[7];
            //             schemaRow["IsAutoIncrement"] = (bool)columnInfo[7];
            //             schemaRow["IsKey"]           = this.IsPrimaryKey(pKeyInfo, Convert.ToInt32(columnInfo[5]));
            //             schemaRow["AllowDBNull"]	= ((bool)columnInfo[6]) ? false : true;
            //         }
            //         else
            //         {
            //             schemaRow["IsReadOnly"]		 = false;
            //             schemaRow["IsAutoIncrement"] = false;
            //             schemaRow["IsKey"]			 = false;
            //             schemaRow["AllowDBNull"]	 = System.DBNull.Value;						
            //             schemaRow["BaseSchemaName"]	 = System.DBNull.Value;
            //             schemaRow["BaseTableName"]	 = System.DBNull.Value;
            //             schemaRow["BaseColumnName"]	 = System.DBNull.Value;
            //         }

            //         if (!String.IsNullOrEmpty(schemaRow["BaseSchemaName"].ToString()) 
            //          && schemaRow["BaseSchemaName"].ToString() != currentTable)
            //         {
            //             tableCount++;
            //             currentTable = schemaRow["BaseSchemaName"].ToString();
            //         }

            //         this.schemaTable.Rows.Add(schemaRow);
            //     }

            //     if (tableCount > 1)
            //     {
            //         foreach (DataRow row in this.schemaTable.Rows)
            //         {
            //             row["IsKey"]    = false;
            //             row["IsUnique"] = false;
            //         }
            //     }

            //     this.schemaTable.EndLoadData();

            //     columnsCmd.Dispose();
            //     primaryKeyCmd.Dispose();
            // }
            
            return new ReadOnlyCollection<DbColumn>(columns);
        }
        
        private string GetColumnsSql()
        {
            return	
                "SELECT " +
                    "pg_namespace.nspname AS TABLE_SCHEMA, " +
                    "pg_class.relname AS TABLE_NAME, " +
                    "pg_attribute.attname AS COLUMN_NAME, " +
                    "pg_attribute.atttypid AS DATA_TYPE, " +
                    "pg_attribute.attlen AS COLUMN_SIZE, " +
                    "pg_attribute.attnum AS ORDINAL_POSITION, " +
                    "pg_attribute.attnotnull AS IS_NOT_NULL, " +
                    "(pg_depend.objid is not null) AS IS_AUTOINCREMENT " +
                "FROM pg_attribute " +
                    "left join pg_class ON pg_attribute.attrelid = pg_class.oid " + 
                    "left join pg_namespace ON pg_class.relnamespace = pg_namespace.oid " + 
                    "left join pg_attrdef ON (pg_class.oid = pg_attrdef.adrelid AND pg_attribute.attnum = pg_attrdef.adnum) " +
                    "left join pg_depend ON (pg_attribute.attrelid = pg_depend.refobjid AND pg_attribute.attnum = pg_depend.refobjsubid  AND pg_depend.deptype = 'i') " +
                "WHERE " +
                    "pg_attribute.attisdropped = false AND " +
                    "pg_attribute.attnum > 0 AND " +
                    "pg_attribute.attnum = @OidNumber AND " +
                    "pg_attribute.attrelid = @OidTable";
        }        
    }
}
