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
