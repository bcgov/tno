#!/bin/bash

env=${1-dev}
echo "Starting up environment $env"

oc project 9b301c-tools

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-deployment}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# Start everything (25 services total - oracle not in dev)

# Stateless Services (10 services)
scale api 2 statefulset $env
scale api-services 1 deployment $env

# Wait until API is running
oc rollout status statefulset/api --timeout=10m -n 9b301c-$env
oc rollout status deployment/api-services --timeout=10m -n 9b301c-$env

scale nginx 1 deployment $env
scale editor 1 deployment $env
scale subscriber 1 deployment $env
scale charts-api 1 deployment $env
scale corenlp 1 deployment $env
scale nlp-service 1 deployment $env
scale ffmpeg-service 1 deployment $env
scale transcription-service 1 deployment $env
scale extract-quotes-service 1 deployment $env

# Kafka Consumers - Stateless (7 services)
scale folder-collection-service 1 deployment $env
scale content-service 1 deployment $env
scale indexing-service 1 deployment $env
scale event-handler-service 1 deployment $env
scale notification-service 1 deployment $env
scale reporting-service 1 deployment $env
scale ches-retry-service 1 deployment $env

# Kafka Consumers - Single-Instance (4 services)
scale scheduler-service 1 deployment $env
scale filemonitor-service 1 deployment $env
scale syndication-service 1 deployment $env
scale image-service 1 deployment $env

# Supporting Services (4 services - oracle not in dev)
# scale oracle 1 deployment $env  # Not deployed in dev
# scale kowl 1 deployment $env
scale nginx-editor 1 deployment $env
scale nginx-subscriber 1 deployment $env

# Services not currently deployed (commented out for future use)
# scale filecopy-service 1 deployment $env
# scale capture-service 1 deployment $env
# scale clip-service 1 deployment $env
# scale indexing-service-cloud 1 deployment $env
