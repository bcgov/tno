#!/bin/bash

env=${1-dev}
echo "Starting up environment $env"

oc project 9b301c-tools

# ---------------------------------------------------------------------------
# Replica counts per service per environment.
# Key format: "service_env". Unlisted combinations default to one replica.
# ---------------------------------------------------------------------------
# A case table also works with macOS's system Bash 3.2.
replicas() {
  case "${1}_${2}" in
    nginx_dev|editor_dev|subscriber_dev | \
    nginx-editor_dev|nginx-subscriber_dev|charts-api_dev | \
    ffmpeg-service_dev|transcription-service_dev|auto-clipper-service_dev | \
    folder-collection-service_dev|content-service_dev|indexing-service_dev | \
    indexing-service-cloud_dev|event-handler-service_dev|notification-service_dev | \
    reporting-service_dev|scheduler-service_dev|scheduler-service_test | \
    scheduler-service_prod|filemonitor-service_dev|filemonitor-service_test | \
    filemonitor-service_prod|syndication-service_dev|syndication-service_test | \
    syndication-service_prod|image-service_dev|image-service_test | \
    image-service_prod|ches-retry-service_dev|ches-retry-service_test | \
    ches-retry-service_prod|automation-service_dev|automation-service_test | \
    automation-service_prod|content-analysis-service_dev|content-analysis-service_test | \
    content-analysis-service_prod) echo 1 ;;
    api_dev|api_test|api-services_dev | \
    api-services_test|nginx_test|editor_test | \
    subscriber_test|nginx-editor_test|nginx-subscriber_test | \
    charts-api_test|ffmpeg-service_test|transcription-service_test | \
    auto-clipper-service_test|folder-collection-service_test|content-service_test | \
    indexing-service-cloud_test|event-handler-service_test|notification-service_test | \
    reporting-service_test) echo 2 ;;
    api_prod|api-services_prod|nginx_prod | \
    editor_prod|subscriber_prod|nginx-editor_prod | \
    nginx-subscriber_prod|charts-api_prod|folder-collection-service_prod | \
    content-service_prod|indexing-service-cloud_prod|event-handler-service_prod | \
    notification-service_prod|reporting-service_prod) echo 3 ;;
    ffmpeg-service_prod|transcription-service_prod|auto-clipper-service_prod) echo 6 ;;
    *) echo 1 ;;
  esac
}

scale() {
  local name=${1-}
  local replicas=${2-0}
  local type=${3-deployment}
  local env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas || _failures+=("scale $type/$name")
}

# A failed step is recorded and the rest still start; the script then exits non-zero.
_failures=()

# ---------------------------------------------------------------------------

# Stateless Services
scale api              $(replicas api $env)              statefulset $env
scale api-services     $(replicas api-services $env)     deployment  $env

# Wait until API is running
oc rollout status statefulset/api      --timeout=10m -n 9b301c-$env || _failures+=("statefulset/api not ready")
oc rollout status deployment/api-services --timeout=10m -n 9b301c-$env || _failures+=("deployment/api-services not ready")

scale nginx            $(replicas nginx $env)            deployment $env
scale editor           $(replicas editor $env)           deployment $env
scale subscriber       $(replicas subscriber $env)       deployment $env
scale charts-api       $(replicas charts-api $env)       deployment $env
scale ffmpeg-service   $(replicas ffmpeg-service $env)   deployment $env
scale transcription-service   $(replicas transcription-service $env)   deployment $env
scale auto-clipper-service    $(replicas auto-clipper-service $env)    deployment $env
scale nginx-editor     $(replicas nginx-editor $env)     deployment $env
scale nginx-subscriber $(replicas nginx-subscriber $env) deployment $env

# Kafka Consumers
scale folder-collection-service $(replicas folder-collection-service $env) deployment $env
scale content-service  $(replicas content-service $env)  deployment $env
# DEV runs the local indexer; TEST and PROD only have the Elastic Cloud one.
if [[ "$env" == "dev" ]]; then
  scale indexing-service $(replicas indexing-service $env) deployment $env
else
  scale indexing-service-cloud $(replicas indexing-service-cloud $env) deployment $env
fi
scale event-handler-service  $(replicas event-handler-service $env)  deployment $env
scale notification-service   $(replicas notification-service $env)   deployment $env
scale reporting-service      $(replicas reporting-service $env)      deployment $env

# Kafka Producers - Single-Instance
scale scheduler-service  $(replicas scheduler-service $env)  deployment $env
scale filemonitor-service $(replicas filemonitor-service $env) deployment $env
scale syndication-service $(replicas syndication-service $env) deployment $env
scale image-service      $(replicas image-service $env)      deployment $env
scale ches-retry-service $(replicas ches-retry-service $env) deployment $env
scale automation-service $(replicas automation-service $env) deployment $env
scale content-analysis-service $(replicas content-analysis-service $env) deployment $env

# scale kafka-ui 1 deployment $env

if [[ ${#_failures[@]} -gt 0 ]]; then
  echo "ERROR: the environment did not fully start:" >&2
  printf '  - %s\n' "${_failures[@]}" >&2
  exit 1
fi
