// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using PostgreSql.Data.Schema;
using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;

namespace PostgreSql.Data.Frontend
{
    internal sealed class TypeInfoProvider
    {
        internal static readonly string          NullString       = "Null";
        internal static readonly IFormatProvider InvariantCulture = System.Globalization.CultureInfo.InvariantCulture.NumberFormat;
        internal static bool     IsNullString(string s) => (s == null || s == NullString);

        internal static readonly ReadOnlyDictionary<int, TypeInfo> BaseTypes;

        static TypeInfoProvider()
        {
            Dictionary<int, TypeInfo> types = new Dictionary<int, TypeInfo>(100);

            //
            // CHAR TYPE
            //

            types[18] = new TypeInfo(18, "char", PgDbType.Char, typeof(char), typeof(char));

            //
            // BINARY DATA TYPES
            //

            types[17  ] = new TypeInfo(  17, "bytea"  , PgDbType.Bytea, typeof(byte[]), typeof(PgBinary));
            types[1001] = new TypeInfo(1001, "bytea[]", PgDbType.Array, types[17], typeof(byte[][]), typeof(PgBinary[]));

            //
            // BIT STRING TYPES
            //

            types[1560] = new TypeInfo(1560, "bit"   , PgDbType.Bit, typeof(byte), typeof(PgBit), sizeof(byte));
            types[1562] = new TypeInfo(1562, "varbit", PgDbType.Bit, types[1560], typeof(byte[]), typeof(PgBit));

            //
            // BOOLEAN TYPE
            //

            // boolean | 1 byte | state of true or false

            types[  16] = new TypeInfo(  16, "bool"  , PgDbType.Boolean, typeof(bool), typeof(PgBoolean), sizeof(bool));
            types[1000] = new TypeInfo(1000, "bool[]", PgDbType.Array  , types[16], typeof(bool[]), typeof(PgBoolean[]));

            //
            // CHARACTER TYPES
            //

            // character varying(n), varchar(n) | variable-length with limit

            types[1043] = new TypeInfo(1043, "varchar"  , PgDbType.VarChar, typeof(string), typeof(string));
            types[1015] = new TypeInfo(1015, "varchar[]", PgDbType.Array  , types[1043], typeof(string[]), typeof(string[]));

            // character(n), char(n) | fixed-length, blank padded

            types[1002] = new TypeInfo(1002, "char", PgDbType.Array, typeof(char[]), typeof(string));

            // text	variable unlimited length

            types[  25] = new TypeInfo(  25, "text"  , PgDbType.Text , typeof(string), typeof(string));
            types[1009] = new TypeInfo(1009, "text[]", PgDbType.Array, types[25], typeof(string[]), typeof(string[]));

            //
            // DATE/TIME TYPES
            //

            // timestamp [ (p) ] [ without time zone ] | 8 bytes | both date and time (no time zone) | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1114] = new TypeInfo(1114, "timestamp"  , PgDbType.Timestamp, typeof(DateTime), typeof(PgTimestamp), 8);
            types[1115] = new TypeInfo(1115, "timestamp[]", PgDbType.Array    , types[1114], typeof(DateTime[]), typeof(PgTimestamp[]));

            // timestamp [ (p) ] with time zone | 8 bytes | both date and time, with time zone | 4713 BC | 294276 AD | 1 microsecond / 14 digits

            types[1184] = new TypeInfo(1184, "timestamptz"  , PgDbType.TimestampTZ, typeof(DateTimeOffset), typeof(PgTimestamp), 8);
            types[1185] = new TypeInfo(1185, "timestamptz[]", PgDbType.Array      , types[1184], typeof(DateTimeOffset[]), typeof(PgTimestamp[]));

            // date	| 4 bytes | date (no time of day) | 4713 BC | 5874897 AD | 1 day

            types[1082] = new TypeInfo(1082, "date"  , PgDbType.Date , typeof(DateTime), typeof(PgDate), 4);
            types[1182] = new TypeInfo(1182, "date[]", PgDbType.Array, types[1082], typeof(DateTime[]), typeof(PgDate[]));

            // time [ (p) ] [ without time zone ] | 8 bytes | time of day (no date) | 00:00:00 | 24:00:00 | 1 microsecond / 14 digits

            types[1083] = new TypeInfo(1083, "time"  , PgDbType.Time , typeof(TimeSpan), typeof(PgTime), 8);
            types[1183] = new TypeInfo(1183, "time[]", PgDbType.Array, types[1083], typeof(TimeSpan[]), typeof(PgTime[]));
            
            // time [ (p) ] with time zone | 12 bytes | times of day only, with time zone | 00:00:00+1459 | 24:00:00-1459 | 1 microsecond / 14 digits

            types[1266] = new TypeInfo(1266, "timetz"  , PgDbType.TimeTZ, typeof(DateTimeOffset), typeof(PgTime), 12);
            types[1270] = new TypeInfo(1270, "timetz[]", PgDbType.Array , types[1266], typeof(DateTimeOffset[]), typeof(PgTime));

            // interval [ fields ] [ (p) ] | 16 bytes | time interval | -178000000 years | 178000000 years | 1 microsecond / 14 digits

            types[1186] = new TypeInfo(1186, "interval"  , PgDbType.Interval, typeof(TimeSpan), typeof(PgInterval), 16);
            types[1187] = new TypeInfo(1187, "interval[]", PgDbType.Array   , types[1186], typeof(TimeSpan[]), typeof(PgInterval));

            //
            // NUMERIC TYPES
            // 

            // smallint	| 2 bytes | small-range integer | -32768 to +32767

            types[  21] = new TypeInfo(  21, "smallint"  , PgDbType.SmallInt, typeof(short), typeof(PgInt16), sizeof(short));
            types[  22] = new TypeInfo(  22, "smallint[]", PgDbType.Vector  , types[21], typeof(short[]), typeof(PgInt16[]));
            types[1005] = new TypeInfo(1005, "smallint[]", PgDbType.Array   , types[21], typeof(short[]), typeof(PgInt16[]));

            // integer | 4 bytes | typical choice for integer | -2147483648 to +2147483647

            types[  23] = new TypeInfo(  23, "integer"  , PgDbType.Integer, typeof(int), typeof(PgInt32), sizeof(int));
            types[1007] = new TypeInfo(1007, "integer[]", PgDbType.Array  , types[23], typeof(int[]), typeof(PgInt32[]));

            // bigint | 8 bytes | large-range integer | -9223372036854775808 to +9223372036854775807

            types[  20] = new TypeInfo(  20, "bigint"  , PgDbType.BigInt, typeof(long), typeof(PgInt64), sizeof(long));
            types[1016] = new TypeInfo(1016, "bigint[]", PgDbType.Array , types[20], typeof(long[]), typeof(PgInt64[]));

            // decimal | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point
            // numeric | variable | user-specified precision, exact	up to 131072 digits before the decimal point; up to 16383 digits after the decimal point

            types[1700] = new TypeInfo(1700, "numeric"  , PgDbType.Numeric, typeof(decimal), typeof(PgNumeric));
            types[1231] = new TypeInfo(1231, "numeric[]", PgDbType.Array  , types[1700], typeof(decimal[]), typeof(PgNumeric[]));

            // real | 4 bytes | variable-precision, inexact	6 decimal digits precision

            types[ 700] = new TypeInfo( 700, "real"  , PgDbType.Real , typeof(float), typeof(PgReal), sizeof(float));
            types[1021] = new TypeInfo(1021, "real[]", PgDbType.Array, types[700], typeof(float[]), typeof(PgReal));

            // double precision | 8 bytes | variable-precision, inexact	15 decimal digits precision

            types[ 701] = new TypeInfo( 701, "double"  , PgDbType.Double, typeof(double), typeof(PgDouble), sizeof(double));
            types[1021] = new TypeInfo(1021, "double[]", PgDbType.Array , types[701], typeof(double[]), typeof(PgDouble[]));

            //
            // MONETARY TYPES
            //

            // money | 8 bytes | currency amount | -92233720368547758.08 to +92233720368547758.07

            types[790] = new TypeInfo(790, "money"  , PgDbType.Money, typeof(decimal), typeof(PgMoney), 8);
            types[791] = new TypeInfo(791, "money[]", PgDbType.Array, types[790], typeof(decimal[]), typeof(PgMoney[]));

            //
            // SPECIAL CHARACTER TYPES
            //

            types[  19] = new TypeInfo(  19, "name"    , PgDbType.VarChar, typeof(string), typeof(string));
            types[1042] = new TypeInfo(1042, "bpchar"  , PgDbType.Char   , typeof(string), typeof(string));
            types[1014] = new TypeInfo(1014, "bpchar[]", PgDbType.Char   , types[1042], typeof(string[]), typeof(string[]));

            //
            // GEOMETRIC TYPES
            //

            // point | 16 bytes | Point on a plane (x,y)

            types[ 600] = new TypeInfo( 600, "point"  , PgDbType.Point, typeof(PgPoint), typeof(PgPoint), 16);
            types[1017] = new TypeInfo(1017, "point[]", PgDbType.Array, types[600], typeof(PgPoint[]), typeof(PgPoint[]));

            // line	| 32 bytes | Infinite line {A,B,C}

            types[628] = new TypeInfo(628, "line"  , PgDbType.Line , typeof(PgLine), typeof(PgLine), 32);
            types[629] = new TypeInfo(629, "line[]", PgDbType.Array, types[628], typeof(PgLine[]), typeof(PgLine[]));

            // lseg	| 32 bytes | Finite line segment | ((x1,y1),(x2,y2))

            types[ 601] = new TypeInfo( 601, "lseg"  , PgDbType.LSeg , typeof(PgLSeg), typeof(PgLSeg), 32);
            types[1018] = new TypeInfo(1018, "lseg[]", PgDbType.Array, types[601], typeof(PgLSeg[]), typeof(PgLSeg[]));

            // box | 32 bytes | Rectangular box | ((x1,y1),(x2,y2))

            types[ 603] = new TypeInfo( 603, "box"  , PgDbType.Box  , typeof(PgBox), typeof(PgBox), 32);
            types[1020] = new TypeInfo(1020, "box[]", PgDbType.Array, types[603], typeof(PgBox[]), typeof(PgBox[]));

            // path | 16+16n bytes | Closed path (similar to polygon) |	((x1,y1),...)
            // path | 16+16n bytes | Open path                        | [(x1,y1),...]

            types[ 602] = new TypeInfo( 602, "path"  , PgDbType.Path , typeof(PgPath), typeof(PgPath));
            types[1019] = new TypeInfo(1019, "path[]", PgDbType.Array, types[602], typeof(PgPath[]), typeof(PgPath[]));

            // polygon | 40+16n bytes | Polygon (similar to closed path) | ((x1,y1),...)

            types[ 604] = new TypeInfo( 604, "polygon"  , PgDbType.Polygon, typeof(PgPolygon), typeof(PgPolygon));
            types[1027] = new TypeInfo(1027, "polygon[]", PgDbType.Array  , types[604], typeof(PgPolygon[]), typeof(PgPolygon[]));

            // circle | 24 bytes | Circle | <(x,y),r> (center point and radius)

            types[718] = new TypeInfo(718, "circle"  , PgDbType.Circle, typeof(PgCircle), typeof(PgCircle), 24);
            types[719] = new TypeInfo(719, "circle[]", PgDbType.Array , types[718], typeof(PgCircle[]), typeof(PgCircle[]));

            //
            // POSTGIS TYPES
            //

            types[17321] = new TypeInfo(17321, "box3d", PgDbType.Box3D, typeof(PgBox3D), typeof(PgBox3D), 48);
            types[17335] = new TypeInfo(17335, "box2d", PgDbType.Box2D, typeof(PgBox2D), typeof(PgBox2D), 16);

            //
            // Network Address Types
            //

            // inet	| 7 or 19 bytes	| IPv4 and IPv6 hosts and networks

            types[869] = new TypeInfo(869, "inet", PgDbType.IPAddress, typeof(IPAddress), typeof(IPAddress));

            // cidr	| 7 or 19 bytes	| IPv4 and IPv6 hosts and networks

            // types[650] = new TypeInfo(650, "cidr", PgDbType.IPAddress, typeof(IPAddress), typeof(IPAddress));

            // macaddr | 6 bytes | MAC addresses

            types[829] = new TypeInfo(829, "macaddr", PgDbType.MacAddress, typeof(PhysicalAddress), typeof(PhysicalAddress), 6);

            //
            // OBJECT IDENTIFIER TYPES
            //

            types[  26] = new TypeInfo(  26, "oid"  , PgDbType.Integer, typeof(int), typeof(PgInt32), sizeof(int));
            types[  30] = new TypeInfo(  30, "oid[]", PgDbType.Vector , types[26], typeof(int[]), typeof(PgInt32[]));
            types[1028] = new TypeInfo(1028, "oid[]", PgDbType.Array  , types[26], typeof(int[]), typeof(PgInt32[]));

            types[1033] = new TypeInfo(1033, "aclitem"  , PgDbType.VarChar, typeof(string), typeof(string));
            types[1034] = new TypeInfo(1034, "aclitem[]", PgDbType.Array  , types[1033], typeof(string), typeof(string[]));

            types[  24] = new TypeInfo(  24, "regproc"  , PgDbType.VarChar, typeof(string), typeof(string[]));
            types[1790] = new TypeInfo(1790, "refcursor", PgDbType.VarChar, typeof(string), typeof(string[]));
            types[2205] = new TypeInfo(2205, "regclass" , PgDbType.VarChar, typeof(string), typeof(string[]));

            //
            // Uuid
            // 
            types[2950] = new TypeInfo(2950, "uuid"  , PgDbType.Uuid, typeof(Guid), typeof(Guid), 16);
            types[2951] = new TypeInfo(2951, "uuid[]", PgDbType.Array, types[2950], typeof(Guid[]), typeof(Guid[]));

            //
            // PSEUDO-TYPES
            //

            types[ 705] = new TypeInfo( 705, "unknown", PgDbType.Text, typeof(string), typeof(string));
            types[2278] = new TypeInfo(2278, "void"   , PgDbType.Void, typeof(void), typeof(void));

            //
            // INTERNAL TYPES
            //
            types[194] = new TypeInfo(194, "pg_node_tree", PgDbType.Text, typeof(string), typeof(string));

            //
            // Enum types
            //
            types[3500]  = new TypeInfo( 3500, "anyenum", PgDbType.Enum, typeof(Enum), typeof(Enum));
            types[11483] = new TypeInfo(11483, "pg_enum", PgDbType.Enum, typeof(Enum), typeof(Enum));

            BaseTypes = new ReadOnlyDictionary<int, TypeInfo>(types);
        }

