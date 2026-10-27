# Development roadmap

Each phase ends with reviewed domain rules, migrations, API contracts, frontend validation, tests, and updated documentation before the next phase begins.

1. **Foundation (complete)** - solution structure, module composition, API concerns, frontend shell, contracts, tests, documentation, and CI-ready commands.
2. **Identity and permissions (complete)** - Identity model, JWT/rotating refresh flow, roles, explicit permissions, password management, development bootstrap administrator, security audit history, authorization policies, management UI, migration, and automated security tests.
3. **Administration and configuration (complete)** — company/branch/fiscal settings, document numbering, configurable approval rules and PO tiers, expense allocation, expiry alerts, optimistic concurrency, permission-aware UI, audit log, migration, and automated tests.
4. **Catalog and master data (complete)** — typed items/materials, UOM, groups, customers, vendors, tables, warehouses, riders, recipes/BOMs, item/group inspection criteria, permissions, status history, UI, migration, and automated tests.
5. **Inventory foundation (complete)** — stock ledger, lots/batches, manufacturing and expiry dates, reservations/releases, receipt/issue/adjustment/wastage, blocking, configurable expiry alerts, concurrency, permissions, management UI, migration, ports, and negative-inventory safeguards.
6. **Sales (complete)** — dine-in/takeaway/delivery orders, table assignment, add-ons, manager-approved discounts, immutable final bills, closure/cancellation, permissions, UI, migration, status history, and automated tests.
7. **Kitchen/KOT (complete)** — initial/additional KOT generation, recipe-based FEFO ingredient reservations, preparation board/status and history, instructions, cancellation/release rules, permissions, SignalR updates, UI, migration, and automated tests.
8. **Production (complete)** — plans/orders, recipe snapshot calculations, release and FEFO reservation, material requirements, goods issue, actual consumption, completion, wastage, variance, permissions, UI, migration, and automated tests.
9. **Procurement (complete)** — PR, configurable approval, RFQ, quotation comparison and selection, tiered PO approval, inspection, goods receipt into Inventory, vendor returns, permissions, UI, migration, and tests.
10. **Returns (complete)** — original-bill-verified return requests/RMAs, configurable sequential approval, returned-item inspection, evidence-gated restock or disposal, credit notes, refund requests, before/after-payment voids, permissions, UI, migration, and automated tests.
11. **Payments (complete)** — cash/COD/bank transfer recording, explicit verification, settlement, replaceable gateway abstraction, Easypaisa/JazzCash simulators, refund execution, permissions, UI, migration, and automated tests.
12. **Delivery** — rider availability, balanced automatic assignment, override, history, status, confirmation, COD collection, and submission.
13. **Finance** — A/P invoices, outgoing payments, expenses, allocation, configurable posting rules, COGS, and profit calculations.
14. **Reporting** — operational/financial projections, daily/monthly reports, alerts, dashboards, exports, and reconciliation checks.
15. **Testing, deployment and documentation** — end-to-end scenarios, security/performance/accessibility testing, migration rehearsals, deployment guide, and user guide.
