# ADR 013: Procurement contract boundaries

- Status: Accepted
- Date: 2026-09-26

## Decision

Procurement owns source-to-stock documents in schema `procurement`. Catalog references, Administration approval routes, and Inventory postings cross module boundaries only through shared in-process contracts. Approval steps are snapshotted at purchase-order creation, and stock is posted only after required inspection passes.

## Consequences

Approval thresholds remain configurable, failed material cannot enter unrestricted stock, and owner modules protect their persistence while the system remains one modular-monolith API and SQL Server database.