        internal static DbType GetDbType(PgDbType providerType)
        {
            switch (providerType)
            {
                case PgDbType.Boolean:
                    return DbType.Boolean;

                case PgDbType.Bytea:
                    return DbType.Binary;

                case PgDbType.Char:
                case PgDbType.Text:
                case PgDbType.VarChar:
                    return DbType.String;

                case PgDbType.Bit:
                    return DbType.Byte;

                case PgDbType.SmallInt:
                    return DbType.Int16;

                case PgDbType.Integer:
                    return DbType.Int32;

                case PgDbType.BigInt:
                    return DbType.Int64;

                case PgDbType.Date:
                    return DbType.Date;

                case PgDbType.Double:
                    return DbType.Double;

                case PgDbType.Money:
                    return DbType.Currency;

                case PgDbType.Numeric:
                    return DbType.Decimal;

                case PgDbType.Real:
                    return DbType.Single;

                case PgDbType.Time:
                    return DbType.Time;

                case PgDbType.Timestamp:
                    return DbType.DateTime;

                case PgDbType.TimeTZ:
                case PgDbType.TimestampTZ:
                    return DbType.DateTimeOffset;

                case PgDbType.Uuid:
                    return DbType.Guid;

                case PgDbType.IPAddress:
                case PgDbType.MacAddress:
                case PgDbType.Enum:
                case PgDbType.Void:
                    return DbType.Object;

                default:
                    throw new NotSupportedException("Invalid data type specified.");
            }
        }

