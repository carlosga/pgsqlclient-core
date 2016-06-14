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
        public static void SelectSimpleTest()
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
    }

    public sealed class InventoryItem
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

        public InventoryItem()
        {
        }
    }

   public sealed class InventoryItemBinding
        : ITypeBinding<InventoryItem>
    {
        public string Name
        {
            get { return "inventory_item"; }
        }

        public InventoryItem Read(ITypeReader r)
        {
            if (r == null)
            {
                throw new ArgumentNullException("r");
            }

            return new InventoryItem
            {
                Name       = (string)r.ReadCompositeValue()
              , SupplierId = (int)r.ReadCompositeValue()
              , Price      = (decimal)r.ReadCompositeValue()
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
}
