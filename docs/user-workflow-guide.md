# What each BrewNest workspace does

This guide describes the implemented app as of 20 October 2026. Open `https://localhost:7240` after starting both projects. The sidebar's implemented workspaces require login and the matching explicit permissions; an `Owner/Admin` development account receives all current permissions when development seeding is enabled. Granting a permission changes the next JWT, so sign out and in again. Buttons are convenience controls, while the API enforces permissions and returns Problem Details for rejected requests.

| Workspace/screen | What you do | What the system records or changes |
| --- | --- | --- |
| Overview `/` | Search or open a workspace. | Navigation only; cards mark future modules separately. |
| Identity login `/identity/login` | Sign in or sign out. | ASP.NET Identity checks the password/active account; JWT and rotating refresh tokens manage the session; security events record attempts and revocations. |
| Identity `/identity` and My security `/identity/profile` | Review your access and change your own password. | A password change revokes old sessions; no password is exposed to Blazor. |
| Users `/identity/users` | Create, activate/deactivate, reset passwords, and assign roles. | Identity keeps security/audit history; disabling or changing access revokes tokens. No security record is physically deleted. |
| Roles `/identity/roles` | Create roles and grant explicit permissions. | Role-permission assignments are versioned/revocable; affected users must sign in again. |
| Administration `/administration/settings` | Maintain company, branch, fiscal, numbering, alert, and allocation settings. | Configuration stays in the `administration` schema; changes have audit metadata. |
| Approvals `/administration/approvals` | Review/edit approval routes and PO value tiers. | Sales, Procurement, and Returns resolve configured required roles through a port; limits are not compiled into those modules. |
| Audit `/administration/audit` | Review administrative change history. | Read-only view of preserved audit records. |
| Catalog items `/catalog/items` | Manage menu items, ingredients, packaging, groups, and units. | Active master data, prices/costs, item type and inspection flags are owned by `catalog`; inactive data is excluded from new selections. |
| Catalog parties `/catalog/parties` | Manage customers and vendors. | Contact data is validated; phone numbers use international `+` and 8–15 digits. |
| Catalog resources `/catalog/resources` | Manage tables, warehouses, and rider profiles. | Sales/Inventory/Procurement selectors consume active references through contracts. Rider dispatch is not yet implemented. |
| Catalog recipes `/catalog/recipes` | Define menu-item BOM, yield, ingredients and wastage percentages. | Kitchen and Production consume recipe snapshots; they do not query Catalog tables. |
| Catalog quality `/catalog/quality` | Configure item/category inspection requirements and criteria. | Procurement receiving checks mandatory criteria before stock is released. |
| Inventory lots `/inventory/lots` | Receive stock into a warehouse/lot with manufacture and expiry dates. | Goods receipt increases on-hand; Catalog validates item/warehouse IDs; expired or blocked stock cannot be issued or reserved. |
| Inventory reservations `/inventory/reservations` | Reserve/release eligible lot stock. | Reserved stock reduces available but not on-hand; Kitchen and Production use the same Inventory boundary. |
| Inventory transactions `/inventory/transactions` | Issue, adjust, waste, block or unblock lots. | Physical movements change on-hand and append ledger records; negative stock is blocked. |
| Inventory movements `/inventory/movements` | Trace receipts, reservations, issues, adjustments and wastage. | Read-only immutable movement projection. |
| Inventory expiry `/inventory/expiry` | Review first/escalated/critical/expired alerts. | Alerts use configured thresholds; expired lots are blocked automatically. |
| Sales orders `/sales/orders` and detail `/sales/orders/{id}` | Create dine-in/takeaway/delivery orders, choose active customer/table/menu items, add/change lines, confirm, bill and close. | Each order snapshots prices, maintains status history, prevents conflicting table assignment, and issues a final bill. Delivery execution is future work. |
| Sales discount approvals `/sales/discount-approvals` | Request and manager-approve/reject discounts. | Requester, approver, reason, percentage and amount are kept; unapproved discounts block final billing. |
| Kitchen queue `/kitchen/queue` | Choose a confirmed Sales order, generate initial/additional KOTs, progress lines, or cancel a queued ticket. | Only unsent Sales lines become new KOT lines; active recipes drive FEFO ingredient reservation in Inventory; status history and SignalR queue updates are published. |
| Kitchen history `/kitchen/history` | Inspect older tickets and transitions. | Read-only KOT audit trail. |
| Production plans `/production/plans` | Plan a recipe-based quantity and release it. | A plan produces production orders with calculated BOM requirements. |
| Production orders `/production/orders` and detail | Reserve requirements, post actual consumption/wastage, complete or cancel before issue. | Inventory reservations and goods issues change stock; actual-versus-required variance and status history remain. |
| Procurement requisitions `/procurement/requisitions` | Request Catalog material and submit/approve it. | The requesting-department approval route governs advancement. |
| Procurement sourcing `/procurement/sourcing` | Invite vendors by RFQ, capture quotations, compare/select a quote. | A selected quote generates a PO with Administration's amount-based approval steps. |
| Procurement orders `/procurement/orders` | Approve/reject each PO step. | Required roles and decisions are preserved; receiving stays blocked until approval completes. |
| Procurement receipts `/procurement/receipts` | Receive PO material, inspect configured criteria, and return accepted stock to a vendor. | Passed/exempt material posts an Inventory lot; failed inspection stays restricted. Partial vendor returns deduct eligible physical stock and the screen shows remaining returnable quantity. |
| Returns requests `/returns/requests` | Select an original bill/line, request an RMA, approve by configured route, inspect and decide restock or disposal. | Sales verifies the original bill; sealed/package/batch/date/quality evidence gates restock through Inventory. Disposal does not increase stock. Completion creates a credit note and Payments refund request. |
| Returns voids `/returns/voids` | Request and approve an unpaid or paid order void. | Unpaid void cancels Sales; paid void needs Sales Manager and Finance, cancels Sales and queues a refund for the unrefunded paid amount. History is retained. |
| Payments transactions `/payments/transactions` | Record Cash, COD, Bank Transfer, Easypaisa or JazzCash against a bill; verify and settle. | Payment records stay in `payments`. Wallet adapters generate local simulator references; they do not move real money. In Development, use the displayed test number to simulate a declined wallet charge. |
| Payments refunds `/payments/refunds` | Process or reject approved return/void refunds. | Refunds are linked to their Sales order/original payment when uniquely identifiable; a settled wallet payment routes through the matching simulator, otherwise staff handle the manual simulation path. |

