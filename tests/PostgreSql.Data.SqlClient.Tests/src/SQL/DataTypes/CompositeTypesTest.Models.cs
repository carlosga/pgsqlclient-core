// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace PostgreSql.Data.SqlClient.Tests
{
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
}