        internal static PgDbType GetProviderType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                    return PgDbType.VarChar;

                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return PgDbType.Char;

                case DbType.Boolean:
                    return PgDbType.Boolean;

                case DbType.Binary:
                    return PgDbType.Bytea;

                case DbType.Byte:
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

                case DbType.Object:
                    return PgDbType.Composite;

                case DbType.Guid:
                    return PgDbType.Uuid;

                // case DbType.VarNumeric:
                // case DbType.SByte:
                // case DbType.UInt16:
                // case DbType.UInt32:
                // case DbType.UInt64:
                default:
                    throw ADP.DbTypeNotSupported(dbType.ToString());
            }
        }

        internal static TypeInfo GetTypeInfo(PgDbType pgDbType)
            => BaseTypes.Values.First(x => x.PgDbType == pgDbType);

        internal static TypeInfo GetTypeInfo(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return BaseTypes[Oid.Unknown];
            }
            if (value is INullable)
            {
                return BaseTypes.Values.First(x => x.PgType == value.GetType());
            }
            var typeInfo = BaseTypes.Values.FirstOrDefault(x => x.SystemType == value.GetType());
            if (typeInfo == null)
            {
                typeInfo = BaseTypes[Oid.Unknown];
            }
            return typeInfo;
        }

        internal static TypeInfo GetArrayTypeInfo(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                throw new PgNullValueException();
            }
            if (value is INullable)
            {
                return BaseTypes.Values.First(x => x.PgDbType == PgDbType.Array && x.PgType == value.GetType());
            }
            var typeInfo = BaseTypes.Values.First(x => x.PgDbType == PgDbType.Array && x.SystemType == value.GetType());
            if (typeInfo == null)
            {
                typeInfo = BaseTypes[Oid.Unknown];
            }
            return typeInfo;
        }

        internal static TypeInfo GetVectorTypeInfo(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                throw new PgNullValueException();
            }
            if (value is INullable)
            {
                return BaseTypes.Values.First(x => x.PgDbType == PgDbType.Vector && x.PgType == value.GetType());
            }
            var typeInfo = BaseTypes.Values.First(x => x.PgDbType == PgDbType.Vector && x.SystemType == value.GetType());
            if (typeInfo == null)
            {
                typeInfo = BaseTypes[Oid.Unknown];
            }
            return typeInfo;
        }

        private ReadOnlyDictionary<int, TypeInfo> _types;
        private DbConnectionOptions               _connectionOptions;
        private int                               _count;
        private SemaphoreSlim                     _activeSemaphore;
        
        private SemaphoreSlim LazyEnsureActiveSemaphoreInitialized()
        {
            return LazyInitializer.EnsureInitialized(ref _activeSemaphore, () => new SemaphoreSlim(1, 1));
        }        

        internal TypeInfoProvider(DbConnectionOptions connectionOptions)
        {
            _types             = null;
            _count             = 0;
            _connectionOptions = connectionOptions;
        }

        internal int AddRef() => Interlocked.Increment(ref _count);

        internal int Release()
        {
            if (Interlocked.Exchange(ref _count, 0) != 0)
            {
                if (Interlocked.Decrement(ref _count) == 0)
                {
                    _types             = null;
                    _connectionOptions = null;
                }
            }
            return _count;
        }

        internal TypeInfo GetTypeInfo(int oid)
        {
            if (BaseTypes.ContainsKey(oid))
            {
                return BaseTypes[oid];
            }
            if (_types == null)
            {               
                var sem = LazyEnsureActiveSemaphoreInitialized();
                sem.Wait();

                if (_types == null)
                {
                    _types = DiscoverTypes(_connectionOptions);                    
                }
                
                sem.Release();
            }
            if (_types.ContainsKey(oid))
            {
                return _types[oid];
            }
            
            throw new NotSupportedException($"Data Type with OID='{oid}' is not supported");
        }

        private static ReadOnlyDictionary<int, TypeInfo> DiscoverTypes(DbConnectionOptions connectionOptions)
        {
            var types = new Dictionary<int, TypeInfo>(10);

            using (var connection = new Connection(connectionOptions))
            {
                connection.Open();

                var compositeProvider = new CompositeTypeInfoProvider(connection);
                var enumProvider      = new EnumTypeInfoProvider(connection);
                
                compositeProvider.GetTypeInfo(ref types);
                enumProvider.GetTypeInfo(ref types);
            }

            return new ReadOnlyDictionary<int, TypeInfo>(types);
        }
    }
}
