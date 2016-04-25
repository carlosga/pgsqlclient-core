// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.PgTypes
{
    public enum Oid : int
    {
        Byte        = 18
      , Bytea       = 17
      , Bytea_      = 1001
      , Bit         = 1560
      , VarBit      = 1562
      , Boolean     = 16
      , Boolean_    = 1000
      , Varchar     = 1043
      , Varchar_    = 1015
      , Char        = 1002
      , Text        = 25
      , Text_       = 1009
      , Timestamp   = 1114
      , Timestamp_  = 1115
      , TimestampTZ = 1184
      , TimestampTZ_= 1185
      , Date        = 1082
      , Date_       = 1182
      , Time        = 1083
      , Time_       = 1183
      , TimeTZ      = 1266
      , TimeTZ_     = 1270
      , Interval    = 1186
      , Interval_   = 1187
      , SmallInt    = 21
      , SmallInt_   = 22
      , SmallInt__  = 1005
      , Integer     = 23
      , Integer_    = 1007
      , BigInt      = 20
      , BigInt_     = 1016
      , Numeric     = 1700
      , Numeric_    = 1231
      , Real        = 700
      , Real_       = 1021
      , Double      = 701
      , Double_     = 1021
      , Money       = 790
      , Money_      = 791
      , Name        = 19
      , BpChar      = 1042
      , BpChar_     = 1014
      , Point       = 600
      , Point_      = 1017
      , Line        = 628
      , Line_       = 629
      , LSeg        = 601
      , LSeg_       = 1018
      , Box         = 603
      , Box_        = 1020
      , Path        = 602
      , Path_       = 1019
      , Polygon     = 604
      , Polygon_    = 1027
      , Circle      = 718
      , Circle_     = 719
      , Box3d       = 17321
      , Box2d       = 17335
      , Inet        = 869
      , MacAddr     = 829
      , Oid         = 26
      , Oid_        = 30
      , Oid__       = 1028
      , AclItem     = 1033
      , AclItem_     = 1034
      , RegProc     = 24
      , RefCursor   = 1790
      , RegClass    = 2205
      , Unknown     = 705
      , Void        = 2278
    }
}
