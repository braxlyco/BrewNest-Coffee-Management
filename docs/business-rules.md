# Confirmed business rules

These rules are design constraints for future module implementation. Values identified as configurable must be stored as Administration data rather than C# constants.

## Discounts

- Every discount requires manager approval.
- Store the requesting user, approving manager, approval date/time, reason, percentage, and amount.
- An order cannot be finalized while a discount is unapproved.
- Discount approvals and later changes must remain auditable.

## Inventory reservation and consumption

- Generating or confirming a KOT reserves the required ingredients.
- Reserved stock reduces available stock but does not reduce physical on-hand stock.
- `Available Stock = On-Hand Stock - Reserved Stock`.

### Kitchen order tickets

- KOT generation requires a confirmed Sales order, active recipes for every newly ticketed menu item, and sufficient eligible Inventory stock.
- The initial KOT contains unticketed active order quantities; an additional KOT contains only quantities added since prior non-cancelled KOTs.
- Recipe ingredient requirements include the configured recipe wastage percentage and use three-decimal quantity precision.
- Generating a KOT reserves ingredients through Inventory; it does not reduce physical on-hand stock.
- A KOT may be cancelled and its reservations released only while queued and completely unstarted.
- After preparation begins, cancellation must use a future reversal or wastage transaction and cannot silently increase available stock.
- Preparation follows `Pending -> Preparing -> Ready`, with UTC status history and actor attribution.

### Production

- Releasing a plan snapshots the active Catalog recipe and generates one production order per plan line.
- Required material equals base BOM quantity plus the recipe's expected wastage quantity.
- Releasing an order reserves eligible Inventory lots using FEFO without reducing on-hand stock.
- Goods issue records actual consumption and actual wastage separately and reduces physical on-hand stock.
- `Consumption Variance = Actual Consumption + Actual Wastage - Required Quantity`.
- Negative Inventory, blocked lots, and expired lots cannot satisfy a reservation or goods issue.
- Draft and released production orders may be cancelled; released orders release remaining reservations.
- An order cannot be cancelled after goods issue without a later reversal or wastage transaction.
- Production completion requires posted material usage and preserves UTC status history and actor attribution.
- Goods Issue deducts ingredients from on-hand inventory.
- Production completion records actual consumption, wastage, and variance.
- Cancellation before Goods Issue releases the reservation.
- Cancellation after Goods Issue requires a reversal or wastage transaction; the original transaction remains recorded.
- Negative inventory is blocked.
- Blocked or expired stock cannot be reserved or issued.

## Returned-food restocking

A returned food item may enter unrestricted stock only when all of the following are true:

- The original bill is found and verified.
- The item is sealed.
- Packaging is intact.
- Manufacturing date is checked.
- Expiry date is valid.
- Batch or lot details are verified.
- Quality inspection passes.
- The responsible manager approves restocking.

An opened, damaged, expired, or inspection-failed item goes to wastage/disposal and must not increase available inventory.

### Returns, credit notes, refunds, and voids

- A return request requires a verified original Sales bill and cannot exceed the quantity originally billed for any line.
- Approval steps are resolved from Administration configuration and executed sequentially by users holding the required role.
- Each returned line records seal, packaging, manufacturing-date, expiry, batch/lot, quality, inspector, inspection time, remarks, supporting reference, and final disposition evidence.
- Restocking is posted only through the Inventory contract after all food-safety evidence passes and the configured Store/QC approval is satisfied.
- Failed, opened, damaged, expired, or unverifiable items are disposed through Inventory without increasing stock.
- Completion issues an immutable credit-note record and sends a refund request through Payments; live payment credentials remain outside Returns.
- A void before payment follows the configured Sales Manager route and cancels through Sales. A void after payment follows the configured Sales Manager then Finance route and requests a refund through Payments.
- Return, approval, inspection, credit, refund, and void transitions preserve actor-attributed UTC history and are never physically deleted.

## Configurable approvals

Approval definitions are role-based configuration owned by Administration:

