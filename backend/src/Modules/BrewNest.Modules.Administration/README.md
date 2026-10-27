# Administration module

## Responsibility

Company and branch settings, fiscal years, numbering, approval policies, allocation settings, alerts, and audit oversight.

## Owned data

Configuration records, approval definitions and tiers, document sequences, and audit records.

## Implemented use cases

- Read and update company settings.
- Create and update branches, fiscal years, and human-readable document-number sequences.
- Create and update approval policies with soft-revocable role steps and non-overlapping value bands.
- Read and update ordered expiry-alert thresholds.
- Create and update expense-allocation settings.
- Review append-only Administration audit entries.

The initial migration seeds all confirmed approval routes, three editable purchase-order tiers, company defaults, and 15/7/3-day expiry thresholds. It creates only configuration data; transactional documents remain out of scope.

## Authorization

- `administration.settings.read`
- `administration.settings.manage`
- `administration.approvals.read`
- `administration.approvals.manage`
- `administration.audit.read`

The module contributes these definitions through `IPermissionDefinitionProvider`; Identity owns grants and emits them as JWT claims.

## Persistence

`AdministrationDbContext` uses the `administration` SQL Server schema and a schema-local `__EFMigrationsHistory` table. Configuration aggregates use `rowversion` optimistic concurrency, UTC `DateTimeOffset` audit fields, and inactive/revoked states instead of physical deletion. Migrations live in `Infrastructure/Persistence/Migrations` and the idempotent deployment script lives in `Infrastructure/Persistence/Scripts`.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.
