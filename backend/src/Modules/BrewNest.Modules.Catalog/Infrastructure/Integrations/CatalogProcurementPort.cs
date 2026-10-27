using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Catalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Catalog.Infrastructure.Integrations;
internal sealed class CatalogProcurementPort(CatalogDbContext database):IProcurementCatalogPort
{
    public async Task<ProcurementReferenceData> GetReferencesAsync(CancellationToken token)
    {
        var items=await database.Items.AsNoTracking().Include(x=>x.ItemGroup).Where(x=>x.IsActive&&x.ItemType!=Domain.Entities.CatalogItemType.MenuItem).OrderBy(x=>x.Name).ToArrayAsync(token);
        var requirements=await database.InspectionRequirements.AsNoTracking().Include(x=>x.Criteria).Where(x=>x.IsActive&&x.IsRequired).ToArrayAsync(token);
        var vendors=await database.Vendors.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Name).Select(x=>new ProcurementVendorReference(x.Id,x.Code,x.Name)).ToArrayAsync(token);
        var warehouses=await database.Warehouses.AsNoTracking().Where(x=>x.IsActive).OrderBy(x=>x.Name).Select(x=>new ProcurementWarehouseReference(x.Id,x.Code,x.Name)).ToArrayAsync(token);
        return new(items.Select(x=>Map(x,requirements)).ToArray(),vendors,warehouses);
    }
    public async Task<ProcurementItemReference?> GetItemAsync(Guid id,CancellationToken token)
    {
        var item=await database.Items.AsNoTracking().Include(x=>x.ItemGroup).SingleOrDefaultAsync(x=>x.Id==id&&x.IsActive&&x.ItemType!=Domain.Entities.CatalogItemType.MenuItem,token);if(item is null)return null;
        var requirements=await database.InspectionRequirements.AsNoTracking().Include(x=>x.Criteria).Where(x=>x.IsActive&&x.IsRequired&&(x.ItemId==id||(x.ItemId==null&&x.ItemGroupId==item.ItemGroupId))).ToArrayAsync(token);return Map(item,requirements);
    }
    private static ProcurementItemReference Map(Domain.Entities.CatalogItem item,IEnumerable<Domain.Entities.InspectionRequirement> requirements){var applicable=requirements.Where(x=>x.ItemId==item.Id||(x.ItemId==null&&x.ItemGroupId==item.ItemGroupId)).ToArray();return new(item.Id,item.Code,item.Name,item.RequiresInspection||item.ItemGroup?.RequiresInspection==true||applicable.Length>0,applicable.SelectMany(x=>x.Criteria).Where(x=>x.RevokedAt==null).OrderBy(x=>x.Sequence).Select(x=>new ProcurementInspectionCriterion(x.Id,x.Name,x.ResultType.ToString(),x.MinimumValue,x.MaximumValue,x.Unit,x.IsMandatory)).ToArray());}
}
