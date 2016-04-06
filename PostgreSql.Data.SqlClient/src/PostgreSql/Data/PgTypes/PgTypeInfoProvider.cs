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
        internal static readonly ReadOnlyDictionary<int, PgTypeInfo> Types;

        internal static readonly string          NullString       = "Null";
        internal static readonly IFormatProvider InvariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        
        internal static bool     IsNullString(string s) => (s == null || s == NullString);

        static PgTypeInfoProvider()
        {
            Dictionary<int, PgTypeInfo> types = new Dictionary<int, PgTypeInfo>(100);

            //
            // BYTE TYPE
            //
            types[18] = new PgTypeInfo(18, "byte", "char", PgDbType.Byte, PgTypeFormat.Binary, typeof(byte), typeof(PgByte), sizeof(byte));
            
            //
            // BINARY DATA TYPES
            //

            types[17  ] = new PgTypeInfo(  17, "bytea"  , "bytea" , PgDbType.Bytea, PgTypeFormat.Binary, typeof(byte[]), typeof(PgBinary));
            types[1001] = new PgTypeInfo(1001, "bytea[]", "_bytea", PgDbType.Array, types[17], typeof(byte[][]), typeof(PgBinary[]));

            //
            // BIT STRING TYPES
            //

            types[1560] = new PgTypeInfo(1560, "bit"   , "bit"   , PgDbType.Byte, PgTypeFormat.Binary, typeof(byte), typeof(PgBit), sizeof(byte));
            types[1562] = new PgTypeInfo(1562, "varbit", "varbit", PgDbType.Byte, types[1560], typeof(byte[]), typeof(PgBit));

            //
            // BOOLEAN TYPE
            //

            // boolean | 1 byte | state of true or false

            types[  16] = new PgTypeInfo(  16, "bool"  , "bool" , PgDbType.Bool , PgTypeFormat.Binary, typeof(bool), typeof(PgBoolean), sizeof(bool));
            types[1000] = new PgTypeInfo(1000, "bool[]", "_bool", PgDbType.Array, types[16], typeof(bool[]), typeof(PgBoolean[]));
            
            //
            // CHARACTER TYPES
            //

            // character varying(n), varchar(n) | variable-length with limit

            types[1043] = new PgTypeInfo(1043, "varchar"  , "varchar" , PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(PgString));
            types[1015] = new PgTypeInfo(1015, "varchar[]", "_varchar", PgDbType.Array  , types[1043], typeof(string[]), typeof(PgString[]));

            // character(n), char(n) | fixed-length, blank padded

#warning TODO: This char type isn't char(n)
            types[1002] = new PgTypeInfo(1002, "char[]", "_char", PgDbType.Array, types[18], typeof(char[]), typeof(PgString));

            // text	variable unlimited length

            types[  25] = new PgTypeInfo(  25, "text"  , "text" , PgDbType.Text , PgTypeFormat.Text, typeof(string), typeof(PgString));
            types[1009] = new PgTypeInfo(1009, "text[]", "_text", PgDbType.Array, types[25], typeof(string[]), typeof(PgString[]));

            //
            // DATE/TIME TYPES
            //

            // timestamp [ (p) ] [ without time zone ] | 8 bytes | both date and time (no time zone) | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1114] = new PgTypeInfo(1114, "timestamp"  , "timestamp" , PgDbType.Timestamp, PgTypeFormat.Binary, typeof(DateTime), typeof(PgTimestamp), 8);
            types[1115] = new PgTypeInfo(1115, "timestamp[]", "_timestamp", PgDbType.Array    , types[1114], typeof(DateTime[]), typeof(PgTimestamp[]));

            // timestamp [ (p) ] with time zone | 8 bytes | both date and time, with time zone | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1184] = new PgTypeInfo(1184, "timestamptz"  , "timestamptz" , PgDbType.TimestampTZ, PgTypeFormat.Binary, typeof(DateTimeOffset), typeof(PgTimestamp), 8);
            types[1185] = new PgTypeInfo(1185, "timestamptz[]", "_timestamptz", PgDbType.Array      , types[1184], typeof(DateTimeOffset[]), typeof(PgTimestamp[]));

            // date	| 4 bytes | date (no time of day) | 4713 BC | 5874897 AD | 1 day

            types[1082] = new PgTypeInfo(1082, "date"  , "date" , PgDbType.Date , PgTypeFormat.Binary, typeof(DateTime), typeof(PgDate), 4);
            types[1182] = new PgTypeInfo(1182, "date[]", "_date", PgDbType.Array, types[1082], typeof(DateTime[]), typeof(PgDate[]));

            // time [ (p) ] [ without time zone ] | 8 bytes | time of day (no date) | 00:00:00 | 24:00:00 | 1 microsecond / 14 digits

            types[1083] = new PgTypeInfo(1083, "time"  , "time" , PgDbType.Time , PgTypeFormat.Binary, typeof(TimeSpan), typeof(PgTime), 8);
            types[1183] = new PgTypeInfo(1183, "time[]", "_time", PgDbType.Array, types[1083], typeof(TimeSpan[]), typeof(PgTime[]));
            
            // time [ (p) ] with time zone | 12 bytes | times of day only, with time zone | 00:00:00+1459 | 24:00:00-1459 | 1 microsecond / 14 digits

            types[1266] = new PgTypeInfo(1266, "timetz"  , "timetz" , PgDbType.TimeTZ, PgTypeFormat.Binary, typeof(DateTimeOffset), typeof(PgTime), 12);
            types[1270] = new PgTypeInfo(1270, "timetz[]", "_timetz", PgDbType.Array , types[1266], typeof(DateTimeOffset[]), typeof(PgTime));

            // interval [ fields ] [ (p) ] | 16 bytes | time interval | -178000000 years | 178000000 years | 1 microsecond / 14 digits

            types[1186] = new PgTypeInfo(1186, "interval"  , "interval" , PgDbType.Interval, PgTypeFormat.Binary, typeof(TimeSpan), typeof(PgInterval), 16);
            types[1187] = new PgTypeInfo(1187, "interval[]", "_interval", PgDbType.Array   , types[1186], typeof(TimeSpan[]), typeof(PgInterval));

            //
            // NUMERIC TYPES
            // 

            // smallint	| 2 bytes | small-range integer | -32768 to +32767

            types[  21] = new PgTypeInfo(  21, "smallint"  , "int2"      , PgDbType.SmallInt, PgTypeFormat.Binary, typeof(short), typeof(PgInt16), sizeof(short));
            types[  22] = new PgTypeInfo(  22, "smallint[]", "int2vector", PgDbType.Vector  , types[21], typeof(short[]), typeof(PgInt16[]));
            types[1005] = new PgTypeInfo(1005, "smallint[]", "_int2"     , PgDbType.Array   , types[21], typeof(short[]), typeof(PgInt16[]));
            
            // integer | 4 bytes | typical choice for integer | -2147483648 to +2147483647
            
            types[  23] = new PgTypeInfo(  23, "integer"  , "int4" , PgDbType.Integer, PgTypeFormat.Binary, typeof(int), typeof(PgInt32), sizeof(int));
            types[1007] = new PgTypeInfo(1007, "integer[]", "_int4", PgDbType.Array  , types[23], typeof(int[]), typeof(PgInt32[]));

            // bigint | 8 bytes | large-range integer | -9223372036854775808 to +9223372036854775807

            types[  20] = new PgTypeInfo(  20, "bigint"  , "int8" , PgDbType.BigInt, PgTypeFormat.Binary, typeof(long), typeof(PgInt64), sizeof(long));
            types[1016] = new PgTypeInfo(1016, "bigint[]", "_int8", PgDbType.Array , types[20], typeof(long[]), typeof(PgInt64[]));

            // decimal | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
            // numeric | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point

            types[1700] = new PgTypeInfo(1700, "numeric"  ,  "numeric", PgDbType.Numeric, PgTypeFormat.Text, typeof(decimal), typeof(PgDecimal));
            types[1231] = new PgTypeInfo(1231, "numeric[]", "_numeric", PgDbType.Array  , types[1700], typeof(decimal[]), typeof(PgDecimal[]));

            // real | 4 bytes | variable-precision, inexact	6 decimal digits precision

            types[ 700] = new PgTypeInfo( 700, "real"  , "float4" , PgDbType.Real , PgTypeFormat.Binary, typeof(float), typeof(PgReal), sizeof(float));
            types[1021] = new PgTypeInfo(1021, "real[]", "_float4", PgDbType.Array, types[700], typeof(float[]), typeof(PgReal));

            // double precision | 8 bytes | variable-precision, inexact	15 decimal digits precision

            types[ 701] = new PgTypeInfo( 701, "double"  , "float8" , PgDbType.Double, PgTypeFormat.Binary, typeof(double), typeof(PgDouble), sizeof(double));
            types[1021] = new PgTypeInfo(1021, "double[]", "_float8", PgDbType.Array , types[701], typeof(double[]), typeof(PgDouble[]));

            //
            // MONETARY TYPES
            //

            // money | 8 bytes | currency amount | -92233720368547758.08 to +92233720368547758.07

            types[790] = new PgTypeInfo(790, "money"  , "money" , PgDbType.Money, PgTypeFormat.Binary, typeof(decimal), typeof(PgMoney), 8);
            types[791] = new PgTypeInfo(791, "money[]", "_money", PgDbType.Array, types[790], typeof(decimal[]), typeof(PgMoney[]));

            //
            // SPECIAL CHARACTER TYPES
            //

            types[  19] = new PgTypeInfo(  19, "name"    , "name"   , PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(PgString));
            types[1042] = new PgTypeInfo(1042, "bpchar"  , "bpchar" , PgDbType.Char, PgTypeFormat.Text, typeof(string), typeof(PgString));
            types[1014] = new PgTypeInfo(1014, "bpchar[]", "_bpchar", PgDbType.Char, types[1042], typeof(string[]), typeof(PgString[]));

            //
            // GEOMETRIC TYPES
            //

            // point | 16 bytes | Point on a plane (x,y)

            types[ 600] = new PgTypeInfo( 600, "point"  , "point" , PgDbType.Point, types[701], PgTypeFormat.Binary, typeof(PgPoint), typeof(PgPoint), 16);
            types[1017] = new PgTypeInfo(1017, "point[]", "_point", PgDbType.Array, types[600], typeof(PgPoint[]), typeof(PgPoint[]));

            // line	| 32 bytes | Infinite line {A,B,C}

            types[628] = new PgTypeInfo(628, "line"  , "line" , PgDbType.Line , types[701], PgTypeFormat.Binary, typeof(PgLine), typeof(PgLine), 32);
            types[629] = new PgTypeInfo(629, "line[]", "_line", PgDbType.Array, types[628], typeof(PgLine[]), typeof(PgLine[]));

            // lseg	| 32 bytes | Finite line segment | ((x1,y1),(x2,y2))

            types[ 601] = new PgTypeInfo( 601, "lseg"  , "lseg" , PgDbType.LSeg , types[600], PgTypeFormat.Binary, typeof(PgLSeg), typeof(PgLSeg), 32);
            types[1018] = new PgTypeInfo(1018, "lseg[]", "_lseg", PgDbType.Array, types[601], typeof(PgLSeg[]), typeof(PgLSeg[]));

            // box | 32 bytes | Rectangular box | ((x1,y1),(x2,y2))
            
            types[ 603] = new PgTypeInfo( 603, "box"  , "box" , PgDbType.Box  , types[600], PgTypeFormat.Binary, typeof(PgBox), typeof(PgBox[]), 32);
            types[1020] = new PgTypeInfo(1020, "box[]", "_box", PgDbType.Array, types[603], typeof(PgBox[]), typeof(PgBox[]));

            // path | 16+16n bytes | Closed path (similar to polygon) |	((x1,y1),...)
            // path | 16+16n bytes | Open path                        | [(x1,y1),...]

            types[ 602] = new PgTypeInfo( 602, "path"  , "path" , PgDbType.Path , PgTypeFormat.Binary, typeof(PgPath), typeof(PgPath), 16);
            types[1019] = new PgTypeInfo(1019, "path[]", "_path", PgDbType.Array, types[602], typeof(PgPath[]), typeof(PgPath[]));

            // polygon | 40+16n bytes | Polygon (similar to closed path) | ((x1,y1),...)

            types[ 604] = new PgTypeInfo( 604, "polygon"  , "polygon" , PgDbType.Polygon, PgTypeFormat.Binary, typeof(PgPolygon), typeof(PgPolygon), 40);
            types[1027] = new PgTypeInfo(1027, "polygon[]", "_polygon", PgDbType.Array  , types[604], typeof(PgPolygon[]), typeof(PgPolygon[]));

            // circle | 24 bytes | Circle | <(x,y),r> (center point and radius)

            types[718] = new PgTypeInfo(718, "circle"  , "circle" , PgDbType.Circle, PgTypeFormat.Binary, typeof(PgCircle), typeof(PgCircle), 24);
            types[719] = new PgTypeInfo(719, "circle[]", "_circle", PgDbType.Array , types[718], typeof(PgCircle[]), typeof(PgCircle[]));

            //
            // POSTGIS TYPES
            //

            types[17321] = new PgTypeInfo(17321, "box3d", "box3d", PgDbType.Box3D, PgTypeFormat.Binary, typeof(PgBox3D), typeof(PgBox3D), 48);
            types[17335] = new PgTypeInfo(17335, "box2d", "box2d", PgDbType.Box2D, PgTypeFormat.Binary, typeof(PgBox2D), typeof(PgBox2D), 16);

            //
            // Network Address Types
            //

            // cidr	7 or 19 bytes	IPv4 and IPv6 networks
#warning TODO: Add ??

            // inet	| 7 or 19 bytes	| IPv4 and IPv6 hosts and networks

            types[869] = new PgTypeInfo(869, "inet", "inet", PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string));

            // macaddr | 6 bytes | MAC addresses

            types[829] = new PgTypeInfo(829, "macaddr", "macaddr", PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string), 6);

            //
            // OBJECT IDENTIFIER TYPES
            //

            types[  26] = new PgTypeInfo(  26, "oid"        , "oid"      , PgDbType.Integer, PgTypeFormat.Binary, typeof(int), typeof(PgInt32), sizeof(int));
            types[  30] = new PgTypeInfo(  30, "oidvector[]", "oidvector", PgDbType.Vector , types[26], typeof(int[]), typeof(PgInt32[]));
            types[1028] = new PgTypeInfo(1028, "oid[]"      , "_oid"     , PgDbType.Array  , types[26], typeof(int[]), typeof(PgInt32[]));

            types[1033] = new PgTypeInfo(1033, "aclitem"    , "aclitem"  , PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string));
            types[1034] = new PgTypeInfo(1034, "aclitem[]"  , "_aclitem" , PgDbType.Array  , types[1033], typeof(string), typeof(string[]));

            types[  24] = new PgTypeInfo(  24, "regproc"    , "regproc"  , PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string[]));
            types[1790] = new PgTypeInfo(1790, "refcursor"  , "refcursor", PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string[]));
            types[2205] = new PgTypeInfo(2205, "regclass"   , "regclass" , PgDbType.VarChar, PgTypeFormat.Text, typeof(string), typeof(string[]));

            //
            // PSEUDO-TYPES
            //

            types[ 705] = new PgTypeInfo( 705, "unknown", "unknown", PgDbType.Text, PgTypeFormat.Text  , typeof(string), typeof(string));
            types[2278] = new PgTypeInfo(2278, "void"   , "void"   , PgDbType.Void, PgTypeFormat.Binary, typeof(void), typeof(void), 0);

            Types = new ReadOnlyDictionary<int, PgTypeInfo>(types);
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

        internal static PgTypeInfo GetTypeInfo(PgDbType pgDbType)
        {
            return Types.Values.First(x => x.PgDbType == pgDbType);
        }

        internal static PgTypeInfo GetTypeInfo(object value)
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
