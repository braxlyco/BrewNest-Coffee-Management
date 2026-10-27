# BrewNest architecture

## Architectural style

BrewNest is a modular monolith: one deployable ASP.NET Core API hosts independently organized business modules. This keeps deployment straightforward while preserving boundaries that make later changes localized. The system is not split into microservices, and it does not introduce message brokers, containers, or orchestration in the initial setup.

Every module follows the same layers:

```text
Domain -> Application -> Infrastructure / Endpoints
```

Domain contains business behavior and rules. Application coordinates use cases through commands, queries, interfaces, DTOs, and validation. Infrastructure implements persistence and external adapters. Endpoints translate HTTP requests and responses and contain no business logic.

## Frontend and backend separation

The Blazor WebAssembly client lives under `frontend/`; the API and modules live under `backend/`. They build as separate applications and communicate only over HTTP/JSON and SignalR. Shared public message shapes live in `BrewNest.Contracts`; EF entities never cross the API boundary.

Each backend business module has its own project. Each matching frontend feature has `Pages`, `Components`, `Services`, `Models`, `Validation`, and `State` folders. The root frontend `Services` directory is reserved for genuinely cross-cutting clients such as the base API and SignalR connections, not module business services.

## Module boundaries and composition

The API host references all module projects and supplies their assemblies to `AddBrewNestModules`. The loader discovers implementations of `IBrewNestModule`, calls each module's service registration, stores the composition roots in DI, and maps their endpoints after the host is built.

Modules may reference BuildingBlocks and Contracts. They must not reference another business module or read another module's schema directly. Cross-module work uses stable interfaces/contracts or in-process integration events. Integration event handlers must be idempotent where retries are possible.

## API communication

The browser uses a typed `IBrewNestApiClient` foundation over `HttpClient`. Module-specific typed clients remain inside their frontend feature folder. API errors use RFC-style Problem Details and include a trace identifier. The development OpenAPI document is exposed at `/openapi/v1.json`.

DTO validation occurs at both the Blazor boundary and the backend application boundary. Endpoint definitions remain thin and never expose EF Core entities.

## Database strategy

BrewNest uses one SQL Server database. Each module uses a dedicated schema (`identity`, `administration`, `catalog`, `sales`, and so on) and its own EF Core `DbContext` where practical. A module owns its tables and migrations. The Identity migration lives beneath `Infrastructure/Persistence/Migrations`; its migration-history table also uses the `identity` schema.

Internal keys are separate from human-readable document numbers. Auditable timestamps use `DateTimeOffset` and are stored as UTC. Money uses `decimal(18,2)` and material quantities normally use `decimal(18,3)`. Important aggregates use optimistic concurrency tokens. Transaction records are never physically deleted; reversal, cancellation, adjustment, return, or credit-note records preserve history.

## Authentication and authorization

ASP.NET Core Identity owns users, password hashes, lockout state, roles, and reset-token providers in the Identity module. Module-owned tables add explicit permissions, soft-revocable role grants, soft-revocable user-role assignments, hashed refresh tokens, and append-only security audit events in the `identity` schema.

The separate Blazor application authenticates with short-lived signed JWT access tokens and opaque rotating refresh tokens. Only refresh-token hashes are persisted. Reuse of an already rotated token is treated as replay and revokes its token family. Logout, password changes/resets, user disablement, role changes, and permission changes revoke affected refresh sessions. JWT signing keys, administrator seed credentials, and connection strings come from user-secrets or environment variables.

Authorization policies map one-to-one to explicit permission claims such as `identity.users.manage`. Endpoints require the narrow policy appropriate to their operation. Blazor route authorization improves the user experience, but the API remains the authoritative enforcement boundary. The frontend keeps its session in browser session storage, automatically refreshes near-expiry access tokens, and clears local state whenever refresh fails.

Module permission definitions are contributed through `IPermissionDefinitionProvider`. Identity remains the owner of permission and role-grant persistence, while modules declare only their own permission names and descriptions. This avoids a business-module reference from Identity and allows the development seeder to discover the complete permission catalog.

## Administration configuration

Administration owns mutable company, branch, fiscal-year, document-numbering, approval, expiry-alert, and expense-allocation settings in the `administration` schema. Required approval routes and the three purchase-order value tiers are initial data, not conditional C# logic, so authorized users can revise them later. The 15/7/3-day expiry thresholds are seeded configuration with ordering validation.

