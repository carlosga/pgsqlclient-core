// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

namespace PostgreSql.Data.PgTypes
{
    internal static class PgTypeInfoProvider
    {
        internal static readonly ReadOnlyDictionary<int, TypeInfo> Types;

        internal static readonly string          NullString       = "Null";
        internal static readonly IFormatProvider InvariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        
        internal static bool     IsNullString(string s) => (s == null || s == NullString);

        static PgTypeInfoProvider()
        {
            Dictionary<int, TypeInfo> types = new Dictionary<int, TypeInfo>(100);

            //
            // BYTE TYPE
            //
            types[18] = new TypeInfo(18, "byte", "char", PgDbType.Byte, TypeFormat.Binary, typeof(byte), typeof(PgByte), sizeof(byte));
            
            //
            // BINARY DATA TYPES
            //

            types[17  ] = new TypeInfo(  17, "bytea"  , "bytea" , PgDbType.Bytea, TypeFormat.Binary, typeof(byte[]), typeof(PgBinary));
            types[1001] = new TypeInfo(1001, "bytea[]", "_bytea", PgDbType.Array, types[17], typeof(byte[][]), typeof(PgBinary[]));

            //
            // BIT STRING TYPES
            //

            types[1560] = new TypeInfo(1560, "bit"   , "bit"   , PgDbType.Byte, TypeFormat.Binary, typeof(byte), typeof(PgBit), sizeof(byte));
            types[1562] = new TypeInfo(1562, "varbit", "varbit", PgDbType.Byte, types[1560], typeof(byte[]), typeof(PgBit));

            //
            // BOOLEAN TYPE
            //

            // boolean | 1 byte | state of true or false

            types[  16] = new TypeInfo(  16, "bool"  , "bool" , PgDbType.Bool , TypeFormat.Binary, typeof(bool), typeof(PgBoolean), sizeof(bool));
            types[1000] = new TypeInfo(1000, "bool[]", "_bool", PgDbType.Array, types[16], typeof(bool[]), typeof(PgBoolean[]));
            
            //
            // CHARACTER TYPES
            //

            // character varying(n), varchar(n) | variable-length with limit

            types[1043] = new TypeInfo(1043, "varchar"  , "varchar" , PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(PgString));
            types[1015] = new TypeInfo(1015, "varchar[]", "_varchar", PgDbType.Array  , types[1043], typeof(string[]), typeof(PgString[]));

            // character(n), char(n) | fixed-length, blank padded

#warning TODO: This char type isn't char(n)
            types[1002] = new TypeInfo(1002, "char[]", "_char", PgDbType.Array, types[18], typeof(char[]), typeof(PgString));

            // text	variable unlimited length

            types[  25] = new TypeInfo(  25, "text"  , "text" , PgDbType.Text , TypeFormat.Text, typeof(string), typeof(PgString));
            types[1009] = new TypeInfo(1009, "text[]", "_text", PgDbType.Array, types[25], typeof(string[]), typeof(PgString[]));

            //
            // DATE/TIME TYPES
            //

            // timestamp [ (p) ] [ without time zone ] | 8 bytes | both date and time (no time zone) | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1114] = new TypeInfo(1114, "timestamp"  , "timestamp" , PgDbType.Timestamp, TypeFormat.Binary, typeof(DateTime), typeof(PgTimestamp), 8);
            types[1115] = new TypeInfo(1115, "timestamp[]", "_timestamp", PgDbType.Array    , types[1114], typeof(DateTime[]), typeof(PgTimestamp[]));

            // timestamp [ (p) ] with time zone | 8 bytes | both date and time, with time zone | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1184] = new TypeInfo(1184, "timestamptz"  , "timestamptz" , PgDbType.TimestampTZ, TypeFormat.Binary, typeof(DateTimeOffset), typeof(PgTimestamp), 8);
            types[1185] = new TypeInfo(1185, "timestamptz[]", "_timestamptz", PgDbType.Array      , types[1184], typeof(DateTimeOffset[]), typeof(PgTimestamp[]));

            // date	| 4 bytes | date (no time of day) | 4713 BC | 5874897 AD | 1 day

            types[1082] = new TypeInfo(1082, "date"  , "date" , PgDbType.Date , TypeFormat.Binary, typeof(DateTime), typeof(PgDate), 4);
            types[1182] = new TypeInfo(1182, "date[]", "_date", PgDbType.Array, types[1082], typeof(DateTime[]), typeof(PgDate[]));

            // time [ (p) ] [ without time zone ] | 8 bytes | time of day (no date) | 00:00:00 | 24:00:00 | 1 microsecond / 14 digits

            types[1083] = new TypeInfo(1083, "time"  , "time" , PgDbType.Time , TypeFormat.Binary, typeof(TimeSpan), typeof(PgTime), 8);
            types[1183] = new TypeInfo(1183, "time[]", "_time", PgDbType.Array, types[1083], typeof(TimeSpan[]), typeof(PgTime[]));
            
            // time [ (p) ] with time zone | 12 bytes | times of day only, with time zone | 00:00:00+1459 | 24:00:00-1459 | 1 microsecond / 14 digits

            types[1266] = new TypeInfo(1266, "timetz"  , "timetz" , PgDbType.TimeTZ, TypeFormat.Binary, typeof(DateTimeOffset), typeof(PgTime), 12);
            types[1270] = new TypeInfo(1270, "timetz[]", "_timetz", PgDbType.Array , types[1266], typeof(DateTimeOffset[]), typeof(PgTime));

            // interval [ fields ] [ (p) ] | 16 bytes | time interval | -178000000 years | 178000000 years | 1 microsecond / 14 digits

            types[1186] = new TypeInfo(1186, "interval"  , "interval" , PgDbType.Interval, TypeFormat.Binary, typeof(TimeSpan), typeof(PgInterval), 16);
            types[1187] = new TypeInfo(1187, "interval[]", "_interval", PgDbType.Array   , types[1186], typeof(TimeSpan[]), typeof(PgInterval));

            //
            // NUMERIC TYPES
            // 

            // smallint	| 2 bytes | small-range integer | -32768 to +32767

            types[  21] = new TypeInfo(  21, "smallint"  , "int2"      , PgDbType.SmallInt, TypeFormat.Binary, typeof(short), typeof(PgInt16), sizeof(short));
            types[  22] = new TypeInfo(  22, "smallint[]", "int2vector", PgDbType.Vector  , types[21], typeof(short[]), typeof(PgInt16[]));
            types[1005] = new TypeInfo(1005, "smallint[]", "_int2"     , PgDbType.Array   , types[21], typeof(short[]), typeof(PgInt16[]));
            
            // integer | 4 bytes | typical choice for integer | -2147483648 to +2147483647
            
            types[  23] = new TypeInfo(  23, "integer"  , "int4" , PgDbType.Integer, TypeFormat.Binary, typeof(int), typeof(PgInt32), sizeof(int));
            types[1007] = new TypeInfo(1007, "integer[]", "_int4", PgDbType.Array  , types[23], typeof(int[]), typeof(PgInt32[]));

            // bigint | 8 bytes | large-range integer | -9223372036854775808 to +9223372036854775807

            types[  20] = new TypeInfo(  20, "bigint"  , "int8" , PgDbType.BigInt, TypeFormat.Binary, typeof(long), typeof(PgInt64), sizeof(long));
            types[1016] = new TypeInfo(1016, "bigint[]", "_int8", PgDbType.Array , types[20], typeof(long[]), typeof(PgInt64[]));

            // decimal | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
            // numeric | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point

            types[1700] = new TypeInfo(1700, "numeric"  ,  "numeric", PgDbType.Numeric, TypeFormat.Text, typeof(decimal), typeof(PgDecimal));
            types[1231] = new TypeInfo(1231, "numeric[]", "_numeric", PgDbType.Array  , types[1700], typeof(decimal[]), typeof(PgDecimal[]));

            // real | 4 bytes | variable-precision, inexact	6 decimal digits precision

            types[ 700] = new TypeInfo( 700, "real"  , "float4" , PgDbType.Real , TypeFormat.Binary, typeof(float), typeof(PgReal), sizeof(float));
            types[1021] = new TypeInfo(1021, "real[]", "_float4", PgDbType.Array, types[700], typeof(float[]), typeof(PgReal));

            // double precision | 8 bytes | variable-precision, inexact	15 decimal digits precision

            types[ 701] = new TypeInfo( 701, "double"  , "float8" , PgDbType.Double, TypeFormat.Binary, typeof(double), typeof(PgDouble), sizeof(double));
            types[1021] = new TypeInfo(1021, "double[]", "_float8", PgDbType.Array , types[701], typeof(double[]), typeof(PgDouble[]));

            //
            // MONETARY TYPES
            //

            // money | 8 bytes | currency amount | -92233720368547758.08 to +92233720368547758.07

            types[790] = new TypeInfo(790, "money"  , "money" , PgDbType.Money, TypeFormat.Binary, typeof(decimal), typeof(PgMoney), 8);
            types[791] = new TypeInfo(791, "money[]", "_money", PgDbType.Array, types[790], typeof(decimal[]), typeof(PgMoney[]));

            //
            // SPECIAL CHARACTER TYPES
            //

            types[  19] = new TypeInfo(  19, "name"    , "name"   , PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(PgString));
            types[1042] = new TypeInfo(1042, "bpchar"  , "bpchar" , PgDbType.Char, TypeFormat.Text, typeof(string), typeof(PgString));
            types[1014] = new TypeInfo(1014, "bpchar[]", "_bpchar", PgDbType.Char, types[1042], typeof(string[]), typeof(PgString[]));

            //
            // GEOMETRIC TYPES
            //

            // point | 16 bytes | Point on a plane (x,y)

            types[ 600] = new TypeInfo( 600, "point"  , "point" , PgDbType.Point, types[701], TypeFormat.Binary, typeof(PgPoint), typeof(PgPoint), 16);
            types[1017] = new TypeInfo(1017, "point[]", "_point", PgDbType.Array, types[600], typeof(PgPoint[]), typeof(PgPoint[]));

            // line	| 32 bytes | Infinite line {A,B,C}

            types[628] = new TypeInfo(628, "line"  , "line" , PgDbType.Line , types[701], TypeFormat.Binary, typeof(PgLine), typeof(PgLine), 32);
            types[629] = new TypeInfo(629, "line[]", "_line", PgDbType.Array, types[628], typeof(PgLine[]), typeof(PgLine[]));

            // lseg	| 32 bytes | Finite line segment | ((x1,y1),(x2,y2))

            types[ 601] = new TypeInfo( 601, "lseg"  , "lseg" , PgDbType.LSeg , types[600], TypeFormat.Binary, typeof(PgLSeg), typeof(PgLSeg), 32);
            types[1018] = new TypeInfo(1018, "lseg[]", "_lseg", PgDbType.Array, types[601], typeof(PgLSeg[]), typeof(PgLSeg[]));

            // box | 32 bytes | Rectangular box | ((x1,y1),(x2,y2))
            
            types[ 603] = new TypeInfo( 603, "box"  , "box" , PgDbType.Box  , types[600], TypeFormat.Binary, typeof(PgBox), typeof(PgBox[]), 32);
            types[1020] = new TypeInfo(1020, "box[]", "_box", PgDbType.Array, types[603], typeof(PgBox[]), typeof(PgBox[]));

            // path | 16+16n bytes | Closed path (similar to polygon) |	((x1,y1),...)
            // path | 16+16n bytes | Open path                        | [(x1,y1),...]

            types[ 602] = new TypeInfo( 602, "path"  , "path" , PgDbType.Path , TypeFormat.Binary, typeof(PgPath), typeof(PgPath), 16);
            types[1019] = new TypeInfo(1019, "path[]", "_path", PgDbType.Array, types[602], typeof(PgPath[]), typeof(PgPath[]));

            // polygon | 40+16n bytes | Polygon (similar to closed path) | ((x1,y1),...)

            types[ 604] = new TypeInfo( 604, "polygon"  , "polygon" , PgDbType.Polygon, TypeFormat.Binary, typeof(PgPolygon), typeof(PgPolygon), 40);
            types[1027] = new TypeInfo(1027, "polygon[]", "_polygon", PgDbType.Array  , types[604], typeof(PgPolygon[]), typeof(PgPolygon[]));

            // circle | 24 bytes | Circle | <(x,y),r> (center point and radius)

            types[718] = new TypeInfo(718, "circle"  , "circle" , PgDbType.Circle, TypeFormat.Binary, typeof(PgCircle), typeof(PgCircle), 24);
            types[719] = new TypeInfo(719, "circle[]", "_circle", PgDbType.Array , types[718], typeof(PgCircle[]), typeof(PgCircle[]));

            //
            // POSTGIS TYPES
            //

            types[17321] = new TypeInfo(17321, "box3d", "box3d", PgDbType.Box3D, TypeFormat.Binary, typeof(PgBox3D), typeof(PgBox3D), 48);
            types[17335] = new TypeInfo(17335, "box2d", "box2d", PgDbType.Box2D, TypeFormat.Binary, typeof(PgBox2D), typeof(PgBox2D), 16);

            //
            // Network Address Types
            //

            // cidr	7 or 19 bytes	IPv4 and IPv6 networks
#warning TODO: Add ??

            // inet	| 7 or 19 bytes	| IPv4 and IPv6 hosts and networks

            types[869] = new TypeInfo(869, "inet", "inet", PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string));

