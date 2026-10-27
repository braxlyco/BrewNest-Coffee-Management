# ADR 015: Replaceable payment gateways and owned payment ledger

- **Status:** Accepted
- **Date:** 2026-09-26

## Decision

Payments owns payment, verification, settlement, refund, and status-history records in schema `payments`. Sales supplies immutable bill snapshots through a contract. Returns requests refunds through a Payments-owned port, and Delivery will submit COD through a dedicated port.

Easypaisa and JazzCash implement `IPaymentGateway`. Development registrations are sandbox simulators without keys or network calls. A live adapter can later replace either registration without changing application rules or stored contracts.

## Consequences

- Other modules cannot mutate payment tables.
- Overpayment, verification, settlement, and refund rules have one owner.
- Provider secrets remain external to source and provider replacement is localized.
- Simulator references are visibly marked and cannot be confused with live gateway receipts.
