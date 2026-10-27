# ADR 008: Use a unified typed Catalog item model

- Status: Accepted
- Date: 2026-09-20

## Context

Menu items, raw materials, and packaging materials share identifiers, names, units, groups, costs, lifecycle, audit fields, and inspection behavior. Three unrelated tables would duplicate these invariants and complicate recipes and later inventory references.

## Decision

Catalog stores them in one `Items` table with an explicit `ItemType` of `MenuItem`, `RawMaterial`, or `PackagingMaterial`. Application validation enforces type-specific fields. Recipes may produce menu items and consume only raw or packaging materials.

## Consequences

Later modules can reference one stable item identifier while retaining explicit item semantics. Type changes are intentionally unsupported because they would invalidate downstream meaning; replacement master data should be created instead.
