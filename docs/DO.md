# Root Bash command reference

Run `bash do ...` from the repository root, or `bash /path/to/mmi/do ...` from another
directory. The script changes to its own repository root. It replaces the root and
`openshift/Makefile`; do not change to `openshift` before invoking it.

## Platforms and dependencies

- Linux: Bash 3.2+ and standard Unix utilities.
- macOS: system Bash 3.2 is supported; GNU grep/sed are not required by the entrypoint,
  setup, worktree, or database helpers.
- Windows: use Git Bash or WSL, not PowerShell/cmd directly. In WSL, install tools in
  WSL and enable the container engine's WSL integration. Git Bash container mounts
  use Windows paths; automatic MSYS argument conversion is disabled.
- `setup` / `move`: Python 3 available as `python3`. `worktree`: Git.
- Root `db-*`: .NET 10 SDK and `dotnet-ef` 10.0.14 on PATH. Install the tool with
  `dotnet tool install --global dotnet-ef --version 10.0.14`; add `$HOME/.dotnet/tools`
  to PATH if necessary.
- `docker`: a running Docker engine. Stack commands need `docker compose` or legacy
  `docker-compose`. Database commands need only Docker, with no local .NET or Python.
- `podman`: Podman; stack commands need a Compose provider. Start a Podman machine
  on macOS/Windows. Database commands do not need Compose or local .NET.
- `oc`: the OpenShift CLI and a login for cluster commands. ACR image/release commands
  retain their Docker and registry credential requirements; Azure CLI is used when
  existing Docker or explicitly supplied ACR credentials are unavailable.

## Discovery and arguments

```bash
bash do help
bash do help db
bash do docker help
bash do podman help
bash do oc help
```

Commands use standard flags: `-n api`, `-p main`, `-e dev`. Short values may be
attached (`-napi`); switches may be grouped (`-vy`). SQL export accepts `--from 0`,
`--to <version>`, and `--output path` (or `-o path`); long value options also accept
`--output=path`. `--offline` skips database access when listing migrations.
`-h` / `--help` works after a command as well as after a group.

Boolean switches (`-v`, `-y`, `-d`, `--offline`) take no value; omit to disable.
`--volumes`, `--yes`, and `--dry-run` are aliases for `-v`, `-y`, and `-d`.
`-r` is a rollback switch for `kafka-update`, but takes a value for topic replication,
image tagging, port forwarding, or database recreation. `oc release -m` enables the
migration step; `oc deploy -m <version>` selects a migration target.

Unknown options, missing/empty values, and unexpected positional arguments fail
before work starts. Make-style `n=api` arguments are rejected. Quote values containing
spaces or shell characters. Repeating a value option uses the last value. Local
Compose `-n` selects one **service** (`api`, not `tno-api`); use `-p` for a profile or
run the command separately for multiple services. `oc release -n "api app"` explicitly
accepts a list of components. Environment variables still use ordinary shell assignment
syntax (for example `DB_NETWORK=none bash do docker db-migrations --offline`).

## Configuration and worktrees

| Command | Behavior |
| --- | --- |
| `bash do move` | Move ignored environment files from old layouts without overwriting conflicting settings. |
| `bash do setup` | Run `move`, generate missing environment files, create the app's `node_modules` directory. Prompts for configuration. |
| `bash do worktree` | Discover worktrees, prompt for source branch (default `master`), copy environment files into the selected worktree, print a `cd` command. Uses `KEPLER_GLOBAL_WORKTREE_FOLDER` in root `.env`. |

## Local and container database commands

The same EF commands/options run with local tools or inside an SDK container:

```bash
bash do db-migrations --offline
bash do db-add -n 1.5.17
bash do db-apply -n 1.5.17
bash do docker db-migrations
bash do docker db-add -n 1.5.17
bash do docker db-script --from 1.5.16 --to 1.5.17 --output artifacts/migration.sql
bash do podman db-rollback -n 1.5.16
```

Use only one of the equivalent `db-add` examples for a particular migration.

