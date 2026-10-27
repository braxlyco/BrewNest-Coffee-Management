# Returns module

## Responsibility

Return authorization, original-bill verification, inspection, restocking/disposal decisions, credit notes, refund requests, and before/after-payment void workflows.

## Owned data

Return requests, RMA numbers, lines and inspection evidence, sequential approval decisions, dispositions, credit notes, refund references, void records, and status history in the `returns` SQL Server schema.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

The host supplies owner-side adapters for `IReturnsSalesPort`, `IReturnsApprovalPolicyPort`, `IReturnsInventoryPort`, and `IReturnsPaymentPort`. Returned food enters stock only after the complete evidence gate passes; otherwise it is recorded as disposal and never increases available stock. All mutable aggregates use row-version concurrency, all times are UTC `DateTimeOffset`, and transaction records are retained.

An approved paid void requests a refund for the verified, unrefunded balance and cancels the Sales order through its contract. An unpaid void cancels the order without a refund. Duplicate active/completed void requests are rejected. Return inspection checks the available verified payment before posting the final disposition and credit; an unpaid or partly paid bill cannot currently be refunded beyond its collected amount. Cross-module posting is sequential and retries use idempotent document references; there is no distributed transaction.

## Permissions

- `returns.read`
- `returns.manage`
- `returns.approve`
- `returns.inspect`
- `returns.voids.manage`
- `returns.voids.approve`
- `returns.refunds.manage`

## Migration

The initial migration is in `Infrastructure/Persistence/Migrations`. Apply it with:

```powershell
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Returns --startup-project backend/src/BrewNest.Api --context ReturnsDbContext
```

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
