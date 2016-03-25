// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Data;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgType
    {
        internal static PgTypeCollection GetSupportedTypes()
        {
            var dataTypes = new PgTypeCollection(60);
           
            dataTypes.Add(  16, "bool"       , PgDataType.Boolean        ,    0, PgTypeFormat.Binary, 1);
            dataTypes.Add(  17, "bytea"      , PgDataType.Binary         ,    0, PgTypeFormat.Binary, Int32.MaxValue);
            dataTypes.Add(  18, "char"       , PgDataType.Char           ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  19, "name"       , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  20, "int8"       , PgDataType.Int8           ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add(  21, "int2"       , PgDataType.Int2           ,    0, PgTypeFormat.Binary, 2);
            dataTypes.Add(  22, "int2vector" , PgDataType.Vector         ,   21, PgTypeFormat.Binary, 2);
            dataTypes.Add(  23, "int4"       , PgDataType.Int4           ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(  24, "regproc"    , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(  25, "text"       , PgDataType.Text           ,    0, PgTypeFormat.Text  , Int32.MaxValue);
            dataTypes.Add(  26, "oid"        , PgDataType.Int4           ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(  30, "oidvector"  , PgDataType.Vector         ,   26, PgTypeFormat.Binary, 4);
            dataTypes.Add( 600, "point"      , PgDataType.Point          ,  701, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 601, "lseg"       , PgDataType.LSeg           ,  600, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add( 602, "path"       , PgDataType.Path           ,    0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 603, "box"        , PgDataType.Box            ,  600, PgTypeFormat.Binary, 32, ";");
            dataTypes.Add( 604, "polygon"    , PgDataType.Polygon        ,    0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add( 628, "line"       , PgDataType.Line           ,  701, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add( 629, "_line"      , PgDataType.Array          ,  628, PgTypeFormat.Binary, 32);
            dataTypes.Add( 718, "circle"     , PgDataType.Circle         ,    0, PgTypeFormat.Binary, 24, ",");
            dataTypes.Add( 719, "_circle"    , PgDataType.Array          ,  718, PgTypeFormat.Binary, 24);
            dataTypes.Add( 700, "float4"     , PgDataType.Float          ,    0, PgTypeFormat.Text  , 4);
            dataTypes.Add( 701, "float8"     , PgDataType.Double         ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add( 705, "unknown"    , PgDataType.Text           ,    0, PgTypeFormat.Binary, 0);
            dataTypes.Add( 790, "money"      , PgDataType.Currency       ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add( 829, "macaddr"    , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 6);
            dataTypes.Add( 869, "inet"       , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1000, "_bool"      , PgDataType.Array          ,   16, PgTypeFormat.Binary, 1);
            dataTypes.Add(1002, "_char"      , PgDataType.Array          ,   18, PgTypeFormat.Binary, 0);
            dataTypes.Add(1005, "_int2"      , PgDataType.Array          ,   21, PgTypeFormat.Binary, 2);
            dataTypes.Add(1007, "_int4"      , PgDataType.Array          ,   23, PgTypeFormat.Binary, 4);
            dataTypes.Add(1009, "_text"      , PgDataType.Array          ,   25, PgTypeFormat.Binary, 0);
            dataTypes.Add(1016, "_int8"      , PgDataType.Array          ,   20, PgTypeFormat.Binary, 8);
            dataTypes.Add(1017, "_point"     , PgDataType.Array          ,  600, PgTypeFormat.Binary, 16);
            dataTypes.Add(1018, "_lseg"      , PgDataType.Array          ,  601, PgTypeFormat.Binary, 32);
            dataTypes.Add(1019, "_path"      , PgDataType.Array          ,  602, PgTypeFormat.Binary, -1);
            dataTypes.Add(1020, "_box"       , PgDataType.Array          ,  603, PgTypeFormat.Binary, 32);
            dataTypes.Add(1021, "_float4"    , PgDataType.Array          ,  700, PgTypeFormat.Binary, 4);
            dataTypes.Add(1027, "_polygon"   , PgDataType.Array          ,  604, PgTypeFormat.Binary, 16);
            dataTypes.Add(1028, "_oid"       , PgDataType.Array          ,   26, PgTypeFormat.Binary, 4);
            dataTypes.Add(1033, "aclitem"    , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 12);
            dataTypes.Add(1034, "_aclitem"   , PgDataType.Array          , 1033, PgTypeFormat.Text  , 0);
            dataTypes.Add(1042, "bpchar"     , PgDataType.Char           ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1043, "varchar"    , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(1082, "date"       , PgDataType.Date           ,    0, PgTypeFormat.Binary, 4);
            dataTypes.Add(1083, "time"       , PgDataType.Time           ,    0, PgTypeFormat.Text  , 8);
            dataTypes.Add(1114, "timestamp"  , PgDataType.Timestamp      ,    0, PgTypeFormat.Text  , 8);
            dataTypes.Add(1184, "timestamptz", PgDataType.TimestampWithTZ,    0, PgTypeFormat.Binary, 8);
            dataTypes.Add(1186, "interval"   , PgDataType.Interval       ,    0, PgTypeFormat.Binary, 12);
            dataTypes.Add(1266, "timetz"     , PgDataType.TimeWithTZ     ,    0, PgTypeFormat.Binary, 12);
            dataTypes.Add(1560, "bit"        , PgDataType.Byte           ,    0, PgTypeFormat.Text  , 1);
            dataTypes.Add(1562, "varbit"     , PgDataType.Byte           ,    0, PgTypeFormat.Binary, 0);
            dataTypes.Add(1700, "numeric"    , PgDataType.Decimal        ,    0, PgTypeFormat.Text  , 8);
            dataTypes.Add(1790, "refcursor"  , PgDataType.Refcursor      ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(2205, "regclass"   , PgDataType.VarChar        ,    0, PgTypeFormat.Text  , 0);
            dataTypes.Add(2278, "void"       , PgDataType.Void           ,    0, PgTypeFormat.Binary, 0);
                      
            // PostGIS datatypes         
            dataTypes.Add(17321, "box3d", PgDataType.Box3D, 0, PgTypeFormat.Text, 48, ",", "BOX3D");
            dataTypes.Add(17335, "box2d", PgDataType.Box2D, 0, PgTypeFormat.Text, 16, ",", "BOX");
            // dataTypes.Add(-1    , "polygon2d"   , PgDataType.Box2D      , 0, PgTypeFormat.Text, 16, ",", "POLYGON");

            return dataTypes;
        }

        private int                   _oid;
        private readonly string       _name;
        private readonly PgDataType   _dataType;
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

        internal PgDataType   DataType    => _dataType;
        internal string       Name        => _name;
        internal Type         SystemType  => _systemType;
        internal int          ElementType => _elementType; 
        internal PgTypeFormat Format      => _format;
        internal int          Size        => _size;
        internal bool         IsArray     => (_dataType == PgDataType.Array);
        internal bool         IsBinary    => (_dataType == PgDataType.Binary);
        internal bool         IsRefCursor => (_dataType == PgDataType.Refcursor);
        internal string       Delimiter   => _delimiter;
        internal string       Prefix      => _prefix;

        internal bool IsNumeric
        {
            get
            {
                return (_dataType == PgDataType.Currency
                     || _dataType == PgDataType.Int2
                     || _dataType == PgDataType.Int4
                     || _dataType == PgDataType.Int8
                     || _dataType == PgDataType.Float
                     || _dataType == PgDataType.Double
                     || _dataType == PgDataType.Decimal
                     || _dataType == PgDataType.Byte);
            }
        }

        internal bool IsPrimitive
        {
            get
            {
                return (_dataType == PgDataType.Boolean
                     || _dataType == PgDataType.Byte
                     || _dataType == PgDataType.Int2
                     || _dataType == PgDataType.Int4
                     || _dataType == PgDataType.Int8
                     || _dataType == PgDataType.Char
                     || _dataType == PgDataType.Double
                     || _dataType == PgDataType.Float);
            }
        }

        internal PgType(int oid, string name, PgDataType dataType, int elementType, PgTypeFormat format, int size)
            : this(oid, name, dataType, elementType, format, size, String.Empty)
        {
        }

        internal PgType(int          oid
                      , string       name
                      , PgDataType   dataType
                      , int          elementType
                      , PgTypeFormat format
                      , int          size
                      , string       delimiter)
            : this(oid, name, dataType, elementType, format, size, delimiter, String.Empty)
        {
        }

        internal PgType(int          oid
                      , string       name
                      , PgDataType   dataType
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
    }
}
