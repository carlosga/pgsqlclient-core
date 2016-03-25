// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.Protocol
{
    // This is a exact copy of PgDbType enum for
    // allow a better and more simple handling of
    // data types in the protocol implementation.
    internal enum PgDataType
    {
        Array
      , Binary
      , Boolean
      , Box
      , Box2D
      , Box3D
      , Byte
      , Char
      , Circle
      , Currency
      , Date
      , Decimal
      , Double
      , Float
      , Int2
      , Int4
      , Int8
      , Interval
      , Line
      , LSeg
      , Numeric
      , Path
      , Point
      , Polygon
      , Refcursor
      , Text
      , Time
      , TimeWithTZ
      , Timestamp
      , TimestampWithTZ
      , VarChar
      , Vector
      , Void
    }
}
