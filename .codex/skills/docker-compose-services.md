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
4. `bash do docker db-update`
5. `bash do docker up -n broker` and `bash do docker kafka-update`
6. `bash do docker up -n elastic` and `bash do docker elastic-update`
7. `bash do docker up -n api`
8. `bash do docker up -n indexing`
9. `bash do docker up -n editor`, `bash do docker up -n subscriber`, `bash do docker up -n nginx`

## Useful Runtime URLs

- Editor via nginx: `http://localhost:40080`
- Subscriber via nginx: `http://localhost:40081`
- API: `http://localhost:40010`
- Keycloak: `http://localhost:40001`
- Elastic: `http://localhost:40003`
- Kafka UI (kafka-ui): `http://localhost:40180`

## Troubleshooting

- Rebuild service: `bash do docker refresh -n <service>`
- Full cleanup: `bash do docker nuke` (destructive)
- Verify required `.env` files exist after `bash do setup`.
