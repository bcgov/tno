#!/bin/bash

#################################################
# Delete a Kafka topic, and every message in it, in an environment.
#
# Usage: topic-delete.sh -t topic [-e environment] [-z bootstrap] [-i broker pod index] [-d] [-y]
#################################################

set -o errexit -o pipefail -o noclobber -o nounset

! getopt --test > /dev/null
if [[ ${PIPESTATUS[0]} -ne 4 ]]; then
    echo 'I’m sorry, `getopt --test` failed in this environment.'
    exit 1
fi

OPTIONS=e:t:b:z:i:dy
LONGOPTS=environment:,topic:,bootstrap:,index:,dry-run,yes

! PARSED=$(getopt --options=$OPTIONS --longoptions=$LONGOPTS --name "$0" -- "$@")
if [[ ${PIPESTATUS[0]} -ne 0 ]]; then
    exit 2
fi
eval set -- "$PARSED"

environment=local topic="" yes=false
while true; do
  case "$1" in
    -e|--environment) environment="$2"; shift 2 ;;
    -t|--topic) topic="$2"; shift 2 ;;
    -b|-z|--bootstrap) export KAFKA_BOOTSTRAP="$2"; shift 2 ;;
    -i|--index) export KAFKA_BROKER_POD="kafka-broker-$2"; shift 2 ;;
    -d|--dry-run) export KAFKA_DRY_RUN=true; shift ;;
    -y|--yes) yes=true; shift ;;
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

if [ "$yes" != true ] && [ "${KAFKA_DRY_RUN:-false}" != "true" ]; then
  read -r -p "Delete topic '$topic' and every message in it from $environment? [y/N] " answer
  [[ "$answer" =~ ^[Yy]$ ]] || { echo "Cancelled."; exit 1; }
fi

kafka_check_access
delete_topic "$topic"
