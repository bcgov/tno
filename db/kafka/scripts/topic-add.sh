#!/bin/bash

#################################################
# Create a Kafka topic, or bring an existing one up to date, in an environment.
# Prefer a migration (db/kafka/migrations) for topics a feature needs; this is for one-off work.
#
# Usage: topic-add.sh -t topic [-e environment] [-z bootstrap] [-p partitions] [-r replication]
#                     [-c key=value,...] [-i broker pod index] [-d]
#################################################

set -o errexit -o pipefail -o noclobber -o nounset

! getopt --test > /dev/null
if [[ ${PIPESTATUS[0]} -ne 4 ]]; then
    echo 'I’m sorry, `getopt --test` failed in this environment.'
    exit 1
fi

OPTIONS=e:t:b:z:p:r:c:i:d
LONGOPTS=environment:,topic:,bootstrap:,partitions:,replication:,config:,index:,dry-run

! PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")
if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    exit 2
fi
eval set -- "$PARSED"

environment=local topic="" partitions="" replication="" config=""
while true; do
  case "$1" in
    -e|--environment) environment="$2"; shift 2 ;;
    -t|--topic) topic="$2"; shift 2 ;;
    -b|-z|--bootstrap) export KAFKA_BOOTSTRAP="$2"; shift 2 ;;
    -p|--partitions) partitions="$2"; shift 2 ;;
    -r|--replication) replication="$2"; shift 2 ;;
    -c|--config) config="$2"; shift 2 ;;
    -i|--index) export KAFKA_BROKER_POD="kafka-broker-$2"; shift 2 ;;
    -d|--dry-run) export KAFKA_DRY_RUN=true; shift ;;
    --) shift; break ;;
    *) echo "Programming error"; exit 3 ;;
  esac
done

if [ -z "$topic" ]; then
  read -r -p 'Topic: ' topic
fi

SCRIPTS_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=kafka.sh
. "$SCRIPTS_DIR/kafka.sh"
kafka_load_environment "$environment"
kafka_check_access

args=()
[ -n "$partitions" ] && args+=(--partitions "$partitions")
[ -n "$replication" ] && args+=(--replication-factor "$replication")
[ -n "$config" ] && args+=(--config "$config")
ensure_topic "$topic" "${args[@]}"
