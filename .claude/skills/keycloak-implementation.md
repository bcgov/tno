# Skill: Keycloak Implementation

## Where Keycloak Is Implemented

- Container/runtime config: `auth/keycloak/**`
- API auth wiring: `api/net/Program.cs`
- Keycloak helper layer: `api/net/Keycloak/**`
- Shared Keycloak client library: `libs/net/keycloak/**`
- Admin endpoints for sync/ops: `api/net/Areas/Admin/Controllers/KeycloakController.cs`

## API Authentication Flow

- API uses JWT Bearer auth (`AddAuthentication().AddJwtBearer(...)`).
- Authority, audience, issuer, validation flags come from `Keycloak` config section.
- SignalR token support is handled in JWT events (`OnMessageReceived`) for hub paths.
- Client-role authorization is enabled by custom policy provider + handler.

## Required Key Config (Environment)

- `Keycloak__Authority`
- `Keycloak__Audience`
- `Keycloak__Issuer`
- `Keycloak__ValidateIssuer`
- `Keycloak__ValidateAudience`
- `Keycloak__Secret` (optional signing secret path)
- Service account block under `Keycloak__ServiceAccount__*`

## Local Setup Notes

- Keycloak depends on Postgres in compose.
- Realm import files live in `auth/keycloak/config` and mount into container.
- After first startup, verify realm exists at `http://localhost:40001`.
- Update API Keycloak secrets using `./tools/scripts/kc-key-update.sh` when needed.

## Sync And User Management

- `IKeycloakHelper` handles user activation, role sync, and key linking.
- Keycloak and local DB roles/users are synchronized through helper/controller flows.
- Prefer helper/service abstractions over direct controller-level Keycloak HTTP calls.
