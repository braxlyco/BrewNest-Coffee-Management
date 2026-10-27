# ADR 009: Inventory ledger and lot balances

- Status: Accepted
- Date: 2026-09-23

## Context

BrewNest must answer current availability quickly while preserving auditable evidence for every physical and reserved stock change. Stock also needs batch traceability, expiry blocking, and safe cross-module reservation access without coupling Inventory to future transaction modules.

## Decision

Inventory stores a mutable, row-versioned balance per Catalog item, warehouse, and lot alongside an append-only movement ledger. On-hand and reserved quantities are stored; available quantity is derived as `OnHand - Reserved`. Database constraints and application rules prohibit negative balances and `Reserved > OnHand`.

Reservations are explicit records with quantity, released quantity, consumed quantity, source reference, status, audit fields, and concurrency token. Receipts, issues, adjustments, wastage, reservation changes, and block changes create ledger rows. Expired lots are blocked before they can be reserved or issued.

Catalog keys cross the boundary through contracts. Future Kitchen, Sales, Production, Procurement, and Returns integrations call Inventory ports rather than its tables.

## Consequences

Current balances remain efficient to query, while the ledger supports audit and reconciliation. Each mutation must update the balance and append its movement in one database transaction. Ledger entries are never edited or deleted; corrections use new adjustments or reversal-style operations. Cross-module adapters can evolve without adding business-module project references.
