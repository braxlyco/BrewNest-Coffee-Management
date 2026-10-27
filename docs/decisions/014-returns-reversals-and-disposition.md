# ADR 014: Returns use explicit reversals and evidence-based disposition

- **Status:** Accepted
- **Date:** 2026-09-26

## Context

Customer returns and sale voids affect records owned by Sales, Inventory, Payments, and later Finance. Direct table updates would break module ownership and could erase the audit trail required for approvals, food safety, and refunds.

## Decision

Returns owns the RMA, inspection evidence, configured approval snapshots, disposition, credit-note record, refund reference, void request, and status history in schema `returns`. It communicates with owner modules exclusively through shared ports implemented by those owners.

Restocking is permitted only when every required sealed/package/manufacturing/expiry/batch/quality check passes and the configured responsible role approves it. Every failure is routed to disposal and cannot increase stock. Unpaid voids ask Sales to cancel; paid voids ask Payments to create a refund request. Original documents remain unchanged except through their owner's explicit operation.

## Consequences

- Cross-module persistence boundaries remain enforceable by architecture tests.
- Credit, refund, inventory, and Sales effects retain separate traceable references.
- Approval configuration can change without modifying Returns code.
- Later Finance posting can consume stable credit/refund contracts without Returns owning accounting ledgers.
