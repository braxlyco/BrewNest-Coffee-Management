using System.Security.Claims;
using BrewNest.Contracts.Inventory;
using BrewNest.Contracts.Kitchen;
using BrewNest.Modules.Inventory.Domain.Entities;
using BrewNest.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Inventory.Application.Services;

internal enum InventoryResultStatus { Success, Validation, NotFound, Conflict }
internal sealed record InventoryResult<T>(InventoryResultStatus Status,T? Value,string[] Errors)
{
    public bool Succeeded=>Status==InventoryResultStatus.Success;
    public static InventoryResult<T> Success(T value)=>new(InventoryResultStatus.Success,value,[]);
    public static InventoryResult<T> Failure(InventoryResultStatus status,params string[] errors)=>new(status,default,errors);
}
internal sealed record InventoryRequestContext(string Actor)
{
    public static InventoryRequestContext FromHttpContext(HttpContext context)=>new(context.User.FindFirstValue(ClaimTypes.NameIdentifier)??context.User.Identity?.Name??"anonymous");
}

internal sealed class InventoryService(InventoryDbContext dbContext,IInventoryCatalogReferenceValidator catalogValidator,IInventoryExpiryPolicyProvider expiryPolicy,TimeProvider timeProvider):IInventoryReservationPort,IKitchenInventoryPort
{
    private DateTimeOffset Now=>timeProvider.GetUtcNow();
    private DateOnly Today=>DateOnly.FromDateTime(Now.UtcDateTime);

