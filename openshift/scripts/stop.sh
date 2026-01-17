#!/bin/bash

env=${1-ABC}
echo "Stopping environment $env"

oc project 9b301c-tools

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-deployment}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# Stop everything (25 services total - oracle not in dev)

# Stateless Services (10 services)
scale nginx 0 deployment $env
scale editor 0 deployment $env
scale subscriber 0 deployment $env
scale nginx-editor 0 deployment $env
scale nginx-subscriber 0 deployment $env
scale charts-api 0 deployment $env
scale corenlp 0 deployment $env
scale nlp-service 0 deployment $env
scale ffmpeg-service 0 deployment $env
scale transcription-service 0 deployment $env
scale extract-quotes-service 0 deployment $env

# Kafka Consumers - Stateless (7 services)
scale folder-collection-service 0 deployment $env
scale content-service 0 deployment $env
scale indexing-service 0 deployment $env
scale event-handler-service 0 deployment $env
scale notification-service 0 deployment $env
scale reporting-service 0 deployment $env
scale ches-retry-service 0 deployment $env
scale auto-clipper-service 0 deployment $env

# Kafka Consumers - Single-Instance (4 services)
scale scheduler-service 0 deployment $env
scale filemonitor-service 0 deployment $env
scale syndication-service 0 deployment $env
scale image-service 0 deployment $env

scale api-services 0 deployment $env
scale api 0 statefulset $env

# Supporting Services (4 services - oracle not in dev)
# scale oracle 0 deployment $env  # Not deployed in dev
# scale kowl 0 deployment $env

# Services not currently deployed (commented out for future use)
# scale filecopy-service 0 deployment $env
# scale capture-service 0 deployment $env
# scale clip-service 0 deployment $env
# scale indexing-service-cloud 0 deployment $env
