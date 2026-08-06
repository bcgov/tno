# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Is

TNO / MMI (Media Monitoring Insights) — a BC Government news aggregation and media monitoring platform. Microservices architecture with a .NET 9 REST API, two React frontends, ~18 .NET background services, Kafka for inter-service messaging, PostgreSQL, and Elasticsearch.

## Build & Run Commands

### Docker (primary dev workflow)

```bash
make setup            # Generate .env files and directories (first-time setup)
make up p=api         # Start a profile: all, api, editor, subscriber, kafka, service, ingest, utility
make up n=tno-api     # Start a single named container
make refresh n=tno-api  # Stop → rebuild → start a single container
make down             # Stop and remove all containers (volumes/data are kept)
make down v=1         # ...and delete the volumes — wipes the database (asks to confirm; y=1 skips)
make nuke             # Full reset: down v=1 + delete all config
```

All `make` commands use four compose files together:
`docker-compose.yml`, `docker-compose.override.yml`, `db/kafka/docker-compose.yml`, `services/docker-compose.yml`

### Database

```bash
make db-update        # Apply EF Core migrations (runs db-update.sh)
make db-refresh       # Drop and reinitialize
make elastic-update   # Apply Elasticsearch migrations (the target is not es-update)
make kafka-update     # Apply Kafka topic migrations (n=migration, r=rollback)
```

`db-update` and `elastic-update` each build an image and run it as a one-shot container
**on `tno-net`**, reaching postgres and elasticsearch by their compose service names
(`database:5432`, `elastic:9200`). The containers must be up first, and a `.env` pointing at
`host.docker.internal` will not resolve from inside them.

### .NET (local, outside Docker)

```bash
dotnet restore
dotnet build
dotnet format --verify-no-changes   # Lint check (used in CI)
dotnet test                         # Run all tests
dotnet test libs/net/tests/core/    # Run a specific test project
```

### React (Editor / Subscriber apps)

Both apps use **Yarn 3** (not npm). Run commands from `app/editor/` or `app/subscriber/`:

```bash
yarn install
yarn build
yarn test             # Vitest (single run)
yarn test:watch       # Vitest interactive
yarn lint             # ESLint (zero warnings enforced)
yarn lint:fix
```

### VS Code Debugging

`.vscode/launch.json` has named launch configs for every service (e.g., "Run API", "Run Syndication Service"). Each sets `cwd` and `envFile` to the service's own directory and `.env` file. Use these rather than `dotnet run` to ensure environment variables are loaded correctly.

## Architecture

### Service Communication

**Kafka is the backbone.** All service-to-service communication flows through Kafka topics defined in each service's `appsettings.json` (e.g., `index`, `transcribe`, `notify`, `hub`, `reporting`, `nlp`, `ffmpeg`). The API itself is a Kafka consumer — it uses the `hub` topic to deliver SignalR messages to connected browser clients via `KafkaHubLifetimeManager`.

**SignalR** hub is at `/hub`. Both React apps connect via SignalR for real-time updates. The API's `KafkaHubLifetimeManager` (in `libs/net/kafka/SignalR/`) consumes the `hub` Kafka topic and fans out messages to connected users.

### Key Components

| Path                   | Role                                                                                      |
| ---------------------- | ----------------------------------------------------------------------------------------- |
| `api/net/`             | ASP.NET Core 9 REST API — JWT auth, Swagger at `/api-docs`, versioned routes under `/api` |
| `app/editor/`          | React editor UI — content creation, review, publishing                                    |
| `app/subscriber/`      | React subscriber UI — report consumption, alerts                                          |
| `services/net/{name}/` | ~18 console microservices, each a Kafka consumer/producer                                 |
| `libs/net/dal/`        | EF Core `TNOContext`, all 180+ migrations, entity configs                                 |
| `libs/net/entities/`   | Domain model entities                                                                     |
| `libs/net/models/`     | API DTOs / response models                                                                |
| `libs/net/kafka/`      | `IKafkaListener`, `IKafkaMessenger`, `KafkaHubLifetimeManager`                            |
| `libs/net/services/`   | Base class for all microservices (Serilog, health checks, service loop)                   |
| `libs/npm/`            | Shared React component libraries consumed by both apps                                    |

