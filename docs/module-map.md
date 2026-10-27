# Module map

Event names below are architectural intentions for later phases, not implemented message contracts. Modules depend only on BuildingBlocks, Contracts, framework libraries, and explicitly defined cross-module ports/events.

## Identity

- **Purpose:** authentication, signed JWT access tokens, rotating refresh tokens, users, roles, explicit permissions, and password management.
- **Owned data:** users, roles, explicit permissions, soft-revocable grants/assignments, credential hashes, hashed refresh tokens, security audit events.
- **Main use cases:** sign in/out, rotate/revoke sessions, inspect current access, manage users/roles/permissions, reset/change password, enforce permission policies.
- **Dependencies:** Administration policy lookups through contracts where required; no direct module data access.
- **Publishes:** `UserCreated`, `UserStatusChanged`, `UserPermissionsChanged`.
- **Consumes:** none initially.

## Administration

- **Purpose:** tenant/branch configuration, fiscal periods, numbering, approvals, alerts, allocations, and audit oversight.
- **Owned data:** company/branch/fiscal settings, document sequences, approval policies/tiers, alert offsets, allocation settings, audit log.
- **Main use cases:** configure settings, resolve approval route, issue document numbers, review audit history.
- **Dependencies:** Identity actor/role identifiers via contracts.
- **Publishes:** `ConfigurationChanged`, `ApprovalPolicyChanged`, `FiscalYearChanged`.
- **Consumes:** auditable activity notifications from all modules.

## Catalog

- **Purpose:** café master data and recipe/BOM definitions.
- **Owned data:** menu items, raw/packaging materials, UOM, groups, recipes, customers, vendors, tables, warehouses, riders.
- **Main use cases:** maintain masters, recipe versions, item/category inspection requirements, party and location records.
- **Dependencies:** Administration branch and numbering settings.
- **Publishes:** `ItemChanged`, `RecipeChanged`, `VendorChanged`, `RiderProfileChanged`.
- **Consumes:** `ConfigurationChanged`.

## Sales

- **Purpose:** order capture, discounts, billing, closure, and sales history for every fulfillment mode.
- **Owned data:** sales orders, lines, table assignments, discounts, bills, status history.
- **Main use cases:** open/change order, add items, request/approve discount, finalize bill, close/cancel order.
- **Dependencies:** Catalog menu-item/customer/table snapshots through `ISalesCatalogPort`. Later phases will add Administration numbering/approval adapters, Inventory availability, Payments status, and Delivery request ports without direct table access.
- **Publishes:** `OrderConfirmed`, `OrderChanged`, `OrderCancelled`, `BillFinalized`, `SaleClosed`.
- **Consumes:** none in Phase 6; future consumers are `PaymentCompleted`, `PaymentFailed`, `DeliveryCompleted`, and `ReturnApproved`.

## Kitchen

- **Purpose:** KOT lifecycle, preparation queue, instructions, and live kitchen updates.
- **Owned data:** KOTs, KOT lines, preparation state/history, kitchen instructions.
- **Main use cases:** generate/add KOT, change preparation status, display kitchen queue, notify clients.
- **Dependencies:** confirmed order snapshots through `IKitchenSalesOrderPort`; active recipe snapshots through `IKitchenRecipePort`; atomic FEFO reservation/release through `IKitchenInventoryPort`; authenticated SignalR transport.
- **Publishes:** `KotConfirmed`, `PreparationStatusChanged`, `KotCancelled`.
- **Consumes:** confirmed Sales snapshots on explicit KOT generation in Phase 7; event-driven `OrderConfirmed`, `OrderChanged`, and `OrderCancelled` consumption remains a future integration option.

## Production

- **Purpose:** plan and execute preparation/production with expected and actual consumption.
- **Owned data:** plans, production documents, requirements, releases, actual consumption, wastage, variances.
- **Main use cases:** plan, calculate BOM demand, release, issue materials, complete, record variance.
- **Dependencies:** active recipe snapshots through `IProductionRecipePort`; FEFO reservation, release, goods issue, and wastage posting through `IProductionInventoryPort`; configurable document-number provider.
- **Publishes:** `ProductionReleased`, `MaterialConsumptionRecorded`, `ProductionCompleted`.
- **Consumes:** current Catalog recipe snapshots at plan release and synchronous Inventory results during execution; event-driven KOT demand is deferred.

## Inventory

- **Purpose:** authoritative stock quantities, lots, reservations, movements, blocking, wastage, and expiry.
- **Owned data:** stock ledger, balances, batches/lots, reservations, receipts/issues, adjustments, blocks, expiry alerts.
- **Main use cases:** check availability, reserve/release, issue/receive, adjust, waste, block expired stock, trace movement.
- **Dependencies:** Catalog item/warehouse identifiers through `IInventoryCatalogReferenceValidator`; expiry configuration through `IInventoryExpiryPolicyProvider`; no direct access to either module's tables.
- **Publishes:** `StockReserved`, `ReservationReleased`, `StockIssued`, `StockReceived`, `StockBlocked`, `StockShortageDetected`.
- **Consumes:** `KotConfirmed`, `KotCancelled`, `ProductionCompleted`, `GoodsReceiptAccepted`, `ReturnRestockApproved`.

