# BrewNest — Coffee Shop Management System

BrewNest is a web-based coffee shop operations system covering sales, kitchen tickets, inventory, production, procurement, returns, payments, and administration. It uses a modular .NET backend and a Blazor WebAssembly frontend.

The implemented workflows run from Identity through Payments. Delivery dispatch, Finance, and Reporting are planned modules; their navigation shells are not presented as finished operations. Wallet integrations use local simulators and do not move real money.

## Technology stack

- .NET 10 LTS and C#
- ASP.NET Core Web API with built-in OpenAPI
- Blazor WebAssembly
- Entity Framework Core with SQL Server
- ASP.NET Core Identity and JWT bearer authentication
- SignalR for KOT and alert notifications
- xUnit
- Built-in dependency injection and Git

Vendored Bootstrap assets retain their [MIT license](frontend/src/BrewNest.Web/wwwroot/lib/bootstrap/LICENSE).

## Solution structure

The frontend uses a responsive café-themed workspace with searchable module cards, quick actions, mobile navigation, and shared form/table styles. Implemented module cards open their actual pages; Delivery, Finance, and Reporting are clearly marked as future work. See [UI design and review checklist](docs/ui-design.md) for interaction details and browser verification steps.

```text
backend/src/BrewNest.Api                 API host/composition root
backend/src/BrewNest.BuildingBlocks      Shared technical abstractions
backend/src/Modules/*                    Independently organized business modules
backend/tests/*                          Unit and integration tests
frontend/src/BrewNest.Web                Separate Blazor WebAssembly application
shared/BrewNest.Contracts                Transport and integration contracts
docs                                     Architecture, rules, roadmap, and ADRs
```

The API may reference modules. A module may reference only `BrewNest.BuildingBlocks`, `BrewNest.Contracts`, and required framework packages. Business modules must not reference one another or access another module's tables.

## Prerequisites

- .NET SDK 10.0.401 or a compatible newer 10.0 patch
- SQL Server 2022 or later for module persistence work
- A trusted local ASP.NET Core HTTPS development certificate
- Git

Verify the SDK:

```powershell
dotnet --info
```

## Local setup

For a fresh Windows laptop and SQL Server Express demo database, follow [Windows laptop setup](docs/windows-laptop-setup.md). The database and .NET user-secrets do not travel with a Git pull. The script's default mode only checks prerequisites; `-Apply` initializes the local database after a private administrator-password prompt.

```powershell
dotnet restore BrewNest.sln
dotnet build BrewNest.sln --no-restore
dotnet test BrewNest.sln --no-build
```

If HTTPS has not been configured locally:

```powershell
dotnet dev-certs https --trust
```

## Run the backend

```powershell
dotnet run --project backend/src/BrewNest.Api --launch-profile https
```

The development endpoints are:

- Health: `https://localhost:7241/health`
- Registered modules: `https://localhost:7241/api/modules`
- OpenAPI document: `https://localhost:7241/openapi/v1.json`
- SignalR hub: `https://localhost:7241/hubs/notifications`

## Run the frontend

In a second terminal:

```powershell
dotnet run --project frontend/src/BrewNest.Web --launch-profile https
```

Open `https://localhost:7240`. The API base URL is read from `frontend/src/BrewNest.Web/wwwroot/appsettings.json`.

## Configuration and secrets

`backend/src/BrewNest.Api/appsettings.json` contains non-secret defaults only. Copy values from `appsettings.Development.example.json` conceptually, then store actual development secrets outside source control:

```powershell
dotnet user-secrets init --project backend/src/BrewNest.Api
dotnet user-secrets set "ConnectionStrings:BrewNestDatabase" "<local SQL Server connection string>" --project backend/src/BrewNest.Api
dotnet user-secrets set "Jwt:SigningKey" "<at least 32 cryptographically random bytes>" --project backend/src/BrewNest.Api
dotnet user-secrets set "Identity:DevelopmentSeed:Enabled" "true" --project backend/src/BrewNest.Api
dotnet user-secrets set "Identity:DevelopmentSeed:AdministratorEmail" "<development administrator email>" --project backend/src/BrewNest.Api
dotnet user-secrets set "Identity:DevelopmentSeed:AdministratorPassword" "<development-only strong password>" --project backend/src/BrewNest.Api
dotnet user-secrets set "Identity:DevelopmentSeed:AdministratorFullName" "<administrator display name>" --project backend/src/BrewNest.Api
```

