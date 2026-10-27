using BrewNest.Modules.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
namespace BrewNest.Modules.Procurement.Infrastructure.Persistence;
/// <summary>Owns Procurement persistence.</summary>
public sealed class ProcurementDbContext(DbContextOptions<ProcurementDbContext> options):DbContext(options)
{
    internal DbSet<PurchaseRequisition> Requisitions=>Set<PurchaseRequisition>(); internal DbSet<RequestForQuotation> Rfqs=>Set<RequestForQuotation>(); internal DbSet<VendorQuotation> Quotations=>Set<VendorQuotation>(); internal DbSet<PurchaseOrder> PurchaseOrders=>Set<PurchaseOrder>(); internal DbSet<GoodsReceipt> GoodsReceipts=>Set<GoodsReceipt>(); internal DbSet<VendorReturn> VendorReturns=>Set<VendorReturn>(); internal DbSet<ProcurementStatusHistory> StatusHistory=>Set<ProcurementStatusHistory>();
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder){modelBuilder.HasDefaultSchema("procurement");modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcurementDbContext).Assembly);base.OnModelCreating(modelBuilder);}
}
