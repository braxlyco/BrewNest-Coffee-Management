# Production module

## Responsibility

Production planning and execution based on recipes, material requirements, actual consumption, and variance.

Phase 8 implements dated plans, order generation from Catalog recipe snapshots, BOM and expected-wastage calculation, FEFO Inventory reservation, goods issue, actual consumption/wastage, variance, completion, cancellation controls, optimistic concurrency, explicit permissions, thin endpoints, and a Blazor management workspace.

## Owned data

Production plans, production documents, material requirements, consumption, wastage, and variance records.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

Catalog implements `IProductionRecipePort`; Inventory implements `IProductionInventoryPort`. Production persists snapshots and external identifiers only and never reads either module's schema.

## Permissions

- `production.read`
- `production.plans.manage`
- `production.orders.release`
- `production.materials.issue`
- `production.completion.manage`

## Persistence

`ProductionDbContext` owns the `production` schema. Plans, orders, and requirements use SQL Server row versions. Status history is append-only. Document prefixes come from `Production:DocumentNumbers`.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
