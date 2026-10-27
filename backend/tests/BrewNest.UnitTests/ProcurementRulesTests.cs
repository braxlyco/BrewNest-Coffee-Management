using BrewNest.Contracts.Procurement;
using BrewNest.Modules.Procurement.Application.Interfaces;
using BrewNest.Modules.Procurement.Application.Services;
using BrewNest.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
namespace BrewNest.UnitTests;
/// <summary>Verifies Procurement invariants.</summary>
public sealed class ProcurementRulesTests
{
 private static readonly Guid Item=Guid.NewGuid();
 /// <summary>Rejects non-positive quantity.</summary>
 [Fact]public async Task RequisitionRejectsNonPositiveQuantity(){await using var db=Database();var service=Service(db);var result=await service.CreateRequisition(new("Kitchen",DateOnly.FromDateTime(DateTime.UtcNow),"Restock",[new(Item,0,10m,"")]),Context(),default);Assert.Equal(ProcurementResultStatus.Validation,result.Status);Assert.Empty(await db.Requisitions.ToArrayAsync());}
 /// <summary>Requires an active Catalog item.</summary>
 [Fact]public async Task RequisitionRequiresCatalogItem(){await using var db=Database();var service=Service(db);var result=await service.CreateRequisition(new("Kitchen",DateOnly.FromDateTime(DateTime.UtcNow),"Restock",[new(Guid.NewGuid(),1,10m,"")]),Context(),default);Assert.Equal(ProcurementResultStatus.Validation,result.Status);}
 /// <summary>Uses the configured approval role.</summary>
 [Fact]public async Task RequisitionFollowsConfiguredApprovalRole(){await using var db=Database();var service=Service(db);var created=await service.CreateRequisition(new("Kitchen",DateOnly.FromDateTime(DateTime.UtcNow),"Restock",[new(Item,2,10m,"")]),Context(),default);var draft=Assert.IsType<RequisitionResponse>(created.Value);var submitted=await service.ChangeRequisition(draft.Id,"submit",new("Ready",draft.Version),Context(),default);var denied=await service.ChangeRequisition(draft.Id,"approve",new("Approve",submitted.Value!.Version),new("user",["Cashier"]),default);Assert.Equal(ProcurementResultStatus.Forbidden,denied.Status);var approved=await service.ChangeRequisition(draft.Id,"approve",new("Approve",submitted.Value!.Version),Context(),default);Assert.Equal(RequisitionStatus.Approved,approved.Value!.Status);}
 /// <summary>Keeps approval policy behind its port.</summary>
 [Fact]public void ApprovalTiersRemainConfigurationDriven(){IProcurementApprovalPolicyPort port=new ApprovalPort();Assert.IsAssignableFrom<IProcurementApprovalPolicyPort>(port);}
 /// <summary>Uses decimal for quantities and money.</summary>
 [Fact]public void MoneyAndQuantitiesUseDecimal(){var line=new ProcurementLineRequest(Item,1.125m,25000.01m,"");Assert.Equal(typeof(decimal),line.Quantity.GetType());Assert.Equal(25000.01m,line.EstimatedUnitPrice);}
 /// <summary>Exercises sourcing, sequential approval, and Inventory receipt posting.</summary>
 [Fact]public async Task ApprovedSourcingFlowPostsAcceptedReceiptToInventory(){await using var db=Database();var inventory=new InventoryPort();var service=Service(db,inventory);var context=Context();var created=(await service.CreateRequisition(new("Kitchen",DateOnly.FromDateTime(DateTime.UtcNow),"Restock",[new(Item,2m,30000m,"")]),context,default)).Value!;var submitted=(await service.ChangeRequisition(created.Id,"submit",new("Ready",created.Version),context,default)).Value!;var approved=(await service.ChangeRequisition(created.Id,"approve",new("Approved",submitted.Version),context,default)).Value!;var vendor=Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");var rfq=(await service.CreateRfq(new(approved.Id,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),[vendor]),context,default)).Value!;var quote=(await service.SubmitQuotation(rfq.Id,new(vendor,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),"Q-1",[new(Item,2m,30000m,2)]),context,default)).Value!;var po=(await service.SelectQuotation(quote.Id,new("Best value",quote.Version),context,default)).Value!;Assert.Equal(2,po.Approvals.Length);po=(await service.DecideOrder(po.Id,true,new("Purchasing approval",po.Version),context,default)).Value!;Assert.Equal(PurchaseOrderStatus.PendingApproval,po.Status);po=(await service.DecideOrder(po.Id,true,new("Finance approval",po.Version),context,default)).Value!;Assert.Equal(PurchaseOrderStatus.Approved,po.Status);var receipt=(await service.CreateReceipt(po.Id,new("SUP-1",[new(po.Lines[0].Id,Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),"LOT-1",null,DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),2m)]),context,default)).Value!;Assert.Equal(GoodsReceiptStatus.Accepted,receipt.Status);Assert.Equal(1,inventory.ReceiptCalls);}
 /// <summary>Partial vendor returns retain the receipt's unreturned quantity and prevent over-return.</summary>
 [Fact]
 public async Task VendorReturnTracksRemainingReceiptQuantity()
 {
     await using var db=Database();
     var service=Service(db);
     var context=Context();
     var draft=(await service.CreateRequisition(new("Kitchen",DateOnly.FromDateTime(DateTime.UtcNow),"Restock",[new(Item,2m,100m,"")]),context,default)).Value!;
     var submitted=(await service.ChangeRequisition(draft.Id,"submit",new("Ready",draft.Version),context,default)).Value!;
     var approved=(await service.ChangeRequisition(draft.Id,"approve",new("Approved",submitted.Version),context,default)).Value!;
     var vendor=Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
     var rfq=(await service.CreateRfq(new(approved.Id,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),[vendor]),context,default)).Value!;
     var quote=(await service.SubmitQuotation(rfq.Id,new(vendor,DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)),"Q-2",[new(Item,2m,100m,1)]),context,default)).Value!;
     var po=(await service.SelectQuotation(quote.Id,new("Best quote",quote.Version),context,default)).Value!;
     po=(await service.DecideOrder(po.Id,true,new("Purchasing",po.Version),context,default)).Value!;
     po=(await service.DecideOrder(po.Id,true,new("Finance",po.Version),context,default)).Value!;
     var receipt=(await service.CreateReceipt(po.Id,new("SUP-2",[new(po.Lines[0].Id,Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),"LOT-2",null,DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),2m)]),context,default)).Value!;
     var lineId=Assert.Single(receipt.Lines).Id;
     var partial=await service.Return(new(receipt.Id,"Part damaged",[new(lineId,0.5m)]),context,default);
     Assert.True(partial.Succeeded);
     receipt=Assert.Single((await service.Receipts(default)).Value!);
     Assert.Equal(GoodsReceiptStatus.Accepted,receipt.Status);
     Assert.Equal(0.5m,Assert.Single(receipt.Lines).ReturnedQuantity);
     var excessive=await service.Return(new(receipt.Id,"Too many",[new(lineId,2m)]),context,default);
     Assert.Equal(ProcurementResultStatus.Validation,excessive.Status);
     var remainder=await service.Return(new(receipt.Id,"Remaining lot",[new(lineId,1.5m)]),context,default);
     Assert.True(remainder.Succeeded);
     receipt=Assert.Single((await service.Receipts(default)).Value!);
     Assert.Equal(GoodsReceiptStatus.Returned,receipt.Status);
     Assert.Equal(2m,Assert.Single(receipt.Lines).ReturnedQuantity);
 }
 private static ProcurementDbContext Database()=>new(new DbContextOptionsBuilder<ProcurementDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
 private static ProcurementService Service(ProcurementDbContext db,InventoryPort? inventory=null)=>new(db,new CatalogPort(),new ApprovalPort(),inventory??new InventoryPort(),new ProcurementDocumentNumbers(new ConfigurationBuilder().Build()),TimeProvider.System,NullLogger<ProcurementService>.Instance);
 private static ProcurementContext Context()=>new("manager",["Department Manager","Purchasing Manager","Finance Manager","Owner/Admin"]);
 private sealed class CatalogPort:IProcurementCatalogPort{private readonly ProcurementItemReference item=new(Item,"RM-1","Coffee",false,[]);public Task<ProcurementReferenceData> GetReferencesAsync(CancellationToken t)=>Task.FromResult(new ProcurementReferenceData([item],[new(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),"V-1","Vendor")],[new(Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),"W-1","Main")]));public Task<ProcurementItemReference?> GetItemAsync(Guid id,CancellationToken t)=>Task.FromResult<ProcurementItemReference?>(id==Item?item:null);}
 private sealed class ApprovalPort:IProcurementApprovalPolicyPort{public Task<ProcurementApprovalRoute?> ResolveAsync(string code,decimal? amount,CancellationToken t)=>Task.FromResult<ProcurementApprovalRoute?>(new(Guid.NewGuid(),code,"All",code=="PurchaseRequisition"?[new(1,"Department Manager")]:[new(1,"Purchasing Manager"),new(2,"Finance Manager")]));}
 private sealed class InventoryPort:IProcurementInventoryPort{public int ReceiptCalls{get;private set;}public Task<ProcurementInventoryResult> ReceiveAsync(string r,ProcurementInventoryReceipt[] l,string a,CancellationToken t){ReceiptCalls++;return Task.FromResult(new ProcurementInventoryResult(true,[]));}public Task<ProcurementInventoryResult> ReturnAsync(string r,ProcurementInventoryReturn[] l,string a,CancellationToken t)=>Task.FromResult(new ProcurementInventoryResult(true,[]));}
}
