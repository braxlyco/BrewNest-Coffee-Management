# Procurement module

## Responsibility

Owns requisitions, RFQs, vendor quotation snapshots/comparison, purchase orders and approval decisions, goods receipts, inspection results, vendor returns, and append-only status history in SQL schema `procurement`.

## Owned data

Purchase requisitions/lines, RFQs/invitations, quotations/lines, purchase orders/lines, approval decisions, receipt lots, criterion results, vendor returns, and status history.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Catalog references use `IProcurementCatalogPort`, configurable Administration routes use `IProcurementApprovalPolicyPort`, and accepted receipts/vendor returns use `IProcurementInventoryPort`.

Money uses `decimal(18,2)`, quantities use `decimal(18,3)`, audit timestamps are UTC `DateTimeOffset`, and mutable documents use SQL row versions. The initial migration and idempotent deployment script are under `Infrastructure/Persistence`.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
