#!/bin/bash

env=${1-ABC}
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
# scale api 0 sts
scale charts-api 0 dc $env
scale api-services 0 dc $env
scale editor 0 dc $env
scale subscriber 0 dc $env

# scale capture-service 0 dc $env
scale contentmigration-service 0 dc $env
scale contentmigration-historic-service 0 dc $env
scale filemonitor-service 0 dc $env
scale syndication-service 0 dc $env
scale image-service 0 dc $env

scale indexing-service 0 dc $env
scale content-service 0 dc $env
scale content-historic-service 0 dc $env

scale filecopy-service 0 dc $env
scale folder-collection-service 0 dc $env

# scale clip-service 0 dc $env
scale ffmpeg-service 0 dc $env
scale nlp-service 0 dc $env
scale extract-quotes-service 0 dc $env
scale transcription-service 0 dc $env

scale scheduler-service 0 dc $env
scale reporting-service 0 dc $env
scale notification-service 0 dc $env
scale event-handler-service 0 dc $env
scale ches-retry-service 0 dc $env
