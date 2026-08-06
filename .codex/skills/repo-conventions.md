# Skill: Repository Conventions

## Apply Always

- Follow `AGENTS.md` and root `CLAUDE.md` constraints.
- Use Yarn for `app/editor` and `app/subscriber`.
- Use new SQL migrations for schema changes; do not squash old migrations.
- Prefer existing architecture patterns for API routes, DAL services, and service managers.

## Build Requirements

- `app/subscriber/src/**` -> `cd app/subscriber && yarn build`
- `app/editor/src/**` -> `cd app/editor && yarn build`
- `api/net/**` -> `dotnet build api/net/`
- `services/net/<name>/**` -> `dotnet build services/net/<name>/`
- `libs/net/**` -> `dotnet build libs/net/TNO.sln`
