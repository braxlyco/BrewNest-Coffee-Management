using BrewNest.BuildingBlocks.Modules;
using BrewNest.Modules.Administration.DependencyInjection;
using BrewNest.Modules.Catalog.DependencyInjection;
using BrewNest.Modules.Delivery.DependencyInjection;
using BrewNest.Modules.Finance.DependencyInjection;
using BrewNest.Modules.Identity.DependencyInjection;
using BrewNest.Modules.Inventory.DependencyInjection;
using BrewNest.Modules.Kitchen.DependencyInjection;
using BrewNest.Modules.Payments.DependencyInjection;
using BrewNest.Modules.Procurement.DependencyInjection;
using BrewNest.Modules.Production.DependencyInjection;
using BrewNest.Modules.Reporting.DependencyInjection;
using BrewNest.Modules.Returns.DependencyInjection;
using BrewNest.Modules.Sales.DependencyInjection;

namespace BrewNest.UnitTests;

/// <summary>Verifies the compile-time module dependency and composition boundaries.</summary>
public sealed class ModuleArchitectureTests
{
    /// <summary>Confirms that every business project supplies one independent module composition root.</summary>
    [Fact]
    public void EveryBusinessModuleExposesOneIndependentCompositionRoot()
    {
        Type[] moduleTypes =
        [
            typeof(IdentityModule), typeof(AdministrationModule), typeof(CatalogModule),
            typeof(SalesModule), typeof(KitchenModule), typeof(ProductionModule),
            typeof(InventoryModule), typeof(ProcurementModule), typeof(ReturnsModule),
            typeof(PaymentsModule), typeof(DeliveryModule), typeof(FinanceModule),
            typeof(ReportingModule)
        ];

        var modules = moduleTypes
            .Select(type => Assert.IsAssignableFrom<IBrewNestModule>(Activator.CreateInstance(type)))
            .ToArray();

        Assert.Equal(13, modules.Length);
        Assert.Equal(13, modules.Select(module => module.Name).Distinct(StringComparer.Ordinal).Count());

        foreach (var moduleType in moduleTypes)
        {
            var businessModuleReferences = moduleType.Assembly.GetReferencedAssemblies()
                .Where(reference => reference.Name?.StartsWith("BrewNest.Modules.", StringComparison.Ordinal) == true);
            Assert.Empty(businessModuleReferences);
        }
    }
}
