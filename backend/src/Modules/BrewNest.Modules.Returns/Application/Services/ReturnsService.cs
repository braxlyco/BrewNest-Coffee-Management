using System.Security.Claims;
using BrewNest.Contracts.Returns;
using BrewNest.Modules.Returns.Application.Interfaces;
using BrewNest.Modules.Returns.Domain.Entities;
using BrewNest.Modules.Returns.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Returns.Application.Services;

internal enum ReturnsResultStatus { Success, Validation, NotFound, Conflict, Forbidden }
internal sealed record ReturnsResult<T>(ReturnsResultStatus Status, T? Value, string[] Errors)
{
    public bool Succeeded => Status == ReturnsResultStatus.Success;
    public static ReturnsResult<T> Ok(T value) => new(ReturnsResultStatus.Success, value, []);
    public static ReturnsResult<T> Fail(ReturnsResultStatus status, params string[] errors) => new(status, default, errors);
}
internal sealed record ReturnsContext(string Actor, string[] Roles)
{
    public static ReturnsContext From(HttpContext context) => new(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.Identity?.Name ?? "anonymous",
        context.User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray());
}

internal sealed class ReturnsService(
    ReturnsDbContext db,
    IReturnsSalesPort sales,
    IReturnsApprovalPolicyPort policies,
    IReturnsInventoryPort inventory,
    IReturnsPaymentPort payments,
    IReturnsDocumentNumbers numbers,
    TimeProvider clock)
{
    private DateTimeOffset Now => clock.GetUtcNow();

    public async Task<ReturnsResult<ReturnResponse[]>> GetReturns(CancellationToken token) =>
        ReturnsResult<ReturnResponse[]>.Ok((await db.ReturnRequests.AsNoTracking().Include(x => x.Lines)
            .OrderByDescending(x => x.CreatedAt).ToArrayAsync(token)).Select(Map).ToArray());

    public async Task<ReturnsResult<VoidResponse[]>> GetVoids(CancellationToken token) =>
        ReturnsResult<VoidResponse[]>.Ok((await db.VoidRequests.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).ToArrayAsync(token)).Select(Map).ToArray());

    public async Task<ReturnsResult<ReturnsStatusHistoryResponse[]>> GetHistory(CancellationToken token) =>
        ReturnsResult<ReturnsStatusHistoryResponse[]>.Ok((await db.StatusHistory.AsNoTracking()
            .OrderByDescending(x => x.ChangedAt).Take(500).ToArrayAsync(token))
            .Select(x => new ReturnsStatusHistoryResponse(x.Id, x.DocumentType, x.DocumentId,
                x.PreviousStatus, x.NewStatus, x.Reason, x.ChangedBy, x.ChangedAt)).ToArray());

    public async Task<ReturnsResult<ReturnResponse>> CreateReturn(CreateReturnRequest request, ReturnsContext context, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(request.BillNumber) || request.BillNumber.Length > 40 ||
            string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500 ||
            request.Lines is null || request.Lines.Length == 0 ||
            request.Lines.Any(x => x.SalesLineId == Guid.Empty || x.Quantity <= 0 || x.Quantity != decimal.Round(x.Quantity, 3)) ||
            request.Lines.Select(x => x.SalesLineId).Distinct().Count() != request.Lines.Length)
            return Validation<ReturnResponse>("Bill, reason, and unique positive three-decimal return lines are required.");

        var bill = await sales.FindBillAsync(request.BillNumber.Trim(), token);
        if (bill is null || bill.OrderStatus is "Cancelled")
            return Validation<ReturnResponse>("The original bill was not found, verified, or active.");

        var previous = await db.ReturnRequests.AsNoTracking().Include(x => x.Lines)
            .Where(x => x.BillId == bill.BillId && x.Status != RequestStatus.Rejected && x.Status != RequestStatus.Cancelled)
            .SelectMany(x => x.Lines).GroupBy(x => x.SalesLineId)
            .Select(group => new { Id = group.Key, Quantity = group.Sum(line => line.Quantity) })
            .ToDictionaryAsync(x => x.Id, x => x.Quantity, token);
        var route = await policies.ResolveAsync("Refund", token);
        if (route is null || route.Steps.Length == 0)
            return Conflict<ReturnResponse>("No active Refund approval policy is configured.");

        var document = new ReturnRequest
        {
            Id = Guid.NewGuid(), Number = numbers.Return(), RmaNumber = numbers.Rma(),
            SalesOrderId = bill.SalesOrderId, OrderNumber = bill.OrderNumber,
            BillId = bill.BillId, BillNumber = bill.BillNumber,
            Reason = request.Reason.Trim(), Status = RequestStatus.PendingApproval,
            CreatedAt = Now, CreatedBy = context.Actor
        };
        foreach (var requestedLine in request.Lines)
        {
            var original = bill.Lines.SingleOrDefault(x => x.SalesLineId == requestedLine.SalesLineId);
            if (original is null || requestedLine.Quantity + previous.GetValueOrDefault(requestedLine.SalesLineId) > original.Quantity)
                return Validation<ReturnResponse>("A returned quantity exceeds the verified bill quantity.");
            document.Lines.Add(new()
            {
                Id = Guid.NewGuid(), ReturnRequestId = document.Id, SalesLineId = original.SalesLineId,
                ItemId = original.ItemId, ItemCode = original.ItemCode, ItemName = original.ItemName,
                Quantity = requestedLine.Quantity, UnitPrice = original.UnitPrice, Disposition = LineDisposition.Pending
            });
        }
        foreach (var step in route.Steps)
            document.Approvals.Add(new() { Id = Guid.NewGuid(), ReturnRequestId = document.Id, Sequence = step.Sequence, RequiredRole = step.RequiredRole });
        db.ReturnRequests.Add(document);
        History("Return", document.Id, "None", document.Status.ToString(), "Return and RMA created.", context);
        return await Save(() => Map(document), token);
    }

    public async Task<ReturnsResult<ReturnResponse>> DecideReturn(Guid id, ReturnDecisionRequest request, ReturnsContext context, CancellationToken token)
    {
        var document = await FullReturn().SingleOrDefaultAsync(x => x.Id == id, token);
        if (document is null) return NotFound<ReturnResponse>();
        if (document.Status != RequestStatus.PendingApproval) return Conflict<ReturnResponse>("The return is not awaiting approval.");
        if (!Version(document.RowVersion, request.Version)) return Conflict<ReturnResponse>("The return changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500)
            return Validation<ReturnResponse>("An approval reason of at most 500 characters is required.");
        var next = document.Approvals.Where(x => x.Decision == "Pending").OrderBy(x => x.Sequence).First();
        if (!HasRole(context, next.RequiredRole)) return Forbidden<ReturnResponse>($"The next approval requires {next.RequiredRole}.");

        next.Decision = request.Approve ? "Approved" : "Rejected";
        next.DecidedBy = context.Actor;
        next.DecidedAt = Now;
        next.Remarks = request.Reason.Trim();
        var old = document.Status;
        if (!request.Approve) document.Status = RequestStatus.Rejected;
        else if (document.Approvals.All(x => x.Decision == "Approved")) document.Status = RequestStatus.InspectionPending;
        Touch(document, context);
        History("Return", document.Id, old.ToString(), document.Status.ToString(), request.Reason, context);
        return await Save(() => Map(document), token);
    }

    public async Task<ReturnsResult<ReturnResponse>> Inspect(Guid id, Guid lineId, InspectReturnLineRequest request, ReturnsContext context, CancellationToken token)
    {
        var document = await FullReturn().SingleOrDefaultAsync(x => x.Id == id, token);
        if (document is null) return NotFound<ReturnResponse>();
        if (document.Status != RequestStatus.InspectionPending) return Conflict<ReturnResponse>("The return is not awaiting inspection.");
        var line = document.Lines.SingleOrDefault(x => x.Id == lineId);
        if (line is null) return NotFound<ReturnResponse>();
        if (line.Disposition != LineDisposition.Pending) return Conflict<ReturnResponse>("The return line was already inspected.");
        if (!Version(line.RowVersion, request.Version)) return Conflict<ReturnResponse>("The return line changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Criterion) || request.Criterion.Length > 200 ||
            string.IsNullOrWhiteSpace(request.Result) || request.Result.Length > 500 ||
            string.IsNullOrWhiteSpace(request.Remarks) || request.Remarks.Length > 500 ||
            string.IsNullOrWhiteSpace(request.SupportingReference) || request.SupportingReference.Length > 500 ||
            request.LotNumber is null || request.LotNumber.Length > 100)
            return Validation<ReturnResponse>("Criterion, result, remarks, and supporting reference are required within their length limits.");

        var today = DateOnly.FromDateTime(Now.UtcDateTime);
        var eligible = request.IsSealed && request.IsPackagingIntact && request.ManufacturingDateChecked &&
            request.ManufacturingDate is not null && request.ManufacturingDate <= today &&
            request.ExpiryDate is not null && request.ExpiryDate > today &&
            request.ManufacturingDate <= request.ExpiryDate && request.BatchVerified && request.QualityPassed &&
            !string.IsNullOrWhiteSpace(request.LotNumber) && request.WarehouseId != Guid.Empty;
        if (request.RequestRestock && !eligible)
            return Validation<ReturnResponse>("Restocking requires verified bill, sealed item, intact packaging, checked dates, valid expiry, verified batch, and passed quality inspection.");

        if (document.Lines.Count(x => x.Disposition == LineDisposition.Pending) == 1)
        {
            var creditAmount = decimal.Round(document.Lines.Sum(x => x.Quantity * x.UnitPrice), 2);
            var paymentState = await payments.GetStateAsync(document.SalesOrderId, document.BillNumber, creditAmount, token);
            if (paymentState.PaidAmount - paymentState.RefundedAmount < creditAmount)
                return Conflict<ReturnResponse>("The return credit exceeds the verified, unrefunded payment amount.");
        }

        ReturnsPortResult posted;
        if (request.RequestRestock)
        {
            var route = await policies.ResolveAsync("ReturnedFoodRestocking", token);
            if (route is null || route.Steps.Length == 0 || route.Steps.Any(step => !HasRole(context, step.RequiredRole)))
                return Forbidden<ReturnResponse>("The configured Store/QC Manager approval role is required for restocking.");
            posted = await inventory.RestockAsync(document.RmaNumber,
                new(line.ItemId, request.WarehouseId, request.LotNumber.Trim(), request.ManufacturingDate, request.ExpiryDate!.Value, line.Quantity),
                context.Actor, token);
            line.Disposition = LineDisposition.Restocked;
        }
        else
        {
            posted = await inventory.DisposeAsync(document.RmaNumber,
                new(line.ItemId, request.LotNumber.Trim() is { Length: > 0 } lot ? lot : "UNVERIFIED", line.Quantity, "Failed/opened/damaged return"),
                context.Actor, token);
            line.Disposition = LineDisposition.Wastage;
        }
        if (!posted.Succeeded) return Conflict<ReturnResponse>(posted.Errors);

        line.IsSealed = request.IsSealed;
        line.IsPackagingIntact = request.IsPackagingIntact;
        line.ManufacturingDateChecked = request.ManufacturingDateChecked;
        line.ManufacturingDate = request.ManufacturingDate;
        line.ExpiryDate = request.ExpiryDate;
        line.LotNumber = request.LotNumber.Trim();
        line.WarehouseId = request.WarehouseId == Guid.Empty ? null : request.WarehouseId;
        line.BatchVerified = request.BatchVerified;
        line.QualityPassed = request.QualityPassed;
        line.Criterion = request.Criterion.Trim();
        line.Result = request.Result.Trim();
        line.Remarks = request.Remarks.Trim();
        line.SupportingReference = request.SupportingReference.Trim();
        line.Inspector = context.Actor;
        line.InspectedAt = Now;

        if (document.Lines.All(x => x.Disposition != LineDisposition.Pending))
        {
            var amount = decimal.Round(document.Lines.Sum(x => x.Quantity * x.UnitPrice), 2);
            var credit = new CreditNote
            {
                Id = Guid.NewGuid(), Number = numbers.Credit(), ReturnRequestId = document.Id,
                Amount = amount, Reason = document.Reason, CreatedAt = Now, CreatedBy = context.Actor
            };
            db.CreditNotes.Add(credit);
            document.CreditNoteNumber = credit.Number;
            try
            {
                document.RefundReference = await payments.RequestRefundAsync(
                    document.SalesOrderId, credit.Number, amount, document.Reason, context.Actor, token);
            }
            catch (InvalidOperationException exception) { return Conflict<ReturnResponse>(exception.Message); }
            var old = document.Status;
            document.Status = RequestStatus.Completed;
            History("Return", document.Id, old.ToString(), document.Status.ToString(),
                "Inspection completed; credit note and refund request created.", context);
        }
        Touch(document, context);
        return await Save(() => Map(document), token);
    }

    public async Task<ReturnsResult<VoidResponse>> CreateVoid(CreateVoidRequest request, ReturnsContext context, CancellationToken token)
    {
        if (request.SalesOrderId == Guid.Empty || string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500)
            return Validation<VoidResponse>("Sales order and a reason of at most 500 characters are required.");
        var order = await sales.GetOrderAsync(request.SalesOrderId, token);
        if (order is null) return NotFound<VoidResponse>();
        if (order.Status is "Cancelled") return Conflict<VoidResponse>("The Sales order is already cancelled.");
        if (await db.VoidRequests.AnyAsync(x => x.SalesOrderId == order.SalesOrderId &&
                (x.Status == RequestVoidStatus.PendingApproval || x.Status == RequestVoidStatus.Completed), token))
            return Conflict<VoidResponse>("An active or completed void already exists for this Sales order.");
        var payment = await payments.GetStateAsync(order.SalesOrderId, order.BillNumber, order.Amount, token);
        var process = payment.IsPaid ? "VoidAfterPayment" : "VoidBeforePayment";
        var route = await policies.ResolveAsync(process, token);
        if (route is null || route.Steps.Length == 0)
            return Conflict<VoidResponse>($"No active {process} approval policy is configured.");
        var refundAmount = payment.IsPaid ? payment.PaidAmount - payment.RefundedAmount : order.Amount;
        if (refundAmount < 0) return Conflict<VoidResponse>("Previously requested refunds exceed the verified payment amount.");
        var document = new VoidRequest
        {
            Id = Guid.NewGuid(), Number = numbers.Void(), SalesOrderId = order.SalesOrderId,
            OrderNumber = order.OrderNumber, BillNumber = order.BillNumber,
            IsAfterPayment = payment.IsPaid, Amount = refundAmount,
            Status = RequestVoidStatus.PendingApproval, Reason = request.Reason.Trim(),
            CreatedAt = Now, CreatedBy = context.Actor
        };
        foreach (var step in route.Steps)
            document.Approvals.Add(new() { Id = Guid.NewGuid(), VoidRequestId = document.Id, Sequence = step.Sequence, RequiredRole = step.RequiredRole });
        db.VoidRequests.Add(document);
        History("Void", document.Id, "None", document.Status.ToString(), document.Reason, context);
        return await Save(() => Map(document), token);
    }

    public async Task<ReturnsResult<VoidResponse>> DecideVoid(Guid id, ReturnDecisionRequest request, ReturnsContext context, CancellationToken token)
    {
        var document = await db.VoidRequests.Include(x => x.Approvals).SingleOrDefaultAsync(x => x.Id == id, token);
        if (document is null) return NotFound<VoidResponse>();
        if (document.Status != RequestVoidStatus.PendingApproval) return Conflict<VoidResponse>("The void is not awaiting approval.");
        if (!Version(document.RowVersion, request.Version)) return Conflict<VoidResponse>("The void changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Length > 500)
            return Validation<VoidResponse>("An approval reason of at most 500 characters is required.");
        var next = document.Approvals.Where(x => x.Decision == "Pending").OrderBy(x => x.Sequence).First();
        if (!HasRole(context, next.RequiredRole)) return Forbidden<VoidResponse>($"The next approval requires {next.RequiredRole}.");

        next.Decision = request.Approve ? "Approved" : "Rejected";
        next.DecidedBy = context.Actor;
        next.DecidedAt = Now;
        next.Remarks = request.Reason.Trim();
        var old = document.Status;
        if (!request.Approve) document.Status = RequestVoidStatus.Rejected;
        else if (document.Approvals.All(x => x.Decision == "Approved"))
        {
            ReturnsPortResult applied;
            if (document.IsAfterPayment)
            {
                var paymentState = await payments.GetStateAsync(document.SalesOrderId, document.BillNumber, document.Amount, token);
                if (paymentState.PaidAmount - paymentState.RefundedAmount < document.Amount)
                    return Conflict<VoidResponse>("The verified, unrefunded payment amount changed. Reload and retry.");
                if (document.Amount > 0)
                {
                    try
                    {
                        document.RefundReference = await payments.RequestRefundAsync(
                            document.SalesOrderId, document.Number, document.Amount, document.Reason, context.Actor, token);
                    }
                    catch (InvalidOperationException exception) { return Conflict<VoidResponse>(exception.Message); }
                }
                applied = await sales.VoidPaidAsync(document.SalesOrderId, document.Reason, context.Actor, token);
            }
            else
            {
                applied = await sales.VoidUnpaidAsync(document.SalesOrderId, document.Reason, context.Actor, token);
            }
            if (!applied.Succeeded) return Conflict<VoidResponse>(applied.Errors);
            document.Status = RequestVoidStatus.Completed;
        }
        Touch(document, context);
        History("Void", document.Id, old.ToString(), document.Status.ToString(), request.Reason, context);
        return await Save(() => Map(document), token);
    }

    private IQueryable<ReturnRequest> FullReturn() => db.ReturnRequests.AsSplitQuery().Include(x => x.Lines).Include(x => x.Approvals);
    private static bool HasRole(ReturnsContext context, string role) =>
        context.Roles.Any(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x, "Owner/Admin", StringComparison.OrdinalIgnoreCase));
    private void History(string type, Guid id, string old, string next, string reason, ReturnsContext context) =>
        db.StatusHistory.Add(new()
        {
            Id = Guid.NewGuid(), DocumentType = type, DocumentId = id,
            PreviousStatus = old, NewStatus = next, Reason = reason.Trim(),
            ChangedBy = context.Actor, ChangedAt = Now
        });
    private void Touch(ReturnDocument document, ReturnsContext context)
    {
        document.ModifiedAt = Now;
        document.ModifiedBy = context.Actor;
    }
    private static bool Version(byte[] value, string supplied)
    {
        try { return value.SequenceEqual(Convert.FromBase64String(supplied)); }
        catch (FormatException) { return false; }
    }
    private async Task<ReturnsResult<T>> Save<T>(Func<T> map, CancellationToken token)
    {
        try { await db.SaveChangesAsync(token); return ReturnsResult<T>.Ok(map()); }
        catch (DbUpdateConcurrencyException) { return Conflict<T>("The record changed. Reload and retry."); }
        catch (DbUpdateException) { return Conflict<T>("The Returns change conflicts with current data."); }
    }
    private static ReturnResponse Map(ReturnRequest x) => new(
        x.Id, x.Number, x.RmaNumber, x.SalesOrderId, x.OrderNumber, x.BillId, x.BillNumber,
        x.Reason, (ReturnStatus)x.Status, decimal.Round(x.Lines.Sum(y => y.Quantity * y.UnitPrice), 2),
        x.Lines.Select(y => new ReturnLineResponse(
            y.Id, y.SalesLineId, y.ItemId, y.ItemCode, y.ItemName, y.Quantity, y.UnitPrice,
            decimal.Round(y.Quantity * y.UnitPrice, 2), y.IsSealed, y.IsPackagingIntact,
            y.ManufacturingDateChecked, y.ManufacturingDate, y.ExpiryDate, y.LotNumber,
            y.WarehouseId, y.BatchVerified, y.QualityPassed, (ReturnDisposition)y.Disposition,
            y.Inspector, y.InspectedAt, Convert.ToBase64String(y.RowVersion))).ToArray(),
        x.CreditNoteNumber, x.RefundReference, x.CreatedAt, x.CreatedBy, Convert.ToBase64String(x.RowVersion));
    private static VoidResponse Map(VoidRequest x) => new(
        x.Id, x.Number, x.SalesOrderId, x.OrderNumber, x.BillNumber, x.IsAfterPayment,
        x.Amount, (VoidStatus)x.Status, x.Reason, x.RefundReference,
        x.CreatedAt, x.CreatedBy, Convert.ToBase64String(x.RowVersion));
    private static ReturnsResult<T> Validation<T>(params string[] errors) => ReturnsResult<T>.Fail(ReturnsResultStatus.Validation, errors);
    private static ReturnsResult<T> NotFound<T>() => ReturnsResult<T>.Fail(ReturnsResultStatus.NotFound, "The Returns record was not found.");
    private static ReturnsResult<T> Conflict<T>(params string[] errors) => ReturnsResult<T>.Fail(ReturnsResultStatus.Conflict, errors);
    private static ReturnsResult<T> Forbidden<T>(params string[] errors) => ReturnsResult<T>.Fail(ReturnsResultStatus.Forbidden, errors);
}
