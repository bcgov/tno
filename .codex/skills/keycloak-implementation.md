# Skill: Keycloak Implementation

## Where Keycloak Is Implemented

- Container/runtime config: `dependencies/keycloak/**`
- API auth wiring: `src/api/Program.cs`
- Keycloak helper layer: `src/api/Keycloak/**`
- Shared Keycloak client library: `src/libs/keycloak/**`
- Admin endpoints for sync/ops: `src/api/Areas/Admin/Controllers/KeycloakController.cs`

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
- Realm import files live in `dependencies/keycloak/config` and mount into container.
- After first startup, verify realm exists at `http://localhost:40001`.
- Update API Keycloak secrets using `./src/tools/scripts/kc-key-update.sh` when needed.

## Sync And User Management

- `IKeycloakHelper` handles user activation, role sync, and key linking.
- Keycloak and local DB roles/users are synchronized through helper/controller flows.
- Prefer helper/service abstractions over direct controller-level Keycloak HTTP calls.
