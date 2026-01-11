#!/bin/bash

env=${1-dev}
tag=${2-latest}
echo "Deploying to $env"

oc project 9b301c-tools

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

# Get current pod counts for all services (25 total - oracle not in dev)

# Stateless Services (10 services)
podsNginx=$(getPods nginx deployment $env)
podsEditor=$(getPods editor deployment $env)
podsSubscriber=$(getPods subscriber deployment $env)
podsCharts=$(getPods charts-api deployment $env)
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
if [ "$env" != "dev "]; then
  podsIndexingCloud=$(getPods indexing-service-cloud deployment $env)
fi
podsEventHandler=$(getPods event-handler-service deployment $env)
podsNotification=$(getPods notification-service deployment $env)
podsReporting=$(getPods reporting-service deployment $env)
podsChesRetry=$(getPods ches-retry-service deployment $env)

# Kafka Consumers - Single-Instance (4 services)
podsScheduler=$(getPods scheduler-service deployment $env)
podsFileMonitor=$(getPods filemonitor-service deployment $env)
podsSyndication=$(getPods syndication-service deployment $env)
podsImage=$(getPods image-service deployment $env)

# Supporting Services (4 services - oracle not in dev)
# podsOracle=$(getPods oracle deployment $env)  # Not deployed in dev
podsPsql=$(getPods psql deployment $env)
podsKowl=$(getPods kowl deployment $env)
podsNginxEditor=$(getPods nginx-editor deployment $env)
podsNginxSubscriber=$(getPods nginx-subscriber deployment $env)

# Removed/Not Currently Deployed
# podsFilecopy=$(getPods filecopy-service deployment $env)
# podsCapture=$(getPods capture-service deployment $env)
# podsClip=$(getPods clip-service deployment $env)
# podsIndexingCloud=$(getPods indexing-service-cloud deployment $env)

# Stop everything
./stop.sh $env

# Tag all images
echo "Tagging images with tag: $tag"

# Stateless Services
oc tag nginx:$tag nginx:$env
oc tag editor:$tag editor:$env
oc tag subscriber:$tag subscriber:$env
oc tag charts-api:$tag charts-api:$env
oc tag api:$tag api:$env
oc tag corenlp:$tag corenlp:$env
oc tag nlp-service:$tag nlp-service:$env
oc tag ffmpeg-service:$tag ffmpeg-service:$env
oc tag transcription-service:$tag transcription-service:$env
oc tag extract-quotes-service:$tag extract-quotes-service:$env

# Kafka Consumers (Stateless)
oc tag folder-collection-service:$tag folder-collection-service:$env
oc tag content-service:$tag content-service:$env
oc tag indexing-service:$tag indexing-service:$env
oc tag event-handler-service:$tag event-handler-service:$env
oc tag notification-service:$tag notification-service:$env
oc tag reporting-service:$tag reporting-service:$env
oc tag ches-retry-service:$tag ches-retry-service:$env

# Kafka Consumers (Single-Instance)
oc tag scheduler-service:$tag scheduler-service:$env
oc tag filemonitor-service:$tag filemonitor-service:$env
oc tag syndication-service:$tag syndication-service:$env
oc tag image-service:$tag image-service:$env

# Supporting Services (no image tagging needed - use specific versions)
# oracle, psql, kowl, nginx-editor, nginx-subscriber

# Removed/Not Currently Deployed
# oc tag filecopy-service:$tag filecopy-service:$env
# oc tag capture-service:$tag capture-service:$env
# oc tag clip-service:$tag clip-service:$env
# oc tag indexing-service-cloud:$tag indexing-service-cloud:$env

# Start everything
echo "Scaling services back to original replica counts"

# Stateless Services
scale nginx $podsNginx deployment $env
scale editor $podsEditor deployment $env
scale subscriber $podsSubscriber deployment $env
scale charts-api $podsCharts deployment $env
scale api-services $podsApiServices deployment $env
scale corenlp $podsCorenlp deployment $env
scale nlp-service $podsNLP deployment $env
scale ffmpeg-service $podsFFmpeg deployment $env
scale transcription-service $podsTranscription deployment $env
scale extract-quotes-service $podsExtractQuotes deployment $env

# Kafka Consumers (Stateless)
scale folder-collection-service $podsFolderCollection deployment $env
scale content-service $podsContent deployment $env
scale indexing-service $podsIndexing deployment $env
if [ "$env" != "dev" ]; then
  scale indexing-service-cloud $podsIndexingCloud deployment $env
fi
scale event-handler-service $podsEventHandler deployment $env
scale notification-service $podsNotification deployment $env
scale reporting-service $podsReporting deployment $env
scale ches-retry-service $podsChesRetry deployment $env

# Kafka Consumers (Single-Instance)
scale scheduler-service $podsScheduler deployment $env
scale filemonitor-service $podsFileMonitor deployment $env
scale syndication-service $podsSyndication deployment $env
scale image-service $podsImage deployment $env

# Supporting Services
# scale oracle $podsOracle deployment $env  # Not deployed in dev
scale psql $podsPsql deployment $env
scale kowl $podsKowl deployment $env
scale nginx-editor $podsNginxEditor deployment $env
scale nginx-subscriber $podsNginxSubscriber deployment $env

# Removed/Not Currently Deployed
# scale filecopy-service $podsFilecopy deployment $env
# scale capture-service $podsCapture deployment $env
# scale clip-service $podsClip deployment $env
# scale indexing-service-cloud $podsIndexingCloud deployment $env

echo "Deployment complete!"
