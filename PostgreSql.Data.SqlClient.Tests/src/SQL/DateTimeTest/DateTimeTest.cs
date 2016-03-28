// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using NUnit.Framework;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    [TestFixture]
    public static class DateTimeTest
    {
        [Test]
        public static void SelectNullTimestampWithTZ()
        {
            using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
            {
                conn.Open();
                PgParameter p = new PgParameter("@p", PgDbType.TimestampTZ);
                p.Value = DBNull.Value;
                p.Size  = 27;
                PgCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT @p";
                cmd.Parameters.Add(p);

                Assert.True(cmd.ExecuteScalar() is DBNull, "FAILED: ExecuteScalar did not return a result of type DBNull");
            }
        }

        // [Test]
        // public static void ReaderParameterTest()
        // {
        //     string tempTable  = "#t_" + Guid.NewGuid().ToString().Replace('-', '_');
        //     string tempProc   = "#p_" + Guid.NewGuid().ToString().Replace('-', '_');
        //     string tempProcN  = "#pn_" + Guid.NewGuid().ToString().Replace('-', '_');
        //     string prepTable1 = "CREATE TABLE " + tempTable + " (ci int, c0 dateTime, c1 date, c2 time(7), c3 datetime2(3), c4 timestamp with timezone)";
        //     string prepTable2 = "INSERT INTO " + tempTable + " VALUES (0, " +
        //         "'1753-01-01 12:00AM', " +
        //         "'1753-01-01', " +
        //         "'20:12:13.36', " +
        //         "'2000-12-31 23:59:59.997', " +
        //         "'9999-12-31 15:59:59.997 -08:00')";
        //     string prepTable3 = "INSERT INTO " + tempTable + " VALUES (@pi, @p0, @p1, @p2, @p3, @p4)";
        //     string prepProc = "CREATE PROCEDURE " + tempProc + " @p0 datetime OUTPUT, @p1 date OUTPUT, @p2 time(7) OUTPUT, @p3 datetime2(3) OUTPUT, @p4 datetimeoffset OUTPUT";
        //     prepProc += " AS ";
        //     prepProc += " SET @p0 = '1753-01-01 12:00AM'";
        //     prepProc += " SET @p1 = '1753-01-01'";
        //     prepProc += " SET @p2 = '20:12:13.36'";
        //     prepProc += " SET @p3 = '2000-12-31 23:59:59.997'";
        //     prepProc += "SET @p4 = '9999-12-31 15:59:59.997 -08:00'";
        //     string prepProcN = "CREATE PROCEDURE " + tempProcN + " @p0 datetime OUTPUT, @p1 date OUTPUT, @p2 time(7) OUTPUT, @p3 datetime2(3) OUTPUT, @p4 datetimeoffset OUTPUT";
        //     prepProcN += " AS ";
        //     prepProcN += " SET @p0 = NULL";
        //     prepProcN += " SET @p1 = NULL";
        //     prepProcN += " SET @p2 = NULL";
        //     prepProcN += " SET @p3 = NULL";
        //     prepProcN += " SET @p4 = NULL";

        //     using (PgConnection conn = new PgConnection(DataTestClass.PostgreSql9_Northwind))
        //     {
        //         // ReaderParameterTest Setup
        //         conn.Open();
        //         PgCommand cmd = conn.CreateCommand();
        //         cmd.CommandText = prepTable1;
        //         cmd.ExecuteNonQuery();
        //         cmd.CommandText = prepTable2;
        //         cmd.ExecuteNonQuery();

        //         #region parameter
        //         // Parameter Tests
        //         // Test 1
        //         PgCommand cmd2 = conn.CreateCommand();
        //         cmd2.CommandText = prepTable3;
        //         PgParameter pi = cmd2.Parameters.Add("@pi", PgDbType.Int);
        //         PgParameter p0 = cmd2.Parameters.Add("@p0", PgDbType.DateTime);
        //         PgParameter p1 = cmd2.Parameters.Add("@p1", PgDbType.Date);
        //         PgParameter p2 = cmd2.Parameters.Add("@p2", PgDbType.Time);
        //         PgParameter p3 = cmd2.Parameters.Add("@p3", PgDbType.DateTime2);
        //         PgParameter p4 = cmd2.Parameters.Add("@p4", PgDbType.DateTimeOffset);
        //         pi.Value = DBNull.Value;
        //         p0.Value = DBNull.Value;
        //         p1.Value = DBNull.Value;
        //         p2.Value = DBNull.Value;
        //         p3.Value = DBNull.Value;
        //         p4.Value = DBNull.Value;
        //         p3.Scale = 7;
        //         cmd2.ExecuteNonQuery();
        //         pi.Value = 1;
        //         p0.Value = new DateTime(2000, 12, 31);
        //         p1.Value = new DateTime(2000, 12, 31);
        //         p2.Value = new TimeSpan(23, 59, 59);
        //         p3.Value = new DateTime(2000, 12, 31);
        //         p4.Value = new DateTimeOffset(2000, 12, 31, 23, 59, 59, new TimeSpan(-8, 0, 0));
        //         p3.Scale = 3;
        //         cmd2.ExecuteNonQuery();

        //         // Test 2
        //         cmd2.CommandText = "SELECT COUNT(*) FROM " + tempTable + " WHERE @pi = ci AND @p0 = c0 AND @p1 = c1 AND @p2 = c2 AND @p3 = c3 AND @p4 = c4";
        //         pi.Value = 0;
        //         p0.Value = new DateTime(1753, 1, 1, 0, 0, 0);
        //         p1.Value = new DateTime(1753, 1, 1, 0, 0, 0);
        //         p2.Value = new TimeSpan(0, 20, 12, 13, 360);
        //         p3.Value = new DateTime(2000, 12, 31, 23, 59, 59, 997);
        //         p4.Value = new DateTimeOffset(9999, 12, 31, 23, 59, 59, 997, TimeSpan.Zero);
        //         p4.Scale = 3;
        //         object scalarResult = cmd2.ExecuteScalar();
        //         Assert.True(scalarResult.Equals(1), string.Format("FAILED: Execute scalar returned unexpected result. Expected: {0}. Actual: {1}.", 1, scalarResult));

        //         cmd2.Parameters.Clear();
        //         pi = cmd2.Parameters.Add("@pi", PgDbType.Int);
        //         p0 = cmd2.Parameters.Add("@p0", PgDbType.DateTime);
        //         p1 = cmd2.Parameters.Add("@p1", PgDbType.Date);
        //         p2 = cmd2.Parameters.Add("@p2", PgDbType.Time);
        //         p3 = cmd2.Parameters.Add("@p3", PgDbType.DateTime2);
        //         p4 = cmd2.Parameters.Add("@p4", PgDbType.DateTimeOffset);
        //         pi.SqlValue = new SqlInt32(0);
        //         p0.SqlValue = new SqlDateTime(1753, 1, 1, 0, 0, 0);
        //         p1.SqlValue = new DateTime(1753, 1, 1, 0, 0, 0);
        //         p2.SqlValue = new TimeSpan(0, 20, 12, 13, 360);
        //         p3.SqlValue = new DateTime(2000, 12, 31, 23, 59, 59, 997);
        //         p4.SqlValue = new DateTimeOffset(9999, 12, 31, 23, 59, 59, 997, TimeSpan.Zero);
        //         p2.Scale = 3;
        //         p3.Scale = 3;
        //         p4.Scale = 3;
        //         scalarResult = cmd2.ExecuteScalar();
        //         Assert.True(scalarResult.Equals(1), string.Format("FAILED: ExecutScalar returned unexpected result. Expected: {0}. Actual: {1}.", 1, scalarResult));

        //         // Test 3
        //         cmd.CommandText = prepProc;
        //         cmd.ExecuteNonQuery();
        //         cmd.CommandText = prepProcN;
        //         cmd.ExecuteNonQuery();
        //         PgCommand cmd3 = conn.CreateCommand();
        //         cmd3.CommandType = CommandType.StoredProcedure;
        //         cmd3.CommandText = tempProc;
        //         p0 = cmd3.Parameters.Add("@p0", PgDbType.DateTime);
        //         p1 = cmd3.Parameters.Add("@p1", PgDbType.Date);
        //         p2 = cmd3.Parameters.Add("@p2", PgDbType.Time);
        //         p3 = cmd3.Parameters.Add("@p3", PgDbType.DateTime2);
        //         p4 = cmd3.Parameters.Add("@p4", PgDbType.DateTimeOffset);
        //         p0.Direction = ParameterDirection.Output;
        //         p1.Direction = ParameterDirection.Output;
        //         p2.Direction = ParameterDirection.Output;
        //         p3.Direction = ParameterDirection.Output;
        //         p4.Direction = ParameterDirection.Output;
        //         p2.Scale = 7;
        //         cmd3.ExecuteNonQuery();

        //         Assert.True(p0.Value.Equals((new SqlDateTime(1753, 1, 1, 0, 0, 0)).Value), "FAILED: PgParameter p0 contained incorrect value");
        //         Assert.True(p1.Value.Equals(new DateTime(1753, 1, 1, 0, 0, 0)), "FAILED: PgParameter p1 contained incorrect value");
        //         Assert.True(p2.Value.Equals(new TimeSpan(0, 20, 12, 13, 360)), "FAILED: PgParameter p2 contained incorrect value");
        //         Assert.True(p2.Scale.Equals(7), "FAILED: PgParameter p0 contained incorrect scale");
        //         Assert.True(p3.Value.Equals(new DateTime(2000, 12, 31, 23, 59, 59, 997)), "FAILED: PgParameter p3 contained incorrect value");
        //         Assert.True(p3.Scale.Equals(7), "FAILED: PgParameter p3 contained incorrect scale");
        //         Assert.True(p4.Value.Equals(new DateTimeOffset(9999, 12, 31, 23, 59, 59, 997, TimeSpan.Zero)), "FAILED: PgParameter p4 contained incorrect value");
        //         Assert.True(p4.Scale.Equals(7), "FAILED: PgParameter p4 contained incorrect scale");

        //         // Test 4
        //         cmd3.CommandText = tempProcN;
        //         cmd3.ExecuteNonQuery();
        //         Assert.True(p0.Value.Equals(DBNull.Value), "FAILED: PgParameter p0 expected to be NULL");
        //         Assert.True(p1.Value.Equals(DBNull.Value), "FAILED: PgParameter p1 expected to be NULL");
        //         Assert.True(p2.Value.Equals(DBNull.Value), "FAILED: PgParameter p2 expected to be NULL");
        //         Assert.True(p3.Value.Equals(DBNull.Value), "FAILED: PgParameter p3 expected to be NULL");
        //         Assert.True(p4.Value.Equals(DBNull.Value), "FAILED: PgParameter p4 expected to be NULL");

        //         // Date
        //         Assert.False(IsValidParam(PgDbType.Date, "c1", new DateTimeOffset(1753, 1, 1, 0, 0, 0, TimeSpan.Zero), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Date, "c1", new SqlDateTime(1753, 1, 1, 0, 0, 0), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Date, "c1", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Date, "c1", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Date, "c1", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Local), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Date, "c1", new TimeSpan(), conn, tempTable), "FAILED: Invalid param for Date PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Date, "c1", "1753-1-1", conn, tempTable), "FAILED: Invalid param for Date PgDbType");

        //         // Time
        //         Assert.False(IsValidParam(PgDbType.Time, "c2", new DateTimeOffset(1753, 1, 1, 0, 0, 0, TimeSpan.Zero), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Time, "c2", new SqlDateTime(1753, 1, 1, 0, 0, 0), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Time, "c2", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Unspecified), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Time, "c2", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Utc), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.False(IsValidParam(PgDbType.Time, "c2", new DateTime(1753, 1, 1, 0, 0, 0, DateTimeKind.Local), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Time, "c2", TimeSpan.Parse("20:12:13.36"), conn, tempTable), "FAILED: Invalid param for Time PgDbType");
        //         Assert.True(IsValidParam(PgDbType.Time, "c2", "20:12:13.36", conn, tempTable), "FAILED: Invalid param for Time PgDbType");

        //         // DateTime2
        //         DateTime dt = DateTime.Parse("2000-12-31 23:59:59.997");
        //         Assert.False(IsValidParam(PgDbType.DateTime2, "c3", new DateTimeOffset(1753, 1, 1, 0, 0, 0, TimeSpan.Zero), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.False(IsValidParam(PgDbType.DateTime2, "c3", new SqlDateTime(2000, 12, 31, 23, 59, 59, 997), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTime2, "c3", new DateTime(2000, 12, 31, 23, 59, 59, 997, DateTimeKind.Unspecified), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTime2, "c3", new DateTime(2000, 12, 31, 23, 59, 59, 997, DateTimeKind.Utc), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTime2, "c3", new DateTime(2000, 12, 31, 23, 59, 59, 997, DateTimeKind.Local), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.False(IsValidParam(PgDbType.DateTime2, "c3", new TimeSpan(), conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTime2, "c3", "2000-12-31 23:59:59.997", conn, tempTable), "FAILED: Invalid param for DateTime2 PgDbType");

        //         // DateTimeOffset
        //         DateTimeOffset dto = DateTimeOffset.Parse("9999-12-31 23:59:59.997 +00:00");
        //         Assert.True(IsValidParam(PgDbType.DateTimeOffset, "c4", dto, conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.False(IsValidParam(PgDbType.DateTimeOffset, "c4", new SqlDateTime(1753, 1, 1, 0, 0, 0), conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTimeOffset, "c4", new DateTime(9999, 12, 31, 15, 59, 59, 997, DateTimeKind.Unspecified), conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTimeOffset, "c4", dto.UtcDateTime, conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTimeOffset, "c4", dto.LocalDateTime, conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.False(IsValidParam(PgDbType.DateTimeOffset, "c4", new TimeSpan(), conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         Assert.True(IsValidParam(PgDbType.DateTimeOffset, "c4", "9999-12-31 23:59:59.997 +00:00", conn, tempTable), "FAILED: Invalid param for DateTimeOffset PgDbType");
        //         #endregion

        //         #region reader
        //         // Reader Tests
        //         cmd.CommandText = "SELECT * FROM " + tempTable;
        //         using (SqlDataReader rdr = cmd.ExecuteReader())
        //         {
        //             object[] values = new object[rdr.FieldCount];
        //             object[] sqlValues = new object[rdr.FieldCount];
        //             object[] psValues = new object[rdr.FieldCount];

        //             while (rdr.Read())
        //             {
        //                 rdr.GetValues(values);
        //                 rdr.GetSqlValues(sqlValues);
        //                 rdr.GetProviderSpecificValues(psValues);

        //                 for (int i = 0; i < rdr.FieldCount; ++i)
        //                 {
        //                     if (!rdr.IsDBNull(i))
        //                     {
        //                         bool parsingSucceeded = true;
        //                         try
        //                         {
        //                             switch (i)
        //                             {
        //                                 case 0:
        //                                     rdr.GetInt32(i);
        //                                     break;
        //                                 case 1:
        //                                     rdr.GetDateTime(i);
        //                                     break;
        //                                 case 2:
        //                                     rdr.GetDateTime(i);
        //                                     break;
        //                                 case 3:
        //                                     rdr.GetTimeSpan(i);
        //                                     break;
        //                                 case 4:
        //                                     rdr.GetDateTime(i);
        //                                     break;
        //                                 case 5:
        //                                     rdr.GetDateTimeOffset(i);
        //                                     break;
        //                                 default:
        //                                     Console.WriteLine("Received unknown column number {0} during ReaderParameterTest.", i);
        //                                     parsingSucceeded = false;
        //                                     break;
        //                             }
        //                         }
        //                         catch (InvalidCastException)
        //                         {
        //                             parsingSucceeded = false;
        //                         }
        //                         Assert.True(parsingSucceeded, "FAILED: SqlDataReader parsing failed for column: " + i);

        //                         // Check if each value cast is equivalent to the others
        //                         // Using ToString() helps get around different representations (mainly for values[] and GetValue())
        //                         string[] valueStrList =
        //                             {
        //                                 sqlValues[i].ToString(), rdr.GetSqlValue(i).ToString(), psValues[i].ToString(),
        //                                 rdr.GetProviderSpecificValue(i).ToString(), values[i].ToString(), rdr.GetValue(i).ToString()
        //                             };
        //                         string[] valueNameList = { "sqlValues[]", "GetSqlValue", "psValues[]", "GetProviderSpecificValue", "values[]", "GetValue" };

        //                         for (int valNum = 0; valNum < valueStrList.Length; valNum++)
        //                         {
        //                             string currValueStr = valueStrList[valNum].ToString();
        //                             for (int otherValNum = 0; otherValNum < valueStrList.Length; otherValNum++)
        //                             {
        //                                 if (valNum == otherValNum) continue;

        //                                 Assert.True(currValueStr.Equals(valueStrList[otherValNum]),
        //                                     string.Format("FAILED: Value from {0} not equivalent to {1} result", valueNameList[valNum], valueNameList[otherValNum]));
        //                             }
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //         #endregion
        //     }
        // }

    //     [Test]
    //     public static void TypeVersionKnobTest()
    //     {
    //         string tempTable = "#t_" + Guid.NewGuid().ToString().Replace('-', '_');
    //         string prepTable1 = "CREATE TABLE " + tempTable + " (ci int, c0 dateTime, c1 date, c2 time(7), c3 datetime2(3), c4 datetimeoffset)";
    //         string prepTable2 = "INSERT INTO " + tempTable + " VALUES (0, " +
    //             "'1753-01-01 12:00AM', " +
    //             "'1753-01-01', " +
    //             "'20:12:13.36', " +
    //             "'2000-12-31 23:59:59.997', " +
    //             "'9999-12-31 15:59:59.997 -08:00')";
    //         string prepTable3 = "INSERT INTO " + tempTable + " VALUES (NULL, NULL, NULL, NULL, NULL, NULL)";

    //         using (PgConnection conn = new PgConnection(new PgConnectionStringBuilder(DataTestClass.SQL2008_Master) { TypeSystemVersion = "SQL Server 2008" }.ConnectionString))
    //         {
    //             conn.Open();
    //             PgCommand cmd = conn.CreateCommand();
    //             cmd.CommandText = prepTable1;
    //             cmd.ExecuteNonQuery();
    //             cmd.CommandText = prepTable2;
    //             cmd.ExecuteNonQuery();
    //             cmd.CommandText = prepTable3;
    //             cmd.ExecuteNonQuery();

    //             cmd.CommandText = "SELECT * FROM " + tempTable;
    //             using (SqlDataReader rdr = cmd.ExecuteReader())
    //             {
    //                 while (rdr.Read())
    //                 {
    //                     for (int i = 2; i < rdr.FieldCount; ++i)
    //                     {
    //                         Assert.True(IsNotString(rdr, i), string.Format("FAILED: IsNotString failed for column: {0}", i));
    //                     }
    //                     for (int i = 2; i < rdr.FieldCount; ++i)
    //                     {
    //                         ValidateReader(rdr, i);
    //                     }
    //                 }
    //             }
    //         }

    //         using (PgConnection conn = new PgConnection(new PgConnectionStringBuilder(DataTestClass.SQL2008_Master) { TypeSystemVersion = "SQL Server 2005" }.ConnectionString))
    //         {
    //             conn.Open();
    //             PgCommand cmd = conn.CreateCommand();
    //             cmd.CommandText = prepTable1;
    //             cmd.ExecuteNonQuery();
    //             cmd.CommandText = prepTable2;
    //             cmd.ExecuteNonQuery();
    //             cmd.CommandText = prepTable3;
    //             cmd.ExecuteNonQuery();

    //             cmd.CommandText = "SELECT * FROM " + tempTable;
    //             using (SqlDataReader rdr = cmd.ExecuteReader())
    //             {
    //                 while (rdr.Read())
    //                 {
    //                     for (int i = 2; i < rdr.FieldCount; ++i)
    //                     {
    //                         Assert.True(IsString(rdr, i), string.Format("FAILED: IsString failed for column: {0}", i));
    //                     }
    //                     for (int i = 2; i < rdr.FieldCount; ++i)
    //                     {
    //                         ValidateReader(rdr, i);
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     private static bool IsValidParam(PgDbType dbType, string col, object value, PgConnection conn, string tempTable)
    //     {
    //         try
    //         {
    //             PgCommand cmd = new PgCommand("SELECT COUNT(*) FROM " + tempTable + " WHERE " + col + " = @p", conn);
    //             cmd.Parameters.Add("@p", dbType).Value = value;
    //             cmd.ExecuteScalar();
    //         }
    //         catch (InvalidCastException)
    //         {
    //             return false;
    //         }
    //         return true;
    //     }

    //     private static bool IsString(SqlDataReader rdr, int column)
    //     {
    //         if (!rdr.IsDBNull(column))
    //         {
    //             try
    //             {
    //                 rdr.GetString(column);
    //             }
    //             catch (InvalidCastException)
    //             {
    //                 return false;
    //             }
    //         }

    //         try
    //         {
    //             rdr.GetSqlString(column);
    //         }
    //         catch (InvalidCastException)
    //         {
    //             return false;
    //         }

    //         try
    //         {
    //             rdr.GetSqlChars(column);
    //         }
    //         catch (InvalidCastException)
    //         {
    //             return false;
    //         }

    //         object o = rdr.GetValue(column);
    //         if (o != DBNull.Value && !(o is string))
    //         {
    //             return false;
    //         }

    //         o = rdr.GetSqlValue(column);
    //         if (!(o is SqlString))
    //         {
    //             return false;
    //         }

    //         return true;
    //     }

    //     private static bool IsNotString(SqlDataReader rdr, int column)
    //     {
    //         if (!rdr.IsDBNull(column))
    //         {
    //             try
    //             {
    //                 rdr.GetString(column);
    //                 return false;
    //             }
    //             catch (InvalidCastException)
    //             {
    //             }
    //         }

    //         try
    //         {
    //             rdr.GetSqlString(column);
    //             return false;
    //         }
    //         catch (InvalidCastException)
    //         {
    //         }

    //         try
    //         {
    //             rdr.GetSqlChars(column);
    //             return false;
    //         }
    //         catch (InvalidCastException)
    //         {
    //         }

    //         object o = rdr.GetValue(column);
    //         if (o is string)
    //         {
    //             return false;
    //         }

    //         o = rdr.GetSqlValue(column);
    //         if (o is SqlString)
    //         {
    //             return false;
    //         }

    //         return true;
    //     }

    //     private static void ValidateReader(SqlDataReader rdr, int column)
    //     {
    //         bool validateSucceeded = false;
    //         Action[] nonDbNullActions =
    //         {
    //             () => rdr.GetDateTime(column),
    //             () => rdr.GetTimeSpan(column),
    //             () => rdr.GetDateTimeOffset(column),
    //             () => rdr.GetString(column)
    //         };
    //         Action[] genericActions =
    //         {
    //             () => rdr.GetSqlString(column),
    //             () => rdr.GetSqlChars(column),
    //             () => rdr.GetSqlDateTime(column)
    //         };

    //         Action<Action[]> validateParsingActions =
    //             (testActions) =>
    //             {
    //                 foreach (Action testAction in testActions)
    //                 {
    //                     try
    //                     {
    //                         testAction();
    //                         validateSucceeded = true;
    //                     }
    //                     catch (InvalidCastException)
    //                     {
    //                     }
    //                 }
    //             };

    //         if (!rdr.IsDBNull(column))
    //         {
    //             validateParsingActions(nonDbNullActions);
    //         }
    //         validateParsingActions(genericActions);

    //         // Server 2008 & 2005 seem to represent DBNull slightly differently. Might be related to a Timestamp IsDBNull bug
    //         // in SqlDataReader, which requires different server versions to handle NULLs differently.
    //         // Empty string is expected for DBNull SqlValue (as per API), but SqlServer 2005 returns "Null" for it.
    //         // See GetSqlValue code path in SqlDataReader for more details
    //         if (!validateSucceeded && rdr.IsDBNull(column) && rdr.GetValue(column).ToString().Equals(""))
    //         {
    //             validateSucceeded = true;
    //         }

    //         Assert.True(validateSucceeded, string.Format("FAILED: SqlDataReader failed reader validation for column: {0}. Column literal value: {1}", column, rdr.GetSqlValue(column)));
    //     }
    }
}
