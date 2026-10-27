namespace BrewNest.Contracts.Catalog;

/// <summary>Stable Catalog identifiers used only by the opt-in development demo-data seeders.</summary>
public static class DemoCatalogIds
{
    /// <summary>Each unit.</summary>
    public static readonly Guid EachUnit = Guid.Parse("d1000000-0000-0000-0000-000000000001");
    /// <summary>Kilogram unit.</summary>
    public static readonly Guid KilogramUnit = Guid.Parse("d1000000-0000-0000-0000-000000000002");
    /// <summary>Litre unit.</summary>
    public static readonly Guid LitreUnit = Guid.Parse("d1000000-0000-0000-0000-000000000003");
    /// <summary>Menu item group.</summary>
    public static readonly Guid MenuGroup = Guid.Parse("d2000000-0000-0000-0000-000000000001");
    /// <summary>Raw-material item group.</summary>
    public static readonly Guid RawMaterialGroup = Guid.Parse("d2000000-0000-0000-0000-000000000002");
    /// <summary>Packaging item group.</summary>
    public static readonly Guid PackagingGroup = Guid.Parse("d2000000-0000-0000-0000-000000000003");
    /// <summary>Espresso menu item.</summary>
    public static readonly Guid EspressoItem = Guid.Parse("d3000000-0000-0000-0000-000000000001");
    /// <summary>Cappuccino menu item.</summary>
    public static readonly Guid CappuccinoItem = Guid.Parse("d3000000-0000-0000-0000-000000000002");
    /// <summary>Coffee beans raw material.</summary>
    public static readonly Guid CoffeeBeansItem = Guid.Parse("d3000000-0000-0000-0000-000000000003");
    /// <summary>Milk raw material.</summary>
    public static readonly Guid MilkItem = Guid.Parse("d3000000-0000-0000-0000-000000000004");
    /// <summary>Paper cup packaging material.</summary>
    public static readonly Guid CupItem = Guid.Parse("d3000000-0000-0000-0000-000000000005");
    /// <summary>Cup lid packaging material.</summary>
    public static readonly Guid LidItem = Guid.Parse("d3000000-0000-0000-0000-000000000006");
    /// <summary>Main demo warehouse.</summary>
    public static readonly Guid MainWarehouse = Guid.Parse("d4000000-0000-0000-0000-000000000001");
}
