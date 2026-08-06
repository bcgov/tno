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
9. `make up n=editor n=subscriber n=nginx`

## Useful Runtime URLs

- Editor via nginx: `http://localhost:40080`
- Subscriber via nginx: `http://localhost:40081`
- API: `http://localhost:40010`
- Keycloak: `http://localhost:40001`
- Elastic: `http://localhost:40003`
- Kafka UI (kowl): `http://localhost:40180`

## Troubleshooting

- Rebuild service: `make refresh n=<service>`
- Full cleanup: `make nuke` (destructive)
- Verify required `.env` files exist after `make setup`.
