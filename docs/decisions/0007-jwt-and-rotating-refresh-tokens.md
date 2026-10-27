# ADR: JWT access tokens with rotating refresh tokens

- **Status:** Accepted
- **Date:** 2026-09-20

## Context

The Blazor WebAssembly frontend is a physically separate browser application and cannot use a server-rendered Identity cookie flow as its primary API credential. Access must remain revocable enough for administration changes without persisting reusable bearer credentials in plaintext.

## Decision

Issue short-lived HMAC-signed JWT access tokens containing the current roles and explicit permissions. Issue a cryptographically random opaque refresh token alongside each access token, persist only its SHA-256 hash, and rotate it on every refresh. Treat reuse of a rotated token as replay and revoke its complete token family.

Revoke affected refresh sessions on logout, password changes/resets, user disablement, user-role changes, role disablement, and role-permission changes. Keep signing keys and development seed credentials in user-secrets or environment variables.

## Consequences

The API can authorize requests without server-side access-token lookup, while refresh-session history remains auditable and revocable. A permissions change may remain represented in an already issued access token until its short expiry; refresh tokens are revoked immediately so changed access is not renewed. Production deployment must use a high-entropy signing key, HTTPS, secure browser and logging practices, and an appropriate access-token lifetime.
