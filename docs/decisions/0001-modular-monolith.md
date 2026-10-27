# ADR: Use a modular monolith

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

BrewNest spans several business capabilities while still needing simple deployment and local operation.

## Decision

Build one ASP.NET Core API host with independent module projects and enforced dependency rules.

## Consequences

This reduces operational complexity and permits local transactions while requiring discipline around module schemas, contracts, and integration events. Microservices are explicitly deferred.
