# ADR 007: Store mutable operating rules as Administration data

- Status: Accepted
- Date: 2026-09-20

## Context

Approval routes, purchase-order limits, expiry thresholds, numbering, and allocation rules must change without a C# deployment. Hard-coded values would make policy changes slow and difficult to audit.

## Decision

The Administration module owns these settings in the `administration` schema. Confirmed defaults are migration seed data. Authorized API operations update them with optimistic concurrency and append-only audit entries. Other modules will consume stable contracts or interfaces rather than query Administration tables.

## Consequences

Operational rules can be changed and audited without recompilation. Consumers must handle inactive or missing configuration explicitly, and future cross-module lookups require defined ports or integration events.