## When the kitchen cannot send an order

If a kitchen ticket reports that an ingredient is short, the message now names the material and shows the required and currently available quantities. Open Inventory > Stock lots to check a usable lot for that material. Receive more stock if needed, or review Inventory > Reservations for an unused reservation that can be released. Blocked and expired lots do not count as usable stock. Then return to Kitchen and send the same confirmed order again; the failed attempt does not create a ticket or reserve partial stock.

Staff-facing screens use item names, lot numbers, and document numbers. Database identifiers remain in the API for internal linking and are not intended as operational instructions.

## Important boundaries and limits

- `Delivery`, `Finance`, and `Reporting` have module shells/navigation but no completed transactional workflow. Do not interpret their cards as finished features or use the riders list as a dispatch screen.
- KOT generation currently starts from the Kitchen screen after Sales confirmation; it is not an automatic event consumer. SignalR updates the queue after a ticket change.
- Wallet/COD operations are simulations and records; no real bank, wallet, rider or finance settlement occurs. Never use live credentials or real customer data in the demo database.
- A single SQL Server database holds separate module schemas. Cross-module calls use contracts, but some multi-module writes are not yet a single atomic database transaction. Back up before production-like use and verify both source and destination records when retrying a failed operation.
- For your own verification, use the step-by-step [acceptance scenarios](e2e-test-plan.md) and run the automated tests described in the README.
