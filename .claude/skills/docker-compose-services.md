# Skill: Docker Compose Services

## Compose Files Used

- `docker-compose.yml`
- `docker-compose.override.yml`
- `dependencies/kafka/docker-compose.yml`
- `src/services/docker-compose.yml`

## Recommended (Bash Wrapper)

- Start all: `bash do docker up`
- Start core profile: `bash do docker up -p main`
- Start one service: `bash do docker up -n <service>`
- Stop/remove: `bash do docker down` (volumes kept; `bash do docker down -v` also deletes them, wiping the database)

`-n` is the **compose service** name (`api`, `automation`, `editor`), never the container name.
Containers are `tno-<service>`; `bash do docker refresh` selects Compose services, so `-n tno-api` fails. See
[the do-commands skill](do-commands.md) for the full rule.

## `.env` Changes Need a Recreate, Not a Restart

`docker restart tno-<service>` reuses the container's original environment — `env_file` is only
read when the container is **created**. After editing any `.env`, recreate the container:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml \
  -f dependencies/kafka/docker-compose.yml -f src/services/docker-compose.yml up -d <service>
```

Compose detects the env change and recreates only that container (no image rebuild needed).
Verify with `docker exec tno-<service> printenv <VAR>`.

## Service Names

- `docker-compose.yml` — `database`, `keycloak`, `elastic`, `api`, `charts`, `editor`,
  `subscriber` (both build `src/app/`; `APP_MODE` picks the design), `nginx`, `ssh`, `oracle`, `oracleclient`, `autoheal`, `indexer`,
  `backup-service`
- `dependencies/kafka/docker-compose.yml` — `broker`, `kafka-ui`
- `src/services/docker-compose.yml` — `syndication`, `fileupload`, `image`, `filemonitor`, `content`,
  `contentmigration`, `indexing`, `transcription`, `auto-clipper`, `notification`,
  `reporting`, `folder-collection`, `automation`, `content-analysis`, `ffmpeg`, `scheduler`,
  `event-handler`, `elasticsearch-mcp`

Regenerate the list rather than trusting this one to stay current:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml \
  -f dependencies/kafka/docker-compose.yml -f src/services/docker-compose.yml \
  --profile all config --services | sort
```

`--profile all` omits services gated behind their own profile (e.g. `indexer`); swap in that
profile name, or read the compose file, when you need one of those.

`ches-retry` has a `src/services/` project but **no** compose service — build and run it directly.

## Direct Docker Compose Equivalent

```bash
docker-compose \
  --env-file .env \
  -f docker-compose.yml \
  -f docker-compose.override.yml \
  -f dependencies/kafka/docker-compose.yml \
  -f src/services/docker-compose.yml \
  up -d
```

## Core Startup Order (Local)

1. `bash do setup`
2. `bash do docker up -n database`
3. `bash do docker up -n keycloak`
4. `bash do docker db-apply`
5. `bash do docker up -n broker` and `bash do docker kafka-update`
6. `bash do docker up -n elastic` and `bash do docker elastic-update`
7. `bash do docker up -n api`
8. `bash do docker up -n indexing`
9. `bash do docker up -n editor`, `bash do docker up -n subscriber`, and `bash do docker up -n nginx`

## Useful Runtime URLs

- Editor via nginx: `http://localhost:40080`
- Subscriber via nginx: `http://localhost:40081`
- API: `http://localhost:40010`
- Keycloak: `http://localhost:40001`
- Elastic: `http://localhost:40003`
- Kafka UI (kafka-ui): `http://localhost:40180`

## Rebuilding After a Code Change

| Changed | Rebuild |
| --- | --- |
| `src/services/<name>/` | `bash do docker refresh -n <name>` |
| `src/api/` | `bash do docker refresh -n api` |
| `src/libs/**` | `api` plus every service that consumes the change. `src/libs/models/Areas/Admin/**` is served to the UIs *by the API* — an editor-visible change there needs `api`, not `editor`. |
| `src/app/src/`, `src/app/public/`, `src/app/*.html` | nothing — the `editor` and `subscriber` containers bind-mount them and Vite hot-reloads. Only changes outside those (deps, Dockerfile, `vite.config.mts`) need `bash do docker refresh -n editor` / `-n subscriber`. |

Rebuilding `api` takes the API down for several minutes. The .NET services treat that as a
critical failure — `The service is stopping: 'RequestFailed'`, then a ~120s restart backoff.
Those errors are expected during an `api` rebuild; confirm recovery with a fresh
`Subscribing to topics: …` line in the service's log instead of assuming it stayed down.

## Troubleshooting

- Rebuild service: `bash do docker refresh -n <service>` (one service per call)
- Full cleanup: `bash do docker nuke` (destructive)
- Verify required `.env` files exist after `bash do setup`.