| Command | Options and behavior |
| --- | --- |
| `db-migrations` | List migrations and applied state. `--offline` passes `--no-connect`. |
| `db-add` | Required `-n <version>`. Generate migration/designer/snapshot, then convert the new migration to `SeedMigration` with `PreUp`, `PostUp`, `PreDown`, `PostDown` hooks. Does not require VS Code. |
| `db-apply` | Optional `-n <target>`; omitted means latest. An older target rolls back; `-n 0` reverts all. |
| `db-rollback` | Required `-n <target>` or `-n 0`; invokes EF database update to that target. |
| `db-remove` | Remove the last migration using EF `--force`, including rollback if it is applied and the database is reachable. This is not an offline-only operation. |
| `db-drop` | EF database drop, with its confirmation prompt. `-y` passes `--force`. |
| `db-refresh` | Revert migrations to `0`, then apply latest. Destroys migrated data. |
| `db-clean` | Force-drop the database, then apply latest. Destroys all database data. |
| `db-script` | `--from 0`, `--to <latest>` by default; `--output db-migration.sql`. Output is relative to the repository root and must stay inside it. SQL generation does not apply it. |
| `db-recreate` | `-r 0`, `-n Initial` defaults. Roll back to `r`, remove the last migration, add `n` with seed hooks, then apply latest. Use explicit version names for TNO work. Stops on the first failure. |

All commands restore packages before running EF, so a fresh checkout works. Offline
means no database connection, not no NuGet access. Commands use
`src/libs/dal/TNO.DAL.csproj` from the DAL directory, including the
existing design-time factory. They read `src/libs/dal/.env` and optional
`connectionstrings*.json`; exported `ConnectionStrings__TNO`, `DB_POSTGRES_USERNAME`,
`DB_POSTGRES_PASSWORD`, and `ASPNETCORE_ENVIRONMENT` take precedence.

The generated DAL environment uses Docker's `database:5432`. For **host** EF commands,
use a host-reachable connection string. With the default Compose port:

```bash
ConnectionStrings__TNO='Host=localhost;Port=40000;Database=tno' bash do db-migrations
```

The username/password still come from the existing DAL environment. If `DATABASE_PORT`
is customized, use that published port. Export the connection string for a series of
host commands; the wrapper does not rewrite environment files or guess remote targets.

**Container mode:** the script builds the cached `tno:db-tools` image from
`src/tools/scripts/do/Dockerfile.db` (.NET SDK 10.0, EF 10.0.14), then runs the same root
command inside it. It joins `tno-net`, mounts the checkout at `/workspace`, and maps
`host.docker.internal` to the host gateway. Start the local database with
`bash do docker up -n database` (or Podman) first. `DB_NETWORK=<network>` changes the
network; `DB_NETWORK=none` omits the network selection for offline generation or a
separately reachable database—it does not disable outbound networking.

Generated migrations, snapshots, seed-hook edits, and SQL output persist in the checkout.
Library `bin`/`obj` paths are temporary mounts, so host and container build artifacts
cannot mix. NuGet packages are restored into a temporary container cache. Containers
use the host UID/GID on Unix, and keep stdin open (with a TTY when available) for EF
confirmation prompts. An unavailable engine/network, failed build, or failed EF command
returns nonzero and stops subsequent steps.

`bash do docker db-nuke` / `bash do podman db-nuke` are **container-only lifecycle**
operations: stop Keycloak/database, delete the database container and volume, start it,
run the existing migration bundle, restart Keycloak. There is no root `db-nuke`.
The compound `renew` command also retains its existing migration-bundle apply step.

For version naming, seed SQL layout, backup/rollback, and consolidation rules, read the
repository [database migration skill](../.codex/skills/db-migrations.md).

## Docker and Podman stack commands

Use `bash do docker <command>` or substitute `podman`. The four Compose files are
`docker-compose.yml`, `docker-compose.override.yml`, `dependencies/kafka/docker-compose.yml`,
and `src/services/docker-compose.yml`.

| Command | Options and behavior |
| --- | --- |
| `build` | `-n service`, `-p profile`; build with `--force-rm`. |
| `up` | `-n service`, `-p profile`; start detached with root `.env`. Recreates changed configuration. |
| `stop` | `-n service`, `-p profile`; stop containers. |
| `restart` | `-n service`, `-p profile`; stop, then up. |
| `refresh` | `-n service`, `-p profile`; stop, remove selected containers, build, up. |
| `remove` | `-n service`, `-p profile`; Compose `rm -s -v` (anonymous volumes, not project named volumes). |
| `down` | `-p profile`; remove stack, keep named volumes by default. `-v` deletes volumes after confirmation; `-y` skips it. |
| `nuke` | Down with volumes, then the existing configuration backup/removal helper. `-y` skips the volume prompt only; configuration prompts remain. |
| `refresh-keycloak` | Stop/remove Keycloak, drop/recreate its database, build/start. |
| `renew` | Stop indexing; refresh API/editor/subscriber; apply database and Elasticsearch migrations; refresh indexing. |
| `elastic-update` | Build/run the Elasticsearch migration image on `tno-net`. |
| `reset-consumer-offset` | Prompt for group/topic, reset to earliest in the local broker. Stop the relevant consumer first. |

