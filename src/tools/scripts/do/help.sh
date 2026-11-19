#!/usr/bin/env bash

do_help() {
  cat <<'HELP'
Usage: bash do <command> [options]
       bash do docker <command> [options]
       bash do podman <command> [options]
       bash do oc <command> [options]

Linux / macOS: Bash 3.2 or newer. Windows: Git Bash or WSL.
Run from the repository root, or invoke /path/to/repo/do from anywhere.
Docker uses Compose v2 with a docker-compose fallback; Podman uses podman compose.
OpenShift image/release operations retain their Docker/ACR tooling requirements.
Flags use standard syntax: -n api, -p main, -v, -y, -d, --offline.
Use --from, --to, --output (or -o) for SQL export. -h/--help shows help.
Use -n api or -p main to select a local service or profile.
Examples: bash do docker up -n api
          bash do podman up -p main
          bash do oc deploy -e dev -n api -t latest
HELP
  case "${1:-}" in
    db) database_help; return ;;
    '') cat <<'HELP'
move                     Move existing local .env files to src/ and dependencies/ without overwriting settings.
setup                    Setup local environment for development, generate configuration files.
worktree                 List available worktrees, copy .env files from selected source branch, and print cd command.
HELP
      database_help
      ;;
    docker|podman) cat <<'HELP'
nuke                     Stop all containers, delete all containers, volumes, and configuration (-y skips the confirmation)
build                    Builds all containers or the one specified (args: -n {service name}, -p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]})
up                       Starts all containers or the one specified (args: -n {service name}, -p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
stop                     Stops all containers or the one specified (args: -n {service name}, -p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
down                     Stops all containers and removes them (-p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}, -v also deletes the volumes (all local data), -y skips the confirmation)
restart                  Restart all containers or the one specified (-n {service name}, -p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
refresh                  Stop, build, and start all containers or the one specified (args: -n {service name}, -p {profile name, [all,api,editor,subscriber,kafka,service,utility,ingest]}))
refresh-keycloak         Stop, build, drop DB and recreate
remove                   Remove all containers
renew                    Refresh all relevant services that were impacted by prior Pull Request.
db-nuke                  Stop and delete the database container and volume, then rebuild and start.
elastic-update           Run the elasticsearch migration.
reset-consumer-offset    Reset the consumer group topic offset.
kafka-update             Apply the Kafka topic migrations (-e environment {local,dev,test,prod} [local], -n version [all], -r roll back version n, -b bootstrap server, -p default partitions, -f default replication factor, -d dry run, -y skip confirmation)
kafka-topic-add          Create or update a kafka topic (-e environment [local], -t topic, -b bootstrap server, -p partitions, -r replication factor, -c config key=value[,...], -i broker pod index, -d dry run)
kafka-topic-delete       Delete a kafka topic and its messages (-e environment [local], -t topic, -b bootstrap server, -i broker pod index, -d dry run, -y skip confirmation)
HELP
      database_help
      ;;
    oc) cat <<'HELP'
switch-current           Switch environment within the current project prefix (old root make switch; -n tno returns to TNO tools).
switch                   Switch active project (-n environment {dev,test,prod,tools} [tools]).
clean                    Delete old pods that are no longer running (-n environment {dev,test,prod,tools} [dev]).
pod                      Get the full pod names matching a label (-n label selector, e.g. name=api).
logs                     Show logs for all containers within a pod (-n label selector, -i pod index [0]).
rsh                      Remote shell into a pod (-l label selector, -i pod index [0]).
port-forward             Port forward to a pod (-n label selector, -l local port, -r remote port, -i pod index [0]).
db-connect               Port forward to the database (-e environment {dev,test,prod,tools} [dev], -l local port [22222], -r remote port [5432]).
release                  Build and push every image to ACR, then deploy the whole environment (-e environment {dev,test,prod} [dev], -t tag [short git commit], -n "components" to build [all], -m run db-migration first, -d dry run, -y skip confirmation).
deploy                   Deploy to environment - retags images to e and restarts (-e environment {dev,test,prod} [dev], -t source tag to promote [latest], -n single workload name, e.g. charts-api, db-migration, or elastic-migration [all], -m migration target/version, 0 reverts all for db-migration only [apply all pending], -s db credential secret for either migration job, e.g. montford [whatever the api uses], -p elastic-migration step {prepare,copy,verify,cutover} of migration m [all]).
stop                     Stop environment - scales every service to 0 (-e environment {dev,test,prod} [dev]).
up                       Start up environment - scales every service to its per-environment replica count (-e environment {dev,test,prod} [dev]).
annotate                 Annotate an object to force a rollout (-n object type/name, e.g. deployment/api-services, -e environment {dev,test,prod} [dev]).
kafka-update             Apply the Kafka topic migrations to an environment (-e environment {dev,test,prod} [dev], -n version [all], -r roll back version n, -p default partitions, -f default replication factor, -d dry run, -y skip confirmation). Settings: dependencies/kafka/environments/<e>.conf.
kafka-topic-add          Create/update one topic (-e dev, -t topic, -b bootstrap, -p partitions, -r replicas, -c config, -i broker index, -d).
kafka-topic-delete       Delete one topic (-e dev, -t topic, -b bootstrap, -i broker index, -d, -y).
kafka-topics             List the Kafka topics in an environment with their partitions (-e environment {dev,test,prod} [dev]).
build                    Build a linux/amd64 image tagged for ACR (-n component including elastic-migration, -t tag [latest], -e Dockerfile variant {prod,local} [prod]).
push                     Push a locally built image to ACR (-n component including elastic-migration, -t tag [latest]).
pull                     Pull an image from ACR to the local registry (-n component including elastic-migration, -t tag [latest]).
tag                      Retag an image in ACR so both tags point at the same image (-n component including elastic-migration, -f from tag [latest], -t to tag, -r where to tag {remote,local} [remote]).
project                  Switch to TNO project namespace within Openshift: 9b301c
HELP
      ;;
    *) die "Unknown help group: $1" ;;
  esac
}

database_help() {
  cat <<'HELP'

Database commands: bash do db-* (host .NET 10 + dotnet-ef 10.0.14)
                   bash do docker db-* / bash do podman db-* (SDK container)
Options and behavior are the same in both modes. Configuration: src/libs/dal/.env;
exported ConnectionStrings__TNO / DB_POSTGRES_USERNAME / DB_POSTGRES_PASSWORD override it.
Host commands require a host-reachable database address (usually localhost:40000).
Containers use tno-net by default; DB_NETWORK overrides it (none omits the network option).

db-migrations            List migrations and applied state (--offline skips database access).
db-add                   Add migration and TNO seed hooks (-n version required).
db-apply                 Apply migrations (-n target optional; default latest; 0 rolls back all).
db-rollback              Roll back to -n version or -n 0 (required).
db-remove                Remove last migration with EF --force; may roll back the database.
db-drop                  Drop database (EF prompts; -y skips confirmation).
db-refresh               Roll back all migrations to 0, then apply latest (destroys data).
db-clean                 Force-drop database, then apply latest (destroys data).
db-script                Export SQL (--from 0, --to latest, --output db-migration.sql relative to root).
db-recreate              Roll back to -r 0, remove last, add -n Initial with seed hooks, apply latest.

Generated files persist in the checkout. Container bin/obj and package caches are temporary.
db-nuke is container-only: it removes the database container/volume and restarts Keycloak.
See docs/DO.md for prerequisites, configuration, and the full command reference.
HELP
}