Configuration records use SQL Server row-version concurrency. Updates create an append-only audit entry containing actor, UTC timestamp, action, and serialized before/after DTO snapshots. Records are activated or deactivated rather than physically deleted. The module does not own business transactions and does not query Identity tables; its API policies rely on JWT permission claims.

## Catalog master data

Catalog owns reference data in the `catalog` schema. Menu items, raw materials, and packaging materials share one item aggregate with an explicit type because identity, costing, units, grouping, lifecycle, and inspection behavior are common. Type-specific validation prevents sales prices on material masters and requires a sales price for menu items.

Recipe/BOM lines and inspection criteria use soft-revoked revisions so configuration history is retained. Inspection requirements target exactly one item or item group. Catalog exposes identifiers and DTOs for later operational modules but owns no stock balance, purchase, sales, delivery, or production transaction.

## Inventory foundation

Inventory owns lot balances, reservations, and the append-only stock movement ledger in the `inventory` schema. Catalog item and warehouse identifiers cross the boundary through contracts; Inventory has no Catalog project reference and never reads Catalog tables. A replaceable reference-validation port allows a later Catalog-owned lookup adapter without coupling persistence models.

On-hand quantity represents physical stock. Reserved quantity represents committed but not yet issued stock, and available quantity is calculated as `OnHand - Reserved`. Application validation plus database constraints prevent negative quantities and prevent reserved stock from exceeding on-hand stock. Receipts increase physical stock; reservations change only reserved stock; issues reduce physical stock and optionally consume a reservation; adjustments and wastage always create movement evidence.

Mutable lots and reservations use SQL Server row versions. Movement records are immutable and carry the resulting balances, reference, actor, and UTC timestamp. Expired lots are automatically blocked before query, reservation, or issue operations. Alert thresholds come through `IInventoryExpiryPolicyProvider`, with 15/7/3-day configuration defaults and a future Administration adapter seam.

## Sales orders and billing

Sales owns dine-in, takeaway, and delivery orders in the `sales` schema. The aggregate stores item name, code, and price snapshots obtained through `ISalesCatalogPort`; the Catalog adapter implements that contract without exposing Catalog entities or allowing Sales to query Catalog tables. Customer and table identifiers remain optional references accompanied by the order-time customer/contact snapshot.

Order types have explicit validation: dine-in requires a table, delivery requires a customer name, phone, and address, and only one active order may occupy a table. Lines may be added or cancelled while an order is amendable. Any item change cancels an active discount decision so approval always applies to the current commercial value.

Every discount starts as a request with requester, UTC timestamp, reason, percentage, and calculated amount. A separate `sales.discounts.approve` policy protects the manager decision endpoint. Pending discounts block final billing; only approved discounts reduce the bill. Bills capture immutable gross, discount, and net amounts. Billed orders can be closed, while billed void/refund behavior remains reserved for the Returns and Payments phases.

Orders, lines, and discounts use row-version optimistic concurrency. Important state transitions append status history. Cancellation preserves the order and all lines; Sales never physically deletes transactional records. Human-readable order and bill numbers are generated behind `ISalesDocumentNumberProvider`, leaving an Administration-backed sequence adapter as a later integration.

## Real-time updates

SignalR is registered in the host at `/hubs/notifications`, with an authenticated hub foundation and a reconnecting Blazor client. Kitchen and alert modules will publish targeted, small notification messages; clients will reload authoritative state from the API rather than treating hub messages as the source of truth.

Kitchen now publishes `KitchenQueueChanged` after a KOT is generated, cancelled, or changes preparation state. The Blazor queue authenticates its hub connection with the current JWT, reconnects automatically, and reloads the authoritative API projection whenever a notification arrives. Notifications contain only a ticket identifier, action, and timestamp.

## Kitchen/KOT workflow

Kitchen owns tickets, ticket lines, external Inventory reservation references, and append-only preparation history in the `kitchen` schema. It never queries Sales, Catalog, or Inventory tables. Owner-side adapters provide confirmed Sales order snapshots, active Catalog recipe snapshots, and atomic Inventory reservation/release operations through shared contracts.

The first KOT includes all active order lines. Later KOTs contain only positive quantities not represented by an existing non-cancelled ticket. Recipe requirements include configured wastage percentages and are rounded to three decimal places. Inventory allocates eligible stock using first-expiry-first-out ordering and prevents partial reservation when total stock is insufficient.