| Operation | Required approval |
|---|---|
| Discount | Manager |
| Void before payment | Sales Manager |
| Void after payment | Sales Manager and Finance |
| Refund | Sales Manager and Finance |
| Purchase Requisition | Requesting department manager |
| Returned-food restocking | Store/QC Manager |
| Stock adjustment | Store Manager |
| Wastage | Kitchen or Store Manager |
| Purchase Order | Tiered by document value |

Seed, but do not hard-code, these editable PO approval tiers:

| Inclusive value range | Required roles |
|---|---|
| Up to PKR 25,000 | Purchasing Manager |
| PKR 25,001 to PKR 100,000 | Purchasing Manager and Finance Manager |
| Above PKR 100,000 | Purchasing Manager, Finance Manager, and Owner/Admin |

Approval records preserve requester, approver, role, decision, date/time, remarks, and the approved document version/value.

## Expiry management

Seed these editable alert offsets:

- First alert: 15 days before expiry.
- Escalated alert: 7 days before expiry.
- Critical alert: 3 days before expiry.

Expired stock is automatically blocked. Blocked or expired stock cannot be reserved or issued to production. Changing an alert offset requires no code change.

## Rider allocation

- Only active and available riders are eligible.
- Automatic selection chooses the eligible rider with the fewest active deliveries.
- A manager may override the automatic selection; the override and reason remain auditable.
- A rider becomes available again after delivery completion.
- Preserve all assignment and reassignment history.

## Payments

- Supported methods: Cash, COD, Bank Transfer, Easypaisa, and JazzCash.
- Payment verification and refund processing preserve external references and status history.
- Easypaisa and JazzCash use an `IPaymentGateway` abstraction.
- Initial implementations are simulator/sandbox adapters only.
- Live secrets, tokens, and credentials must never be stored in source code or committed settings.
- Every payment must match a verified Sales order and final bill; committed payments cannot exceed the bill's net amount.
- Cash, COD, and bank-transfer records remain pending until explicitly verified. Failed verification is retained.
- Easypaisa and JazzCash are automatically authorized only through their configured sandbox adapters during development.
- Only verified payments may be settled, and settlement reference, actor, remarks, and UTC time are preserved.
- Refund requests are idempotent by source reference and are retained through requested, completed, failed, or cancelled states.

## Profit reporting

- `Gross Profit = Net Sales - Cost of Goods Sold`.
- `Net Profit = Gross Profit - Allocated Operating Expenses`.

Profit reports expose gross sales, discounts, returns, net sales, ingredient cost, packaging cost, wastage cost, cost of goods sold, gross profit, allocated operating expenses, and net profit. Calculation inputs remain traceable to their source transactions and configurable posting/allocation rules.

## Quality inspection

- Inspection applies only when the item or purchasing category is configured to require it.
- Inspection criteria are configurable by item/category.
- Capture inspector, inspection date/time, criterion, result, remarks, and supporting reference.
- Failed material must not enter unrestricted stock.
- A passed receipt follows the configured release process; failure routes stock to blocked/rejected disposition.

## Procurement

- Requisitions need the configured department approval role before sourcing.
- RFQs require an approved requisition and active vendors.
- Selecting a quotation snapshots the matching Administration purchase-order approval tier.
- Approval steps execute sequentially and require the configured role; rejection terminates the route.
- The seeded PKR 25,000 and PKR 100,000 tier boundaries remain editable in Administration.
- Inspection applies only when Catalog item/category configuration requires it, and all mandatory criteria must be captured.
- Failed material never posts to unrestricted Inventory. Passed or exempt lots post through the Inventory contract.
- Vendor returns reduce available, never reserved, stock and create append-only ledger movements.

## Technical accounting and audit invariants

- Transaction records are never physically deleted.
- Corrections use reversal, cancellation, adjustment, return, wastage, or credit-note records.
- Important documents preserve status history and optimistic concurrency information.
- Created/modified audit fields use UTC `DateTimeOffset` values and capture the acting user.
- Internal database IDs remain separate from human-readable document numbers.
- Monetary values use `decimal` (normally `decimal(18,2)`); quantities use `decimal` (normally `decimal(18,3)`).
