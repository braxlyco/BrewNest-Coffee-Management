# Delivery module

## Responsibility

Rider availability and allocation, delivery progress, confirmation, and COD custody.

## Owned data

Rider availability state, delivery assignments and history, delivery status, and COD collection/submission records.

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
