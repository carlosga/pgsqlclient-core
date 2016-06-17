// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;
using PostgreSql.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace PostgreSql.Data.SqlClient.Tests
{
    public class CompositeTypesTest
    {
        [Fact]
        public static void ReadCompositeWithBindingTest()
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

            var connStr  = DataTestClass.PostgreSql9_Northwind;
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
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [Fact]
        public static void ReadCompositeWithoutBindingTest()
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

            var connStr  = DataTestClass.PostgreSql9_Northwind;

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
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public static void ReadCompositeWithNullValuesTest()
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

            var connStr  = DataTestClass.PostgreSql9_Northwind;
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
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [Fact]
        public static void ReadNestedCompositeWithBindingTest()
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

            var connStr  = DataTestClass.PostgreSql9_Northwind;
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
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        [Fact]
        public static void ReadNullNestedCompositeTest()
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

            var connStr  = DataTestClass.PostgreSql9_Northwind;
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
                using (var connection = new PgConnection(connStr)) 
                {
                    connection.Open();
                    using (var command = new PgCommand(dropSql, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    public sealed class InventoryItem
    {
        public string Name
        {
            get;
            set;
        }

        public int? SupplierId
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }
    }

    public sealed class InventoryItemBinding
        : ITypeBinding<InventoryItem>
    {
        public string Name => "inventory_item";
        public Type   Type => typeof(InventoryItem);

        public InventoryItem Read(ITypeReader r)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            return new InventoryItem
            {
                Name       = r.ReadValue<string>()
              , SupplierId = r.ReadValue<int?>()
              , Price      = r.ReadValue<decimal>()
            };
        }

        public void Write(ITypeWriter w, InventoryItem value)
        {
            throw new NotSupportedException();
        }

        object ITypeBinding.Read(ITypeReader r)
        {
            return Read(r);
        }

        void ITypeBinding.Write(ITypeWriter w, object value)
        {
            Write(w, (InventoryItem)value);
        }
    }

    public sealed class Discount
    {
        public int Type
        {
            get;
            set;
        }

        public decimal Percentage
        {
            get;
            set;
        }
    }

    public sealed class InventoryItemWithDiscount
    {
        public string Name
        {
            get;
            set;
        }

        public int SupplierId
        {
            get;
            set;
        }

        public decimal Price
        {
            get;
            set;
        }

        public Discount Discount
        {
            get;
            set;
        }
    }

    public sealed class DiscountBinding
        : ITypeBinding<Discount>
    {
        public string Name => "discount";
        public Type   Type => typeof(Discount);

        public Discount Read(ITypeReader r)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            return new Discount
            {
                Type       = r.ReadValue<int>()
              , Percentage = r.ReadValue<decimal>()
            };
        }

        public void Write(ITypeWriter w, Discount value)
        {
            throw new NotSupportedException();
        }

        object ITypeBinding.Read(ITypeReader r)
        {
            return Read(r);
        }

        void ITypeBinding.Write(ITypeWriter w, object value)
        {
            Write(w, (Discount)value);
        }
    }

    public sealed class InventoryItemWithDiscountBinding
        : ITypeBinding<InventoryItemWithDiscount>
    {
        public string Name => "inventory_item_with_discount";
        public Type   Type => typeof(InventoryItemWithDiscount);

        public InventoryItemWithDiscount Read(ITypeReader r)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            return new InventoryItemWithDiscount
            {
                Name       = r.ReadValue<string>()
              , SupplierId = r.ReadValue<int>()
              , Price      = r.ReadValue<decimal>()
              , Discount   = r.ReadValue<Discount>()
            };
        }

        public void Write(ITypeWriter w, InventoryItemWithDiscount value)
        {
            throw new NotSupportedException();
        }

        object ITypeBinding.Read(ITypeReader r)
        {
            return Read(r);
        }

        void ITypeBinding.Write(ITypeWriter w, object value)
        {
            Write(w, (InventoryItemWithDiscount)value);
        }
    }
}
