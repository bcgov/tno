#!/bin/bash

env=${1-dev}
tag=${2-latest}
echo "Deploying to $env"

# Check for skopeo
if ! command -v skopeo &>/dev/null; then
  echo "ERROR: skopeo is not installed."
  echo ""
  echo "Install skopeo:"
  echo "  RHEL/Fedora:    sudo dnf install skopeo"
  echo "  Ubuntu/Debian:  sudo apt install skopeo"
  echo "  macOS:          brew install skopeo"
  exit 1
fi

oc project 9b301c-tools

# Extract ACR registry details from an existing deployment image
_acr_image=$(oc get deployment nginx -n 9b301c-$env -o jsonpath='{.spec.template.spec.containers[0].image}' 2>/dev/null)
ACR_HOST=$(echo "$_acr_image" | cut -d'/' -f1)
if [[ -z "$ACR_HOST" ]]; then
  ACR_HOST="bcgov-c4awhwfpcremdbga.azurecr.io"
fi

echo "Using ACR registry: $ACR_HOST"

# Extract ACR credentials from the imagePullSecret on the nginx deployment
_secret_name=$(oc get deployment nginx -n 9b301c-$env -o jsonpath='{.spec.template.spec.imagePullSecrets[0].name}' 2>/dev/null)
if [[ -z "$_secret_name" ]]; then
  echo "ERROR: Could not find imagePullSecret on nginx deployment in 9b301c-$env"
  exit 1
fi
_dockerconfig=$(oc get secret "$_secret_name" -n 9b301c-$env -o jsonpath='{.data.\.dockerconfigjson}' | base64 -d)
ACR_USER=$(echo "$_dockerconfig" | jq -r ".auths[\"$ACR_HOST\"].username")
ACR_PASS=$(echo "$_dockerconfig" | jq -r ".auths[\"$ACR_HOST\"].password")

# Get current pod counts
getPods () {
  name=${1-}
  type=${2-deployment}
  env=${3-'dev'}
  result=$(oc get $type -n 9b301c-$env $name -o "jsonpath={.status.availableReplicas}" 2>/dev/null)
  # Ensure we always return a number (default to 0 if empty)
  if [ -z "$result" ]; then
    result="0"
  fi
  echo "$result"
}

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-deployment}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# Retag an image in ACR: source tag → env tag (manifest copy, no layer download)
acr_tag () {
  local image=$1
  skopeo copy \
    --src-creds "$ACR_USER:$ACR_PASS" \
    --dest-creds "$ACR_USER:$ACR_PASS" \
    "docker://$ACR_HOST/$image:$tag" \
    "docker://$ACR_HOST/$image:$env"
}

# Get current pod counts for all services (25 total - oracle not in dev)

# Stateless Services (10 services)
podsNginx=$(getPods nginx deployment $env)
podsEditor=$(getPods editor deployment $env)
podsSubscriber=$(getPods subscriber deployment $env)
podsNginxEditor=$(getPods nginx-editor deployment $env)
podsNginxSubscriber=$(getPods nginx-subscriber deployment $env)
podsCharts=$(getPods charts-api deployment $env)
podsApi=$(getPods api statefulset $env)
podsApiServices=$(getPods api-services deployment $env)
podsCorenlp=$(getPods corenlp deployment $env)
podsNLP=$(getPods nlp-service deployment $env)
podsFFmpeg=$(getPods ffmpeg-service deployment $env)
podsTranscription=$(getPods transcription-service deployment $env)
podsExtractQuotes=$(getPods extract-quotes-service deployment $env)

# Kafka Consumers - Stateless (7 services)
podsFolderCollection=$(getPods folder-collection-service deployment $env)
podsContent=$(getPods content-service deployment $env)
podsIndexing=$(getPods indexing-service deployment $env)
if [[ "$env" != "dev" ]]; then
  podsIndexingCloud=$(getPods indexing-service-cloud deployment $env)
fi
podsEventHandler=$(getPods event-handler-service deployment $env)
podsNotification=$(getPods notification-service deployment $env)
podsReporting=$(getPods reporting-service deployment $env)
podsChesRetry=$(getPods ches-retry-service deployment $env)
podsAutoClipper=$(getPods auto-clipper-service deployment $env)

# Kafka Consumers - Single-Instance (4 services)
podsScheduler=$(getPods scheduler-service deployment $env)
podsFileMonitor=$(getPods filemonitor-service deployment $env)
podsSyndication=$(getPods syndication-service deployment $env)
podsImage=$(getPods image-service deployment $env)

# Supporting Services
podsKowl=$(getPods kowl deployment $env)

# Stop everything
./stop.sh $env

# Tag all images in ACR ($tag → $env)
echo "Tagging images in ACR ($ACR_HOST): $tag → $env"

# Stateless Services
acr_tag nginx
acr_tag editor
acr_tag subscriber
acr_tag charts-api
acr_tag api
acr_tag corenlp
acr_tag nlp-service
acr_tag ffmpeg-service
acr_tag transcription-service
acr_tag extract-quotes-service

# Kafka Consumers (Stateless)
acr_tag folder-collection-service
acr_tag content-service
acr_tag indexing-service
acr_tag event-handler-service
acr_tag notification-service
acr_tag reporting-service
acr_tag ches-retry-service
acr_tag auto-clipper-service

# Kafka Producers (Single-Instance)
acr_tag scheduler-service
acr_tag filemonitor-service
acr_tag syndication-service
acr_tag image-service

# Start everything
echo "Scaling services back to original replica counts"

# Stateless Services
scale api $podsApi statefulset $env
scale api-services $podsApiServices deployment $env

# Wait until the API is running
oc rollout status statefulset/api --timeout=10m -n 9b301c-$env
oc rollout status deployment/api-services --timeout=10m -n 9b301c-$env

scale nginx $podsNginx deployment $env
scale editor $podsEditor deployment $env
scale subscriber $podsSubscriber deployment $env
scale nginx-editor $podsNginxEditor deployment $env
scale nginx-subscriber $podsNginxSubscriber deployment $env
scale charts-api $podsCharts deployment $env
scale corenlp $podsCorenlp deployment $env
scale nlp-service $podsNLP deployment $env
scale ffmpeg-service $podsFFmpeg deployment $env
scale transcription-service $podsTranscription deployment $env
scale extract-quotes-service $podsExtractQuotes deployment $env

# Kafka Consumers
scale folder-collection-service $podsFolderCollection deployment $env
scale content-service $podsContent deployment $env
scale indexing-service $podsIndexing deployment $env
if [[ "$env" != "dev" ]]; then
  scale indexing-service-cloud $podsIndexingCloud deployment $env
fi
scale event-handler-service $podsEventHandler deployment $env
scale notification-service $podsNotification deployment $env
scale reporting-service $podsReporting deployment $env
scale ches-retry-service $podsChesRetry deployment $env
scale auto-clipper-service $podsAutoClipper deployment $env

# Kafka Producers (Single-Instance)
scale scheduler-service $podsScheduler deployment $env
scale filemonitor-service $podsFileMonitor deployment $env
scale syndication-service $podsSyndication deployment $env
scale image-service $podsImage deployment $env

# Supporting Services
# scale oracle $podsOracle deployment $env  # Not deployed in dev
scale kowl $podsKowl deployment $env

echo "Deployment complete!"
