# Skill: Bash Do Commands

Use for TNO local setup, container lifecycle, EF migrations, Kafka administration,
and OpenShift operations. Run the root `bash do` entrypoint; the root and OpenShift
Makefiles have been removed.

## Select the execution context

- `bash do setup`, `move`, `worktree`: host configuration helpers.
- `bash do db-*`: host .NET 10 SDK and dotnet-ef 10.0.14, from `src/libs/dal`.
- `bash do docker db-*` / `bash do podman db-*`: the same EF operation/options in an
  SDK container; no host .NET required. Generated source/SQL persists in the checkout.
- `bash do docker ...` / `bash do podman ...`: local stack and data services.
- `bash do oc ...`: cluster and ACR commands, still invoked from the repository root.

Read [the command reference](../../docs/DO.md) for complete command/option tables,
platform prerequisites, database connection settings, destructive-command behavior,
and validation. Discover current options with `bash do help`, `bash do help db`,
and `bash do docker help` / `bash do podman help` / `bash do oc help`.

## Important distinctions

- Options use flags (`-n api`, `-p main`, `--offline`); Make-style assignments are rejected.
  Quote values containing spaces. `-n api` is a Compose
  service, never `-n tno-api`. Select one service or a profile (`-p main`); repeat a
  command for multiple services. Repeated value options use the last value.
- `oc release -n "api app"` explicitly accepts multiple build components. ACR build
  uses `-n app`; Compose uses `-n editor` / `-n subscriber` for the two running designs.
- `oc build -e prod` selects a Dockerfile variant; `oc deploy -e prod` selects a cluster
  environment. Do not interchange these meanings.
- `oc switch` selects a TNO namespace; `oc switch-current` preserves the former root
  switch command's behavior of retaining the current namespace prefix.
- Boolean switches `-v`, `-d`, `-y`, and `--offline` take no value; omit to disable.
  `down` keeps named volumes unless `-v` is supplied. `nuke -y` skips only the volume
  confirmation; the configuration helper still prompts. `kafka-update -r` is a
  rollback switch; other `-r` uses take a value. `oc release -m` enables migration,
  while `oc deploy -m <version>` selects a target. SQL uses `--from`, `--to`, `--output`
  (or `-o`). `-h`/`--help` works on commands; short switches may be grouped (`-vy`).
- Host EF uses DAL configuration; generated `Host=database:5432` is container-only.
  Export `ConnectionStrings__TNO` with `Host=localhost;Port=40000;Database=tno` for the
  default local database. Do not overwrite committed settings or expose credentials.
- Container EF defaults to `tno-net`; `DB_NETWORK` overrides it. `DB_NETWORK=none`
  omits explicit network selection for offline commands. `db-nuke` is container-only.
- `db-refresh` rolls migrations back to 0 then applies latest; `db-clean` force-drops
  the database then applies latest. Both destroy data. `db-remove` retains EF's
  `--force` behavior and can roll back an applied migration. Read
  [database migration guidance](db-migrations.md) before changing migration history.
- `db-add -n <version>` installs TNO's seed hooks in the newly generated migration;
  do not use a bare EF add and forget those hooks. No editor launch is required.
- Linux, macOS Bash 3.2, and Windows Git Bash/WSL are supported. Avoid Bash 4-only
  features, GNU-only sed/grep, and unquoted paths when editing these helpers.

## Typical commands

```bash
bash do setup
bash do docker up -n database
bash do docker db-migrations
bash do docker db-apply
bash do docker up -p main
bash do db-migrations --offline
bash do db-add -n 1.5.17
bash do docker db-script --from 1.5.16 --to 1.5.17 --output artifacts/migration.sql
bash do docker refresh -n api
bash do oc kafka-update -e dev -d
bash do oc build -n api -t candidate
bash do oc deploy -e dev -n api -t candidate
```

Choose operations according to the user's task; these examples are not a sequence
that should be executed automatically. Script validation uses mocked commands and
offline EF operations, not destructive resets against the developer's database.
