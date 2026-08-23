# Skill: Make Commands

## Discover Commands

- Show all targets: `make help`

## Common Development Flow

- Initialize local config files: `make setup`
- Start all default services: `make up`
- Stop and remove running stack: `make down` (volumes kept; `make down v=1` also deletes them, wiping the database)
- Rebuild and restart selected service: `make refresh n=<service>`

## Service/Profile Control

- Start one service: `make up n=api`
- Stop one service: `make stop n=api`
- Use profile bundles: `make up p=main`
- Build one service only: `make build n=editor`

### `n=` takes the compose service name, not the container name

Containers are named `tno-<service>`, but `n=` is always the **service** name — `n=api`, not
`n=tno-api`. `make refresh` calls `tools/scripts/docker-remove.sh`, which prepends the prefix
itself (`docker rm -f tno-$1` / `docker image rm -f tno:$1`), so a `tno-` prefixed value resolves
to `tno-tno-api` and fails. Service names live in the four compose files; `make help` and
`docker-compose ... config --services` both list them.

`up`, `build` and `stop` expand `$(n)` unquoted, so they accept several services at once:
`make up n="editor subscriber nginx"`. Note that make keeps only the **last** assignment of a
repeated variable, so `make up n=editor n=subscriber` starts *subscriber only* — quote the list
instead. `make refresh` must be given exactly one service: its remove step reads only the first
name, leaving the others' stale images in place.

## Data/Infra Commands

- Apply DB migrations: `make db-update`
- Reset/recreate DB: `make db-refresh`
- Apply Elastic updates: `make elastic-update`
- Apply Kafka migrations: `make kafka-update`

## New Worktree Command

- Run: `make worktree`
- What it does:
  - Reads `KEPLER_GLOBAL_WORKTREE_FOLDER` from root `.env`.
  - Prompts for it if missing and saves it in `.env`.
  - Lists worktrees found in `./.worktrees` and the global folder.
  - Prompts selection and copies `.env` and `*.env` from local `dev` worktree.
  - Prints a ready-to-paste `cd "<target-worktree>"` command.

## Notes

- Makefile scripts assume bash-compatible shell.
- Many targets call scripts in `tools/scripts/`; inspect there for implementation details.
