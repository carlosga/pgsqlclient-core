# Postgres composite types binding

pgsqlclient uses data type bindings to resolve composite types, bindings should implement the ITypeBinding or ITypeBinding<T> interface and
are registered in a per connection string basis using the TypeBindingContext and TypeBindingProvider classes, composite types without registered 
bindings are supported also and will return their values will be returned as arrays of objects.

Nested and arrays of composite types are supported.

**NOTE: Composite type parameters are not supported yet**

## Binding implementation sample

```csharp
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
        ...
    }

    object ITypeBinding.Read(ITypeReader r)
    {
        return Read(r);
    }

    void ITypeBinding.Write(ITypeWriter w, object value)
    {
        ...
    }
}

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
          , SupplierId = r.ReadValue<int>()
          , Price      = r.ReadValue<decimal>()
          , Discount   = r.ReadValue<Discount>()
        };
    }

    public void Write(ITypeWriter w, InventoryItem value)
    {
        ...
    }

    object ITypeBinding.Read(ITypeReader r)
    {
        return Read(r);
    }

    void ITypeBinding.Write(ITypeWriter w, object value)
    {
        ...
    }
}
```

## Binding registration sample

```csharp
var connStr  = "...";
var provider = TypeBindingContext.Register(connStr);

provider.RegisterBinding<DiscountBinding>();
provider.RegisterBinding<InventoryItemBinding>();
```
