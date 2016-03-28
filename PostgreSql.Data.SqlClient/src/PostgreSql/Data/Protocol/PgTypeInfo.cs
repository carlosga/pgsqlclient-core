// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using PostgreSql.Data.SqlClient;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgTypeInfo
    {
        internal static PgTypeInfoCollection GetSupportedTypes()
        {
            var dataTypes = new PgTypeInfoCollection(60);
           
            dataTypes.Add(  16, "bool"       , PgDbType.Boolean    ,    0, PgTypeFormat.Binary, 1);
            dataTypes.Add(  17, "bytea"      , PgDbType.Bytea      ,    0, PgTypeFormat.Binary, Int32.MaxValue);
            dataTypes.Add(  18, "char"       , PgDbType.Char       ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  19, "name"       , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  20, "int8"       , PgDbType.Int8       ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add(  21, "int2"       , PgDbType.Int2       ,    0, PgTypeFormat.Binary, 2);
            dataTypes.Add(  22, "int2vector" , PgDbType.Vector     ,   21, PgTypeFormat.Binary, 2);
            dataTypes.Add(  23, "int4"       , PgDbType.Int4       ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(  24, "regproc"    , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  25, "text"       , PgDbType.Text       ,    0, PgTypeFormat.Text  , Int32.MaxValue);
            dataTypes.Add(  26, "oid"        , PgDbType.Int4       ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(  30, "oidvector"  , PgDbType.Vector     ,   26, PgTypeFormat.Binary, 4);
            dataTypes.Add( 600, "point"      , PgDbType.Point      ,  701, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 601, "lseg"       , PgDbType.LSeg       ,  600, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add( 602, "path"       , PgDbType.Path       ,    0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 603, "box"        , PgDbType.Box        ,  600, PgTypeFormat.Binary, 32, ";");
            dataTypes.Add( 604, "polygon"    , PgDbType.Polygon    ,    0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 628, "line"       , PgDbType.Line       ,  701, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add( 629, "_line"      , PgDbType.Array      ,  628, PgTypeFormat.Binary, 32);
            dataTypes.Add( 718, "circle"     , PgDbType.Circle     ,    0, PgTypeFormat.Binary, 24, ",");
            dataTypes.Add( 719, "_circle"    , PgDbType.Array      ,  718, PgTypeFormat.Binary, 24);
            dataTypes.Add( 700, "float4"     , PgDbType.Float4     ,    0, PgTypeFormat.Text  , 4);
            dataTypes.Add( 701, "float8"     , PgDbType.Float8     ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add( 705, "unknown"    , PgDbType.Text       ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add( 790, "money"      , PgDbType.Currency   ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add( 829, "macaddr"    , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 6);
            dataTypes.Add( 869, "inet"       , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1000, "_bool"      , PgDbType.Array      ,   16, PgTypeFormat.Binary, 1);
            dataTypes.Add(1002, "_char"      , PgDbType.Array      ,   18, PgTypeFormat.Binary, 0);
            dataTypes.Add(1005, "_int2"      , PgDbType.Array      ,   21, PgTypeFormat.Binary, 2);
            dataTypes.Add(1007, "_int4"      , PgDbType.Array      ,   23, PgTypeFormat.Binary, 4);
            dataTypes.Add(1009, "_text"      , PgDbType.Array      ,   25, PgTypeFormat.Binary, 0);
            dataTypes.Add(1016, "_int8"      , PgDbType.Array      ,   20, PgTypeFormat.Binary, 8);
            dataTypes.Add(1017, "_point"     , PgDbType.Array      ,  600, PgTypeFormat.Binary, 16);
            dataTypes.Add(1018, "_lseg"      , PgDbType.Array      ,  601, PgTypeFormat.Binary, 32);
            dataTypes.Add(1019, "_path"      , PgDbType.Array      ,  602, PgTypeFormat.Binary, -1);
            dataTypes.Add(1020, "_box"       , PgDbType.Array      ,  603, PgTypeFormat.Binary, 32);
            dataTypes.Add(1021, "_float4"    , PgDbType.Array      ,  700, PgTypeFormat.Binary, 4);
            dataTypes.Add(1027, "_polygon"   , PgDbType.Array      ,  604, PgTypeFormat.Binary, 16);
            dataTypes.Add(1028, "_oid"       , PgDbType.Array      ,   26, PgTypeFormat.Binary, 4);
            dataTypes.Add(1033, "aclitem"    , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 12);
            dataTypes.Add(1034, "_aclitem"   , PgDbType.Array      , 1033, PgTypeFormat.Text  , 0);
            dataTypes.Add(1042, "bpchar"     , PgDbType.Char       ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1043, "varchar"    , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1082, "date"       , PgDbType.Date       ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(1083, "time"       , PgDbType.Time       ,    0, PgTypeFormat.Text  , 8);
            dataTypes.Add(1114, "timestamp"  , PgDbType.Timestamp  ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add(1184, "timestamptz", PgDbType.TimestampTZ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add(1186, "interval"   , PgDbType.Interval   ,    0, PgTypeFormat.Binary, 12);
            dataTypes.Add(1266, "timetz"     , PgDbType.TimeTZ     ,    0, PgTypeFormat.Binary, 12);
            dataTypes.Add(1560, "bit"        , PgDbType.Byte       ,    0, PgTypeFormat.Binary, 1);
            dataTypes.Add(1562, "varbit"     , PgDbType.Byte       ,    0, PgTypeFormat.Binary, 0);
            dataTypes.Add(1700, "decimal"    , PgDbType.Decimal    ,    0, PgTypeFormat.Text  , 8);
            dataTypes.Add(1790, "refcursor"  , PgDbType.Refcursor  ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(2205, "regclass"   , PgDbType.VarChar    ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(2278, "void"       , PgDbType.Void       ,    0, PgTypeFormat.Binary, 0);
                      
            // PostGIS datatypes         
            dataTypes.Add(17321, "box3d", PgDbType.Box3D, 0, PgTypeFormat.Text, 48, ",", "BOX3D");
            dataTypes.Add(17335, "box2d", PgDbType.Box2D, 0, PgTypeFormat.Text, 16, ",", "BOX");
            // dataTypes.Add(-1    , "polygon2d"   , PgDbType.Box2D      , 0, PgTypeFormat.Text, 16, ",", "POLYGON");

            return dataTypes;
        }

        private int                   _oid;
        private readonly string       _name;
        private readonly PgDbType     _dataType;
        private readonly PgTypeFormat _format;
        private readonly Type         _systemType;
        private readonly int          _elementType;
        private readonly int          _size;
        private readonly string       _delimiter;
        private readonly string       _prefix;

        internal int Oid
        {
            get { return _oid; }
            set { _oid = value; }
        }

        internal PgDbType     DataType    => _dataType;
        internal string       Name        => _name;
        internal Type         SystemType  => _systemType;
        internal int          ElementType => _elementType; 
        internal PgTypeFormat Format      => _format;
        internal int          Size        => _size;
        internal bool         IsArray     => (_dataType == PgDbType.Array);
        internal bool         IsBinary    => (_dataType == PgDbType.Bytea);
        internal bool         IsRefCursor => (_dataType == PgDbType.Refcursor);
        internal string       Delimiter   => _delimiter;
        internal string       Prefix      => _prefix;

        internal bool IsNumeric
        {
            get
            {
                return (_dataType == PgDbType.Currency
                     || _dataType == PgDbType.Int2
                     || _dataType == PgDbType.Int4
                     || _dataType == PgDbType.Int8
                     || _dataType == PgDbType.Float4
                     || _dataType == PgDbType.Float8
                     || _dataType == PgDbType.Decimal
                     || _dataType == PgDbType.Byte);
            }
        }

        internal bool IsPrimitive
        {
            get
            {
                return (_dataType == PgDbType.Boolean
                     || _dataType == PgDbType.Byte
                     || _dataType == PgDbType.Int2
                     || _dataType == PgDbType.Int4
                     || _dataType == PgDbType.Int8
                     || _dataType == PgDbType.Char
                     || _dataType == PgDbType.Float4
                     || _dataType == PgDbType.Float8);
            }
        }

        internal PgTypeInfo(int oid, string name, PgDbType dataType, int elementType, PgTypeFormat format, int size)
            : this(oid, name, dataType, elementType, format, size, String.Empty)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , PgDbType     dataType
                          , int          elementType
                          , PgTypeFormat format
                          , int          size
                          , string       delimiter)
            : this(oid, name, dataType, elementType, format, size, delimiter, String.Empty)
        {
        }

        internal PgTypeInfo(int          oid
                          , string       name
                          , PgDbType     dataType
                          , int          elementType
                          , PgTypeFormat format
                          , int          size
                          , string       delimiter
                          , string       prefix)
        {
            _oid         = oid;
            _name        = name;
            _dataType    = dataType;
            _elementType = elementType;
            _format      = format;
            _size        = size;
            _delimiter   = delimiter;
            _prefix      = prefix;
            _systemType  = TypeHelper.GetSystemType(_dataType);
        }
        
        public override string ToString()
        {
            return _name;
        }
    }
}
