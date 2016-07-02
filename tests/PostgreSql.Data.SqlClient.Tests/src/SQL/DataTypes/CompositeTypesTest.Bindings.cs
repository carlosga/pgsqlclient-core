// Copyright (c) Carlos Guzmán Álvarez. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using PostgreSql.Data.Bindings;
using System;

namespace PostgreSql.Data.SqlClient.Tests
{
    public sealed class InventoryItemBinding
        : ITypeBinding<InventoryItem>
    {
        public string Schema => "public";
        public string Name   => "inventory_item";
        public Type   Type   => typeof(InventoryItem);

        public InventoryItem Read(ITypeReader r)
        {
            return new InventoryItem
            {
                Name       = r.ReadValue<string>()
              , SupplierId = r.ReadValue<int?>()
              , Price      = r.ReadValue<decimal>()
            };
        }

        public void Write(ITypeWriter w, InventoryItem value)
        {
            w.WriteValue<string>(value.Name);
            w.WriteValue<int?>(value.SupplierId);
            w.WriteValue<decimal>(value.Price);
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

    public sealed class DiscountBinding
        : ITypeBinding<Discount>
    {
        public string Schema => "public";
        public string Name   => "discount";
        public Type   Type   => typeof(Discount);

        public Discount Read(ITypeReader r)
        {
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
        public string Schema => "public";
        public string Name   => "inventory_item_with_discount";
        public Type   Type   => typeof(InventoryItemWithDiscount);

        public InventoryItemWithDiscount Read(ITypeReader r)
        {
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
