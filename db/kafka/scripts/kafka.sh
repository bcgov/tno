#!/bin/bash

#################################################
# Kafka topic helpers, sourced by migration.sh, topic-add.sh and topic-delete.sh.
#
# Commands run 'kafka-topics' and 'kafka-configs' inside a broker: the local Docker container
# (environment 'local'), or a broker pod in the OpenShift namespace of the environment. The
# environment's settings come from db/kafka/environments/<environment>.conf; command line options
# and exported variables override them.
#
# Settings (all may be set in the environment file or exported before running):
#   KAFKA_TARGET             docker | openshift
#   KAFKA_CONTAINER          the local broker container (docker)
#   KAFKA_NAMESPACE          the OpenShift namespace (openshift)
#   KAFKA_BROKER_POD         the broker pod commands run in (openshift)
#   KAFKA_BOOTSTRAP          the bootstrap server, as seen from inside the broker
#   KAFKA_PARTITIONS         partitions of a new topic that does not specify its own
#   KAFKA_REPLICATION_FACTOR replication factor of a new topic that does not specify its own
#   KAFKA_TOPIC_CONFIG       comma-separated key=value configuration applied to every topic
#   KAFKA_DRY_RUN            true prints the changes instead of making them
#
# Per-topic overrides, where the topic name is upper-cased and '-' and '.' become '_':
#   KAFKA_TOPIC_<NAME>_PARTITIONS          e.g. KAFKA_TOPIC_ANALYSIS_BACKFILL_PARTITIONS=12
#   KAFKA_TOPIC_<NAME>_REPLICATION_FACTOR
#   KAFKA_TOPIC_<NAME>_CONFIG              e.g. KAFKA_TOPIC_ANALYSIS_DLQ_CONFIG=retention.ms=2592000000
#################################################

KAFKA_SCRIPTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
KAFKA_DIR="$(dirname "$KAFKA_SCRIPTS_DIR")"

