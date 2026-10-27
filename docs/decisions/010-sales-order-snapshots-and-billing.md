# ADR 010: Sales order snapshots and immutable billing

- **Status:** Accepted
- **Date:** 2026-09-23

## Context

Sales must validate Catalog references while preserving module boundaries. Historical orders and final bills must remain meaningful if a menu item is renamed or repriced. Discounts require manager approval and must not silently survive a later order-value change.

## Decision

Sales stores Catalog identifiers together with order-time item code, name, and price snapshots. It obtains current reference data through `ISalesCatalogPort`; it never reads Catalog tables directly.

An item change cancels any pending or approved discount, requiring a new request and decision for the revised order. Final billing is permitted only for a confirmed order with active lines and no pending discount. The bill stores immutable gross, discount, and net amounts. Billed voids, payment collection, KOT generation, inventory reservation, and delivery execution are deferred to their owning modules.

Orders, lines, and discounts use optimistic concurrency. Status transitions are append-only, and cancellation replaces physical deletion.

## Consequences

- Historical prices and descriptions remain stable.
- Catalog can evolve without changing completed Sales documents.
- Discount approval always corresponds to the latest order value.
- Later modules integrate through contracts/events rather than shared tables.
- The initial number provider is replaceable by an Administration-backed sequence adapter.
