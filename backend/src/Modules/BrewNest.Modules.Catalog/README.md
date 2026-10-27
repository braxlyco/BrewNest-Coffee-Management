# Catalog module

## Responsibility

Shared café master data including items, materials, units, recipes, parties, tables, warehouses, and riders.

## Owned data

Menu and material masters, units, groups, recipes/BOM definitions, customers, vendors, tables, warehouses, and rider profiles.

## Implemented capabilities

- Menu, raw-material, and packaging-material masters with type-specific validation.
- Units of measure and item groups.
- Customers, vendors, dining tables, warehouses, and rider profiles/availability.
- Recipe/BOM headers with decimal yield, material quantities, wastage percentages, and soft-revoked line revisions.
- Item- or group-level quality-inspection requirements with configurable pass/fail, numeric, or text criteria.
- Soft activation/deactivation and append-only status history.
- Customer/vendor phones, when provided, and rider phones use an international `+` country code with 8–15 digits total. Existing stored contacts are retained; new edits must meet this format.
- The Catalog-owned `IInventoryCatalogReferenceValidator` confirms that a goods-receipt item and warehouse are active, without allowing Inventory to query Catalog tables.

Catalog deliberately contains no stock balances, reservations, orders, receipts, production execution, sales, or procurement transactions.

## Authorization

- `catalog.masterdata.read` / `catalog.masterdata.manage`
- `catalog.recipes.read` / `catalog.recipes.manage`
- `catalog.quality.read` / `catalog.quality.manage`

## Persistence

`CatalogDbContext` owns the `catalog` schema and schema-local migration history. Money is `decimal(18,2)`; recipe quantities are `decimal(18,3)`. Master aggregates use SQL Server `rowversion`, UTC `DateTimeOffset` audit fields, and soft status changes. Migrations and the idempotent deployment script are under `Infrastructure/Persistence`.

The opt-in `CatalogDevelopmentSeeder` adds idempotent `DEMO-*` master data only when the host is in Development and `DemoData:Enabled` is true.

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
