# ADR: Organize code module first

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

Frequent feature changes must be easy to locate without traversing global technical-layer folders.

## Decision

Give every backend capability its own project and every frontend capability its own feature folder, with consistent internal layers.

## Consequences

Changes stay localized and ownership is clear, at the cost of repeated folder structure and explicit cross-module contracts.
