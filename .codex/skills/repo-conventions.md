# Skill: Repository Conventions

## Apply Always

- Follow `AGENTS.md` and root `CLAUDE.md` constraints.
- Use npm (Node 26) for `app/` — the editor and subscriber designs are one Vite project.
- Use new SQL migrations for schema changes; do not squash old migrations.
- Prefer existing architecture patterns for API routes, DAL services, and service managers.

## Build Requirements

- `app/**` -> `cd app && npm run typecheck && npm run build`
- `api/net/**` -> `dotnet build api/net/`
- `services/net/<name>/**` -> `dotnet build services/net/<name>/`
- `libs/net/**` -> `dotnet build libs/net/TNO.sln`