### .NET Configuration Pattern

- `appsettings.json` — production defaults (Kafka: `kafka-broker-0.kafka-headless:9092`)
- `appsettings.Development.json` — dev defaults (Kafka: `host.docker.internal:40102`)
- `.env` files — override via environment variables (`Kafka__Consumer__BootstrapServers=broker:29094`)

`WebApplication.CreateBuilder` adds `appsettings.json` and `appsettings.Development.json` as **default** providers. The explicit `builder.Configuration` chain in `Program.cs` only adds `connectionstrings.json`, then `AddEnvironmentVariables()` last (highest priority). Environment variables from `.env` (loaded by `docker-compose env_file` or VS Code `envFile`) therefore override appsettings values.

`DotNetEnv.Env.Load()` runs at startup to load `.env` from the working directory for local dev without Docker.

### Authentication

Keycloak (local: `host.docker.internal:40001/realms/mmi`, production: `loginproxy.gov.bc.ca`). The API validates JWT Bearer tokens and uses `KeycloakClientRoleHandler` for dynamic role-based authorization. Service accounts use a separate Keycloak client (`mmi-service-account`).

### Database

PostgreSQL via EF Core. Migrations live in `libs/net/dal/Migrations/`. Always run `make db-update` after pulling changes that add migrations. The `TNOContext` in `libs/net/dal/` is the single EF context shared across API and all services.

Migrations derive from `SeedMigration`, not `Migration`. Each one may carry raw SQL beside its
generated DDL, in `Migrations/<version>/Up/{PreUp,PostUp}/*.sql` (and `Down/{PreDown,PostDown}/`),
run in filename order around the EF operations. Two consequences:

- Some migrations **depend** on their PreUp script — 1.0.5 converts `tag.id` from `varchar(6)`
  with an explicit `USING` cast, because EF's own `AlterColumn` emits a bare `ALTER COLUMN ...
  TYPE integer` that postgres rejects.
- `SeedMigration` resolves the scripts from `AppDomain.CurrentDomain.BaseDirectory/Migrations`
  at runtime and **skips them silently** when that folder is missing (a postgres `NOTICE`, no
  error). Anything that packages the migrations — `libs/net/Dockerfile` builds an `efbundle` —
  must copy the `.sql` files next to the executable.

Seed scripts are expected to be idempotent (`ON CONFLICT DO NOTHING`) and to guard on their
prerequisites, so they no-op rather than fail on a database that lacks them.

### Elasticsearch

Index migrations live in `tools/elastic/migration/Migrations/<version>/up/{pre,post}/*.json` and
create versioned indexes (`content_v1.0.10`) behind the aliases the API queries (`content`,
`unpublished_content`).

The mappings declare scalar fields only; the nested objects documents also carry (`source`,
`mediaType`, `owner`, `series`) arrive through **dynamic mapping** when content is first indexed.
A freshly migrated, empty index therefore has no mapping for them, and Elasticsearch answers any
**sort** on one with a `400 No mapping found for [...] in order to sort on` — which the API
surfaces as a 500.

### Docker Ports (local)

| Service                 | Port  |
| ----------------------- | ----- |
| PostgreSQL              | 40000 |
| Keycloak                | 40001 |
| Elasticsearch           | 40003 |
| API                     | 40010 |
| nginx (editor)          | 40080 |
| nginx (subscriber)      | 40081 |
| Kafka broker (external) | 40102 |
| Editor UI               | 40082 |
| Subscriber UI           | 40083 |

The browser apps call the API through nginx, not the API port directly — an editor request is
`http://localhost:40080/api/editor/...`.

## Code Style

`.editorconfig` enforces: 4-space indent for C# and TypeScript, 2-space for JSON/YAML/Markdown, LF line endings, final newline. CI fails on `dotnet format` violations and ESLint warnings (`--max-warnings 0`).
