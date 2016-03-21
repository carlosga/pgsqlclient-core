// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using PostgreSql.Data.PgTypes;
using System;
using System.Data;
using PostgreSql.Data.PostgreSqlClient;

namespace PostgreSql.Data.Protocol
{
    internal static class TypeHelper
    {
        internal static DbType ProviderDbTypeToDbType(PgDbType providerType)
        {
            switch (providerType)
            {
                case PgDbType.Boolean:
                    return DbType.Boolean;

                case PgDbType.Byte:
                    return DbType.Byte;

                case PgDbType.Binary:
                    return DbType.Binary;

                case PgDbType.Char:
                case PgDbType.Text:
                case PgDbType.VarChar:
                    return DbType.String;

                case PgDbType.Int2:
                    return DbType.Int16;

                case PgDbType.Int4:
                    return DbType.Int32;

                case PgDbType.Int8:
                    return DbType.Int64;

                case PgDbType.Date:
                    return DbType.Date;

                case PgDbType.Time:
                    return DbType.Time;

                case PgDbType.Timestamp:
                    return DbType.DateTime;

                case PgDbType.TimeWithTZ:
                case PgDbType.TimestampWithTZ:
                    return DbType.DateTimeOffset;

                case PgDbType.Decimal:
                case PgDbType.Numeric:
                    return DbType.Decimal;

                case PgDbType.Float:
                    return DbType.Single;

                case PgDbType.Double:
                    return DbType.Double;

                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }
        }

        internal static PgDbType DbTypeToProviderType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.String:
                    return PgDbType.VarChar;

                case DbType.AnsiStringFixedLength:
                case DbType.StringFixedLength:
                    return PgDbType.Char;

                case DbType.Binary:
                case DbType.Object:
                    return PgDbType.Binary;

                case DbType.Boolean:
                    return PgDbType.Boolean;

                case DbType.Byte:
                    return PgDbType.Byte;

                case DbType.Currency:
                    return PgDbType.Currency;

                case DbType.Date:
                    return PgDbType.Date;

                case DbType.DateTime:
                    return PgDbType.Timestamp;

                case DbType.DateTimeOffset:
                    return PgDbType.TimestampWithTZ;

                case DbType.Decimal:
                    return PgDbType.Decimal;

                case DbType.Double:
                    return PgDbType.Double;

                case DbType.Int16:
                case DbType.UInt16:
                    return PgDbType.Int2;

                case DbType.Int32:
                case DbType.UInt32:
                    return PgDbType.Int4;

                case DbType.Int64:
                case DbType.UInt64:
                    return PgDbType.Int8;

                case DbType.Single:
                    return PgDbType.Float;

                case DbType.Time:
                    return PgDbType.Time;

