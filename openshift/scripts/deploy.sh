#!/bin/bash

env=${1-dev}
tag=${2-latest}
echo "Deploying to $env"

oc project 9b301c-tools

# Stop environment
getPods () {
  name=${1-}
  type=${2-dc}
  env=${3-'dev'}
  result=$(oc get $type -n 9b301c-$env $name -o "jsonpath={.status.availableReplicas}")
  echo $result
  return $result
}

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-dc}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

podsApi=$(getPods api sts $env)
podsApiServices=$(getPods api-services dc $env)
podsCharts=$(getPods charts-api dc $env)
podsEditor=$(getPods editor dc $env)
podsSubscriber=$(getPods subscriber dc $env)

# podsCapture=$(getPods capture-service dc $env)
podsFileMonitor=$(getPods filemonitor-service dc $env)
podsSyndication=$(getPods syndication-service dc $env)
podsImage=$(getPods image-service dc $env)

podsIndexing=$(getPods indexing-service dc $env)
if [[ "$env" != "dev" ]]; then
  podsIndexingCloud=$(getPods indexing-service-cloud dc $env)
fi
podsContent=$(getPods content-service dc $env)

podsFolderCollection=$(getPods folder-collection-service dc $env)

# podsClip=$(getPods clip-service dc $env)
podsFFmpeg=$(getPods ffmpeg-service dc $env)
podsNLP=$(getPods nlp-service dc $env)
podsExtractQuotes=$(getPods extract-quotes-service dc $env)
podsTranscription=$(getPods transcription-service dc $env)

podsScheduler=$(getPods scheduler-service dc $env)
podsReporting=$(getPods reporting-service dc $env)
podsNotification=$(getPods notification-service dc $env)
podsEventHandler=$(getPods event-handler-service dc $env)
podsChesRetry=$(getPods ches-retry-service dc $env)

# Stop everything
./stop.sh $env

# APIs
scale api-services 0 dc $env
oc tag api:$tag api:$env

scale api-services $podsApiServices dc $env
oc rollout restart sts/api -n 9b301c-$env
oc rollout status --watch --timeout=600s sts/api -n 9b301c-$env

oc tag charts-api:$tag charts-api:$env

# Web Application
oc tag editor:$tag editor:$env
oc tag subscriber:$tag subscriber:$env

# Ingest Services
oc tag filemonitor-service:$tag filemonitor-service:$env
oc tag syndication-service:$tag syndication-service:$env
oc tag image-service:$tag image-service:$env

# Store Services
oc tag indexing-service:$tag indexing-service:$env
oc tag content-service:$tag content-service:$env

# Utility Services
oc tag folder-collection-service:$tag folder-collection-service:$env

# Transform Services
oc tag ffmpeg-service:$tag ffmpeg-service:$env
oc tag nlp-service:$tag nlp-service:$env
oc tag extract-quotes-service:$tag extract-quotes-service:$env
oc tag transcription-service:$tag transcription-service:$env

# Output Services
oc tag scheduler-service:$tag scheduler-service:$env
oc tag reporting-service:$tag reporting-service:$env
oc tag notification-service:$tag notification-service:$env
oc tag event-handler-service:$tag event-handler-service:$env
oc tag ches-retry-service:$tag ches-retry-service:$env

# Start everything
scale api $podsApi sts $env
scale charts-api $podsCharts dc $env
scale editor $podsEditor dc $env
scale subscriber $podsSubscriber dc $env

scale filemonitor-service $podsFileMonitor dc $env
scale syndication-service $podsSyndication dc $env
scale image-service $podsImage dc $env

scale indexing-service $podsIndexing dc $env
if [[ "$env" != "dev" ]]; then
  scale indexing-service-cloud $podsIndexingCloud dc $env
fi
scale content-service $podsContent dc $env

scale folder-collection-service $podsFolderCollection dc $env

scale ffmpeg-service $podsFFmpeg dc $env
scale nlp-service $podsNLP dc $env
scale extract-quotes-service $podsExtractQuotes dc $env
scale transcription-service $podsTranscription dc $env

scale scheduler-service $podsScheduler dc $env
scale reporting-service $podsReporting dc $env
scale notification-service $podsNotification dc $env
scale event-handler-service $podsEventHandler dc $env
scale ches-retry-service $podsChesRetry dc $env
