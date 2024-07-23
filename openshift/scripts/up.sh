#!/bin/bash

env=${1-dev}
echo "Deploying to $env"

oc project 9b301c-tools

scale () {
  name=${1-}
  replicas=${2-0}
  type=${3-dc}
  env=${4-'dev'}
  oc scale $type $name -n 9b301c-$env --replicas=$replicas
}

# Stop everyting
# scale api 1 sts
scale charts-api 1 dc $env
scale api-services 1 dc $env
scale editor 1 dc $env
scale subscriber 1 dc $env

# scale capture-service 1 dc $env
scale contentmigration-service 1 dc $env
scale contentmigration-recent-service 1 dc $env
scale contentmigration-historic-service 1 dc $env
scale filemonitor-service 1 dc $env
scale syndication-service 1 dc $env
scale image-service 1 dc $env

scale indexing-service 2 dc $env
scale content-service 2 dc $env
scale content-current-service 1 dc $env
scale content-historic-service 1 dc $env

scale filecopy-service 1 dc $env
scale folder-collection-service 1 dc $env

# scale clip-service 1 dc $env
scale ffmpeg-service 1 dc $env
scale nlp-service 1 dc $env
scale extract-quotes-service 1 dc $env
scale transcription-service 2 dc $env

scale scheduler-service 1 dc $env
scale reporting-service 1 dc $env
scale notification-service 1 dc $env
scale event-handler-service 1 dc $env
