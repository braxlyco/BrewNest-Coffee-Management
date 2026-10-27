using System.Security.Claims;
using System.Data;
using System.Text.RegularExpressions;
using BrewNest.Contracts.Payments;
using BrewNest.Contracts.Returns;
using BrewNest.Modules.Payments.Application.Interfaces;
using BrewNest.Modules.Payments.Domain.Entities;
using BrewNest.Modules.Payments.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace BrewNest.Modules.Payments.Application.Services;

internal enum PaymentsResultStatus { Success, Validation, NotFound, Conflict }
internal sealed record PaymentsResult<T>(PaymentsResultStatus Status, T? Value, string[] Errors)
{
    public bool Succeeded => Status == PaymentsResultStatus.Success;
    public static PaymentsResult<T> Ok(T value) => new(PaymentsResultStatus.Success, value, []);
    public static PaymentsResult<T> Fail(PaymentsResultStatus status, params string[] errors) => new(status, default, errors);
}

internal sealed record PaymentsContext(string Actor)
{
    public static PaymentsContext From(HttpContext context) => new(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.Identity?.Name ?? "anonymous");
}

internal sealed class PaymentsService(
    PaymentsDbContext database,
    IPaymentsSalesPort sales,
    IEnumerable<IPaymentGateway> gateways,
    IPaymentDocumentNumbers numbers,
    TimeProvider clock) : IReturnsPaymentPort, IPaymentsCodPort
{
    private DateTimeOffset Now => clock.GetUtcNow();

    public async Task<PaymentsResult<PaymentResponse[]>> GetPayments(CancellationToken token) =>
        PaymentsResult<PaymentResponse[]>.Ok((await database.Payments.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).ToArrayAsync(token)).Select(Map).ToArray());

    public async Task<PaymentsResult<RefundResponse[]>> GetRefunds(CancellationToken token) =>
        PaymentsResult<RefundResponse[]>.Ok((await database.Refunds.AsNoTracking()
            .OrderByDescending(x => x.CreatedAt).ToArrayAsync(token)).Select(Map).ToArray());

    public async Task<PaymentsResult<PaymentStatusHistoryResponse[]>> GetHistory(CancellationToken token) =>
        PaymentsResult<PaymentStatusHistoryResponse[]>.Ok((await database.StatusHistory.AsNoTracking()
            .OrderByDescending(x => x.ChangedAt).Take(500).ToArrayAsync(token))
            .Select(x => new PaymentStatusHistoryResponse(x.Id, x.DocumentType, x.DocumentId, x.PreviousStatus,
                x.NewStatus, x.Reason, x.ChangedBy, x.ChangedAt)).ToArray());

    public async Task<PaymentsResult<PaymentResponse>> Create(CreatePaymentRequest request, PaymentsContext context, CancellationToken token)
    {
        if (request.SalesOrderId == Guid.Empty || string.IsNullOrWhiteSpace(request.BillNumber) || request.BillNumber.Length > 40 ||
            request.Amount <= 0 || request.Amount != decimal.Round(request.Amount, 2) ||
            !Enum.IsDefined(request.Method) || request.Notes is null || request.Notes.Length > 500 ||
            request.PayerReference is null || request.PayerReference.Length > 150)
        {
            return Validation<PaymentResponse>("Sales order, bill, positive two-decimal amount, and valid notes are required.");
        }

        if (request.Method is not PaymentMethod.Cash && string.IsNullOrWhiteSpace(request.PayerReference))
        {
            return Validation<PaymentResponse>("A payer, bank, wallet, or rider reference is required for this method.");
        }
        if (request.Method is PaymentMethod.Easypaisa or PaymentMethod.JazzCash &&
            !Regex.IsMatch(request.PayerReference, @"^\+[1-9]\d{7,14}$", RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100)))
        {
            return Validation<PaymentResponse>("Wallet phone must include a country code and 8–15 digits, for example +923001234567.");
        }

        var bill = await sales.GetBillAsync(request.SalesOrderId, request.BillNumber.Trim(), token);
        if (bill is null)
        {
            return Validation<PaymentResponse>("The Sales bill could not be verified.");
        }

        if (bill.Status is "Cancelled")
        {
            return Validation<PaymentResponse>("A cancelled Sales order cannot receive a payment.");
        }

        var committed = await database.Payments.AsNoTracking()
            .Where(x => x.SalesOrderId == bill.SalesOrderId && x.BillNumber == bill.BillNumber &&
                x.Status != PaymentStatus.Failed && x.Status != PaymentStatus.Cancelled)
            .SumAsync(x => x.Amount, token);
        if (committed + request.Amount > bill.NetAmount)
        {
            return Validation<PaymentResponse>("The payment exceeds the remaining bill balance.");
        }

        var payment = new Payment
        {
            Id = Guid.NewGuid(), Number = numbers.Payment(), SalesOrderId = bill.SalesOrderId,
            OrderNumber = bill.OrderNumber, BillNumber = bill.BillNumber, Amount = request.Amount,
            Method = request.Method, Status = PaymentStatus.PendingVerification,
            PayerReference = request.PayerReference.Trim(), Notes = request.Notes.Trim(),
            CreatedAt = Now, CreatedBy = context.Actor
        };

        if (request.Method is PaymentMethod.Easypaisa or PaymentMethod.JazzCash)
        {
            var gateway = gateways.Single(x => x.Method == request.Method);
            var result = await gateway.AuthorizeAsync(payment.Number, payment.Amount, payment.PayerReference, token);
            if (!result.Succeeded)
            {
                return Conflict<PaymentResponse>(result.Error ?? "The sandbox gateway declined the payment.");
            }

            payment.GatewayReference = result.Reference;
            payment.Status = PaymentStatus.Verified;
            payment.VerifiedAt = Now;
            payment.VerifiedBy = $"{request.Method}-sandbox";
        }

        database.Payments.Add(payment);
        History("Payment", payment.Id, "None", payment.Status.ToString(), "Payment recorded.", context.Actor);
        return await Save(() => Map(payment), token);
    }

    public async Task<PaymentsResult<PaymentResponse>> Verify(Guid id, VerifyPaymentRequest request, PaymentsContext context, CancellationToken token)
    {
        var payment = await database.Payments.SingleOrDefaultAsync(x => x.Id == id, token);
        if (payment is null) return NotFound<PaymentResponse>();
        if (payment.Status != PaymentStatus.PendingVerification) return Conflict<PaymentResponse>("Only a pending payment can be verified.");
        if (!Version(payment.RowVersion, request.Version)) return Conflict<PaymentResponse>("The payment changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Remarks) || request.Remarks.Length > 500) return Validation<PaymentResponse>("Verification remarks must be 1–500 characters.");

        var previous = payment.Status;
        payment.Status = request.Approve ? PaymentStatus.Verified : PaymentStatus.Failed;
        payment.VerifiedAt = Now;
        payment.VerifiedBy = context.Actor;
        Touch(payment, context.Actor);
        History("Payment", payment.Id, previous.ToString(), payment.Status.ToString(), request.Remarks, context.Actor);
        return await Save(() => Map(payment), token);
    }

    public async Task<PaymentsResult<PaymentResponse>> Settle(Guid id, SettlePaymentRequest request, PaymentsContext context, CancellationToken token)
    {
        var payment = await database.Payments.SingleOrDefaultAsync(x => x.Id == id, token);
        if (payment is null) return NotFound<PaymentResponse>();
        if (payment.Status != PaymentStatus.Verified) return Conflict<PaymentResponse>("Only a verified payment can be settled.");
        if (!Version(payment.RowVersion, request.Version)) return Conflict<PaymentResponse>("The payment changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.SettlementReference) || request.SettlementReference.Length > 150 ||
            string.IsNullOrWhiteSpace(request.Remarks) || request.Remarks.Length > 500)
            return Validation<PaymentResponse>("Settlement reference and remarks are required.");

        payment.Status = PaymentStatus.Settled;
        payment.SettledAt = Now;
        payment.SettledBy = context.Actor;
        payment.SettlementReference = request.SettlementReference.Trim();
        Touch(payment, context.Actor);
        History("Payment", payment.Id, PaymentStatus.Verified.ToString(), payment.Status.ToString(), request.Remarks, context.Actor);
        return await Save(() => Map(payment), token);
    }

    public async Task<PaymentsResult<RefundResponse>> ProcessRefund(Guid id, ProcessRefundRequest request, PaymentsContext context, CancellationToken token)
    {
        var refund = await database.Refunds.SingleOrDefaultAsync(x => x.Id == id, token);
        if (refund is null) return NotFound<RefundResponse>();
        if (refund.Status != RefundStatus.Requested) return Conflict<RefundResponse>("Only a requested refund can be processed.");
        if (!Version(refund.RowVersion, request.Version)) return Conflict<RefundResponse>("The refund changed. Reload and retry.");
        if (string.IsNullOrWhiteSpace(request.Remarks) || request.Remarks.Length > 500) return Validation<RefundResponse>("Processing remarks must be 1–500 characters.");

        var previous = refund.Status;
        if (!request.Approve)
        {
            refund.Status = RefundStatus.Cancelled;
        }
        else
        {
            Payment? payment = refund.PaymentId is null ? null : await database.Payments.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == refund.PaymentId, token);
            if (payment?.Method is PaymentMethod.Easypaisa or PaymentMethod.JazzCash)
            {
                refund.Status = RefundStatus.Processing;
                var result = await gateways.Single(x => x.Method == payment.Method)
                    .RefundAsync(refund.Number, refund.Amount, refund.SourceReference, token);
                refund.Status = result.Succeeded ? RefundStatus.Completed : RefundStatus.Failed;
                refund.GatewayReference = result.Reference;
                if (!result.Succeeded) request = request with { Remarks = $"{request.Remarks} {result.Error}".Trim() };
            }
            else
            {
                refund.Status = RefundStatus.Completed;
                refund.GatewayReference = $"SIM-MANUAL-REF-{Guid.NewGuid():N}";
            }
        }

        refund.ProcessedAt = Now;
        refund.ProcessedBy = context.Actor;
        refund.Remarks = request.Remarks.Trim();
        Touch(refund, context.Actor);
        History("Refund", refund.Id, previous.ToString(), refund.Status.ToString(), refund.Remarks, context.Actor);
        return await Save(() => Map(refund), token);
    }

    public async Task<ReturnsPaymentState> GetStateAsync(Guid orderId, string? billNumber, decimal amount, CancellationToken token)
    {
        var query = database.Payments.AsNoTracking().Where(x => x.SalesOrderId == orderId &&
            (x.Status == PaymentStatus.Verified || x.Status == PaymentStatus.Settled));
        if (!string.IsNullOrWhiteSpace(billNumber)) query = query.Where(x => x.BillNumber == billNumber);
        var payments = await query.OrderByDescending(x => x.CreatedAt).ToArrayAsync(token);
        var refunded = await database.Refunds.AsNoTracking()
            .Where(x => x.SalesOrderId == orderId &&
                (x.Status == RefundStatus.Requested || x.Status == RefundStatus.Processing || x.Status == RefundStatus.Completed))
            .SumAsync(x => x.Amount, token);
        return new(payments.Length > 0, payments.Sum(x => x.Amount), refunded, payments.FirstOrDefault()?.Number);
    }

    public async Task<string> RequestRefundAsync(Guid salesOrderId, string source, decimal amount, string reason, string actor, CancellationToken token)
    {
        if (salesOrderId == Guid.Empty || string.IsNullOrWhiteSpace(source) || source.Length > 100 || string.IsNullOrWhiteSpace(reason) || reason.Length > 500 ||
            amount <= 0 || amount != decimal.Round(amount, 2))
            throw new ArgumentException("A valid Sales order, refund source, reason, and positive two-decimal amount are required.");
        source = source.Trim();
        var strategy = database.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            // A retried commit can be ambiguous; re-query the idempotency source on every attempt.
            database.ChangeTracker.Clear();
            await using var transaction = database.Database.IsRelational()
                ? await database.Database.BeginTransactionAsync(IsolationLevel.Serializable, token)
                : null;
            var existing = await database.Refunds.SingleOrDefaultAsync(x => x.SourceReference == source, token);
            if (existing is not null)
            {
                if (existing.Amount != amount || existing.SalesOrderId != salesOrderId)
                    throw new InvalidOperationException("The refund source already exists for another amount or Sales order.");
                return existing.Number;
            }

            var payments = await database.Payments.AsNoTracking()
                .Where(x => x.SalesOrderId == salesOrderId &&
                    (x.Status == PaymentStatus.Verified || x.Status == PaymentStatus.Settled))
                .ToArrayAsync(token);
            var alreadyRefunded = await database.Refunds.AsNoTracking()
                .Where(x => x.SalesOrderId == salesOrderId &&
                    (x.Status == RefundStatus.Requested || x.Status == RefundStatus.Processing || x.Status == RefundStatus.Completed))
                .SumAsync(x => x.Amount, token);
            if (payments.Sum(x => x.Amount) - alreadyRefunded < amount)
                throw new InvalidOperationException("The refund exceeds the verified, unrefunded payment amount.");

            var refund = new Refund
            {
                Id = Guid.NewGuid(), Number = numbers.Refund(), SalesOrderId = salesOrderId,
                PaymentId = payments.Length == 1 ? payments[0].Id : null,
                SourceReference = source, Amount = amount,
                Status = RefundStatus.Requested, Reason = reason.Trim(), CreatedAt = Now, CreatedBy = actor
            };
            database.Refunds.Add(refund);
            History("Refund", refund.Id, "None", refund.Status.ToString(), refund.Reason, actor);
            await database.SaveChangesAsync(token);
            if (transaction is not null) await transaction.CommitAsync(token);
            return refund.Number;
        });
    }

    public async Task<Guid> RecordCodAsync(Guid salesOrderId, string billNumber, decimal amount, string riderReference, string actor, CancellationToken token)
    {
        var result = await Create(new(salesOrderId, billNumber, amount, PaymentMethod.Cod, riderReference, "COD collected by rider."), new(actor), token);
        if (!result.Succeeded) throw new InvalidOperationException(string.Join(' ', result.Errors));
        return result.Value!.Id;
    }

    private void History(string type, Guid id, string previous, string next, string reason, string actor) =>
        database.StatusHistory.Add(new() { Id = Guid.NewGuid(), DocumentType = type, DocumentId = id, PreviousStatus = previous,
            NewStatus = next, Reason = reason.Trim(), ChangedBy = actor, ChangedAt = Now });

    private void Touch(PaymentDocument document, string actor) { document.ModifiedAt = Now; document.ModifiedBy = actor; }
    private static bool Version(byte[] value, string supplied) { try { return value.SequenceEqual(Convert.FromBase64String(supplied)); } catch (FormatException) { return false; } }

    private async Task<PaymentsResult<T>> Save<T>(Func<T> map, CancellationToken token)
    {
        try { await database.SaveChangesAsync(token); return PaymentsResult<T>.Ok(map()); }
        catch (DbUpdateConcurrencyException) { return Conflict<T>("The record changed. Reload and retry."); }
        catch (DbUpdateException) { return Conflict<T>("The payment operation conflicts with current data."); }
    }

    private static PaymentResponse Map(Payment x) => new(x.Id, x.Number, x.SalesOrderId, x.OrderNumber, x.BillNumber,
        x.Amount, x.Method, x.Status, x.PayerReference, x.GatewayReference, x.Notes, x.CreatedAt, x.CreatedBy,
        x.VerifiedAt, x.VerifiedBy, x.SettledAt, x.SettledBy, x.SettlementReference, Convert.ToBase64String(x.RowVersion));
    private static RefundResponse Map(Refund x) => new(x.Id, x.Number, x.SourceReference, x.PaymentId, x.Amount,
        x.Status, x.Reason, x.GatewayReference, x.CreatedAt, x.CreatedBy, x.ProcessedAt, x.ProcessedBy, x.Remarks,
        Convert.ToBase64String(x.RowVersion));
    private static PaymentsResult<T> Validation<T>(params string[] errors) => PaymentsResult<T>.Fail(PaymentsResultStatus.Validation, errors);
    private static PaymentsResult<T> Conflict<T>(params string[] errors) => PaymentsResult<T>.Fail(PaymentsResultStatus.Conflict, errors);
    private static PaymentsResult<T> NotFound<T>() => PaymentsResult<T>.Fail(PaymentsResultStatus.NotFound, "The payment record was not found.");
}
