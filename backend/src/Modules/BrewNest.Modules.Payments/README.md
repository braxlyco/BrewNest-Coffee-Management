# Payments module

## Responsibility

Customer payment recording, verification, settlement, refund execution, and replaceable wallet adapters.

## Owned data and boundaries

Payments owns payments, payer/gateway references, verification and settlement evidence, refund requests, and append-only status history in the `payments` schema. It references only BuildingBlocks and Contracts, verifies bills through `IPaymentsSalesPort`, serves Returns through `IReturnsPaymentPort`, and exposes `IPaymentsCodPort` for Delivery. It never reads another module's tables.

`IPaymentGateway` isolates providers. Initial Easypaisa and JazzCash adapters are simulators and use no credentials. Cash, COD, and bank transfer remain manual verification workflows.

The wallet adapters refuse authorization and refunds outside Development. In Development, the non-secret `Payments:Simulation:DeclinePayerReference` setting (default `+999000000000`) makes a wallet authorization decline for a negative-path demo; an ordinary valid test phone simulates success. No live money moves.

Wallet payer numbers use international format (a leading `+` and 8–15 digits). A refund is linked to its Sales order, and new requests cannot exceed verified payments less earlier active refunds. With one original payment, a wallet refund uses that wallet's sandbox adapter. Multiple tender payments use a clearly marked manual simulator because automatic allocation across methods is not yet represented. Legacy refunds without a payment link retain a null Sales order identifier; review those separately before reconciling historical balances.

## Permissions

- `payments.read`
- `payments.create`
- `payments.verify`
- `payments.settle`
- `payments.refunds.read`
- `payments.refunds.process`

Transactions are retained, mutable records use row-version concurrency, money uses `decimal(18,2)`, and audit/status timestamps use UTC `DateTimeOffset`.

## Migration

The additive `LinkRefundToSalesOrder` migration preserves existing refunds and fills the new nullable Sales order link where an original payment link exists. Apply it before serving new refund requests.

```powershell
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Payments --startup-project backend/src/BrewNest.Api --context PaymentsDbContext
```
