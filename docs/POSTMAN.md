# Testing the API with Postman

Import [TNO.postman_collection.json](../src/api/TNO.postman_collection.json) into
Postman using **Import**, then select the file. The collection contains API requests
and an `auth` folder for requesting Keycloak tokens. The `api` folder contains
590 operations from the current .NET API definition, grouped by area and resource.
The `charts` folder covers the ten routes in `src/charts-api/src/routes.ts`.
Other folders contain separate integration utilities; they are not part of the
.NET API and may require their own services and credentials.

## Start the local services

From the repository root, run `make move` if upgrading an existing checkout, then
`make setup` for missing environment files and `make up n=api`. The API depends on
PostgreSQL and Keycloak; requests that use search or messaging also need Elasticsearch
and Kafka running. Use `make up p=main` for the main application stack.

## Configure a Postman environment

Create an environment named `TNO local`, add these variables, and select that environment
before sending requests. These ports are the Compose defaults; use your root `.env`
overrides if they differ.

| Variable | Local value |
| --- | --- |
| `keycloak-scheme` | `http` |
| `keycloak-host` | `localhost` |
| `keycloak-port` | `40001` |
| `keycloak-client-id` | `mmi-service-account` |
| `realm` | `mmi` |
| `service-account-secret` | The Keycloak client's secret (see below) |
| `scheme` | `http` |
| `host` | `localhost` |
| `port` | `40010` |
| `root-path` | `/api` |
| `charts-port` | `40014` |
| `access-token` | Leave empty; the token request populates this |

To use the nginx gateway instead of the API directly, set `port` to `40080` and
keep `root-path` as `/api`. The gateway must also be running.

Open the Keycloak admin console at `http://localhost:40001`, sign in with the local
admin credentials (`admin` / `password`), select realm **mmi**, then open
**Clients → mmi-service-account → Credentials** and copy the client secret into
`service-account-secret`. The checked-in local realm import is in
`dependencies/keycloak/config/realm-export.json`; the running Keycloak client's
secret is authoritative if it has changed since import. Keep secrets and tokens
local to your Postman environment; do not commit them or export them with shared collections.

## Request an access token

1. In the imported collection, open **TNO → auth → token: service-account**.
2. Verify the method is **POST** and the URL resolves to:
   `http://localhost:40001/realms/mmi/protocol/openid-connect/token`.
3. This request uses **No Auth** and an **x-www-form-urlencoded** body:

   | Field | Value |
   | --- | --- |
   | `client_id` | `{{keycloak-client-id}}` |
   | `grant_type` | `client_credentials` |
   | `audience` | `{{keycloak-client-id}}` (already in the collection) |
   | `client_secret` | `{{service-account-secret}}` |

4. Click **Send**. A successful response is HTTP **200** with `access_token`,
   `token_type`, and `expires_in` fields.
5. The request's response script saves `access_token` as the selected environment's
   `access-token` variable. Confirm it is populated before sending API requests.
   An unsuccessful token response clears the old token to avoid reusing stale credentials.

This authenticates the service account. It does not impersonate an editor or subscriber.
The collection also includes `token: test user`, but that uses the password grant and
an `mmi-test` client. It is not the default local login path: the seeded local users
in older realm imports may have incomplete profiles that prevent direct grants. See
the [local account repair instructions](../dependencies/keycloak/README.md#local-browser-test-accounts).
Use the service-account request
for service API testing; use the application browser login for interactive user flows.

## Send an API request

The collection's authorization is **Bearer Token**, with token `{{access-token}}`.
Requests that inherit collection authorization send `Authorization: Bearer <token>`
automatically. For a new request, select **Inherit auth from parent**, or explicitly
set **Bearer Token** to `{{access-token}}`.

The default API URL is `http://localhost:40010/api`. Start with the collection's
**api → health → health → GET /Health** request to check connectivity; it uses **No Auth**, so a
successful health response does not prove the token works. Then send an endpoint
appropriate to the service account's permissions. Requests need their own path,
query, and body values. Edit Postman's **Path Variables** for placeholders such as
`:id`. Optional query parameters are disabled until selected. JSON bodies are
schema examples, not seeded records; replace IDs and required business values.
For file uploads, select a local file in **Body → form-data**; let Postman set the
multipart boundary. Some controllers read filters directly from `Request.Query`,
which OpenAPI does not describe; consult the controller for those additional filters.

Charts use the same `scheme` and `host`, with `charts-port` and no `/api` prefix.
Start with **charts → GET /health**. Graph/base64 requests include synthetic chart
data; GET requests encode the data in the `data` query parameter.

When the token expires, send `token: service-account` again to replace `access-token`.
For shared environments, use that environment's API and Keycloak addresses, client,
realm, and credentials rather than the local defaults.

## Troubleshooting

- **Unresolved `{{variables}}`:** select `TNO local` and check the variable names above.
- **`invalid_client` / `invalid_client_credentials`:** verify the realm, client ID,
  and secret against the running Keycloak client. Ensure service accounts are enabled.
- **Token request returns 404:** local Keycloak uses `/realms/mmi/...`, without the
  older `/auth` prefix. The local token, userinfo and OpenID configuration requests
  use this current path. External authentication utilities need their own configuration.
- **API returns 401:** request a fresh token and check the Bearer token is inherited.
  If it persists, inspect `docker logs tno-api` for issuer validation errors. The
  token's `iss` must be accepted by `Keycloak__Issuer` in `src/api/.env`; a token
  requested from local Postman normally has issuer `http://localhost:40001/realms/mmi`.
  After correcting environment configuration, recreate the API with `make up n=api`.
- **API returns 403:** the token is authenticated but lacks permission for that endpoint;
  use the appropriate client or user role for the operation.

## Refresh API requests after endpoint changes

Export OpenAPI from a freshly built API assembly, then regenerate only the `api`
folder. Run these commands from the repository root using .NET 10:

```bash
dotnet build src/api/TNO.API.csproj --configuration Release
dotnet tool install Swashbuckle.AspNetCore.Cli --version 10.3.1 --tool-path /tmp/tno-swagger
cd src/api
/tmp/tno-swagger/swagger tofile --output /tmp/tno-openapi.json bin/Release/net10.0/TNO.API.dll v1
cd ../..
python3 src/tools/scripts/sync-postman.py --openapi /tmp/tno-openapi.json
python3 src/tools/scripts/sync-postman.py --openapi /tmp/tno-openapi.json --check
```

Install the CLI once; reuse the installed tool on subsequent refreshes. The exporter
loads application configuration, so use your local environment. The script retains
one unversioned `/api` route for each operation, omits duplicate version aliases, and
preserves the collection variables, authorization and other folders. Update charts
requests separately when `src/charts-api/src/routes.ts` changes. `--check` reports
drift without rewriting the file.

For automated .NET tests and coverage, see [src/tests/README.md](../src/tests/README.md).