                case DbType.Guid:
                case DbType.VarNumeric:
                case DbType.SByte:
                default:
                    throw new InvalidOperationException("Invalid data type specified.");
            }
        }

        internal static PgDbType GetDbProviderType(object value)
        {
            PgDbType providerType = PgDbType.VarChar;

            switch (value.GetType().GetTypeCode())
            {
                case TypeCode.Byte:
                    providerType = PgDbType.Byte;
                    break;

                case TypeCode.Boolean:
                    providerType = PgDbType.Boolean;
                    break;

                case TypeCode.Object:
                    providerType = PgDbType.Binary;
                    break;

                case TypeCode.String:
                case TypeCode.Char:
                    providerType = PgDbType.Char;
                    break;

                case TypeCode.Int16:
                    providerType = PgDbType.Int2;
                    break;

                case TypeCode.Int32:
                    providerType = PgDbType.Int4;
                    break;

                case TypeCode.Int64:
                    providerType = PgDbType.Int8;
                    break;

                case TypeCode.Single:
                    providerType = PgDbType.Float;
                    break;

                case TypeCode.Double:
                    providerType = PgDbType.Double;
                    break;

                case TypeCode.Decimal:
                    providerType = PgDbType.Decimal;
                    break;

                case TypeCode.DateTime:
                    providerType = PgDbType.Timestamp;
                    break;

                case TypeCode.Empty:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                default:
                    throw new Exception("Value is of invalid data type.");
            }

            return providerType;
        }

        internal static Type GetSystemType(PgDataType dataType)
        {
            switch (dataType)
            {
                case PgDataType.Array:
                case PgDataType.Binary:
                case PgDataType.Vector:
                    return typeof(System.Array);

                case PgDataType.Boolean:
                    return typeof(System.Boolean);

                case PgDataType.Box:
                    return typeof(PostgreSql.Data.PgTypes.PgBox);

                case PgDataType.Circle:
                    return typeof(PostgreSql.Data.PgTypes.PgCircle);

                case PgDataType.Line:
                    return typeof(PostgreSql.Data.PgTypes.PgLine);

                case PgDataType.LSeg:
                    return typeof(PostgreSql.Data.PgTypes.PgLSeg);

                case PgDataType.Path:
                    return typeof(PostgreSql.Data.PgTypes.PgPath);

                case PgDataType.Point:
                    return typeof(PostgreSql.Data.PgTypes.PgPoint);

                case PgDataType.Polygon:
                    return typeof(PostgreSql.Data.PgTypes.PgPolygon);

                case PgDataType.Byte:
                    return typeof(System.Byte);

                case PgDataType.Char:
                case PgDataType.Text:
                case PgDataType.VarChar:
                    return typeof(System.String);

                case PgDataType.Currency:
                case PgDataType.Decimal:
                case PgDataType.Numeric:
                    return typeof(System.Decimal);

                case PgDataType.Date:
                case PgDataType.Time:
                case PgDataType.TimeWithTZ:
                case PgDataType.Timestamp:
                case PgDataType.TimestampWithTZ:
                    return typeof(System.DateTime);

                case PgDataType.Double:
                    return typeof(System.Double);

                case PgDataType.Float:
                    return typeof(System.Single);

                case PgDataType.Int2:
                    return typeof(System.Int16);

                case PgDataType.Int4:
                    return typeof(System.Int32);

                case PgDataType.Int8:
                    return typeof(System.Int64);

                case PgDataType.Refcursor:
                    return typeof(DataTable);

                default:
                    return typeof(System.Object);
            }
        }

        internal static string ConvertToProviderString(PgDbType providerType, object value)
        {
            string returnValue = String.Empty;

            switch (providerType)
            {
                case PgDbType.Array:
                    break;

                case PgDbType.Binary:
                    break;

                case PgDbType.Boolean:
                    returnValue = Convert.ToBoolean(value).ToString().ToLower();
                    break;

                case PgDbType.Box:
                    returnValue = ((PgBox)value).ToString();
                    break;

                case PgDbType.Byte:
                    returnValue = Convert.ToByte(value).ToString();
                    break;

                case PgDbType.Char:
                case PgDbType.VarChar:
                case PgDbType.Text:
                    returnValue = Convert.ToString(value);
                    break;

                case PgDbType.Circle:
                    returnValue = ((PgCircle)value).ToString();
                    break;

                case PgDbType.Currency:
                    returnValue = "$" + Convert.ToSingle(value).ToString();
                    break;

                case PgDbType.Date:
                    returnValue = Convert.ToDateTime(value).ToString("MM/dd/yyyy");
                    break;

                case PgDbType.Decimal:
                case PgDbType.Numeric:
                    returnValue = Convert.ToDecimal(value).ToString();
                    break;

                case PgDbType.Double:
                    returnValue = Convert.ToDouble(value).ToString();
                    break;

                case PgDbType.Float:
                    returnValue = Convert.ToSingle(value).ToString();
                    break;

                case PgDbType.Int2:
                    returnValue = Convert.ToInt16(value).ToString();
                    break;

                case PgDbType.Int4:
                    returnValue = Convert.ToInt32(value).ToString();
                    break;

                case PgDbType.Int8:
                    returnValue = Convert.ToInt64(value).ToString();
                    break;

                case PgDbType.Interval:
                    break;

                case PgDbType.Line:
                    returnValue = ((PgLine)value).ToString();
                    break;

                case PgDbType.LSeg:
                    returnValue = ((PgLSeg)value).ToString();
                    break;

                case PgDbType.Path:
                    returnValue = ((PgPath)value).ToString();
                    break;

                case PgDbType.Point:
                    returnValue = ((PgPoint)value).ToString();
                    break;

                case PgDbType.Polygon:
                    returnValue = ((PgPolygon)value).ToString();
                    break;

                case PgDbType.Time:
                    returnValue = Convert.ToDateTime(value).ToString("HH:mm:ss");
                    break;

                case PgDbType.Timestamp:
                    returnValue = Convert.ToDateTime(value).ToString("MM/dd/yyy HH:mm:ss");
                    break;

                case PgDbType.TimestampWithTZ:
                    returnValue = Convert.ToDateTime(value).ToString("MM/dd/yyy HH:mm:ss zz");
                    break;

                case PgDbType.TimeWithTZ:
                    returnValue = Convert.ToDateTime(value).ToString("HH:mm:ss zz");
                    break;

                case PgDbType.Vector:
                    break;

                default:
                    returnValue = value.ToString();
                    break;
            }

            return $"'{returnValue}'";
        }
    }
}
