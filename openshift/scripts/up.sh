#!/bin/bash

env=${1-dev}
echo "Starting up environment $env"

oc project 9b301c-tools

# ---------------------------------------------------------------------------
# Replica counts per service per environment.
# Key format: "service_env". If no key found, falls back to DEFAULT_REPLICAS
# for that service, then to 1.
# ---------------------------------------------------------------------------
declare -A REPLICAS=(
  # api
  [api_dev]=2              [api_test]=2             [api_prod]=3
  [api-services_dev]=2     [api-services_test]=2    [api-services_prod]=3

  # frontends
  [nginx_dev]=1            [nginx_test]=2           [nginx_prod]=3
  [editor_dev]=1           [editor_test]=2         [editor_prod]=3
  [subscriber_dev]=1       [subscriber_test]=2      [subscriber_prod]=3
  [nginx-editor_dev]=1     [nginx-editor_test]=2    [nginx-editor_prod]=3
  [nginx-subscriber_dev]=1 [nginx-subscriber_test]=2 [nginx-subscriber_prod]=3
  [charts-api_dev]=1       [charts-api_test]=2      [charts-api_prod]=3

  # ML / media services
  [corenlp_dev]=0          [corenlp_test]=0         [corenlp_prod]=0
  [nlp-service_dev]=0      [nlp-service_test]=0     [nlp-service_prod]=0
  [ffmpeg-service_dev]=1   [ffmpeg-service_test]=2  [ffmpeg-service_prod]=6
  [transcription-service_dev]=1 [transcription-service_test]=2 [transcription-service_prod]=6
  [extract-quotes-service_dev]=1 [extract-quotes-service_test]=2 [extract-quotes-service_prod]=6
  [auto-clipper-service_dev]=1 [auto-clipper-service_test]=2 [auto-clipper-service_prod]=6

  # Kafka consumers
  [folder-collection-service_dev]=1 [folder-collection-service_test]=2 [folder-collection-service_prod]=3
  [content-service_dev]=1   [content-service_test]=2   [content-service_prod]=3
  [indexing-service_dev]=1  [indexing-service_test]=2  [indexing-service_prod]=3
  [indexing-service-cloud_dev]=1 [indexing-service-cloud_test]=2 [indexing-service-cloud_prod]=3
  [event-handler-service_dev]=1 [event-handler-service_test]=2 [event-handler-service_prod]=3
  [notification-service_dev]=1 [notification-service_test]=2 [notification-service_prod]=3
  [reporting-service_dev]=1 [reporting-service_test]=2 [reporting-service_prod]=3

  # Kafka producers (single-instance — scale with caution)
  [scheduler-service_dev]=1  [scheduler-service_test]=1  [scheduler-service_prod]=1
  [filemonitor-service_dev]=1 [filemonitor-service_test]=1 [filemonitor-service_prod]=1
  [syndication-service_dev]=1 [syndication-service_test]=1 [syndication-service_prod]=1
  [image-service_dev]=1      [image-service_test]=1      [image-service_prod]=1
  [ches-retry-service_dev]=1 [ches-retry-service_test]=1 [ches-retry-service_prod]=1
)

replicas() {
  local name=$1
  local env=$2
  local key="${name}_${env}"
  echo "${REPLICAS[$key]:-1}"
}

scale() {
  local name=${1-}
  local replicas=${2-0}
  local type=${3-deployment}
  local env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# ---------------------------------------------------------------------------

# Stateless Services
scale api              $(replicas api $env)              statefulset $env
scale api-services     $(replicas api-services $env)     deployment  $env

# Wait until API is running
oc rollout status statefulset/api      --timeout=10m -n 9b301c-$env
oc rollout status deployment/api-services --timeout=10m -n 9b301c-$env

scale nginx            $(replicas nginx $env)            deployment $env
scale editor           $(replicas editor $env)           deployment $env
scale subscriber       $(replicas subscriber $env)       deployment $env
scale charts-api       $(replicas charts-api $env)       deployment $env
scale corenlp          $(replicas corenlp $env)          deployment $env
scale nlp-service      $(replicas nlp-service $env)      deployment $env
scale ffmpeg-service   $(replicas ffmpeg-service $env)   deployment $env
scale transcription-service   $(replicas transcription-service $env)   deployment $env
scale extract-quotes-service  $(replicas extract-quotes-service $env)  deployment $env
scale auto-clipper-service    $(replicas auto-clipper-service $env)    deployment $env
scale nginx-editor     $(replicas nginx-editor $env)     deployment $env
scale nginx-subscriber $(replicas nginx-subscriber $env) deployment $env

# Kafka Consumers
scale folder-collection-service $(replicas folder-collection-service $env) deployment $env
scale content-service  $(replicas content-service $env)  deployment $env
scale indexing-service $(replicas indexing-service $env) deployment $env
if [[ "$env" != "dev" ]]; then
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

# scale kowl 1 deployment $env
