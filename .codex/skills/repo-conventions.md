# Skill: Repository Conventions

## Apply Always

- Follow `AGENTS.md` and root `CLAUDE.md` constraints.
- Use npm (Node 26) for `src/app/` — the editor and subscriber designs are one Vite project.
- Use new SQL migrations for schema changes; do not squash old migrations.
- Prefer existing architecture patterns for API routes, DAL services, and service managers.

## Build Requirements

- `src/app/**` -> `cd src/app && npm run typecheck && npm run build`
- `src/api/**` -> `dotnet build src/api/`
- `src/services/<name>/**` -> `dotnet build src/services/<name>/`
- `src/libs/**` -> `dotnet build src/libs/TNO.sln`
