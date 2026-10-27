# Payments frontend feature

Routes:

- `/payments` - workspace
- `/payments/transactions` - record, verify, and settle payments
- `/payments/refunds` - process approved refund requests

The feature keeps its models and typed API service locally, validates user input in Blazor, and relies on the API for authoritative bill, balance, permission, gateway, concurrency, and refund rules.

The payment form selects a billed Sales order and shows its remaining balance. Verify, settle, and refund actions appear only for users with the relevant permission. Wallet phone entry uses international format, and wallet operations are sandbox only. The Sales order dropdown requires `sales.orders.read` in addition to payment creation access.