## Procurement

- **Purpose:** source materials through controlled request, comparison, approval, inspection, receipt, and return.
- **Owned data:** requisitions, RFQs/invitations, quotation snapshots, selected vendors, purchase orders/lines, approval decisions, receipts, inspection results, vendor returns, and status history in `procurement`.
- **Main use cases:** approve PR, issue RFQ, compare/select quote, resolve tiered approval, approve PO, inspect configured items, receive accepted lots, and return available stock.
- **Dependencies:** `IProcurementCatalogPort`, `IProcurementApprovalPolicyPort`, `IProcurementInventoryPort`, and Identity permission claims; no foreign table access.
- **Publishes:** future `RequisitionApproved`, `PurchaseOrderApproved`, `GoodsReceiptAccepted`, `InspectionFailed`, and `VendorReturnCompleted` events.
- **Consumes:** current synchronous contract responses only; future demand events may be consumed through contracts.

## Returns

- **Purpose:** customer returns, RMA, inspection, disposition, credits, refund requests, and void control.
- **Owned data:** return requests/RMAs, return lines and inspection evidence, sequential approval decisions, dispositions, credit notes, refund references, void records, and append-only status history in schema `returns`.
- **Main use cases:** verify an original bill, enforce billed-quantity limits, authorize and inspect a return, decide restock or disposal, issue a credit note, request a refund, and approve a void according to payment state.
- **Dependencies:** `IReturnsSalesPort` for bill/order verification and unpaid cancellation; `IReturnsApprovalPolicyPort` for Administration-owned approval routes; `IReturnsInventoryPort` for approved restock or disposal; `IReturnsPaymentPort` for payment state and refund requests. Returns never reads another module's tables.
- **Publishes:** `ReturnApproved`, `ReturnRestockApproved`, `ReturnDisposed`, `CreditNoteIssued`, `RefundRequested`, `SaleVoided`.
- **Consumes:** `BillFinalized`, `RefundCompleted`, `RefundFailed`.

## Payments

- **Purpose:** collect, verify, settle, and refund supported payment methods through replaceable adapters.
- **Owned data:** payment records, payer/gateway references, verification evidence, settlements, refund requests, and append-only status history in schema `payments`.
- **Main use cases:** verify a Sales bill and remaining balance, record cash/COD/bank payment, authorize simulated wallet payment, verify, settle, and execute or reject a refund.
- **Dependencies:** `IPaymentsSalesPort` for bill snapshots, `IReturnsPaymentPort` for paid-state/refund requests, `IPaymentsCodPort` for future Delivery handoff, and replaceable `IPaymentGateway` adapters. Payments never reads another module's tables.
- **Publishes:** `PaymentCompleted`, `PaymentFailed`, `RefundCompleted`, `RefundFailed`.
- **Consumes:** `BillFinalized`, `RefundRequested`, `CodSubmitted`.

## Delivery

- **Purpose:** assign riders, track delivery, preserve assignment history, and control COD custody.
- **Owned data:** rider operational availability, assignments/history, delivery states, confirmations, COD collection/submission.
- **Main use cases:** auto-assign least-loaded rider, override assignment, update status, complete delivery, submit COD.
- **Dependencies:** Catalog rider profile contract; Sales delivery request; Identity/Administration manager authorization; Payments COD port.
- **Publishes:** `RiderAssigned`, `DeliveryStatusChanged`, `DeliveryCompleted`, `CodCollected`, `CodSubmitted`.
- **Consumes:** `OrderConfirmed`, `OrderCancelled`, `PaymentCompleted`.

## Finance

- **Purpose:** payables, outgoing payments, expenses, allocation, posting rules, COGS and profit calculation.
- **Owned data:** A/P invoices, vendor payments, expenses, allocations, posting rules, financial summaries.
- **Main use cases:** recognize payable, pay vendor, record/allocate expense, calculate gross/net profit.
- **Dependencies:** Procurement receipt/invoice contracts; Sales/Returns/Payments summaries; Inventory costing events; Administration rules.
- **Publishes:** `VendorPaymentCompleted`, `ExpenseAllocated`, `FinancialPeriodCalculated`.
- **Consumes:** `GoodsReceiptAccepted`, `SaleClosed`, `CreditNoteIssued`, `PaymentCompleted`, `StockIssued`, `ProductionCompleted`.

## Reporting

- **Purpose:** operational, financial, alert, and management views without owning source transactions.
- **Owned data:** read models/projections, report snapshots, dashboard preferences, generated-report metadata.
- **Main use cases:** daily/monthly sales, production, stock, wastage, procurement, returns, payments, delivery, profit, and alerts.
- **Dependencies:** published events and stable query contracts from all source modules.
- **Publishes:** `ReportGenerated`, `AlertRaised`, `AlertEscalated`.
- **Consumes:** business events from every module required to update read models.