    public async Task<InventoryResult<StockLotResponse[]>> GetLotsAsync(Guid? itemId,Guid? warehouseId,CancellationToken token)
    {
        await BlockExpiredLotsAsync("system-expiry",token);var query=dbContext.StockLots.AsNoTracking();if(itemId is not null)query=query.Where(x=>x.ItemId==itemId);if(warehouseId is not null)query=query.Where(x=>x.WarehouseId==warehouseId);var lots=await query.OrderBy(x=>x.ExpiryDate).ThenBy(x=>x.LotNumber).ToArrayAsync(token);return InventoryResult<StockLotResponse[]>.Success(lots.Select(Map).ToArray());
    }
    public async Task<InventoryResult<StockLotResponse>> ReceiveAsync(GoodsReceiptRequest request,InventoryRequestContext context,CancellationToken token)
    {
        var errors=ValidateReceipt(request);if(errors.Length>0)return Validation<StockLotResponse>(errors);if(!await catalogValidator.AreValidAsync(request.ItemId,request.WarehouseId,token))return Validation<StockLotResponse>("The Catalog item or warehouse reference is invalid or inactive.");var lotNumber=Normalize(request.LotNumber);var entity=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.ItemId==request.ItemId&&x.WarehouseId==request.WarehouseId&&x.LotNumber==lotNumber,token);
        if(entity is null){entity=new(){Id=Guid.NewGuid(),ItemId=request.ItemId,WarehouseId=request.WarehouseId,LotNumber=lotNumber,ManufacturingDate=request.ManufacturingDate,ExpiryDate=request.ExpiryDate,CreatedAt=Now,CreatedBy=context.Actor};dbContext.StockLots.Add(entity);}else if(entity.ManufacturingDate!=request.ManufacturingDate||entity.ExpiryDate!=request.ExpiryDate)return Conflict<StockLotResponse>("The existing lot has different manufacturing or expiry dates.");
        entity.OnHandQuantity+=request.Quantity;Touch(entity,context);AddMovement(entity,StockMovementType.GoodsReceipt,request.Quantity,request.ReferenceType,request.ReferenceNumber,request.Remarks,context.Actor);AutoBlockIfExpired(entity,context.Actor);return await SaveAsync(()=>Map(entity),token);
    }
    public async Task<InventoryResult<ReservationResponse[]>> GetReservationsAsync(string? referenceType,string? referenceId,CancellationToken token){var query=dbContext.Reservations.AsNoTracking();if(!string.IsNullOrWhiteSpace(referenceType))query=query.Where(x=>x.ReferenceType==referenceType);if(!string.IsNullOrWhiteSpace(referenceId))query=query.Where(x=>x.ReferenceId==referenceId);var values=await query.OrderByDescending(x=>x.CreatedAt).ToArrayAsync(token);return InventoryResult<ReservationResponse[]>.Success(values.Select(Map).ToArray());}
    public async Task<InventoryResult<ReservationResponse>> ReserveForApiAsync(CreateReservationRequest request,InventoryRequestContext context,CancellationToken token)
    {
        if(request.Quantity<=0)return Validation<ReservationResponse>("Reservation quantity must be positive.");if(!ValidRequired(request.ReferenceType,80)||!ValidRequired(request.ReferenceId,100))return Validation<ReservationResponse>("Reservation reference type and identifier are required and must fit their allowed lengths.");if(request.Remarks is null||request.Remarks.Length>500)return Validation<ReservationResponse>("Reservation remarks cannot exceed 500 characters.");var lot=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.Id==request.StockLotId,token);if(lot is null)return NotFound<ReservationResponse>("The stock lot was not found.");if(AutoBlockIfExpired(lot,context.Actor)){await dbContext.SaveChangesAsync(token);return Conflict<ReservationResponse>("Blocked or expired stock cannot be reserved.");}if(lot.IsBlocked)return Conflict<ReservationResponse>("Blocked or expired stock cannot be reserved.");if(Available(lot)<request.Quantity)return Conflict<ReservationResponse>("Insufficient available stock; negative inventory is blocked.");var reservation=new StockReservation{Id=Guid.NewGuid(),StockLotId=lot.Id,ItemId=lot.ItemId,WarehouseId=lot.WarehouseId,Quantity=request.Quantity,ReferenceType=request.ReferenceType.Trim(),ReferenceId=request.ReferenceId.Trim(),Status=ReservationStatus.Active,ExpiresAt=request.ExpiresAt,Remarks=request.Remarks.Trim(),CreatedAt=Now,CreatedBy=context.Actor};lot.ReservedQuantity+=request.Quantity;Touch(lot,context);dbContext.Reservations.Add(reservation);AddMovement(lot,StockMovementType.Reservation,request.Quantity,request.ReferenceType,request.ReferenceId,request.Remarks,context.Actor);return await SaveAsync(()=>Map(reservation),token);
    }
    public async Task<InventoryResult<ReservationResponse>> ReleaseForApiAsync(Guid id,ReleaseReservationRequest request,InventoryRequestContext context,CancellationToken token)
    {
        if(request.Quantity<=0)return Validation<ReservationResponse>("Release quantity must be positive.");if(!ValidRequired(request.Reason,500))return Validation<ReservationResponse>("A release reason up to 500 characters is required.");var reservation=await dbContext.Reservations.Include(x=>x.StockLot).SingleOrDefaultAsync(x=>x.Id==id,token);if(reservation is null)return NotFound<ReservationResponse>("The reservation was not found.");if(!ApplyVersion(reservation,request.Version))return VersionFailure<ReservationResponse>();var remaining=Remaining(reservation);if(request.Quantity>remaining)return Conflict<ReservationResponse>("Release quantity exceeds the reservation remainder.");reservation.ReleasedQuantity+=request.Quantity;reservation.Status=reservation.ReleasedQuantity+reservation.ConsumedQuantity==reservation.Quantity?ReservationStatus.Released:ReservationStatus.PartiallyReleased;reservation.StockLot.ReservedQuantity-=request.Quantity;Touch(reservation,context);Touch(reservation.StockLot,context);AddMovement(reservation.StockLot,StockMovementType.ReservationRelease,-request.Quantity,reservation.ReferenceType,reservation.ReferenceId,request.Reason,context.Actor);return await SaveAsync(()=>Map(reservation),token);
    }
    public async Task<InventoryResult<StockLotResponse>> IssueAsync(GoodsIssueRequest request,InventoryRequestContext context,CancellationToken token)
    {
        if(request.Quantity<=0)return Validation<StockLotResponse>("Issue quantity must be positive.");if(!ValidRequired(request.ReferenceType,80)||!ValidRequired(request.ReferenceNumber,100)||request.Remarks is null||request.Remarks.Length>500)return Validation<StockLotResponse>("Issue reference type and number are required and values must fit their allowed lengths.");var lot=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.Id==request.StockLotId,token);if(lot is null)return NotFound<StockLotResponse>("The stock lot was not found.");if(!ApplyVersion(lot,request.LotVersion))return VersionFailure<StockLotResponse>();if(AutoBlockIfExpired(lot,context.Actor)){await dbContext.SaveChangesAsync(token);return Conflict<StockLotResponse>("Blocked or expired stock cannot be issued.");}if(lot.IsBlocked)return Conflict<StockLotResponse>("Blocked or expired stock cannot be issued.");
        if(request.ReservationId is not null){var reservation=await dbContext.Reservations.SingleOrDefaultAsync(x=>x.Id==request.ReservationId&&x.StockLotId==lot.Id,token);if(reservation is null)return Validation<StockLotResponse>("The reservation does not belong to this lot.");if(request.ReservationVersion is null||!ApplyVersion(reservation,request.ReservationVersion))return VersionFailure<StockLotResponse>();if(request.Quantity>Remaining(reservation))return Conflict<StockLotResponse>("Issue quantity exceeds the reservation remainder.");lot.ReservedQuantity-=request.Quantity;reservation.ConsumedQuantity+=request.Quantity;reservation.Status=reservation.ReleasedQuantity+reservation.ConsumedQuantity==reservation.Quantity?ReservationStatus.Consumed:ReservationStatus.PartiallyConsumed;Touch(reservation,context);}else if(request.Quantity>Available(lot))return Conflict<StockLotResponse>("Insufficient available stock; negative inventory is blocked.");
        if(request.Quantity>lot.OnHandQuantity)return Conflict<StockLotResponse>("Insufficient on-hand stock; negative inventory is blocked.");lot.OnHandQuantity-=request.Quantity;Touch(lot,context);AddMovement(lot,StockMovementType.GoodsIssue,-request.Quantity,request.ReferenceType,request.ReferenceNumber,request.Remarks,context.Actor);return await SaveAsync(()=>Map(lot),token);
    }
    public async Task<InventoryResult<StockLotResponse>> AdjustAsync(StockAdjustmentRequest request,InventoryRequestContext context,CancellationToken token)
    {
        if(request.QuantityDelta==0)return Validation<StockLotResponse>("Adjustment quantity cannot be zero.");if(!ValidRequired(request.Reason,500)||!ValidRequired(request.ReferenceNumber,100))return Validation<StockLotResponse>("An adjustment reason and reference number within their allowed lengths are required.");var lot=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.Id==request.StockLotId,token);if(lot is null)return NotFound<StockLotResponse>("The stock lot was not found.");if(!ApplyVersion(lot,request.Version))return VersionFailure<StockLotResponse>();var newOnHand=lot.OnHandQuantity+request.QuantityDelta;if(newOnHand<0||newOnHand<lot.ReservedQuantity)return Conflict<StockLotResponse>("Adjustment would create negative inventory or reduce on-hand below reserved stock.");lot.OnHandQuantity=newOnHand;Touch(lot,context);AddMovement(lot,request.QuantityDelta>0?StockMovementType.AdjustmentIncrease:StockMovementType.AdjustmentDecrease,request.QuantityDelta,"StockAdjustment",request.ReferenceNumber,request.Reason,context.Actor);return await SaveAsync(()=>Map(lot),token);
    }
    public async Task<InventoryResult<StockLotResponse>> WasteAsync(WastageRequest request,InventoryRequestContext context,CancellationToken token)
    {
        if(request.Quantity<=0)return Validation<StockLotResponse>("Wastage quantity must be positive.");if(!ValidRequired(request.Reason,500)||!ValidRequired(request.ReferenceNumber,100))return Validation<StockLotResponse>("A wastage reason and reference number within their allowed lengths are required.");var lot=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.Id==request.StockLotId,token);if(lot is null)return NotFound<StockLotResponse>("The stock lot was not found.");if(!ApplyVersion(lot,request.Version))return VersionFailure<StockLotResponse>();if(request.Quantity>Available(lot))return Conflict<StockLotResponse>("Wastage exceeds unreserved on-hand stock.");lot.OnHandQuantity-=request.Quantity;Touch(lot,context);AddMovement(lot,StockMovementType.Wastage,-request.Quantity,"Wastage",request.ReferenceNumber,request.Reason,context.Actor);return await SaveAsync(()=>Map(lot),token);
    }
    public async Task<InventoryResult<StockLotResponse>> SetBlockAsync(Guid id,SetLotBlockRequest request,InventoryRequestContext context,CancellationToken token)
    {
        var lot=await dbContext.StockLots.SingleOrDefaultAsync(x=>x.Id==id,token);if(lot is null)return NotFound<StockLotResponse>("The stock lot was not found.");if(!ApplyVersion(lot,request.Version))return VersionFailure<StockLotResponse>();if(lot.IsBlocked==request.IsBlocked)return InventoryResult<StockLotResponse>.Success(Map(lot));if(!ValidRequired(request.Reason,500))return Validation<StockLotResponse>("A block or unblock reason up to 500 characters is required.");if(!request.IsBlocked&&lot.ExpiryDate is not null&&lot.ExpiryDate<Today)return Conflict<StockLotResponse>("Expired stock cannot be unblocked.");lot.IsBlocked=request.IsBlocked;lot.BlockReason=request.IsBlocked?request.Reason.Trim():null;lot.BlockedAt=request.IsBlocked?Now:null;lot.BlockedBy=request.IsBlocked?context.Actor:null;Touch(lot,context);AddMovement(lot,request.IsBlocked?StockMovementType.Blocked:StockMovementType.Unblocked,0,"StockBlock",lot.LotNumber,request.Reason,context.Actor);return await SaveAsync(()=>Map(lot),token);
    }
    public async Task<InventoryResult<StockMovementResponse[]>> GetMovementsAsync(Guid? lotId,Guid? itemId,Guid? warehouseId,int take,CancellationToken token){var query=dbContext.Movements.AsNoTracking();if(lotId is not null)query=query.Where(x=>x.StockLotId==lotId);if(itemId is not null)query=query.Where(x=>x.ItemId==itemId);if(warehouseId is not null)query=query.Where(x=>x.WarehouseId==warehouseId);var values=await query.OrderByDescending(x=>x.PerformedAt).Take(Math.Clamp(take,1,1000)).ToArrayAsync(token);return InventoryResult<StockMovementResponse[]>.Success(values.Select(Map).ToArray());}
    public async Task<InventoryResult<ExpiryAlertResponse[]>> GetExpiryAlertsAsync(Guid? warehouseId,CancellationToken token){await BlockExpiredLotsAsync("system-expiry",token);var thresholds=await expiryPolicy.GetAsync(token);var maximum=Today.AddDays(thresholds.FirstAlertDays);var query=dbContext.StockLots.AsNoTracking().Where(x=>x.ExpiryDate!=null&&x.ExpiryDate<=maximum&&x.OnHandQuantity>0);if(warehouseId is not null)query=query.Where(x=>x.WarehouseId==warehouseId);var lots=await query.OrderBy(x=>x.ExpiryDate).ToArrayAsync(token);return InventoryResult<ExpiryAlertResponse[]>.Success(lots.Select(x=>MapAlert(x,thresholds)).ToArray());}
    public async Task<ReservationResponse> ReserveAsync(CreateReservationRequest request,string actor,CancellationToken cancellationToken){var result=await ReserveForApiAsync(request,new(actor),cancellationToken);return result.Succeeded?result.Value!:throw new InvalidOperationException(string.Join(' ',result.Errors));}
    public async Task<ReservationResponse> ReleaseAsync(Guid reservationId,ReleaseReservationRequest request,string actor,CancellationToken cancellationToken){var result=await ReleaseForApiAsync(reservationId,request,new(actor),cancellationToken);return result.Succeeded?result.Value!:throw new InvalidOperationException(string.Join(' ',result.Errors));}

    public async Task<KitchenInventoryReservationResult> ReserveAsync(string referenceId,KitchenIngredientRequirement[] requirements,string actor,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(referenceId)||referenceId.Length>100)return KitchenFailure("A Kitchen reservation reference is required.");
        if(requirements.Length==0||requirements.Any(x=>x.ItemId==Guid.Empty||x.Quantity<=0))return KitchenFailure("Kitchen ingredient requirements must contain positive quantities.");
        await BlockExpiredLotsAsync(actor,cancellationToken);
        var normalized=requirements.GroupBy(x=>x.ItemId).Select(x=>new KitchenIngredientRequirement(x.Key,x.Sum(y=>y.Quantity),x.First().Name,x.First().UnitSymbol)).ToArray();
        var itemIds=normalized.Select(x=>x.ItemId).ToArray();
        var lots=await dbContext.StockLots.Where(x=>itemIds.Contains(x.ItemId)&&!x.IsBlocked&&x.OnHandQuantity>x.ReservedQuantity).OrderBy(x=>x.ExpiryDate==null).ThenBy(x=>x.ExpiryDate).ThenBy(x=>x.CreatedAt).ToArrayAsync(cancellationToken);
        foreach(var requirement in normalized)
        {
            var available=lots.Where(x=>x.ItemId==requirement.ItemId).Sum(Available);
            if(available<requirement.Quantity)
            {
                var ingredient=string.IsNullOrWhiteSpace(requirement.Name)?"a required ingredient":requirement.Name.Trim();
                var unit=string.IsNullOrWhiteSpace(requirement.UnitSymbol)?string.Empty:$" {requirement.UnitSymbol.Trim()}";
                return KitchenFailure($"Not enough usable {ingredient} to prepare this order. Required: {requirement.Quantity:N3}{unit}; available: {available:N3}{unit}. Check blocked or expired lots, receive more stock, or release an unused reservation in Inventory, then try again.");
            }
        }
        var allocations=new List<KitchenInventoryAllocation>();
        foreach(var requirement in normalized)
        {
            var remaining=requirement.Quantity;
            foreach(var lot in lots.Where(x=>x.ItemId==requirement.ItemId))
            {
                if(remaining<=0)break;
                var quantity=Math.Min(remaining,Available(lot));
                if(quantity<=0)continue;
                var reservation=new StockReservation{Id=Guid.NewGuid(),StockLotId=lot.Id,ItemId=lot.ItemId,WarehouseId=lot.WarehouseId,Quantity=quantity,ReferenceType="KitchenKOT",ReferenceId=referenceId.Trim(),Status=ReservationStatus.Active,Remarks="Reserved when KOT was generated.",CreatedAt=Now,CreatedBy=actor};
                lot.ReservedQuantity+=quantity;lot.ModifiedAt=Now;lot.ModifiedBy=actor;dbContext.Reservations.Add(reservation);AddMovement(lot,StockMovementType.Reservation,quantity,"KitchenKOT",referenceId,"Reserved when KOT was generated.",actor);allocations.Add(new(reservation.Id,lot.ItemId,quantity));remaining-=quantity;
            }
        }
        try{await dbContext.SaveChangesAsync(cancellationToken);return new(true,allocations.ToArray(),[]);}catch(DbUpdateException){return KitchenFailure("Inventory changed while Kitchen reservations were being allocated. Try again.");}
    }

    public async Task<KitchenInventoryReservationResult> ReleaseAsync(string referenceId,string reason,string actor,CancellationToken cancellationToken)
    {
        if(string.IsNullOrWhiteSpace(referenceId)||!ValidRequired(reason,500))return KitchenFailure("A reservation reference and release reason are required.");
        var reservations=await dbContext.Reservations.Include(x=>x.StockLot).Where(x=>x.ReferenceType=="KitchenKOT"&&x.ReferenceId==referenceId).ToArrayAsync(cancellationToken);
        var allocations=new List<KitchenInventoryAllocation>();
        foreach(var reservation in reservations)
        {
            var remaining=Remaining(reservation);if(remaining<=0)continue;reservation.ReleasedQuantity+=remaining;reservation.Status=ReservationStatus.Released;reservation.ModifiedAt=Now;reservation.ModifiedBy=actor;reservation.StockLot.ReservedQuantity-=remaining;reservation.StockLot.ModifiedAt=Now;reservation.StockLot.ModifiedBy=actor;AddMovement(reservation.StockLot,StockMovementType.ReservationRelease,-remaining,"KitchenKOT",referenceId,reason,actor);allocations.Add(new(reservation.Id,reservation.ItemId,remaining));
        }
        try{await dbContext.SaveChangesAsync(cancellationToken);return new(true,allocations.ToArray(),[]);}catch(DbUpdateConcurrencyException){return KitchenFailure("Inventory reservations changed. Reload and try again.");}
    }

    private async Task BlockExpiredLotsAsync(string actor,CancellationToken token){var expired=await dbContext.StockLots.Where(x=>!x.IsBlocked&&x.ExpiryDate!=null&&x.ExpiryDate<Today&&x.OnHandQuantity>0).ToArrayAsync(token);foreach(var lot in expired){lot.IsBlocked=true;lot.BlockReason="Expired";lot.BlockedAt=Now;lot.BlockedBy=actor;lot.ModifiedAt=Now;lot.ModifiedBy=actor;AddMovement(lot,StockMovementType.ExpiredBlocked,0,"Expiry","", "Automatically blocked after expiry.",actor);}if(expired.Length>0)await dbContext.SaveChangesAsync(token);}
    private bool AutoBlockIfExpired(StockLot lot,string actor){if(lot.IsBlocked||lot.ExpiryDate is null||lot.ExpiryDate>=Today)return false;lot.IsBlocked=true;lot.BlockReason="Expired";lot.BlockedAt=Now;lot.BlockedBy=actor;lot.ModifiedAt=Now;lot.ModifiedBy=actor;AddMovement(lot,StockMovementType.ExpiredBlocked,0,"Expiry","","Automatically blocked after expiry.",actor);return true;}
    private void AddMovement(StockLot lot,StockMovementType type,decimal quantity,string referenceType,string reference,string remarks,string actor)=>dbContext.Movements.Add(new(){Id=Guid.NewGuid(),StockLotId=lot.Id,ItemId=lot.ItemId,WarehouseId=lot.WarehouseId,MovementType=type,Quantity=quantity,OnHandAfter=lot.OnHandQuantity,ReservedAfter=lot.ReservedQuantity,ReferenceType=referenceType.Trim(),ReferenceNumber=reference.Trim(),Remarks=remarks.Trim(),PerformedBy=actor,PerformedAt=Now});
    private void Touch(StockLot lot,InventoryRequestContext context){lot.ModifiedAt=Now;lot.ModifiedBy=context.Actor;} private void Touch(StockReservation value,InventoryRequestContext context){value.ModifiedAt=Now;value.ModifiedBy=context.Actor;}
    private bool ApplyVersion(StockLot lot,string version)=>ApplyVersionCore(dbContext.Entry(lot).Property(x=>x.RowVersion),version);private bool ApplyVersion(StockReservation value,string version)=>ApplyVersionCore(dbContext.Entry(value).Property(x=>x.RowVersion),version);
    private static bool ApplyVersionCore<TEntity>(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyEntry<TEntity,byte[]> property,string version) where TEntity : class {try{var bytes=Convert.FromBase64String(version);if(bytes.Length==0&&property.CurrentValue.Length==0)return true;property.OriginalValue=bytes;return bytes.Length>0;}catch(FormatException){return false;}}
    private async Task<InventoryResult<T>> SaveAsync<T>(Func<T> factory,CancellationToken token){try{await dbContext.SaveChangesAsync(token);return InventoryResult<T>.Success(factory());}catch(DbUpdateConcurrencyException){return VersionFailure<T>();}catch(DbUpdateException){return Conflict<T>("The stock change conflicted with current inventory state.");}}
    private static string[] ValidateReceipt(GoodsReceiptRequest request){var errors=new List<string>();if(request.ItemId==Guid.Empty||request.WarehouseId==Guid.Empty)errors.Add("Catalog item and warehouse identifiers are required.");if(!ValidRequired(request.LotNumber,100))errors.Add("A lot number up to 100 characters is required.");if(request.Quantity<=0)errors.Add("Receipt quantity must be positive.");if(request.ManufacturingDate is not null&&request.ExpiryDate is not null&&request.ManufacturingDate>request.ExpiryDate)errors.Add("Manufacturing date cannot follow expiry date.");if(!ValidRequired(request.ReferenceType,80)||!ValidRequired(request.ReferenceNumber,100))errors.Add("Receipt reference type and number are required and must fit their allowed lengths.");if(request.Remarks is null||request.Remarks.Length>500)errors.Add("Receipt remarks cannot exceed 500 characters.");return errors.ToArray();}
    private static bool ValidRequired(string? value,int maximumLength)=>!string.IsNullOrWhiteSpace(value)&&value.Trim().Length<=maximumLength;
    private static decimal Available(StockLot lot)=>lot.OnHandQuantity-lot.ReservedQuantity;private static decimal Remaining(StockReservation x)=>x.Quantity-x.ReleasedQuantity-x.ConsumedQuantity;private static string Normalize(string value)=>value.Trim().ToUpperInvariant();private static string Version(byte[] rowVersion)=>Convert.ToBase64String(rowVersion);
    private static StockLotResponse Map(StockLot x)=>new(x.Id,x.ItemId,x.WarehouseId,x.LotNumber,x.ManufacturingDate,x.ExpiryDate,x.OnHandQuantity,x.ReservedQuantity,Available(x),x.IsBlocked,x.BlockReason,Version(x.RowVersion));
    private static ReservationResponse Map(StockReservation x)=>new(x.Id,x.StockLotId,x.ItemId,x.WarehouseId,x.Quantity,Remaining(x),x.ReferenceType,x.ReferenceId,x.Status.ToString(),x.ExpiresAt,Version(x.RowVersion));
    private static StockMovementResponse Map(StockMovement x)=>new(x.Id,x.StockLotId,x.ItemId,x.WarehouseId,x.MovementType.ToString(),x.Quantity,x.OnHandAfter,x.ReservedAfter,x.ReferenceType,x.ReferenceNumber,x.Remarks,x.PerformedBy,x.PerformedAt);
    private ExpiryAlertResponse MapAlert(StockLot x,InventoryExpiryThresholds p){var days=x.ExpiryDate!.Value.DayNumber-Today.DayNumber;var severity=days<0?"Expired":days<=p.CriticalAlertDays?"Critical":days<=p.EscalatedAlertDays?"Escalated":"First";return new(x.Id,x.ItemId,x.WarehouseId,x.LotNumber,x.ExpiryDate.Value,days,severity,x.IsBlocked,x.OnHandQuantity,Available(x));}
    private static InventoryResult<T> Validation<T>(params string[] errors)=>InventoryResult<T>.Failure(InventoryResultStatus.Validation,errors);private static InventoryResult<T> Conflict<T>(string error)=>InventoryResult<T>.Failure(InventoryResultStatus.Conflict,error);private static InventoryResult<T> NotFound<T>(string error)=>InventoryResult<T>.Failure(InventoryResultStatus.NotFound,error);private static InventoryResult<T> VersionFailure<T>()=>Conflict<T>("The inventory record changed. Reload it and try again.");
    private static KitchenInventoryReservationResult KitchenFailure(params string[] errors)=>new(false,[],errors);
}
