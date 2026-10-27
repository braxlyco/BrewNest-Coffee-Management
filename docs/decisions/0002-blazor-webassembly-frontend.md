# ADR: Use a separate Blazor WebAssembly frontend

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

The university requires C# and the browser client must remain physically separate from the backend.

## Decision

Build a standalone Blazor WebAssembly project that communicates through HTTP/JSON and SignalR.

## Consequences

Frontend application logic stays in C#, deployment remains separable, and JWT is required because the browser is not server-hosted.
