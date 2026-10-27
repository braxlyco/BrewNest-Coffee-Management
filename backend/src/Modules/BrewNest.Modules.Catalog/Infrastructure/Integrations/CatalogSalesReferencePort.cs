using BrewNest.Contracts.Sales;
using BrewNest.Modules.Catalog.Domain.Entities;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Catalog.Infrastructure.Integrations;

internal sealed class CatalogSalesReferencePort(CatalogDbContext database):ISalesCatalogPort
{
    public async Task<SalesReferenceDataResponse> GetReferencesAsync(CancellationToken cancellationToken)
    {
        var items=await database.Items.AsNoTracking().Where(x=>x.IsActive&&x.ItemType==CatalogItemType.MenuItem&&x.SalesPrice!=null).OrderBy(x=>x.Code).Select(x=>new SalesMenuItemReference(x.Id,x.Code,x.Name,x.SalesPrice!.Value)).ToArrayAsync(cancellationToken);
        var customers=await database.Customers.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Code).Select(x=>new SalesCustomerReference(x.Id,x.Code,x.Name,x.Phone)).ToArrayAsync(cancellationToken);
        var tables=await database.DiningTables.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Code).Select(x=>new SalesDiningTableReference(x.Id,x.Code,x.Name,x.Capacity,x.Area)).ToArrayAsync(cancellationToken);
        return new(items,customers,tables);
    }

    public Task<SalesMenuItemReference?> GetMenuItemAsync(Guid id,CancellationToken cancellationToken)=>database.Items.AsNoTracking().Where(x=>x.Id==id&&x.IsActive&&x.ItemType==CatalogItemType.MenuItem&&x.SalesPrice!=null).Select(x=>new SalesMenuItemReference(x.Id,x.Code,x.Name,x.SalesPrice!.Value)).SingleOrDefaultAsync(cancellationToken);
    public Task<bool> IsCustomerActiveAsync(Guid id,CancellationToken cancellationToken)=>database.Customers.AnyAsync(x=>x.Id==id&&x.IsActive,cancellationToken);
    public Task<bool> IsDiningTableActiveAsync(Guid id,CancellationToken cancellationToken)=>database.DiningTables.AnyAsync(x=>x.Id==id&&x.IsActive,cancellationToken);
}
