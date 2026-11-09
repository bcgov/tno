#!/bin/bash

env=${1-ABC}
echo "Stopping environment $env"

oc project 9b301c-tools

# A failed scale is recorded and the rest still stop; the script then exits non-zero.
_failures=()
scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-deployment}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas || _failures+=("$type/$name")
}

# Stop everything (25 services total - oracle not in dev)

# Stateless Services (10 services)
scale nginx 0 deployment $env
scale editor 0 deployment $env
scale subscriber 0 deployment $env
scale nginx-editor 0 deployment $env
scale nginx-subscriber 0 deployment $env
scale charts-api 0 deployment $env
scale ffmpeg-service 0 deployment $env
scale transcription-service 0 deployment $env

# Kafka Consumers
scale folder-collection-service 0 deployment $env
scale content-service 0 deployment $env
# DEV runs the local indexer; TEST and PROD only have the Elastic Cloud one.
if [[ "$env" == "dev" ]]; then
  scale indexing-service 0 deployment $env
else
  scale indexing-service-cloud 0 deployment $env
fi
scale event-handler-service 0 deployment $env
scale notification-service 0 deployment $env
scale reporting-service 0 deployment $env
scale ches-retry-service 0 deployment $env
scale auto-clipper-service 0 deployment $env

# Kafka Producers - Single-Instance
scale scheduler-service 0 deployment $env
scale filemonitor-service 0 deployment $env
scale syndication-service 0 deployment $env
scale image-service 0 deployment $env
scale automation-service 0 deployment $env
scale content-analysis-service 0 deployment $env

scale api-services 0 deployment $env
scale api 0 statefulset $env

if [[ ${#_failures[@]} -gt 0 ]]; then
  echo "ERROR: could not stop: ${_failures[*]}" >&2
  exit 1
fi

# scale kowl 0 deployment $env
