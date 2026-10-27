# ADR 011: Contract-driven Kitchen workflow and authoritative real-time queue

- **Status:** Accepted
- **Date:** 2026-09-24

## Context

Kitchen needs confirmed Sales lines, Catalog recipes, and Inventory reservations, but module boundaries prohibit direct table access. Multiple browser dashboards must see KOT changes quickly without treating transient notifications as durable state.

## Decision

Sales, Catalog, and Inventory implement shared Kitchen-facing ports inside their own modules. Kitchen persists snapshots and external identifiers only. Inventory performs atomic requirement validation and FEFO lot allocation, while Kitchen stores returned reservation identifiers for traceability.

Kitchen publishes a small `KitchenQueueChanged` SignalR notification only after its database change succeeds. Clients authenticate the hub with their JWT and reload the queue from the HTTP API. The database remains authoritative.

Queued, unstarted tickets can be cancelled and release reservations. Started tickets cannot be cancelled in this phase because released ingredients may already have entered preparation; later Production/Inventory workflows will record reversal or wastage.

## Consequences

- Module schemas remain private.
- Additional KOTs can be generated without duplicating prior quantities.
- Inventory safeguards and expiry blocking remain centralized.
- Lost or duplicate notifications cannot corrupt browser state.
- Cross-module persistence is coordinated with compensating reservation release if Kitchen ticket creation fails.
