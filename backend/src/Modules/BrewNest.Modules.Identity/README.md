# Identity module

## Responsibility

Authentication, JWT and rotating refresh-token issuance, users, roles, explicit permissions, password management, authorization policies, and security audit history.

## Owned data

Identity users, roles, permission grants, soft-revocable user-role assignments, hashed refresh tokens, and append-only security audit events. All tables and the migration-history table use the SQL Server `identity` schema.

## Allowed dependencies

- `BrewNest.BuildingBlocks` for cross-cutting abstractions and module registration.
- `BrewNest.Contracts` for stable API and integration contracts.
- Microsoft framework packages required by this module.

This module must not reference another business module or query another module's database schema directly. Cross-module work uses contracts, interfaces, or in-process integration events. The API host may reference this module to compose the modular monolith.

## Internal structure

- `Domain`: entities, value objects, enums, domain events, and business rules.
- `Application`: interfaces, use-case services, commands, queries, DTOs, and validators.
- `Infrastructure`: persistence, EF configurations, repositories, and external adapters.
- `Endpoints`: thin HTTP endpoint definitions.
- `DependencyInjection`: the module composition root.

## Security behavior

- Passwords use ASP.NET Core Identity hashing and a policy requiring at least 12 characters, upper/lowercase, a digit, a symbol, and four unique characters.
- Five failed logins trigger a 15-minute lockout.
- Access tokens are short lived and signed with a key supplied outside source control.
- Refresh tokens are random opaque values; only SHA-256 hashes are stored.
- Refresh tokens rotate on every use. Replaying a rotated token revokes the complete family.
- Security records are never physically deleted. Assignments and grants are revoked with actor and UTC timestamp history.
- User and role updates use row-version values for optimistic concurrency.

## HTTP surface

The module maps thin endpoints below `/api/identity`: login, refresh, logout, revoke-all, current user, change password, user management, role management, and permission assignment. Management endpoints use explicit `identity.*` permission policies and return contracts from `BrewNest.Contracts`, never EF entities.

## Persistence and migration

`IdentityDbContext` owns the module's persistence. The initial migration is `Infrastructure/Persistence/Migrations/*_InitialIdentity.cs` and seeds only the non-secret permission catalog. Apply it with:

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update --project backend/src/Modules/BrewNest.Modules.Identity --startup-project backend/src/BrewNest.Api --context IdentityDbContext
```

An idempotent SQL Server deployment script generated from that migration is stored beside it as `identity.sql`.

## Development seed

The hosted seeder is inactive unless the host is in `Development` and `Identity:DevelopmentSeed:Enabled` is true. Email, password, display name, database connection, and JWT signing key must be supplied through user-secrets or environment variables. The seed creates/updates the permission catalog and grants every current Identity permission to the development `Owner/Admin` role without logging any secret.
