# Skill: Database Migrations

How to create, apply, roll back, remove, and merge EF Core migrations in this repo.
**Never squash/merge migrations without explicit instruction** — always add a new migration for a schema change.

## Where migrations live

- Project: `libs/net/dal` (`TNO.DAL`). The design-time factory `TNOContextFactory` is in the same
  project, so `dotnet ef` runs from `libs/net/dal` with **no separate startup project**.
- Files per migration: `Migrations/<timestamp>_<version>.cs` + `.Designer.cs`, plus the shared
  `TNOContextModelSnapshot.cs` (current model state — updated by every add/remove).
- Migrations are **named by version**, e.g. `1.6.8`. EF prefixes the 14-digit timestamp; the
  `SeedMigration.Version` = the part after `<timestamp>_`.
- Raw-SQL seeds use `SeedMigration`: drop `*.sql` files (ordered by name, e.g. `0001_*.sql`) into
  `Migrations/<version>/{Up/PreUp, Up, Up/PostUp, Down/PreDown, Down, Down/PostDown}`. The migration's
  `Up()` calls `PreUp/PostUp` and `Down()` calls `PreDown/PostDown`.

## `make db-*` commands (apply-only, run inside the `tno-net` Docker network)

- `make db-update` — Build the migration image and run `dotnet ef database update` (applies **all
  pending** migrations forward). Standard local apply. Uses `libs/net/dal/.env` (`Host=database:5432`).
- `make db-refresh` — `db-drop.sh` (`DROP SCHEMA public CASCADE`) then `make db-update`. Full clean
  rebuild of the schema from scratch. **Destroys all data.**
- `make db-nuke` — Delete the DB container + volume, recreate, `db-update`, restart keycloak. Nuclear reset.

`make db-*` only ever moves **forward** to latest. It cannot target a specific version or roll back —
use `dotnet ef` directly for that.

## Direct `dotnet ef` commands (add / list / rollback / remove)

Run from `libs/net/dal`. Install the tool if missing: `dotnet tool install --global dotnet-ef`
(then `export PATH="$PATH:$HOME/.dotnet/tools"`).

**Connection string:** `dal/.env` points at `Host=database:5432` (Docker-internal). Any command that
touches the DB (`database update`, `migrations list`, `has-pending-model-changes`) must reach the DB.
The container maps Postgres to the host at **`localhost:40000`**. Two options:
- Temporarily set `ConnectionStrings__TNO=Host=localhost;Port=40000;...` in `dal/.env`, run, then restore it.
- Or run the command inside the `tno-net` network.
Offline commands (`migrations add`, `migrations script`, `migrations remove`) do **not** need the DB.

- Create a migration:        `dotnet ef migrations add <version>` (e.g. `1.6.8`)
- List (with applied state):  `dotnet ef migrations list`
- Apply all pending:          `dotnet ef database update`
- Roll back to a version:     `dotnet ef database update <version>` (runs `Down()` of everything after it)
- Remove the **last** migration: `dotnet ef migrations remove`  (must be unapplied — roll back first)
- Verify model == migrations:  `dotnet ef migrations has-pending-model-changes` → expect "No changes"
- Generate SQL offline:        `dotnet ef migrations script <from> <to> -o out.sql --no-build`

## Merging / squashing migrations down — **rollback BEFORE removing, then recreate**

The critical rule: **a migration's `Down()` can only run while its file still exists.** So you must
roll the database back *past* every migration you intend to collapse **before** deleting/rewriting any
files. Deleting files first leaves the DB with applied history rows whose `Down()` no longer exists —
you can no longer cleanly revert, and the schema/history desync.

### Case A — collapsing the most-recent migrations (simple)
1. Roll back to the migration you want to keep as the base:
   `dotnet ef database update <base-version>`
2. Remove the now-unapplied migrations, newest first: `dotnet ef migrations remove` (repeat).
3. Add the single replacement: `dotnet ef migrations add <version>`.
4. Apply forward: `dotnet ef database update` (or `make db-update`) and verify with
   `dotnet ef migrations has-pending-model-changes`.

### Case B — merging a range into one migration that is NOT the newest
When later migrations sit on top of the range (so `migrations remove` can't reach it), fold by hand:
1. **Roll back first** to the migration *before* the range:
   `dotnet ef database update <prev-version>`  ← runs every real `Down()` in the range, dropping its objects.
2. Fold the range's operations into the base migration's `.cs` (`Up()` in order, `Down()` in reverse),
   including any FKs/indexes/`SeedMigration` SQL folders. Update the base `.Designer.cs` snapshot to the
   model state **as of the last migration in the range** (copy that migration's Designer snapshot,
   keeping the base migration's `[Migration("...")]` id and class name).
3. **Delete** the `.cs` + `.Designer.cs` for every other migration in the range.
4. If any history rows for the deleted migrations remain (e.g. they were applied on a shared/local DB
   before you rolled back), remove them: `DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" ~ '<pattern>';`
5. Apply forward and **validate the round-trip**: `dotnet ef database update` then confirm
   `has-pending-model-changes` returns "No changes", and diff the rebuilt table columns against a
   pre-squash baseline.

### Validation (always do this after a merge)
Prove the merged migration applies and reverts from the true baseline:
`dotnet ef database update <prev-version>` (down) → confirm the objects are gone →
`dotnet ef database update` (up) → confirm schema matches baseline, seeds re-ran, and
`has-pending-model-changes` = "No changes".

## Notes

- Don't delete migration files that are already merged to `main` / applied on shared environments —
  squash only local, uncommitted, feature-branch migrations.
- Rolling back drops the affected tables → **local data loss**; re-seed after (SeedMigration `PostUp`
  SQL re-runs automatically on the forward apply).
- After any add/remove, `TNOContextModelSnapshot.cs` must reflect the final model
  (`has-pending-model-changes` is the gate).
