#!/bin/bash

#################################################
# Apply the Kafka topic migrations (db/kafka/migrations/V<version>.sh), in version order, to an
# environment: the local Docker broker, or an OpenShift namespace's brokers. Migrations declare
# topics with 'ensure_topic' and 'delete_topic' (see kafka.sh), so applying them again is safe.
# Rolling back runs the one version's U<version>.sh, which deletes topics and their messages.
#
# Usage: migration.sh [-e environment] [-n version] [-r] [-z bootstrap] [-p partitions]
#                     [--replication factor] [-d] [-y]
#   -e, --environment  local (default), dev, test, prod; settings in db/kafka/environments
#   -n, --version      apply only this version (default: every version)
#   -r, --rollback     roll back the version given with -n
#   -z, --bootstrap    bootstrap server, as seen from inside the broker
#   -p, --partitions   partitions of new topics that do not specify their own
#       --replication  replication factor of new topics that do not specify their own
#   -d, --dry-run      show the changes without making them
#   -y, --yes          do not ask for confirmation
#################################################

set -o errexit -o pipefail -o noclobber -o nounset

! getopt --test > /dev/null
if [[ ${PIPESTATUS[0]} -ne 4 ]]; then
    echo 'I’m sorry, `getopt --test` failed in this environment.'
    exit 1
fi

OPTIONS=e:n:rz:p:dy
LONGOPTS=environment:,version:,rollback,bootstrap:,partitions:,replication:,dry-run,yes

! PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")
if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    exit 2
fi
eval set -- "$PARSED"

environment=local version="" rollback=false yes=false
while true; do
  case "$1" in
    -e|--environment) environment="$2"; shift 2 ;;
    -n|--version) version="$2"; shift 2 ;;
    -r|--rollback) rollback=true; shift ;;
    -z|--bootstrap) export KAFKA_BOOTSTRAP="$2"; shift 2 ;;
    -p|--partitions) export KAFKA_PARTITIONS="$2"; shift 2 ;;
    --replication) export KAFKA_REPLICATION_FACTOR="$2"; shift 2 ;;
    -d|--dry-run) export KAFKA_DRY_RUN=true; shift ;;
    -y|--yes) yes=true; shift ;;
    --) shift; break ;;
    *) echo "Programming error"; exit 3 ;;
  esac
done

SCRIPTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
MIGRATIONS_DIR="$(dirname "$SCRIPTS_DIR")/migrations"
# shellcheck source=kafka.sh
. "$SCRIPTS_DIR/kafka.sh"

kafka_load_environment "$environment"

if [ "$rollback" = true ]; then
  if [ -z "$version" ]; then
    echo >&2 "A rollback deletes topics and their messages: name the version to roll back with -n."
    exit 1
  fi
  prefix=U
else
  prefix=V
fi

# Migrations in version order (V1.0.10 after V1.0.9).
files=()
while IFS= read -r file; do
  files+=("$file")
done < <(find "$MIGRATIONS_DIR" -maxdepth 1 -type f -name "$prefix${version:-*}.sh" | sort -V)
if [ ${#files[@]} -eq 0 ]; then
  echo >&2 "No migration found: $MIGRATIONS_DIR/$prefix${version:-*}.sh"
  exit 1
fi

target="$KAFKA_TARGET"
[ "$KAFKA_TARGET" = "openshift" ] && target="namespace $KAFKA_NAMESPACE (pod $KAFKA_BROKER_POD)"
[ "$KAFKA_TARGET" = "docker" ] && target="container $KAFKA_CONTAINER"
echo "Kafka migration: environment $environment, $target, bootstrap $KAFKA_BOOTSTRAP"
echo "  default partitions $KAFKA_PARTITIONS, replication factor $KAFKA_REPLICATION_FACTOR${KAFKA_TOPIC_CONFIG:+, config $KAFKA_TOPIC_CONFIG}"
echo "  $([ "$rollback" = true ] && echo rollback || echo apply): $(for f in "${files[@]}"; do basename "$f" .sh; done | tr '\n' ' ')"
[ "${KAFKA_DRY_RUN:-false}" = "true" ] && echo "  dry run: nothing is changed"

if [ "$yes" != true ] && [ "${KAFKA_DRY_RUN:-false}" != "true" ] && { [ "$rollback" = true ] || [ "$KAFKA_TARGET" != "docker" ]; }; then
  read -r -p "Continue? [y/N] " answer
  [[ "$answer" =~ ^[Yy]$ ]] || { echo "Cancelled."; exit 1; }
fi

kafka_check_access

for file in "${files[@]}"; do
  echo
  echo "== $(basename "$file" .sh)"
  # shellcheck source=/dev/null
  . "$file"
done
