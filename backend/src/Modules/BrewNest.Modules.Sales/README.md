# Sales module

## Responsibility

Dine-in, takeaway, and delivery order workflows through billing and closure.

Phase 6 implements order capture, Catalog-backed item/customer/table validation, item price snapshots, table exclusivity, manager-approved discounts, final bills, cancellation/closure, status history, optimistic concurrency, explicit permissions, thin endpoints, and the Sales Blazor workspace.

## Owned data

Sales documents, table assignments, discounts, bills, and sales status history.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

Catalog supplies active menu-item, customer, and dining-table references through `ISalesCatalogPort`. The host composes Catalog's adapter with the Sales service. Payment collection, KOT generation, inventory reservation, delivery execution, and billed-order void/refund workflows are intentionally outside this phase.

## Permissions

- `sales.orders.read`
- `sales.orders.manage`
- `sales.discounts.request`
- `sales.discounts.approve`
- `sales.billing.manage`
- `sales.orders.close`

## Persistence

`SalesDbContext` owns the `sales` schema and keeps its migration history there. Run its initial migration after the Identity migration that defines Sales permissions. Orders, lines, and discounts use SQL Server row versions; bills and status history are append-only evidence.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
