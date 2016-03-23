// Ported from the Microsoft System.Data.SqlClient test suite.
// ---------------------------------------------------------------------
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.Tests
{
    // [TestFixture]
    // public static class DataTypes
    // {
    //     [Test]
    //     public static void MaxTypesTest()
    //     {
    //         string connStr = DataTestClass.SQL2005_Master;

    //         string tempTable = $"max_{Guid.NewGuid().ToString().Replace('-', '_')}";
    //         string initStr   = $"create table {tempTable} (col1 varchar(max), col2 varchar(max), col3 varbinary(max))";

    //         string insertNormStr = $"INSERT INTO {tempTable} VALUES('ASCIASCIASCIASCIASCIASCIThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first row', ";
    //         insertNormStr += "N'This is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first row', ";
    //         insertNormStr += "0x010100110011000111000111000011110000111100001111000001111100000111110000011111000001111100000111110000011111000001111100000111110000011111)";

    //         string insertParamStr = $"INSERT {tempTable} VALUES(@x, @y, @z)";
    //         string queryStr       = $"select * from {tempTable}";

    //         using (PgConnection conn = new PgConnection(connStr))
    //         {
    //             conn.Open();

    //             PgCommand cmd = conn.CreateCommand();

    //             cmd.CommandText = initStr;
    //             cmd.ExecuteNonQuery();

    //             try
    //             {
    //                 cmd.CommandText = insertNormStr;
    //                 cmd.ExecuteNonQuery();

    //                 PgCommand cmd2 = new PgCommand(insertParamStr, conn);

    //                 cmd2.Parameters.Add("@x", SqlDbType.VarChar);
    //                 cmd2.Parameters.Add("@y", SqlDbType.VarChar);
    //                 cmd2.Parameters.Add("@z", SqlDbType.Binary);
    //                 cmd2.Parameters[1].Value = "second line, Insert big, Insert Big, This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";
    //                 cmd2.Parameters[1].Value += "This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row";

    //                 byte[] bytes = new byte[2];

    //                 for (int i = 0; i < bytes.Length; ++i)
    //                 {
    //                     bytes[i] = 0xad;
    //                 }
    //                 cmd2.Parameters[2].Value = bytes;
    //                 cmd2.Parameters[0].Value = "This is second row ANSI value";
    //                 cmd2.ExecuteNonQuery();

    //                 cmd.CommandText = queryStr;

    //                 using (PgDataReader reader = cmd.ExecuteReader())
    //                 {
    //                     int currentValue = 0;
    //                     string[][] expectedValues =
    //                     {
    //                         new string[] 
    //                         {
    //                             "ASCIASCIASCIASCIASCIASCIThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first row",
    //                             "This is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first rowThis is the first row",
    //                             "010100110011000111000111000011110000111100001111000001111100000111110000011111000001111100000111110000011111000001111100000111110000011111"
    //                         },
    //                         new string[]
    //                         {
    //                             "This is second row ANSI value",
    //                             "second line, Insert big, Insert Big, This is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second rowThis is the second row",
    //                             "ADAD"
    //                         }
    //                     };

    //                     while (reader.Read())
    //                     {
    //                         Assert.True(currentValue < expectedValues.Length, "ERROR: Received more values than expected");

    //                         char[] stringResult = reader.GetChars(0).Value;
    //                         DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue][0], new string(stringResult, 0, stringResult.Length), "FAILED: Did not receive expected data");
    //                         stringResult = reader.GetChars(1).Value;
    //                         DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue][1], new string(stringResult, 0, stringResult.Length), "FAILED: Did not receive expected data");

    //                         byte[] bb = reader.GetBytes(2).Value;
    //                         char[] cc = new char[bb.Length * 2];
    //                         ConvertBinaryToChar(bb, cc);

    //                         DataTestClass.AssertEqualsWithDescription(expectedValues[currentValue][2], new string(cc, 0, cc.Length), "FAILED: Did not receive expected data");
    //                         currentValue++;
    //                     }
    //                 }
    //             }
    //             finally
    //             {
    //                 cmd.CommandText = "drop table " + tempTable;
    //                 cmd.ExecuteNonQuery();
    //             }
    //         }
    //     }

    //     private static char LocalByteToChar(int b)
    //     {
    //         char c;

    //         if ((b & 0xf) < 10)
    //         {
    //             c = (char)((b & 0xf) + '0');
    //         }
    //         else
    //         {
    //             c = (char)((b & 0xf) - 10 + 'A');
    //         }

    //         return c;
    //     }

    //     private static void ConvertBinaryToChar(byte[] bb, char[] cc)
    //     {
    //         for (int i = 0; i < bb.Length; ++i)
    //         {
    //             cc[2 * i]     = LocalByteToChar((bb[i] >> 4) & 0xf);
    //             cc[2 * i + 1] = LocalByteToChar(bb[i] & 0xf);
    //         }
    //     }
    // }
}
