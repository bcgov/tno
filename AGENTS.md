# Agents Guide — TNO (Today's News Online)

> Agent and AI coding assistant instructions for this repository.

## Repository Overview

TNO is a BC Government news aggregation mono-repo. It ingests news from many source types and presents a unified interface for government staff.

**Stack at a glance:**

| Layer                 | Technology                                          |
| --------------------- | --------------------------------------------------- |
| Frontend (editor)     | React 18, TypeScript, Redux, Formik, SCSS           |
| Frontend (subscriber) | React 18, TypeScript, Redux                         |
| Shared UI lib         | `tno-core` (internal npm package in `libs/npm`)     |
| API                   | .NET (C#), REST + SignalR                           |
| Services              | .NET microservices (`services/net/`)                |
| Database              | PostgreSQL                                          |
| Search                | Elasticsearch                                       |
| Messaging             | Apache Kafka                                        |
| Auth                  | Keycloak (OIDC)                                     |
| Infra                 | Docker Compose (local), OpenShift/Kubernetes (prod) |

---

## Repository Structure

```text
/
├── app/
│   ├── editor/          # React editor app (admin/editor users)
│   └── subscriber/      # React subscriber app (public-facing)
├── api/net/             # .NET REST API
├── services/net/        # .NET microservices (indexing, reporting, etc.)
├── libs/
│   ├── npm/             # tno-core shared React component library
│   └── net/             # Shared .NET libraries
├── db/
│   ├── postgres/        # Migrations and seed scripts
│   ├── elasticsearch/   # Index mappings
│   └── kafka/           # Topic configs
├── auth/keycloak/       # Keycloak realm config
├── openshift/           # OCP kustomize manifests
├── docs/                # Developer documentation
├── test/                # Integration/e2e tests
└── Makefile             # Top-level dev shortcuts
```

---

## Development Environment

### Start everything (Docker)

```bash
make setup      # Generate .env files (first time)
make up         # Start all containers
make down       # Stop and remove containers (v=1 also deletes the volumes/database)
```

Default ports after `make up`:

| Service       | Port        |
| ------------- | ----------- |
| Editor UI     | 40082       |
| Subscriber UI | 40083       |
| API           | 40010       |
| Keycloak      | 40001       |
| PostgreSQL    | 40000       |
| Elasticsearch | 40003       |
| Kafka         | 40101/40102 |

### Frontend (local, no Docker)

```bash
cd app/editor
nvm use 18.19.0
yarn install
yarn start
```

```bash
cd app/subscriber
nvm use 18.19.0
yarn install
yarn start
```

Package manager: **Yarn 3** (PnP/Berry). Do not use `npm install` in `app/` directories.

### tno-core library

```bash
cd libs/npm
yarn install
yarn build
```

After building, link locally or publish. The apps reference `tno-core` by version in their `package.json`.

### .NET API / Services

```bash
cd api/net
dotnet build
dotnet run
```

Uses `appsettings.Development.json` for local config. Requires a running PostgreSQL and Keycloak.

---

## Key Conventions

### TypeScript / React

- Components live under `app/editor/src/features/<feature>/` or `app/subscriber/src/features/<feature>/`.
- Shared components and types come from `tno-core`; import from there, not from sibling apps.
- Forms use **Formik**; use `FormikText`, `FormikCheckbox`, `FormikWysiwyg`, etc. from `tno-core`.
- State management: **Redux** slices under `src/store/slices/`. API hooks under `src/store/hooks/`.
- Use `Show` (from `tno-core`) for conditional rendering instead of inline ternaries where possible.

#### Editor form alignment

- Use the `tno-core` fields (`Select`, `SelectDate`, `Text`, or their Formik equivalents), including their `label` props, so each label and control share a `.frm-in` wrapper.
- The global form rules in `app/editor/src/index.scss` own vertical spacing. Standalone `.frm-in` fields have `0.5em` bottom padding and zero bottom margin. Do not add field-specific margins, offsets, or transforms to align controls. The repeated `.frm-in` selector intentionally overrides later-injected component styles; `SelectDate` otherwise adds `0.25em` of bottom margin and shifts upward in a bottom-aligned row.
- For rows of single-line labeled filters **and their action buttons**, use `<Row className="field-row" gap="1rem">`. Keep the fields and buttons as direct children (`Show` is fine because it renders no wrapper). The global `.field-row` rule aligns children to the bottom and removes the fields' bottom padding; `Row` provides wrapping and `gap` provides spacing. `alignItems="flex-end"` alone aligns wrapper boxes and leaves buttons `0.5em` below the input borders. Do not recreate this rule in feature styles or compensate with button margins.
- For forms with validation messages or multiline labels, check the label, control, and error layout explicitly; aligning wrapper bottoms alone is insufficient.
- Verify alignment in a real browser at wide and wrapped widths, including populated, empty, and disabled fields and every adjacent action button (including conditional buttons). Compare the visible control and button borders, not just wrapper boxes; jsdom tests do not measure layout. Run lint, relevant tests, and the Editor build before declaring the change complete.

### .NET / C#

- API controllers live under `api/net/Controllers/`.
- Services follow the pattern in `services/net/<service-name>/`.
- Database access via Entity Framework Core; migrations in `db/postgres/`.
- Authorization uses Keycloak claims; check `Claim` enum for role constants.

### Database Migrations

EF Core migrations and their model snapshot live in `libs/net/dal/Migrations/`; database deployment assets live in `db/postgres/`. Use the current version as the migration name and keep **one new migration per PR**, including its designer and the final model snapshot.

If the user requests consolidating migrations already applied locally, back up affected data and roll back with the existing migration files before replacing them. Reapply the consolidated migration, restore any backed-up feature data, and verify `__EFMigrationsHistory`, rollback/reapplication tests, and `dotnet ef migrations has-pending-model-changes`. Do not merely delete migration-history rows or rewrite migrations deployed to shared environments.

### Branch & PR Conventions

- Branch names: `<ticket-id>-<short-description>` (e.g., `MMI-3413-improve-ai-summary`).
- Main branches: `main` (current), `dev` (integration), `master` (legacy stable).
- PRs target `main` unless instructed otherwise.

---

## AI / LLM Features

The application integrates LLM functionality for AI-generated report summaries.

- LLM models managed via `ILLMModel` (defined in `tno-core`).
- Admin can configure which models are public via `isPublic` flag.
- Report sections can have AI mode; see `ReportSectionAI` component and `settings.llmId` on `IReportModel`.
- API endpoint handles LLM calls server-side; the frontend only selects the model and triggers generation.

---

## Testing

```bash
# Frontend unit tests
cd app/editor && yarn test

# e2e / integration
cd test && <see test/README.md>
```

Frontend tests: Jest + React Testing Library.
E2e: Playwright (`playwright-automation/`).

---

## Infra / Deployment

- **Local**: Docker Compose (`docker-compose.yml` + `docker-compose.override.yml`).
- **Production**: OpenShift via Kustomize (`openshift/kustomize/`). See `openshift/README.md`.
- CI/CD: GitHub Actions (`.github/workflows/`). Workflows build and push images on push to `dev`/`master`.
- OCP project namespace: `9b301c` (tools namespace for image builds).

---

## Build Verification

After making any code changes, **always build the affected project and resolve all errors before stopping**.

| Files changed | Build command |
|---|---|
| `app/subscriber/src/**` | `cd app/subscriber && yarn build` |
| `app/editor/src/**` | `cd app/editor && yarn build` |
| `libs/net/template/**` | `dotnet build libs/net/template/TNO.TemplateEngine.csproj` |
| `libs/net/dal/**` | `dotnet build libs/net/dal/TNO.Dal.csproj` |
| `api/net/**` | `dotnet build api/net/` |
| `services/net/<name>/**` | `dotnet build services/net/<name>/` |
| Multiple .NET projects | `dotnet build` (solution root) |

If the build fails: read the error output, fix the root cause, rebuild, and repeat until clean. Do not stop while there are build errors.

A stop hook at `.claude/scripts/build-verify.sh` enforces this automatically by detecting changed files via `git status` and running the appropriate build.

---

## Local Dev Gotchas (verified 2026-10)

**`.env` changes need a container recreate, not a restart.** `docker restart` keeps the old
environment. Use `docker-compose -f docker-compose.yml -f docker-compose.override.yml
-f db/kafka/docker-compose.yml -f services/docker-compose.yml up -d <service>`, then verify with
`docker exec tno-<service> printenv <VAR>`.

**API returns 401 for valid Keycloak tokens** → issuer mismatch. Local Keycloak is reached via
three hostnames, so tokens carry three different `iss` values; `Keycloak__Issuer` in
`api/net/.env` must list them all:
`http://localhost:40001/realms/mmi,http://host.docker.internal:40001/realms/mmi,http://localhost:8080/realms/mmi,http://keycloak:8080/realms/mmi,mmi-app,mmi-service-account`.
Keep `keycloak__Authority=http://host.docker.internal:40001/realms/mmi` (must resolve from inside
the API container). Diagnose with `docker logs tno-api | grep IDX10205` — it prints the token
issuer vs the accepted list.

**Services spam Keycloak `CLIENT_LOGIN_ERROR invalid_client_credentials`** → their
`services/net/<name>/.env` still has the placeholder `{YOU WILL NEED TO GET THIS FROM KEYCLOAK}`
in `Auth__Keycloak__Secret`. The local secret is the `mmi-service-account` client secret in
`auth/keycloak/config/realm-export.json`.

**Content not appearing in Elasticsearch** → the indexing service needs `Elastic__Url`,
`Elastic__Username`, `Elastic__Password` in `services/net/indexing/.env`
(`Service__Elasticsearch*` keys are read by nothing). Pipeline order to check: syndication →
Kafka `content` topic → content service → `index` topic → indexing → ES. Compare
`SELECT max(id) FROM content` (postgres) against the max `id` in the `unpublished_content` index;
Kafka replays the backlog once the indexing service is fixed and recreated.

**Local test credentials**: Keycloak admin console `admin`/`password`; realm `mmi` users
`editor`/`admin`/`subscriber` with password `password` (browser login only — direct grants fail
with `resolve_required_actions`). For API testing, get a token via `client_credentials` with
`mmi-service-account` and call `http://localhost:40080/api/...`.

## Do Not

- Do not run `npm install` inside `app/` — use `yarn`.
- Do not commit `.env` files.
- Do not modify generated migration files after they have been applied to any environment.
- Do not import from `app/editor` into `app/subscriber` or vice versa; use `tno-core` for shared code.
- Do not bypass pre-commit hooks (`--no-verify`).
