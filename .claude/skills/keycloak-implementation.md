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

## Local Dev Auth — Verified Working Config (2026-10)

Local Keycloak is reached through **three different hostnames**, so JWTs arrive at the API with
three different `iss` claims. All must be listed in `Keycloak__Issuer` (CSV → `ValidIssuers`) in
`api/net/.env` or the API returns 401 for valid tokens:

```
Keycloak__Issuer=http://localhost:40001/realms/mmi,http://host.docker.internal:40001/realms/mmi,http://localhost:8080/realms/mmi,http://keycloak:8080/realms/mmi,mmi-app,mmi-service-account
```

- `localhost:40001` — browser logins (editor/subscriber apps use `public/keycloak.json`)
- `host.docker.internal:40001` / `localhost:8080` — .NET services fetching service-account tokens
- `mmi-app,mmi-service-account` — production entries; keep them

`keycloak__Authority` must stay `http://host.docker.internal:40001/realms/mmi` — it has to be
resolvable **from inside the API container** (the JWT middleware fetches the OIDC discovery doc
and signing keys from it). Do not change it to `keycloak:8080` or `localhost`.

### Diagnosing a 401

`docker logs tno-api | grep IDX10205` — `ShowPII` is on, so the error prints the token's issuer,
the configured `ValidIssuers`, and the discovery document's issuer side by side.

**`.env` changes need a container recreate, not a restart.** `docker restart tno-api` reuses the
old environment; use `docker-compose <the four -f files> up -d api`. Verify with
`docker exec tno-api printenv Keycloak__Issuer`.

### Service-account secrets in `services/net/*/.env`

Generated `.env` files ship with the placeholder `{YOU WILL NEED TO GET THIS FROM KEYCLOAK}` in
`Auth__Keycloak__Secret`. The real local secret is the `mmi-service-account` client `secret` in
`auth/keycloak/config/realm-export.json`. Symptom when unset: Keycloak logs
`CLIENT_LOGIN_ERROR ... invalid_client_credentials` every ~5s and the service gets 401s from the API.

Some older `.env` files also carry the pre-rename realm: `Auth__OIDC__Token=/realms/tno/...` and
`Auth__Keycloak__Audience=tno-service-account`. The realm is `mmi` and the client is
`mmi-service-account`. Symptom: the service loops
`HTTP Request failed [POST]:[NotFound]: .../realms/tno/protocol/openid-connect/token`.

### Local test credentials

- Keycloak admin console (`master` realm): `admin` / `password` (see `auth/keycloak/.env`)
- Realm `mmi` seeded users: `editor`, `admin`, `subscriber` — password `password`. Direct
  password grants fail with `resolve_required_actions` (browser login works fine); for API
  testing use client credentials instead:

```bash
curl -s -X POST http://localhost:40001/realms/mmi/protocol/openid-connect/token \
  -d "grant_type=client_credentials&client_id=mmi-service-account&client_secret=<realm-export secret>"
```

The resulting token has `editor`+`administrator` roles and works against `http://localhost:40080/api/...`.

## Sync And User Management

- `IKeycloakHelper` handles user activation, role sync, and key linking.
- Keycloak and local DB roles/users are synchronized through helper/controller flows.
- Prefer helper/service abstractions over direct controller-level Keycloak HTTP calls.
