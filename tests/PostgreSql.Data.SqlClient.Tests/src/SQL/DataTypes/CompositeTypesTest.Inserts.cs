// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;
using PostgreSql.Data.Bindings;

namespace PostgreSql.Data.SqlClient.Tests
{
    public partial class CompositeTypesTest
    {
        [Fact]
        public void InsertCompositeWithBindingTest()
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
                    using (var command = new PgCommand("INSERT INTO on_hand VALUES (@InventoryItem, @Count)", connection))
                    {
                        command.Parameters.Add("@InventoryItem", PgDbType.Composite).Value = new InventoryItem {
                            Name        = "fuzzy dice"
                          , SupplierId  = 42
                          , Price       = 1.99M
                        };
                        command.Parameters.AddWithValue("@Count", 1000);
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
    }
}