Without `-p`, `up`/`build` retain Compose defaults; selecting `-n` enables profile `all`.
`stop`/`down`/`remove` default to profile `all`. Use `-p main` for the core stack.

## Kafka commands

Available under `docker`/`podman` (default `-e local`) and `oc` (default `-e dev`).
Environment configuration is in `dependencies/kafka/environments/<e>.conf`. A nonlocal
`e` uses the configured OpenShift broker, even under a container command group.

| Command | Options |
| --- | --- |
| `kafka-update` | `-e environment`, `-n version` (all by default), `-r` rollback selected version, `-b bootstrap`, `-p partitions`, `-f replication`, `-d` dry run, `-y` skip confirmation. |
| `kafka-topic-add` | `-e environment`, `-t topic`, `-b bootstrap`, `-p partitions`, `-r replication`, `-c key=value,...`, `-i broker index`, `-d`. |
| `kafka-topic-delete` | `-e environment`, `-t topic`, `-b bootstrap`, `-i broker index`, `-d`, `-y`. |
| `oc kafka-topics` | `-e dev`; list topics with partition and replication details. |

## OpenShift commands

All examples run at the repository root: `bash do oc <command> ...`.

| Command | Options and defaults |
| --- | --- |
| `project` | Switch to `9b301c-tools`. |
| `switch` | `-n tools`; select `9b301c-<n>`. |
| `switch-current` | Old root `switch` behavior: keep current project prefix and select `-n tools`; `-n tno` returns to TNO tools. |
| `clean` | `-n dev`; delete pods whose phase is Succeeded. |
| `pod` | `-n label-selector`, e.g. `-n app=api`. |
| `logs` | `-n label-selector`, `-i 0`; logs for all containers in selected pod. |
| `rsh` | `-l label-selector`, `-i 0`; remote shell. |
| `port-forward` | `-n label-selector`, `-i 0`, required `-l local-port`, `-r remote-port`. |
| `db-connect` | `-e dev`, `-l 22222`, `-r 5432`; forward PostgreSQL. Host EF commands must use the forwarded port. |
| `stop`, `up` | `-e dev`; stop or restore environment-specific replica counts. |
| `annotate` | `-e dev`, `-n kind/name`; annotate to trigger rollout. |
| `build` | `-n component`, `-t latest`, `-e prod` Dockerfile variant (`prod` or `local`); build linux/amd64 for ACR. |
| `push`, `pull` | `-n component`, `-t latest`; move image to/from ACR. |
| `tag` | `-n component`, `-f latest`, required `-t destination`, `-r remote` (`remote` or `local`). |
| `release` | `-e dev`, `-t <short commit>`, `-n "components"` (all), `-m` database migration first, `-d` dry run, `-y` skip confirmation. Requires clean tree unless `ALLOW_DIRTY=1`. |
| `deploy` | `-e dev`, `-t latest`, optional `-n workload`; promote images and restart. Migration jobs require explicit `-n db-migration` or `-n elastic-migration`. `-m target`, `-s credential-secret`, `-p prepare/copy/verify/cutover` for Elasticsearch. |

OpenShift `db-connect` forwards a port; it does not execute EF. Use host commands over
that port or `oc deploy -n db-migration` for a cluster migration job. See
[OpenShift operations](../openshift/README.md) and the [ACR skill](../.codex/skills/acr-images.md)
for registry behavior and migration-job settings.

## Verification when changing the wrapper

Run `bash -n do`, syntax-check changed helpers, `shellcheck -x do` (and standalone helpers),
`python3 -m unittest discover -s src/tools/scripts/tests -v`, and relevant
`openshift/scripts/tests` tests. `do-validation.yml` runs command contracts on Linux,
macOS system Bash, and Windows Git Bash. For database tooling changes, also build the
SDK image and verify offline SQL generation or migration listing with both tool paths.
Never test reset/drop/update operations against an existing database just to validate dispatch.
