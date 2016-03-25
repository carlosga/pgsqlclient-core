// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System.Data.Common;
using PostgreSql.Data.Protocol;

namespace PostgreSql.Data.PostgreSqlClient
{
    // internal sealed class PgColumnSchema
    //     // : FieldDescriptor
    // {
    //     internal bool? AllowDBNull 
    //     {
    //         get; 
    //         set; 
    //     }
        
    //     internal string BaseCatalogName 
    //     {
    //         get; 
    //         protected set; 
    //     }
        
    //     internal string BaseColumnName 
    //     { 
    //         get; 
    //         protected set; 
    //     }
            
    //     internal string BaseSchemaName 
    //     { 
    //         get; 
    //         protected set; 
    //     }
        
    //     internal string BaseServerName 
    //     { 
    //         get; 
    //         protected set; 
    //     }
            
    //     internal string BaseTableName
    //     {
    //         get; 
    //         protected set; 
    //     }

    //     internal bool? IsAutoIncrement
    //     {
    //         get;
    //         protected set; 
    //     }

    //     internal bool? IsKey 
    //     {
    //         get; 
    //         set; 
    //     }

    //     internal bool? IsIdentity 
    //     {
    //         get;
    //         protected set; 
    //     }
                   
    //     internal bool? IsReadOnly 
    //     {
    //         get;
    //         set; 
    //     }
        
    //     internal bool? IsUnique
    //     {
    //         get;
    //         set; 
    //     }
        
    //     public PgColumnSchema()
    //     {
    //     }        
    // }
    
    public sealed class PgDbColumn
        : DbColumn
    {
        public bool IsArray
        {
            get;
            private set;
        }

        public bool IsRefCursor
        {
            get;
            private set;
        }
        
        internal PgDbColumn(PgFieldDescriptor descriptor)
        {
            ColumnName       = descriptor.Name;
            ColumnOrdinal    = descriptor.ColumnId;
            ColumnSize       = descriptor.Type.Size;
            DataType         = descriptor.Type.SystemType;
            DataTypeName     = descriptor.Type.Name;
            IsArray          = descriptor.Type.IsArray;
            IsExpression     = (descriptor.TableOid == 0 && descriptor.ColumnId == 0);
            IsLong           = descriptor.Type.IsBinary;
            IsRefCursor      = descriptor.Type.IsRefCursor; 
            NumericPrecision = descriptor.NumericPrecision;
            NumericScale     = descriptor.NumericScale;
            
            // ProviderDbType = _statement.RowDescriptor[i].Type.DataType;
        }
    }
}
