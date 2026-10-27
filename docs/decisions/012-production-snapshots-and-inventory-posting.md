# ADR 012: Production recipe snapshots and Inventory-owned posting

- **Status:** Accepted
- **Date:** 2026-09-24

## Context

Production needs repeatable BOM calculations and physical stock operations without coupling to Catalog or Inventory tables. Recipes may change after a plan is released, and actual use may differ from the calculated requirement.

## Decision

Plan release obtains the active recipe through a Catalog-owned port and stores calculated requirement snapshots in Production. Base BOM, expected wastage, required quantity, actual consumption, actual wastage, and signed variance remain separate fields.

Inventory owns FEFO lot selection, reservations, reservation release, goods issue, wastage movements, expiry/block checks, and negative-stock prevention through `IProductionInventoryPort`. Production stores workflow state and Inventory references/results but never edits stock directly.

Orders may be cancelled only before goods issue. Post-issue corrections require explicit future reversal or wastage evidence.

## Consequences

- Released production documents remain stable when recipes change.
- Stock invariants and ledger evidence remain centralized in Inventory.
- Actual usage above the reservation can consume eligible unreserved stock while still preventing negative inventory.
- Cross-module failure after physical issue requires operational reconciliation; later outbox/integration-event work can automate recovery without changing module ownership.
