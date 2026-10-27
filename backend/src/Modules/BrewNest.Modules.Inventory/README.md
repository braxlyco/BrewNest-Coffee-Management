# Inventory module

## Responsibility

Lots, reservations, receipts, issues, adjustments, wastage, expiry controls, and the stock ledger.

## Owned data

Stock balances, batches/lots, reservations, stock movements, blocks, and expiry alerts.

## Implemented foundation

- Lot balances by Catalog item and warehouse identifier, including manufacturing and expiry dates.
- `Available = OnHand - Reserved`, protected by application validation and SQL check constraints.
- Goods receipts, goods issues, adjustments, wastage, reservations, releases, manual blocking, and automatic expiry blocking.
- An append-only movement ledger that records the resulting on-hand and reserved balance after every change.
- SQL Server `rowversion` optimistic concurrency on mutable lots and reservations.
- Configurable 15/7/3-day expiry thresholds supplied through `IInventoryExpiryPolicyProvider`.
- Explicit `inventory.*` authorization policies and Problem Details failures.

`IInventoryReservationPort` is the supported in-process boundary for future Kitchen, Sales, Production, and Procurement work. `IInventoryCatalogReferenceValidator` keeps Catalog references behind a contract; Inventory never queries Catalog tables. The initial adapter validates identifier shape and is intentionally replaceable by a Catalog-owned lookup adapter when cross-module reference queries are introduced.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

## Persistence

`InventoryDbContext` owns the `inventory` schema and a schema-local migration history table. The initial migration and idempotent deployment script are under `Infrastructure/Persistence`. Quantities use `decimal(18,3)`, audit timestamps use UTC `DateTimeOffset`, and transaction/ledger records are never physically deleted.

The opt-in `InventoryDevelopmentSeeder` adds idempotent sample lots, a reservation, and matching ledger entries only when the host is in Development and `DemoData:Enabled` is true.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
