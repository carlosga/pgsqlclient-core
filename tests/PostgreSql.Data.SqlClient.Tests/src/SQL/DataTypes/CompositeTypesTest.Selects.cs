// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class CompositeTypesTest
    {
        [Fact]
        public void RunSequential()
        {
            for (int i = 0; i < 5; i++)
            {
                SelectCompositeArrayTest();
            }
        }

        [Fact]
        public void SelectCompositeArrayTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item[],
    count integer
);
INSERT INTO on_hand VALUES ('{""(fuzzy dice 1, 42, 1.99)"", ""(fuzzy dice 2, 32, 2.05)""}', 1000);
";

            var connStr = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);

            provider.RegisterBinding<InventoryItemBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItem[]>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");
                                Assert.True(item.Length == 2, "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription("fuzzy dice 1", item[0].Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item[0].SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item[0].Price, "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription("fuzzy dice 2", item[1].Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   32, item[1].SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(2.05M, item[1].Price, "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        [Fact]
        public void SelectCompositeArrayNoBindingsTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item[],
    count integer
);
INSERT INTO on_hand VALUES ('{""(fuzzy dice 1, 42, 1.99)"", ""(fuzzy dice 2, 32, 2.05)""}', 1000);
";

            var connStr = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var itemArray = reader.GetFieldValue<object[]>(0);
                                var fcount    = reader.GetInt32(1);

                                Assert.True(itemArray != null, "FAILED: Received a different value than expected.");
                                Assert.True(itemArray.Length == 2, "FAILED: Received a different value than expected.");

                                object[] item1 = (object[])itemArray[0];
                                object[] item2 = (object[])itemArray[1];

                                DataTestClass.AssertEqualsWithDescription("fuzzy dice 1", item1[0], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item1[1], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item1[2], "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription("fuzzy dice 2", item2[0], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   32, item2[1], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(2.05M, item2[2], "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
            }
        }

        [Fact]
        public void SelectCompositeArrayWithNullElementsTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item[],
    count integer
);
INSERT INTO on_hand VALUES ('{""(fuzzy dice 1, 42, 1.99)"", NULL}', 1000);
";

            var connStr = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);

            provider.RegisterBinding<InventoryItemBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItem[]>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");
                                Assert.True(item.Length == 2, "FAILED: Received a different value than expected.");
                                Assert.True(item[1] == null, "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription("fuzzy dice 1", item[0].Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item[0].SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item[0].Price, "FAILED: Received a different value than expected.");

                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        [Fact]
        public void SelectCompositeWithBindingTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item,
    count integer
);
INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);
";

            var connStr = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);

            provider.RegisterBinding<InventoryItemBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItem>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription("fuzzy dice", item.Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item.SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item.Price, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        [Fact]
        public void SelectCompositeWithoutBindingTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item,
    count integer
);
INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99), 1000);
";

            var connStr  = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var values = (object[])reader.GetValue(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(values != null && values.Length == 3, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription("fuzzy dice", values[0], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, values[1], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, values[2], "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
            }
        }

        [Fact]
        public void SelectCompositeWithNullValuesTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item";
            string createSql = 
@"CREATE TYPE inventory_item AS (
    name        text,
    supplier_id integer,
    price       numeric
);
CREATE TABLE on_hand (
    item  inventory_item,
    count integer
);
INSERT INTO on_hand VALUES (ROW('fuzzy dice', NULL, 1.99), 1000);
";

            var connStr  = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);

            provider.RegisterBinding<InventoryItemBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItem>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription("fuzzy dice", item.Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   true, !item.SupplierId.HasValue, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item.Price, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription( 1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        [Fact]
        public void SelectNestedCompositeWithBindingTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item_with_discount; DROP TYPE discount";
            string createSql = 
@"CREATE TYPE discount AS (
    type integer,
    percentage numeric(5,2)
);
CREATE TYPE inventory_item_with_discount AS (
    name        text,
    supplier_id integer,
    price       numeric,
    discount    discount
);
CREATE TABLE on_hand (
    item  inventory_item_with_discount,
    count integer
);
INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99, ROW(1, 10.50)), 1000);
";

            var connStr  = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);


            provider.RegisterBinding<DiscountBinding>();
            provider.RegisterBinding<InventoryItemWithDiscountBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItemWithDiscount>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");
                                Assert.True(item.Discount != null, "FAILED: Received a different value than expected.");

                                // Inventory item
                                DataTestClass.AssertEqualsWithDescription("fuzzy dice", item.Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item.SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item.Price, "FAILED: Received a different value than expected.");

                                // Discount
                                var discount = item.Discount;

                                DataTestClass.AssertEqualsWithDescription(1, discount.Type, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(10.50M, discount.Percentage, "FAILED: Received a different value than expected.");

                                // Other properties
                                DataTestClass.AssertEqualsWithDescription(1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        [Fact]
        public void SelectNullNestedCompositeTest()
        {
            string dropSql   = "DROP TABLE on_hand; DROP TYPE inventory_item_with_discount; DROP TYPE discount";
            string createSql = 
@"CREATE TYPE discount AS (
    type integer,
    percentage numeric(5,2)
);
CREATE TYPE inventory_item_with_discount AS (
    name        text,
    supplier_id integer,
    price       numeric,
    discount    discount
);
CREATE TABLE on_hand (
    item  inventory_item_with_discount,
    count integer
);
INSERT INTO on_hand VALUES (ROW('fuzzy dice', 42, 1.99, NULL), 1000);
";

            var connStr = DataTestClass.PostgreSql_Northwind;

            DropTypes(connStr, dropSql);

            var provider = TypeBindingContext.Register(connStr);

            provider.RegisterBinding<DiscountBinding>();
            provider.RegisterBinding<InventoryItemWithDiscountBinding>();

            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(createSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }

                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand("SELECT * FROM on_hand", connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            int count = 0;

                            while (reader.Read())
                            {
                                var item   = reader.GetFieldValue<InventoryItemWithDiscount>(0);
                                var fcount = reader.GetInt32(1);

                                Assert.True(item != null, "FAILED: Received a different value than expected.");

                                // Inventory item
                                DataTestClass.AssertEqualsWithDescription("fuzzy dice", item.Name, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(   42, item.SupplierId, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1.99M, item.Price, "FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(true, item.Discount == null, $"FAILED: Received a different value than expected.");
                                DataTestClass.AssertEqualsWithDescription(1000, fcount, "FAILED: Received a different value than expected.");

                                count++;
                            }

                            Assert.True(count == 1, "ERROR: Received more results than was expected");
                        }
                    }
                }
            }
            finally
            {
                DropTypes(connStr, dropSql);
                provider.Clear();
            }
        }

        private static void DropTypes(string connStr, string dropSql)
        {
            try
            {
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }            
            }
            catch {}
            finally
            { 
                TypeBindingContext.Clear();
            }
        }
    }
}