A queued KOT may be cancelled only before preparation begins; cancellation releases its remaining Inventory reservations. Once preparation starts, cancellation requires a later reversal or wastage workflow rather than silently restoring stock. Line transitions are `Pending -> Preparing -> Ready`; aggregate ticket state is derived from its lines. Tickets and lines use optimistic concurrency and are never physically deleted.

## Production planning and execution

Production owns plans, generated production orders, snapshotted material requirements, actual consumption/wastage, variance, and append-only status history in the `production` schema. Catalog supplies active recipe snapshots through `IProductionRecipePort`; Production never queries Catalog tables. Releasing a plan calculates each ingredient's base BOM quantity and configured expected wastage at three-decimal precision.

Production order release calls `IProductionInventoryPort` to reserve eligible lots using FEFO allocation. Goods issue consumes the order's reservations first, may use additional unreserved eligible stock when actual usage exceeds the plan, blocks negative inventory, and posts consumption and wastage as distinct ledger movement types. Variance is `Actual Consumption + Actual Wastage - Required Quantity`.

## Procurement source-to-stock boundary

Procurement owns requisitions, RFQs, quotation snapshots, purchase orders, sequential approval decisions, receipts, quality results, vendor returns, and status history in schema `procurement`. Catalog supplies active references and inspection definitions through `IProcurementCatalogPort`; Administration resolves editable amount-based routes through `IProcurementApprovalPolicyPort`; Inventory accepts inspected receipts and vendor returns through `IProcurementInventoryPort`. Procurement never reads another module's tables.

Approval steps are snapshotted when a quotation is selected. Failed inspection material is rejected before Inventory posting; accepted and inspection-exempt lots post with their Procurement document reference.

Draft or released orders can be cancelled; released orders first release remaining reservations. An order cannot be cancelled after goods issue because physical reversal or additional wastage evidence is required. Completion is allowed only after issue, and completing every order completes the parent plan. Plans, orders, and requirements use optimistic concurrency and are never physically deleted.

## Returns, voids, and refund requests

Returns owns RMA records, inspection evidence, approval decisions, disposition, credit notes, refund references, void requests, and append-only history in the `returns` schema. It never reads Sales, Administration, Inventory, Payments, or Catalog tables. Owner-side implementations of `IReturnsSalesPort`, `IReturnsApprovalPolicyPort`, `IReturnsInventoryPort`, and `IReturnsPaymentPort` perform those bounded operations.

Original-bill verification and billed-quantity limits precede an RMA. Approval routes are resolved from Administration and snapshotted as sequential decisions. A returned food line may be restocked only after all sealed/package/manufacturing/expiry/batch/quality evidence passes and the configured Store/QC role approves; otherwise the line is disposed without increasing stock. Completion creates an immutable credit note and requests a refund through Payments.

Void classification is determined from the Payments-owned state. Before-payment voids delegate cancellation to Sales after their configured approval. After-payment voids follow the configured multi-role route and create a Payments refund request. Row versions protect mutable documents, while UTC actor-attributed history preserves every transition.

## Integration adapters

External dependencies are placed behind module-owned interfaces. Payments defines `IPaymentGateway`; Easypaisa and JazzCash currently use simulator adapters that generate visibly synthetic references and require no secrets. Adapter selection is performed through DI so a live provider can later replace a simulator without changing payment application rules.

## Payments, settlement, and refunds

Payments owns payment records, verification, settlement, refunds, and append-only state history in schema `payments`. It verifies an immutable Sales bill snapshot through `IPaymentsSalesPort` and prevents committed amounts from exceeding net bill value. It exposes narrow Returns and future Delivery ports rather than sharing tables.

Cash, COD, and bank transfer begin pending verification. Wallet sandbox authorization may verify Easypaisa or JazzCash immediately. Only verified payments may settle. Refund requests are idempotent by source reference and retain completed, failed, or cancelled outcomes. Mutable documents use row-version concurrency, money uses `decimal(18,2)`, and all audit times use UTC `DateTimeOffset`.

## Cross-cutting policies

- Async I/O throughout application and infrastructure code.
- UTC `DateTimeOffset` audit timestamps with created/modified actors.
- Status history for important business documents.
- No negative inventory and no issue/reservation of blocked stock.
- Structured logging without secrets or sensitive payment data.
- Compiler warnings are treated as errors for repository code.
