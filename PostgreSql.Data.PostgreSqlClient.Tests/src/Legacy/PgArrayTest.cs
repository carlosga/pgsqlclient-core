// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the Initial Developer's Public License Version 1.0. See LICENSE file in the project root for full license information.

using System;
using System.Security.Cryptography;
using NUnit.Framework;

namespace PostgreSql.Data.PostgreSqlClient.UnitTests
{
    [TestFixture]
    [Ignore("Needs configuration")]
    public class PgArrayTest 
        : PgBaseTest
    {
        private int _testArrayLength = 100;

        [Test]
        public void Int2ArrayTest()
        {
            // int id_value = DateTime.Now.Millisecond;

            // string selectText = $"SELECT int2_array FROM public.test_table WHERE int4_field = {id_value}";
            // string insertText = "INSERT INTO public.test_table (int4_field, int2_array) values (@int4_field, @int2_array)";

            // byte[] bytes = new byte[_testArrayLength * 2];
            // RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            // rng.GetBytes(bytes);

            // short[] insert_values = new short[_testArrayLength];
            // Buffer.BlockCopy(bytes, 0, insert_values, 0, bytes.Length);

            // // Insert test data
            // using (PgCommand command = new PgCommand(insertText, Connection))
            // {
            //     command.Parameters.Add("@int4_field", PgDbType.Int4).Value  = id_value;
            //     command.Parameters.Add("@int2_array", PgDbType.Array).Value = insert_values;

            //     int updated = command.ExecuteNonQuery();

            //     Assert.AreEqual(1, updated, "Invalid number of inserted rows");                
            // }
            
            // // Check that inserted values are correct
            // using (PgCommand select = new PgCommand(selectText, Connection))
            // {
            //     using (PgDataReader reader = select.ExecuteReader())
            //     {
            //         if (reader.Read())
            //         {
            //             if (!reader.IsDBNull(0))
            //             {
            //                 short[] select_values = new short[insert_values.Length];
            //                 System.Array.Copy((System.Array)reader.GetValue(0), select_values, select_values.Length);

            //                 for (int i = 0; i < insert_values.Length; i++)
            //                 {
            //                     Assert.AreEqual(insert_values[i] != select_values[i], $"differences at index {i}");
            //                 }
            //             }
            //         }
            //     }
            // }
        }
    }
}