            // macaddr | 6 bytes | MAC addresses

            types[829] = new TypeInfo(829, "macaddr", "macaddr", PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string), 6);

            //
            // OBJECT IDENTIFIER TYPES
            //

            types[  26] = new TypeInfo(  26, "oid"        , "oid"      , PgDbType.Integer, TypeFormat.Binary, typeof(int), typeof(PgInt32), sizeof(int));
            types[  30] = new TypeInfo(  30, "oidvector[]", "oidvector", PgDbType.Vector , types[26], typeof(int[]), typeof(PgInt32[]));
            types[1028] = new TypeInfo(1028, "oid[]"      , "_oid"     , PgDbType.Array  , types[26], typeof(int[]), typeof(PgInt32[]));

            types[1033] = new TypeInfo(1033, "aclitem"    , "aclitem"  , PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string));
            types[1034] = new TypeInfo(1034, "aclitem[]"  , "_aclitem" , PgDbType.Array  , types[1033], typeof(string), typeof(string[]));

            types[  24] = new TypeInfo(  24, "regproc"    , "regproc"  , PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string[]));
            types[1790] = new TypeInfo(1790, "refcursor"  , "refcursor", PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string[]));
            types[2205] = new TypeInfo(2205, "regclass"   , "regclass" , PgDbType.VarChar, TypeFormat.Text, typeof(string), typeof(string[]));

            //
            // PSEUDO-TYPES
            //

            types[ 705] = new TypeInfo( 705, "unknown", "unknown", PgDbType.Text, TypeFormat.Text  , typeof(string), typeof(string));
            types[2278] = new TypeInfo(2278, "void"   , "void"   , PgDbType.Void, TypeFormat.Binary, typeof(void), typeof(void), 0);

            Types = new ReadOnlyDictionary<int, TypeInfo>(types);
        }

        internal static DbType GetDbType(PgDbType providerType)
        {
            switch (providerType)
            {
                case PgDbType.Bool:
                    return DbType.Boolean;

                case PgDbType.Byte:
                    return DbType.Byte;

                case PgDbType.Bytea:
                    return DbType.Binary;

                case PgDbType.Char:
                case PgDbType.Text:
                case PgDbType.VarChar:
                    return DbType.String;

                case PgDbType.SmallInt:
                    return DbType.Int16;

                case PgDbType.Integer:
                    return DbType.Int32;

                case PgDbType.BigInt:
                    return DbType.Int64;

                case PgDbType.Date:
                    return DbType.Date;

                case PgDbType.Time:
                    return DbType.Time;

                case PgDbType.Timestamp:
                    return DbType.DateTime;

                case PgDbType.TimeTZ:
                case PgDbType.TimestampTZ:
                    return DbType.DateTimeOffset;

                case PgDbType.Numeric:
                    return DbType.Decimal;

                case PgDbType.Real:
                    return DbType.Single;

                case PgDbType.Double:
                    return DbType.Double;

                case PgDbType.Money:
                    return DbType.Currency;

                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }
        }

        internal static PgDbType GetProviderType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                case DbType.Object:
                    return PgDbType.VarChar;

                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return PgDbType.Char;

                case DbType.Int16:
                    return PgDbType.SmallInt;

                case DbType.Int32:
                    return PgDbType.Integer;

                case DbType.Int64:
                    return PgDbType.BigInt;

                case DbType.Decimal:
                    return PgDbType.Numeric;

                case DbType.Single:
                    return PgDbType.Real;

                case DbType.Double:
                    return PgDbType.Double;

                case DbType.Binary:
                    return PgDbType.Bytea;

                case DbType.Boolean:
                    return PgDbType.Bool;

                case DbType.Byte:
                    return PgDbType.Byte;

                case DbType.Currency:
                    return PgDbType.Money;

                case DbType.Date:
                    return PgDbType.Date;

                case DbType.Time:
                    return PgDbType.Time;

                case DbType.DateTime:
                    return PgDbType.Timestamp;

                case DbType.DateTimeOffset:
                    return PgDbType.TimestampTZ;

                // case DbType.Guid:
                // case DbType.VarNumeric:
                // case DbType.SByte:
                // case DbType.UInt16:
                // case DbType.UInt32:
                // case DbType.UInt64:
                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }
        }

        internal static TypeInfo GetTypeInfo(PgDbType pgDbType)
        {
            return Types.Values.First(x => x.PgDbType == pgDbType);
        }

        internal static TypeInfo GetTypeInfo(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return Types[1043];   // Varchar by default
            }
            if (value is INullable)
            {
                return Types.Values.First(x => x.PgType == value.GetType());
            }
            return Types.Values.First(x => x.SystemType == value.GetType());
        }
    }
}
