# Kitchen module

## Responsibility

KOT creation, kitchen workflow, preparation status, instructions, and real-time kitchen notifications.

Phase 7 implements initial and additional KOT generation from confirmed Sales snapshots, Catalog recipe expansion, Inventory FEFO reservations, special-instruction snapshots, preparation queues and history, queued-only cancellation with reservation release, optimistic concurrency, explicit permissions, and SignalR queue notifications.

## Owned data

KOT documents, preparation queues, instructions, and kitchen status history.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

The owner modules implement `IKitchenSalesOrderPort`, `IKitchenRecipePort`, and `IKitchenInventoryPort`. Kitchen persists only external identifiers and immutable snapshots. It never queries the `sales`, `catalog`, or `inventory` schemas.

## Permissions

- `kitchen.queue.read`
- `kitchen.tickets.generate`
- `kitchen.preparation.manage`
- `kitchen.tickets.cancel`

## Persistence and notifications

`KitchenDbContext` owns the `kitchen` schema and its migration history. Tickets and lines use SQL Server row versions; preparation history is append-only. Successful changes publish `KitchenQueueChanged` through the authenticated shared SignalR hub. The configured ticket prefix is read from `Kitchen:DocumentNumbers:TicketPrefix`.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
