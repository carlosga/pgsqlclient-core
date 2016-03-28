// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using NUnit.Framework;
using System;
using System.Data;

namespace PostgreSql.Data.SqlClient.UnitTests
{
    [TestFixture]
    [Ignore("Needs configuration")]
    public class PgDataReaderTest
        : PgBaseTest
    {
        [Test]
        public void ReadTest()
        {
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                using (PgCommand command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction))
                {
                    Console.WriteLine("\r\nDataReader - Read Method - Test");

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader.GetValue(i) + "\t");
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        [Test]
        public void GetValuesTest()
        {
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                using (PgCommand command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction))
                {
                    Console.WriteLine("\r\nDataReader - Read Method - Test");

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] values = new object[reader.FieldCount];
                            reader.GetValues(values);

                            for (int i = 0; i < values.Length; i++)
                            {
                                Console.Write(values[i] + "\t");
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        [Test]
        public void IndexerByIndexTest()
        {
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                using (PgCommand command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction))
                {
                    Console.WriteLine("\r\nDataReader - Read Method - Test");

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader[i] + "\t");
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        [Test]
        public void IndexerByNameTest()
        {
            using (PgTransaction transaction = Connection.BeginTransaction())
            {
                using (PgCommand command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction))
                {
                    Console.WriteLine("\r\nDataReader - Read Method - Test");

                    using (PgDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                Console.Write(reader[reader.GetName(i)] + "\t");
                            }

                            Console.WriteLine();
                        }
                    }
                }
            }
        }

        // [Test]
        // public void GetSchemaTableTest()
        // {
        //     PgTransaction transaction = Connection.BeginTransaction();
        //     PgCommand command = new PgCommand("SELECT * FROM public.test_table", Connection, transaction);

        //     PgDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

        //     DataTable schema = reader.GetSchemaTable();

        //     Console.WriteLine();
        //     Console.WriteLine("DataReader - GetSchemaTable Method- Test");

        //     DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

        //     foreach (DataColumn myCol in schema.Columns)
        //     {
        //         Console.Write("{0}\t\t", myCol.ColumnName);
        //     }

        //     Console.WriteLine();

        //     foreach (DataRow myRow in currRows)
        //     {
        //         foreach (DataColumn myCol in schema.Columns)
        //         {
        //             Console.Write("{0}\t\t", myRow[myCol]);
        //         }

        //         Console.WriteLine();
        //     }

        //     reader.Close();
        //     transaction.Rollback();
        //     command.Dispose();
        // }

        // [Test]
        // public void GetSchemaTableWithExpressionFieldTest()
        // {
        //     PgTransaction transaction = Connection.BeginTransaction();
        //     PgCommand command = new PgCommand("SELECT *, 0 AS VALOR FROM public.test_table", Connection, transaction);

        //     PgDataReader reader = command.ExecuteReader(CommandBehavior.SchemaOnly);

        //     DataTable schema = reader.GetSchemaTable();

        //     Console.WriteLine();
        //     Console.WriteLine("DataReader - GetSchemaTable Method- Test");

        //     DataRow[] currRows = schema.Select(null, null, DataViewRowState.CurrentRows);

        //     foreach (DataColumn myCol in schema.Columns)
        //     {
        //         Console.Write("{0}\t\t", myCol.ColumnName);
        //     }

        //     Console.WriteLine();

        //     foreach (DataRow myRow in currRows)
        //     {
        //         foreach (DataColumn myCol in schema.Columns)
        //         {
        //             Console.Write("{0}\t\t", myRow[myCol]);
        //         }

        //         Console.WriteLine();
        //     }

        //     reader.Close();
        //     transaction.Rollback();
        //     command.Dispose();
        // }
    }
}