Restore the repository-local EF tool and apply the migrations before first use. Each module owns its context, schema, migrations, and migration-history table:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Identity --startup-project backend/src/BrewNest.Api --context IdentityDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Administration --startup-project backend/src/BrewNest.Api --context AdministrationDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Catalog --startup-project backend/src/BrewNest.Api --context CatalogDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Inventory --startup-project backend/src/BrewNest.Api --context InventoryDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Sales --startup-project backend/src/BrewNest.Api --context SalesDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Kitchen --startup-project backend/src/BrewNest.Api --context KitchenDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Production --startup-project backend/src/BrewNest.Api --context ProductionDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Procurement --startup-project backend/src/BrewNest.Api --context ProcurementDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Returns --startup-project backend/src/BrewNest.Api --context ReturnsDbContext
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Payments --startup-project backend/src/BrewNest.Api --context PaymentsDbContext
```

Development administrator seeding runs only when the environment is `Development` and the explicit seed flag is enabled. Environment variables are also supported, for example `ConnectionStrings__BrewNestDatabase` and `Jwt__SigningKey`. Never commit database credentials, administrator passwords, signing keys, or live payment credentials.

After starting both applications, browse to `https://localhost:7240/identity/login`. Access tokens are short-lived JWTs; refresh tokens rotate on use and are revoked on logout, password changes, user disablement, role changes, and permission changes. After applying the Identity permission migration, grant the appropriate `administration.*` permissions to a role and sign in again before opening `https://localhost:7240/administration`.

Catalog is available at `https://localhost:7240/catalog` after granting the appropriate `catalog.*` permissions and signing in again.

Inventory is available at `https://localhost:7240/inventory` after applying both the latest Identity permission migration and the Inventory migration, granting the required `inventory.*` permissions, and signing in again. Expiry thresholds are non-secret configuration at `Inventory:Expiry`; their defaults are 15, 7, and 3 days.

Sales is available at `https://localhost:7240/sales`. Apply the Sales migration and latest Identity permission migration, grant the required `sales.*` permissions to a role, and sign in again to refresh the JWT permission claims. Sales covers order entry through billing and closure; Kitchen consumes confirmed-order snapshots for KOT generation. Payments and Returns handle their own payment, refund, and void records through contracts. Delivery execution remains future work.

Kitchen is available at `https://localhost:7240/kitchen`. Apply the Kitchen migration and latest Identity permission migration, grant the `kitchen.*` permissions, and sign in again. Generate a KOT using the ID of a confirmed Sales order whose menu items have active Catalog recipes and sufficient eligible Inventory stock. Live queue updates use the authenticated SignalR hub.

Production is available at `https://localhost:7240/production`. Apply the Production and latest Identity migrations, grant `production.*` permissions, and sign in again. Plans accept Catalog menu items with active recipes; releasing an order reserves eligible Inventory stock, posting goods issue deducts actual consumption and wastage, and completion preserves calculated variance.

Procurement is available at `https://localhost:7240/procurement`. It provides requisition, sourcing, purchase-order approval, receiving/inspection, and vendor-return workflows. Approval routes remain editable in Administration; accepted stock posts into Inventory through contracts.

Returns is available at `https://localhost:7240/returns`. Apply the Returns and latest Identity migrations, grant the required `returns.*` permissions, and sign in again. Return requests verify an original Sales bill, follow configured approvals, require explicit inspection evidence before restocking, create credit notes and simulator refund requests, and preserve before/after-payment void approval history.

Payments is available at `https://localhost:7240/payments`. Apply the Payments and latest Identity migrations, grant the required `payments.*` permissions, and sign in again. Cash, COD, and bank transfers use staff verification; Easypaisa and JazzCash use replaceable sandbox adapters. Verified payments may be settled, and approved Returns requests appear in the refund queue.

## Development demo data

Optional, idempotent demo seeders populate Catalog and Inventory only in the `Development` environment. Enable them locally with:

```powershell
dotnet user-secrets set "DemoData:Enabled" "true" --project backend/src/BrewNest.Api
```

Restart the backend once. The sample includes units, groups, menu items, ingredients, packaging, customers, a vendor, tables, a warehouse, riders, recipes, inspection criteria, four stock lots, one reservation, ledger entries, and a critical-expiry milk lot. Disable future seeding with the same command using `false`; existing demo records are retained so audit references are not physically deleted.

See [architecture](docs/architecture.md), [module map](docs/module-map.md), [business rules](docs/business-rules.md), [screen/workflow guide](docs/user-workflow-guide.md), [end-to-end acceptance scenarios](docs/e2e-test-plan.md), and the [development roadmap](docs/development-roadmap.md).
