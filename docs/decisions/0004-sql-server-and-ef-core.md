# ADR: Use SQL Server and Entity Framework Core

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

The domain is transaction-heavy, auditable, relational, and must use SQL Server for persistence.

## Decision

Use one SQL Server database, module-owned schemas/DbContexts, and EF Core mappings/migrations inside each module.

## Consequences

One database supports consistency and manageable deployment. Schema ownership and no cross-module table access protect modularity.
