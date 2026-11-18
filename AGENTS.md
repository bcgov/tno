# Agents Guide — TNO (Today's News Online)

> Agent and AI coding assistant instructions for this repository.

## Repository Overview

TNO is a BC Government news aggregation mono-repo. It ingests news from many source types and presents a unified interface for government staff.

**Stack at a glance:**

| Layer                 | Technology                                          |
| --------------------- | --------------------------------------------------- |
| Frontend              | One Vite app (`src/app/`), React 19, TypeScript 7, Redux Toolkit, Formik, styled-components, SCSS; two designs: editor and subscriber |
| Shared UI code        | `src/app/src/shared` (imported as `@/shared`)           |
| API                   | .NET (C#), REST + SignalR                           |
| Services              | .NET microservices (`src/services/`)                |
| Database              | PostgreSQL                                          |
| Search                | Elasticsearch                                       |
| Messaging             | Apache Kafka                                        |
| Auth                  | Keycloak (OIDC)                                     |
| Infra                 | Docker Compose (local), OpenShift/Kubernetes (prod) |

---

## Repository Structure

```text
/
├── src/
│   ├── api/             # .NET REST API
│   ├── charts-api/      # Node charts API
│   ├── app/             # React + Vite app (editor and subscriber designs)
│   │   └── src/
│   │       ├── shared/  # Code used by both designs
│   │       ├── editor/
│   │       └── subscriber/
│   ├── libs/            # Shared .NET libraries and EF migrations
│   ├── services/        # Background and ingest services
│   ├── playwright-automation/ # Browser and API end-to-end tests
│   ├── tests/           # .NET test projects
│   └── tools/           # Utility scripts and helper tools
├── dependencies/
│   ├── elasticsearch/
│   ├── kafka/
│   ├── kafka-ui/
│   ├── keycloak/
│   ├── nginx/
│   ├── oracle/
│   ├── postgres/
│   ├── reverse-proxy/
│   └── tools/           # Database helper images
├── openshift/           # Kubernetes manifests and deployment scripts
├── docs/
└── Makefile
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
cd src/app
nvm use                         # Node 26 from .nvmrc
npm ci
APP_MODE=editor npm run dev     # or APP_MODE=subscriber; http://localhost:3000
```

Package manager: **npm** (`package-lock.json`). Yarn is no longer used. `APP_MODE` picks the
design the dev server, preview and production image present; see `src/app/README.md`.

### .NET API / Services

```bash
cd src/api
dotnet build
dotnet run
```

Uses `appsettings.Development.json` for local config. Requires a running PostgreSQL and Keycloak.

---

## Key Conventions

### TypeScript / React

- Components live under `src/app/src/editor/features/<feature>/` or `src/app/src/subscriber/features/<feature>/`.
- Shared components and types come from `@/shared`; import from there, never from the other design. Code both designs need moves to `src/app/src/shared`, which must not import either design.
- Imports use the `@/` alias (`@/shared`, `@/editor/store`, `@/subscriber/components/...`); relative imports stay within a feature.
- Forms use **Formik**; use `FormikText`, `FormikCheckbox`, `FormikWysiwyg`, etc. from `@/shared` (the editor also has `@/editor/components/formik`).
- State management: **Redux Toolkit** slices under `src/app/src/<design>/store/slices/`. API hooks under `src/app/src/<design>/store/hooks/`.
- Use `Show` (from `@/shared`) for conditional rendering instead of inline ternaries where possible.
- Dates use Day.js through `@/shared/utils/date` (never import `dayjs` or `moment` directly).

#### Editor form alignment

- Use the shared fields (`Select`, `SelectDate`, `Text`, or their Formik equivalents), including their `label` props, so each label and control share a `.frm-in` wrapper.
- The global form rules in `src/app/src/editor/index.scss` own vertical spacing. Standalone `.frm-in` fields have `0.5em` bottom padding and zero bottom margin. Do not add field-specific margins, offsets, or transforms to align controls. The repeated `.frm-in` selector intentionally overrides later-injected component styles; `SelectDate` otherwise adds `0.25em` of bottom margin and shifts upward in a bottom-aligned row.
- For rows of single-line labeled filters **and their action buttons**, use `<Row className="field-row" gap="1rem">`. Keep the fields and buttons as direct children (`Show` is fine because it renders no wrapper). The global `.field-row` rule aligns children to the bottom and removes the fields' bottom padding; `Row` provides wrapping and `gap` provides spacing. `alignItems="flex-end"` alone aligns wrapper boxes and leaves buttons `0.5em` below the input borders. Do not recreate this rule in feature styles or compensate with button margins.
- For forms with validation messages or multiline labels, check the label, control, and error layout explicitly; aligning wrapper bottoms alone is insufficient.
- Verify alignment in a real browser at wide and wrapped widths, including populated, empty, and disabled fields and every adjacent action button (including conditional buttons). Compare the visible control and button borders, not just wrapper boxes; jsdom tests do not measure layout. Run lint, relevant tests, and the Editor build before declaring the change complete.

#### Editor paginated tables

- Keep column headers on one line and column widths stable across result pages. Use `table-layout: fixed` with a `colgroup` for bounded columns, leaving the main text column flexible; do not let each page's content determine the widths.
- Give wide tables a minimum width and scroll inside their results panel on narrow screens. Allow body text to wrap, and verify header positions on first, next, and last pages as well as sticky headers and viewport bounds at narrow widths.

### .NET / C#

- API controllers live under `src/api/Controllers/`.
- Services follow the pattern in `src/services/<service-name>/`.
- Database access via Entity Framework Core; migrations in `dependencies/postgres/`.
- Authorization uses Keycloak claims; check `Claim` enum for role constants.

### Database Migrations

EF Core migrations and their model snapshot live in `src/libs/dal/Migrations/`; database deployment assets live in `dependencies/postgres/`. Use the current version as the migration name and keep **one new migration per PR**, including its designer and the final model snapshot.

If the user requests consolidating migrations already applied locally, back up affected data and roll back with the existing migration files before replacing them. Reapply the consolidated migration, restore any backed-up feature data, and verify `__EFMigrationsHistory`, rollback/reapplication tests, and `dotnet ef migrations has-pending-model-changes`. Do not merely delete migration-history rows or rewrite migrations deployed to shared environments.

### Branch & PR Conventions

- Branch names: `<ticket-id>-<short-description>` (e.g., `MMI-3413-improve-ai-summary`).
- Main branches: `main` (current), `dev` (integration), `master` (legacy stable).
- PRs target `main` unless instructed otherwise.

---

## AI / LLM Features

The application integrates LLM functionality for AI-generated report summaries.

- LLM models managed via `ILLMModel` (defined in `@/shared`).
- Admin can configure which models are public via `isPublic` flag.
- Report sections can have AI mode; see `ReportSectionAI` component and `settings.llmId` on `IReportModel`.
- API endpoint handles LLM calls server-side; the frontend only selects the model and triggers generation.

---

## Testing

```bash
# Frontend unit tests (Vitest projects: shared, editor, subscriber)
cd src/app && npm test

# e2e / integration
dotnet test TNO.sln
# Postman: see docs/POSTMAN.md
```

Frontend tests: Vitest + React Testing Library (jsdom).
E2e: Playwright (`src/playwright-automation/`).

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
| `src/app/**` | `cd src/app && npm run typecheck && npm run build` |
| `src/libs/template/**` | `dotnet build src/libs/template/TNO.TemplateEngine.csproj` |
| `src/libs/dal/**` | `dotnet build src/libs/dal/TNO.Dal.csproj` |
| `src/api/**` | `dotnet build src/api/` |
| `src/services/<name>/**` | `dotnet build src/services/<name>/` |
| Multiple .NET projects | `dotnet build` (solution root) |

If the build fails: read the error output, fix the root cause, rebuild, and repeat until clean. Do not stop while there are build errors.

### Before committing, pushing, or creating a PR

- Run the affected project's CI lint command as well as its build and relevant tests. For .NET projects, use `dotnet format <project.csproj> --verify-no-changes`; for `src/app/` changes, run `npm run lint`, `npm run check`, `npm run typecheck`, `npm test` and `npm run build` in `src/app/`. Use the SDK and Node versions required by the repository/CI.
- For API changes, run a fresh CI-equivalent build: `dotnet build src/api/TNO.API.csproj --configuration Release --no-incremental`. The API treats XML diagnostics CS1570, CS1572, CS1573, CS1574, CS1584, CS1587, and CS1591 as errors, covering missing summaries, malformed XML, parameter/signature mismatches, unresolved references, and misplaced comments. Document public endpoints, records, properties, and protected overrides; update parameter tags when signatures change. Do not suppress the diagnostics. Follow the [build verification skill](.codex/skills/build-verify.md), including current-head GitHub annotation checks.
- Review the build log for **new compiler warnings**, not only the exit code. A successful build can still produce GitHub warning annotations. Fix warnings introduced by the change before committing. Verify any claimed pre-existing warning against the base revision and report it separately; do not dismiss new warnings as existing noise.
- After the final edit, rerun affected checks. Do not rely on a passing check from an earlier version of the files. After pushing, inspect the relevant GitHub checks and annotations for the current PR head SHA, and state explicitly if CI is still pending.

The stop hook at `.claude/scripts/build-verify.sh` detects changed files via `git status` and runs the appropriate build. It does not replace the lint checks, tests, or warning review required above.

---

## Local Dev Gotchas (verified 2026-10)

**`.env` changes need a container recreate, not a restart.** `docker restart` keeps the old
environment. Use `docker-compose -f docker-compose.yml -f docker-compose.override.yml
-f dependencies/kafka/docker-compose.yml -f src/services/docker-compose.yml up -d <service>`, then verify with
`docker exec tno-<service> printenv <VAR>`.

**API returns 401 for valid Keycloak tokens** → issuer mismatch. Local Keycloak is reached via
three hostnames, so tokens carry three different `iss` values; `Keycloak__Issuer` in
`src/api/.env` must list them all:
`http://localhost:40001/realms/mmi,http://host.docker.internal:40001/realms/mmi,http://localhost:8080/realms/mmi,http://keycloak:8080/realms/mmi,mmi-app,mmi-service-account`.
Keep `keycloak__Authority=http://host.docker.internal:40001/realms/mmi` (must resolve from inside
the API container). Diagnose with `docker logs tno-api | grep IDX10205` — it prints the token
issuer vs the accepted list.

**Services spam Keycloak `CLIENT_LOGIN_ERROR invalid_client_credentials`** → their
`src/services/<name>/.env` still has the placeholder `{YOU WILL NEED TO GET THIS FROM KEYCLOAK}`
in `Auth__Keycloak__Secret`. The local secret is the `mmi-service-account` client secret in
`dependencies/keycloak/config/realm-export.json`.

**Content not appearing in Elasticsearch** → the indexing service needs `Elastic__Url`,
`Elastic__Username`, `Elastic__Password` in `src/services/indexing/.env`
(`Service__Elasticsearch*` keys are read by nothing). Pipeline order to check: syndication →
Kafka `content` topic → content service → `index` topic → indexing → ES. Compare
`SELECT max(id) FROM content` (postgres) against the max `id` in the `unpublished_content` index;
Kafka replays the backlog once the indexing service is fixed and recreated.

**Local test credentials**: Keycloak admin console `admin`/`password`; realm `mmi` users
`editor`/`admin`/`subscriber` with password `password`. Older realm imports may lack the
required `display_name`, `idir_username` and `idir_user_guid` attributes on editor/subscriber;
repair those local profiles in place as documented in `dependencies/keycloak/README.md`.
For API testing, get a token via `client_credentials` with
`mmi-service-account` and call `http://localhost:40080/api/...`.

## Do Not

- Do not use Yarn in `src/app/` — it is an npm project (`npm ci`, `npm install <pkg>`).
- Do not commit `.env` files.
- Do not modify generated migration files after they have been applied to any environment.
- Do not import between `src/app/src/editor` and `src/app/src/subscriber`, or from either into `src/app/src/shared`; shared code goes in `src/app/src/shared`.
- Do not bypass pre-commit hooks (`--no-verify`).
