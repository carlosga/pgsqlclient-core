// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Data;
using PostgreSql.Data.SqlClient;

namespace PostgreSql.Data.Protocol
{
    internal sealed class PgTypeInfo
    {
        internal static readonly string          NullString       = "Null";
        internal static readonly IFormatProvider InvariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;

        internal static bool IsNullString(string s) => (s == null || s == NullString);

        internal static PgTypeInfoCollection GetSupportedTypes()
        {
            var dataTypes = new PgTypeInfoCollection(60);

            //
            // NUMERIC TYPES
            // 

            // smallint	| 2 bytes | small-range integer | -32768 to +32767
            
            dataTypes.Add(  21, "int2"      , PgDbType.Int16 ,  0, PgTypeFormat.Binary, 2);
            dataTypes.Add(  22, "int2vector", PgDbType.Vector, 21, PgTypeFormat.Binary, 2);
            dataTypes.Add(1005, "_int2"     , PgDbType.Array , 21, PgTypeFormat.Binary, 2);
            
            // integer | 4 bytes | typical choice for integer | -2147483648 to +2147483647
            
            dataTypes.Add(  23, "int4" , PgDbType.Int32,  0, PgTypeFormat.Binary, 4);
            dataTypes.Add(1007, "_int4", PgDbType.Array, 23, PgTypeFormat.Binary, 4);

            // bigint | 8 bytes | large-range integer | -9223372036854775808 to +9223372036854775807
            
            dataTypes.Add(  20, "int8" , PgDbType.Int64,  0, PgTypeFormat.Binary, 8);
            dataTypes.Add(1016, "_int8", PgDbType.Array, 20, PgTypeFormat.Binary, 8);

            // decimal | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
            // numeric | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point

            dataTypes.Add(1700,  "numeric", PgDbType.Numeric, 0   , PgTypeFormat.Text,  8);
            dataTypes.Add(1231, "_numeric", PgDbType.Array  , 1700, PgTypeFormat.Text, -1);

            // real | 4 bytes | variable-precision, inexact	6 decimal digits precision
            
            dataTypes.Add( 700, "float4" , PgDbType.Real ,   0, PgTypeFormat.Text  , 4);
            dataTypes.Add(1021, "_float4", PgDbType.Array, 700, PgTypeFormat.Binary, 4);
            
            // double precision | 8 bytes | variable-precision, inexact	15 decimal digits precision
            
            dataTypes.Add(701, "float8", PgDbType.Double, 0, PgTypeFormat.Binary, 8);

            // smallserial | 2 bytes | small autoincrementing integer | 1 to 32767
            
            // dataTypes.Add(--, "smallserial", PgDbType.SmallSerial, 0, PgTypeFormat.Binary, 2);
            
            // serial | 4 bytes	| autoincrementing integer | 1 to 2147483647
            
            // dataTypes.Add(--, "serial", PgDbType.Serial, 0, PgTypeFormat.Binary, 4);
            
            // bigserial | 8 bytes | large autoincrementing integer | 1 to 9223372036854775807
            
            // dataTypes.Add(  --, "bigserial", PgDbType.BigSerial, 0, PgTypeFormat.Binary, 8);
            
            //
            // MONETARY TYPES
            //
            
            // money | 8 bytes | currency amount | -92233720368547758.08 to +92233720368547758.07
            
            dataTypes.Add(790, "money" , PgDbType.Money,   0, PgTypeFormat.Binary, 8);
            dataTypes.Add(791, "_money", PgDbType.Array, 790, PgTypeFormat.Binary, -1);
            
            //
            // CHARACTER TYPES
            //

            // character varying(n), varchar(n)	variable-length with limit
            
            dataTypes.Add(1043, "varchar" , PgDbType.VarChar,    0, PgTypeFormat.Text, 0);
            dataTypes.Add(1015, "_varchar", PgDbType.Array  , 1043, PgTypeFormat.Text, -1);
            
            // character(n), char(n)	fixed-length, blank padded
            
            dataTypes.Add(  18, "char" , PgDbType.Char ,  0, PgTypeFormat.Text, 0);
            dataTypes.Add(1002, "_char", PgDbType.Array, 18, PgTypeFormat.Text, -1);
            
            // text	variable unlimited length
                        
            dataTypes.Add(  25, "text" , PgDbType.Text ,  0, PgTypeFormat.Text, Int32.MaxValue);
            dataTypes.Add(1009, "_text", PgDbType.Array, 25, PgTypeFormat.Text, -1);
                      
            //
            // SPECIAL CHARACTER TYPES
            //                      
            
            dataTypes.Add(  19, "name"  , PgDbType.VarChar, 0, PgTypeFormat.Text, 0);
            dataTypes.Add(1042, "bpchar", PgDbType.Char   , 0, PgTypeFormat.Text, 0);
                      
            //
            // BINARY DATA TYPES
            //
            
            dataTypes.Add(  17, "bytea" , PgDbType.Bytea,  0, PgTypeFormat.Binary, Int32.MaxValue);
            dataTypes.Add(1001, "_bytea", PgDbType.Array, 17, PgTypeFormat.Binary, -1);

            //
            // DATE/TIME TYPES
            //
            
            // timestamp [ (p) ] [ without time zone ] | 8 bytes | both date and time (no time zone) | 4713 BC | 294276 AD | 1 microsecond / 14 digits
            
            dataTypes.Add(1114, "timestamp"  , PgDbType.Timestamp, 0, PgTypeFormat.Binary, 8);
                                
            // timestamp [ (p) ] with time zone | 8 bytes | both date and time, with time zone | 4713 BC | 294276 AD | 1 microsecond / 14 digits
            
            dataTypes.Add(1184,  "timestamptz", PgDbType.TimestampTZ,    0, PgTypeFormat.Binary,  8);
            dataTypes.Add(1185, "_timestamptz", PgDbType.Array      , 1184, PgTypeFormat.Binary, -1);

            // date	| 4 bytes | date (no time of day) | 4713 BC | 5874897 AD | 1 day
            
            dataTypes.Add(1082,  "date", PgDbType.Date ,    0, PgTypeFormat.Binary,  4);
            dataTypes.Add(1182, "_date", PgDbType.Array, 1082, PgTypeFormat.Binary, -1);

            // time [ (p) ] [ without time zone ] | 8 bytes | time of day (no date) | 00:00:00 | 24:00:00 | 1 microsecond / 14 digits
            
            dataTypes.Add(1083,  "time", PgDbType.Time ,    0, PgTypeFormat.Text,  8);
            dataTypes.Add(1183, "_time", PgDbType.Array, 1083, PgTypeFormat.Text, -1);
            
            // time [ (p) ] with time zone | 12 bytes | times of day only, with time zone | 00:00:00+1459 | 24:00:00-1459 | 1 microsecond / 14 digits

            dataTypes.Add(1266,  "timetz", PgDbType.TimeTZ,    0, PgTypeFormat.Binary, 12);
            dataTypes.Add(1270, "_timetz", PgDbType.Array , 1266, PgTypeFormat.Binary, -1);

            // interval [ fields ] [ (p) ] | 16 bytes | time interval | -178000000 years | 178000000 years | 1 microsecond / 14 digits
                        
            dataTypes.Add(1186,  "interval", PgDbType.Interval,    0, PgTypeFormat.Binary, 16);
            dataTypes.Add(1187, "_interval", PgDbType.Array   , 1186, PgTypeFormat.Binary, -1);

            //
            // BOOLEAN TYPE
            //

            // boolean | 1 byte | state of true or false
            
            dataTypes.Add(  16, "bool" , PgDbType.Bool ,  0, PgTypeFormat.Binary, 1);
            dataTypes.Add(1000, "_bool", PgDbType.Array, 16, PgTypeFormat.Binary, -1);

            //
            // GEOMETRIC TYPES
            //

            // point | 16 bytes | Point on a plane (x,y)
            
            dataTypes.Add( 600, "point" , PgDbType.Point, 701, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add(1017, "_point", PgDbType.Array, 600, PgTypeFormat.Binary, -1);
            
            // line	| 32 bytes | Infinite line {A,B,C}
            
            dataTypes.Add(628, "line" , PgDbType.Line , 701, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add(629, "_line", PgDbType.Array, 628, PgTypeFormat.Binary, -1);

            // lseg	| 32 bytes | Finite line segment | ((x1,y1),(x2,y2))
            
            dataTypes.Add( 601, "lseg" , PgDbType.LSeg ,  600, PgTypeFormat.Binary, 32, ",");
            dataTypes.Add(1018, "_lseg", PgDbType.Array,  601, PgTypeFormat.Binary, -1);

            // box | 32 bytes | Rectangular box | ((x1,y1),(x2,y2))
            dataTypes.Add( 603, "box" , PgDbType.Box  ,  600, PgTypeFormat.Binary, 32, ";");
            dataTypes.Add(1020, "_box", PgDbType.Array,  603, PgTypeFormat.Binary, -1);

            // path | 16+16n bytes | Closed path (similar to polygon) |	((x1,y1),...)
            // path | 16+16n bytes | Open path                        | [(x1,y1),...]
            
            dataTypes.Add( 602, "path" , PgDbType.Path ,   0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add(1019, "_path", PgDbType.Array, 602, PgTypeFormat.Binary, -1);

            // polygon | 40+16n bytes | Polygon (similar to closed path) | ((x1,y1),...)
            
            dataTypes.Add( 604, "polygon" , PgDbType.Polygon,   0, PgTypeFormat.Binary, 16, ",");
            dataTypes.Add(1027, "_polygon", PgDbType.Array  , 604, PgTypeFormat.Binary, -1);

            // circle | 24 bytes | Circle | <(x,y),r> (center point and radius)
            
            dataTypes.Add(718, "circle" , PgDbType.Circle,   0, PgTypeFormat.Binary, 24, ",");
            dataTypes.Add(719, "_circle", PgDbType.Array , 718, PgTypeFormat.Binary, -1);

            //
            // POSTGIS TYPES
            //
            
            dataTypes.Add(17321, "box3d", PgDbType.Box3D, 0, PgTypeFormat.Text, 48, ",", "BOX3D");
            dataTypes.Add(17335, "box2d", PgDbType.Box2D, 0, PgTypeFormat.Text, 16, ",", "BOX");

            //
            // Network Address Types
            //
            
            // cidr	7 or 19 bytes	IPv4 and IPv6 networks
#warning TODO: Add ??            
            
            // inet	| 7 or 19 bytes	| IPv4 and IPv6 hosts and networks
            
            dataTypes.Add( 869, "inet", PgDbType.VarChar, 0, PgTypeFormat.Text, 0);
            
            // macaddr | 6 bytes | MAC addresses
                        
            dataTypes.Add( 829, "macaddr", PgDbType.VarChar, 0, PgTypeFormat.Text, 6);

            //
            // BIT STRING TYPES
            // 
            
            dataTypes.Add(1560, "bit"   , PgDbType.Byte, 0, PgTypeFormat.Binary, 1);
            dataTypes.Add(1562, "varbit", PgDbType.Byte, 0, PgTypeFormat.Binary, 0);

            //
            // OBJECT IDENTIFIER TYPES
            //

            dataTypes.Add(  24, "regproc"  , PgDbType.VarChar  ,    0, PgTypeFormat.Text  ,  0);
            dataTypes.Add(  26, "oid"      , PgDbType.Int32    ,    0, PgTypeFormat.Binary,  4);
            dataTypes.Add(  30, "oidvector", PgDbType.Vector   ,   26, PgTypeFormat.Binary, -1);
            dataTypes.Add(1028, "_oid"     , PgDbType.Array    ,   26, PgTypeFormat.Binary, -1);
            dataTypes.Add(1033, "aclitem"  , PgDbType.VarChar  ,    0, PgTypeFormat.Text  , 12);
            dataTypes.Add(1034, "_aclitem" , PgDbType.Array    , 1033, PgTypeFormat.Text  , -1);
            dataTypes.Add(1790, "refcursor", PgDbType.Refcursor,    0, PgTypeFormat.Text  ,  0);
            dataTypes.Add(2205, "regclass" , PgDbType.VarChar  ,    0, PgTypeFormat.Text  ,  0);
            
            //
            // PSEUDO-TYPES
            //
                      
            dataTypes.Add( 705, "unknown", PgDbType.Text, 0, PgTypeFormat.Text  , 0);
            dataTypes.Add(2278, "void"   , PgDbType.Void, 0, PgTypeFormat.Binary, 0);

            return dataTypes;
        }

        private int                   _oid;
        private readonly string       _name;
        private readonly PgDbType     _providerType;
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

        internal PgDbType     ProviderType => _providerType;
        internal string       Name         => _name;
        internal Type         SystemType   => _systemType;
        internal int          ElementType  => _elementType; 
        internal PgTypeFormat Format       => _format;
        internal int          Size         => _size;
        internal bool         IsArray      => (_providerType == PgDbType.Array);
        internal bool         IsBinary     => (_providerType == PgDbType.Bytea);
        internal bool         IsRefCursor  => (_providerType == PgDbType.Refcursor);
        internal string       Delimiter    => _delimiter;
        internal string       Prefix       => _prefix;

        internal bool IsNumeric
        {
            get
            {
                return (_providerType == PgDbType.Byte
                     || _providerType == PgDbType.Int16
                     || _providerType == PgDbType.Int32
                     || _providerType == PgDbType.Int64
                     || _providerType == PgDbType.Money
                     || _providerType == PgDbType.Real
                     || _providerType == PgDbType.Double
                     || _providerType == PgDbType.Numeric);
            }
        }

        internal bool IsPrimitive
        {
            get
            {
                return (_providerType == PgDbType.Bool
                     || _providerType == PgDbType.Byte
                     || _providerType == PgDbType.Char
                     || _providerType == PgDbType.Int16
                     || _providerType == PgDbType.Int32
                     || _providerType == PgDbType.Int64
                     || _providerType == PgDbType.Real
                     || _providerType == PgDbType.Double);
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
            _oid          = oid;
            _name         = name;
            _providerType = dataType;
            _elementType  = elementType;
            _format       = format;
            _size         = size;
            _delimiter    = delimiter;
            _prefix       = prefix;
            _systemType   = TypeHelper.GetSystemType(_providerType);
        }
        
        public override string ToString()
        {
            return _name;
        }
    }
}