# Load the environment's settings. Variables already set (exported, or from the command line)
# win over the file.
kafka_load_environment() {
  local environment=$1
  local file="$KAFKA_DIR/environments/$environment.conf"
  if [ ! -f "$file" ]; then
    echo >&2 "Kafka environment '$environment' is not configured: $file does not exist."
    return 1
  fi

  local name value
  while IFS='=' read -r name value; do
    [[ -z "$name" || "$name" =~ ^[[:space:]]*# ]] && continue
    name="${name//[[:space:]]/}"
    value="${value%\"}"
    value="${value#\"}"
    if [ -z "${!name+x}" ]; then
      export "$name=$value"
    fi
  done < "$file"

  : "${KAFKA_TARGET:?KAFKA_TARGET is not set for environment '$environment'}"
  : "${KAFKA_BOOTSTRAP:?KAFKA_BOOTSTRAP is not set for environment '$environment'}"
  : "${KAFKA_PARTITIONS:?KAFKA_PARTITIONS is not set for environment '$environment'}"
  : "${KAFKA_REPLICATION_FACTOR:?KAFKA_REPLICATION_FACTOR is not set for environment '$environment'}"
  export KAFKA_ENVIRONMENT="$environment"
}

# Fail early when the broker cannot be reached.
kafka_check_access() {
  if [ "$KAFKA_TARGET" = "openshift" ]; then
    if ! oc whoami > /dev/null 2>&1; then
      echo >&2 "Not logged in to OpenShift. Run 'oc login' first."
      return 1
    fi
    if ! oc get pod "$KAFKA_BROKER_POD" -n "$KAFKA_NAMESPACE" > /dev/null 2>&1; then
      echo >&2 "Broker pod '$KAFKA_BROKER_POD' not found in namespace '$KAFKA_NAMESPACE'."
      return 1
    fi
  elif [ "$KAFKA_TARGET" = "docker" ]; then
    if ! docker inspect "$KAFKA_CONTAINER" > /dev/null 2>&1; then
      echo >&2 "Broker container '$KAFKA_CONTAINER' is not running. Start it with 'make up n=broker'."
      return 1
    fi
  else
    echo >&2 "Unknown KAFKA_TARGET '$KAFKA_TARGET' (docker or openshift)."
    return 1
  fi
}

# Run a Kafka command line tool inside the broker.
kafka_exec() {
  if [ "$KAFKA_TARGET" = "openshift" ]; then
    oc exec -n "$KAFKA_NAMESPACE" "$KAFKA_BROKER_POD" -- "$@"
  else
    docker exec -i "$KAFKA_CONTAINER" "$@"
  fi
}

# Run a command that changes the cluster, or print it in a dry run.
kafka_change() {
  if [ "${KAFKA_DRY_RUN:-false}" = "true" ]; then
    echo "  [dry run] $*"
  else
    kafka_exec "$@"
  fi
}

# The per-topic override of a setting, e.g. kafka_topic_setting analysis-dlq CONFIG.
kafka_topic_setting() {
  local name
  name="KAFKA_TOPIC_$(echo "$1" | tr '[:lower:].-' '[:upper:]__')_$2"
  echo "${!name:-}"
}

# The topics that exist, one per line (cached for the run).
kafka_list_topics() {
  if [ -z "${KAFKA_TOPICS_CACHE+x}" ]; then
    KAFKA_TOPICS_CACHE="$(kafka_exec kafka-topics --list --bootstrap-server "$KAFKA_BOOTSTRAP")"
  fi
  echo "$KAFKA_TOPICS_CACHE"
}

# Whether the topic exists (or a dry run has already planned to create it).
kafka_topic_exists() {
  # A here-string, not a pipe: 'grep -q' exits at the first match, and with pipefail the writer's
  # SIGPIPE (141) would fail the test.
  grep -qx -- "$1" <<< "$(kafka_list_topics)"
}

# Record a change to the topic list made during this run.
kafka_topic_created() {
  kafka_list_topics > /dev/null
  KAFKA_TOPICS_CACHE="$KAFKA_TOPICS_CACHE"$'\n'"$1"
  [ "${KAFKA_DRY_RUN:-false}" = "true" ] && KAFKA_PLANNED="${KAFKA_PLANNED:-}"$'\n'"$1"
  return 0
}

kafka_topic_deleted() {
  kafka_list_topics > /dev/null
  KAFKA_TOPICS_CACHE="$(echo "$KAFKA_TOPICS_CACHE" | grep -vx -- "$1" || true)"
}

# Create a topic, or bring an existing one up to date:
#   ensure_topic <topic> [--partitions N] [--replication-factor N] [--config key=value]...
#
# A new topic gets, in order of precedence, the per-topic override, the migration's value, then
# the environment's default. An existing topic only has its partitions increased when a value is
# given for that topic (override or migration) and it is higher than the current count; partitions
# can never be decreased, and the environment default never resizes an existing topic, since
# adding partitions changes which partition each key goes to. A different replication factor is
# reported, not changed (that needs a partition reassignment). Configuration is applied to new and
# existing topics.
ensure_topic() {
  local topic=$1
  shift
  local partitions="" replication="" configs=()
  while [ $# -gt 0 ]; do
    case "$1" in
      --partitions) partitions="$2"; shift 2 ;;
      --replication-factor) replication="$2"; shift 2 ;;
      --config) configs+=("$2"); shift 2 ;;
      *) echo >&2 "ensure_topic: unknown option '$1'"; return 1 ;;
    esac
  done

  local override
  override="$(kafka_topic_setting "$topic" PARTITIONS)"
  [ -n "$override" ] && partitions="$override"
  override="$(kafka_topic_setting "$topic" REPLICATION_FACTOR)"
  [ -n "$override" ] && replication="$override"

  # Environment-wide configuration, then the migration's, then the per-topic override.
  local config_list=()
  [ -n "${KAFKA_TOPIC_CONFIG:-}" ] && config_list+=("$KAFKA_TOPIC_CONFIG")
  [ ${#configs[@]} -gt 0 ] && config_list+=("$(IFS=,; echo "${configs[*]}")")
  override="$(kafka_topic_setting "$topic" CONFIG)"
  [ -n "$override" ] && config_list+=("$override")
  local config
  config="$(IFS=,; echo "${config_list[*]+"${config_list[*]}"}")"

  if ! kafka_topic_exists "$topic"; then
    local args=(kafka-topics --create --if-not-exists --topic "$topic" --bootstrap-server "$KAFKA_BOOTSTRAP"
      --partitions "${partitions:-$KAFKA_PARTITIONS}" --replication-factor "${replication:-$KAFKA_REPLICATION_FACTOR}")
    if [ -n "$config" ]; then
      local entry
      IFS=',' read -ra entries <<< "$config"
      for entry in "${entries[@]}"; do
        [ -n "$entry" ] && args+=(--config "$entry")
      done
    fi
    echo "Create topic '$topic' (partitions: ${partitions:-$KAFKA_PARTITIONS}, replication factor: ${replication:-$KAFKA_REPLICATION_FACTOR}${config:+, config: $config})"
    kafka_change "${args[@]}"
    kafka_topic_created "$topic"
    return
  fi
  if grep -qx -- "$topic" <<< "${KAFKA_PLANNED:-}"; then
    echo "Topic '$topic' is created earlier in this dry run"
    return
  fi

  local description current_partitions current_replication
  # Keep the first (topic) line; piping to 'head' would SIGPIPE the broker command.
  description="$(kafka_exec kafka-topics --describe --topic "$topic" --bootstrap-server "$KAFKA_BOOTSTRAP")"
  description="${description%%$'\n'*}"
  current_partitions="$(echo "$description" | grep -oE 'PartitionCount:[[:space:]]*[0-9]+' | grep -oE '[0-9]+')"
  current_replication="$(echo "$description" | grep -oE 'ReplicationFactor:[[:space:]]*[0-9]+' | grep -oE '[0-9]+')"
  echo "Topic '$topic' exists (partitions: ${current_partitions:-?}, replication factor: ${current_replication:-?})"

  if [ -n "$partitions" ] && [ -n "$current_partitions" ]; then
    if [ "$partitions" -gt "$current_partitions" ]; then
      echo "  Increase partitions to $partitions"
      kafka_change kafka-topics --alter --topic "$topic" --partitions "$partitions" --bootstrap-server "$KAFKA_BOOTSTRAP"
    elif [ "$partitions" -lt "$current_partitions" ]; then
      echo "  WARNING: $partitions partitions requested, but partitions cannot be decreased; left at $current_partitions."
    fi
  fi
  if [ -n "$replication" ] && [ -n "$current_replication" ] && [ "$replication" != "$current_replication" ]; then
    echo "  WARNING: replication factor $replication requested, but it is $current_replication; changing it needs a partition reassignment (kafka-reassign-partitions)."
  fi
  if [ -n "$config" ]; then
    echo "  Apply config: $config"
    kafka_change kafka-configs --alter --entity-type topics --entity-name "$topic" --add-config "$config" --bootstrap-server "$KAFKA_BOOTSTRAP"
  fi
}

# Delete a topic and every message in it.
delete_topic() {
  local topic=$1
  if ! kafka_topic_exists "$topic"; then
    echo "Topic '$topic' does not exist"
    return
  fi
  echo "Delete topic '$topic'"
  kafka_change kafka-topics --delete --if-exists --topic "$topic" --bootstrap-server "$KAFKA_BOOTSTRAP"
  kafka_topic_deleted "$topic"
}
