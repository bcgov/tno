# Skill: Database Migrations

Use for TNO EF migration creation, application, rollback, removal, and explicitly
requested consolidation. Follow the one-new-migration-per-PR rule in `AGENTS.md`;
never consolidate migrations already deployed to shared environments.

## Paths and seed hooks

- Project: `src/libs/dal/TNO.DAL.csproj`; `TNOContextFactory` provides design-time
  configuration, so no separate startup project is needed.
- Each migration has `Migrations/<timestamp>_<version>.cs`, its `.Designer.cs`, and
  changes to `TNOContextModelSnapshot.cs`. Name it with the current version.
- `bash do db-add -n <version>` (or `bash do docker db-add -n <version>`) converts the
  new migration to `SeedMigration` and inserts `PreUp`, `PostUp`, `PreDown`, `PostDown`.
- Seed SQL belongs in `Migrations/<version>/{Up/PreUp,Up,Up/PostUp,Down/PreDown,Down,Down/PostDown}`.
  Prefix files to control ordering, e.g. `0001_*.sql`. SQL must be copied to build
  output for `SeedMigration` to load it.

## Execution and configuration

Read [the database command reference](../../docs/DO.md#local-and-container-database-commands)
for every option, defaults, prerequisites, and examples. `bash do help db` prints a
compact reference. All commands below run from the repository root:

| Operation | Command |
| --- | --- |
| List applied/pending migrations | `bash do db-migrations` |
| List without connecting | `bash do db-migrations --offline` |
| Add migration + seed hooks | `bash do db-add -n <version>` |
| Apply latest / target | `bash do db-apply [-n <version>]` |
| Roll back | `bash do db-rollback -n <version-or-0>` |
| Remove last | `bash do db-remove` (EF `--force`; can roll back an applied migration) |
| SQL only | `bash do db-script --from <base> --to <target> --output artifacts/migration.sql` |
| Drop database | `bash do db-drop` (EF prompts; `-y` forces) |
| Revert all and reapply | `bash do db-refresh` (data loss) |
| Force-drop and reapply | `bash do db-clean` (data loss) |
| Replace last migration | `bash do db-recreate -r <base> -n <version>` (rollback, remove, add + hooks, update) |

Insert `docker` or `podman` after `do` for the identical EF command inside the SDK
container, without local .NET dependencies. Migration/source/SQL changes persist on
the host; library build products are isolated in temporary container mounts.
`bash do docker db-nuke` is separate container lifecycle management, deleting the
DB container/volume and restarting Keycloak; it has no host EF equivalent.

Host tools: .NET 10 SDK, dotnet-ef 10.0.14. DAL `.env` and optional connectionstrings
JSON files supply configuration; exported connection/credential variables override
it. The generated database hostname `database:5432` only resolves within the Compose
network. For host commands, export a host connection string, for example:

```bash
ConnectionStrings__TNO='Host=localhost;Port=40000;Database=tno' bash do db-migrations
```

Keep existing credentials in the DAL environment. Use a custom published port when
configured, or `oc db-connect` plus its forwarded port. Container commands use
`tno-net` by default (`DB_NETWORK` overrides it). Offline list/add/script operations
still need valid design-time configuration, but need not reach a database. Removal
may query or roll back the database; do not treat it as offline-safe.

## Consolidation: back up and roll back before replacing files

Only do this when requested, for local/feature migrations that have not been deployed
to shared environments. `db-recreate` is a convenience command, not a backup or a
replacement for reviewing the existing migration chain.

1. Back up affected feature data and record the schema/history baseline.
2. With the original migration files still present, run
   `bash do db-rollback -n <last-kept-version>` (or the Docker equivalent). Every old
   `Down()` must execute before its source/designer is removed or replaced.
3. Remove now-unapplied migrations newest first; create the one replacement with
   `db-add -n <version>`. Include its designer, seed hooks/SQL, and final model snapshot.
4. Apply the replacement, restore backed-up feature data, and verify
   `__EFMigrationsHistory`. Never repair consolidation by deleting history rows.
   Unexpected rows indicate the rollback needs investigation, not manual erasure.
5. Test rollback/reapplication against a disposable database from the real baseline,
   then verify `dotnet ef migrations has-pending-model-changes` from `src/libs/dal`
   and build `src/libs/dal/TNO.DAL.csproj`.

If later migrations sit above the intended range, roll back through all affected
migrations first. Preserve their operations and seed behavior in the final chain;
do not manually splice snapshots or rewrite already deployed migrations.
