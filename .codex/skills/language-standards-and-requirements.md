# Skill: Language Standards And Requirements

## Runtime Requirements

- Node: `18.x` (repo targets `>=18.11.0 <19.0.0`)
- Yarn: `3.x` (Berry); use Yarn in `app/*`, `libs/npm`, and `api/node`
- .NET: `net9.0` projects across API/services/libs
- Bash: scripts under `tools/scripts/**` and build helpers
- Python: utility scripts only (keep dependencies minimal)

## C# (.NET)

- Use DI and options binding patterns already used in `api/net/Program.cs`.
- Keep controllers under `api/net/Areas/**/Controllers` and business logic in services/helpers.
- Use nullable reference types and async/await patterns consistently.
- Build after changes: `dotnet build api/net/` or affected project.
- Database rule: add a new migration in `db/postgres/` for schema changes.

## TypeScript (React + Node)

- Prefer typed models and hooks; avoid `any` unless unavoidable.
- Frontend conventions: Redux slices/hooks, Formik controls, and `Show` from `tno-core`.
- Keep import sorting and lint rules passing (`simple-import-sort`, React lint rules).
- Validate with: `yarn build`, `yarn test`, `yarn lint` in affected package.

## JavaScript

- Use only where existing codebase expects JS (legacy config/scripts).
- Follow repo formatting rules (`.editorconfig`, Prettier where configured).
- Avoid introducing new plain JS modules when TS equivalents exist.

## Java

- Java is infrastructure-adjacent in this repository (tooling/container side), not a primary app language.
- Keep Java changes isolated to the relevant tool image/config area and avoid cross-layer coupling.
- Follow `.editorconfig` 4-space indentation for `*.java`.

## Shell (Bash)

- Use `#!/usr/bin/env bash` shebang for new scripts.
- Use strict mode for new scripts: `set -euo pipefail`.
- Quote paths/variables, especially with spaces.
- Prefer reusable scripts in `tools/scripts/` and call them from `Makefile` targets.

## Python

- Keep tooling scripts small and self-contained.
- Prefer stdlib (`argparse`, `json`, `pathlib`) over new dependencies.
- Make script behavior explicit and deterministic for CI/local tooling.

## Node (api/node)

- Keep Chart API in TypeScript and compile via `yarn build`.
- Runtime entry is compiled output (`node build/server.js`).
- Use existing scripts: `yarn start:watch`, `yarn build`, `yarn lint`.

## Formatting Baseline

- Global `.editorconfig` sets LF endings and trimmed trailing whitespace.
- Default indent: 4 spaces for `*.cs`, `*.ts`, `*.java`; 2 spaces for JSON/YAML/Markdown.
- Makefiles must use tabs.
