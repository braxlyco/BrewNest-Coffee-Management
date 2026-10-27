# ADR: Use ASP.NET Core Web API as the backend host

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

The system needs a stable API boundary for a separate browser client and module composition.

## Decision

Use ASP.NET Core minimal endpoints hosted by BrewNest.Api with OpenAPI, Problem Details, health checks, JWT, and SignalR.

## Consequences

The host remains a thin composition root; use-case logic belongs in module application services, not endpoints.
