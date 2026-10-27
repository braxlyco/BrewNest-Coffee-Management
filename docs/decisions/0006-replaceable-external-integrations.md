# ADR: Hide external integrations behind replaceable adapters

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

Payment providers and future external systems can change and live credentials are inappropriate during initial development.

## Decision

Define module-owned interfaces such as IPaymentGateway and select simulator/sandbox or production adapters through DI and configuration.

## Consequences

Business rules remain provider-independent. Adapter-specific models and secrets stay at the infrastructure boundary; production adapters require separate security review.
