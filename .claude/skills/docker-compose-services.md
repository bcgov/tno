# Skill: Docker Compose Services

## Compose Files Used

- `docker-compose.yml`
- `docker-compose.override.yml`
- `db/kafka/docker-compose.yml`
- `services/docker-compose.yml`

## Recommended (Make Wrapper)

- Start all: `make up`
- Start core profile: `make up p=main`
- Start one service: `make up n=<service>`
- Stop/remove: `make down` (volumes kept; `make down v=1` also deletes them, wiping the database)

`n=` is the **compose service** name (`api`, `automation`, `editor`), never the container name.
Containers are `tno-<service>`; `make refresh` adds that prefix itself, so `n=tno-api` fails. See
the `make-commands` skill for the full rule.

## Service Names

- `docker-compose.yml` — `database`, `keycloak`, `elastic`, `api`, `charts`, `editor`,
  `subscriber`, `nginx`, `ssh`, `corenlp`, `oracle`, `oracleclient`, `autoheal`, `indexer`,
  `backup-service`
- `db/kafka/docker-compose.yml` — `broker`, `kowl`
- `services/docker-compose.yml` — `syndication`, `fileupload`, `image`, `filemonitor`, `content`,
  `contentmigration`, `indexing`, `transcription`, `auto-clipper`, `nlp`, `notification`,
  `reporting`, `folder-collection`, `automation`, `extract-quotes`, `ffmpeg`, `scheduler`,
  `event-handler`, `elasticsearch-mcp`

Regenerate the list rather than trusting this one to stay current:

```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml \
  -f db/kafka/docker-compose.yml -f services/docker-compose.yml \
  --profile all config --services | sort
```

`--profile all` omits services gated behind their own profile (e.g. `indexer`); swap in that
profile name, or read the compose file, when you need one of those.

`ches-retry` has a `services/net/` project but **no** compose service — build and run it directly.

## Direct Docker Compose Equivalent

```bash
docker-compose \
  --env-file .env \
  -f docker-compose.yml \
  -f docker-compose.override.yml \
  -f db/kafka/docker-compose.yml \
  -f services/docker-compose.yml \
  up -d
```

## Core Startup Order (Local)

1. `make setup`
2. `make up n=database`
3. `make up n=keycloak`
4. `make db-update`
5. `make up n=broker` and `make kafka-update`
6. `make up n=elastic` and `make elastic-update`
7. `make up n=api`
8. `make up n=indexing`
9. `make up n="editor subscriber nginx"` (make keeps only the last `n=`, so repeating the flag
   would start `nginx` alone)

## Useful Runtime URLs

- Editor via nginx: `http://localhost:40080`
- Subscriber via nginx: `http://localhost:40081`
- API: `http://localhost:40010`
- Keycloak: `http://localhost:40001`
- Elastic: `http://localhost:40003`
- Kafka UI (kowl): `http://localhost:40180`

## Rebuilding After a Code Change

| Changed | Rebuild |
| --- | --- |
| `services/net/<name>/` | `make refresh n=<name>` |
| `api/net/` | `make refresh n=api` |
| `libs/net/**` | `api` plus every service that consumes the change. `libs/net/models/Areas/Admin/**` is served to the UIs *by the API* — an editor-visible change there needs `api`, not `editor`. |
| `app/editor/src/`, `app/subscriber/src/` | nothing — `src/` and `public/` are bind-mounted and Vite hot-reloads. Only changes outside `src/` (deps, Dockerfile, config) need `make refresh n=editor` / `n=subscriber`. |
| `libs/npm/**` | re-pack into the app, then `make refresh n=editor` |

Rebuilding `api` takes the API down for several minutes. The .NET services treat that as a
critical failure — `The service is stopping: 'RequestFailed'`, then a ~120s restart backoff.
Those errors are expected during an `api` rebuild; confirm recovery with a fresh
`Subscribing to topics: …` line in the service's log instead of assuming it stayed down.

## Troubleshooting

- Rebuild service: `make refresh n=<service>` (one service per call)
- Full cleanup: `make nuke` (destructive)
- Verify required `.env` files exist after `make setup`.
