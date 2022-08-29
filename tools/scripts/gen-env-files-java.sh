#!/bin/bash

echo ""
echo "*************************************"
echo "Setting up Docker Configuration Files"
echo "*************************************"

. ./tools/scripts/variables.sh

###########################################################################
# TNO Configuration
###########################################################################

# Database - DAL
if test -f "./libs/java/dal/db/.env"; then
    echo "./libs/java/dal/db/.env exists"
else
echo \
"DB_URL=jdbc:postgresql://host.docker.internal:$portDatabase/$dbName
DB_USERNAME=$dbUser
DB_PASSWORD=$password" >> ./libs/java/dal/db/.env
    echo "./libs/java/dal/db/.env created"
fi

# API - Editor - Java
if test -f "./api/java/editor/.env"; then
    echo "./api/java/editor/.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth/

DB_URL=jdbc:postgresql://host.docker.internal:$portDatabase/$dbName
DB_USERNAME=$dbUser
DB_PASSWORD=$password

ELASTIC_URIS=host.docker.internal:$portElastic
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password

AZURE_STORAGE_CONTAINER_NAME=$dbName
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devaccount1;AccountKey=$azureKey;BlobEndpoint=http://host.docker.internal:$portAzureBlob/devaccount1;

COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=$azureCognitiveServiceKey
COGNITIVE_SERVICES_SPEECH_REGION=$azureCognitiveServiceRegion

AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=$azureVideoAnalyzerKey
AZURE_VIDEO_ANALYZER_ACCOUNT_ID=$azureVideoAccountId
AZURE_VIDEO_ANALYZER_LOCATION=$azureVideoLocation

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./api/java/editor/.env
    echo "./api/java/editor/.env created"
fi

###########################################################################
# Java Services Configuration
###########################################################################

## Syndication Producer
if test -f "./services/java/syndication/.env"; then
    echo "./services/java/syndication/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-syndication

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_CLIENT_ID=syndication-01

MAX_FAILED_ATTEMPTS=5

DATA_SOURCE_MEDIA_TYPE=Syndication
DATA_SOURCE_LOCATION=Internet
# DATA_SOURCE_ID=GHI
# DATA_SOURCE_URL=https://www.globalhungerindex.org/atom.xml
# DATA_SOURCE_TOPIC=news-ghi" >> ./services/java/syndication/.env
    echo "./services/java/syndication/.env created"
fi

## Capture service
if test -f "./services/java/capture/.env"; then
    echo "./services/java/capture/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-capture

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_CLIENT_ID=audio-capture-01

MAX_FAILED_ATTEMPTS=5" >> ./services/java/capture/.env
    echo "./services/java/capture/.env created"
fi

## Audio - Clip Producer
if test -f "./services/java/clip/.env"; then
    echo "./services/java/clip/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-audio

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_CLIENT_ID=audio-clip-01

MAX_FAILED_ATTEMPTS=5" >> ./services/java/clip/.env
    echo "./services/java/clip/.env created"
fi

## NLP Consumer/Producer
if test -f "./services/java/nlp/.env"; then
    echo "./services/java/nlp/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-nlp

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_GROUP_ID=nlp-01
KAFKA_CONSUMER_TOPICS=news-hth,news-ghi
KAFKA_POLL_TIMEOUT=5000
ENABLE_AUTO_COMMIT=true
AUTO_OFFSET_RESET=latest

KAFKA_CLIENT_ID=nlp-01
KAFKA_PRODUCER_TOPIC=news-nlp

MAX_FAILED_ATTEMPTS=5" >> ./services/java/nlp/.env
    echo "./services/java/nlp/.env created"
fi

## Elasticsearch Consumer
if test -f "./services/java/indexing/.env"; then
    echo "./services/java/indexing/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-elastic

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_GROUP_ID=elastic-01
KAFKA_CONSUMER_TOPICS=news-nlp
KAFKA_POLL_TIMEOUT=5000
ENABLE_AUTO_COMMIT=true
AUTO_OFFSET_RESET=earliest

MAX_FAILED_ATTEMPTS=5

ELASTIC_URL=host.docker.internal:$portElastic
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password" >> ./services/java/indexing/.env
    echo "./services/java/indexing/.env created"
fi

## Transcription service
if test -f "./services/net/transcription/.env"; then
    echo "./services/net/transcription/.env exists"
else
echo \
"API_HOST_URL=http://host.docker.internal:$portApi
KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth
KEYCLOAK_REALM=tno
KEYCLOAK_CLIENT_ID=tno-service-account
KEYCLOAK_CLIENT_SECRET={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

KAFKA_LOGS_TOPIC=logs-capture

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_CLIENT_ID=audio-capture-01
Service__AzureCognitiveServicesKey={ENTER A VALID KEY} 

MAX_FAILED_ATTEMPTS=5" >> ./services/net/transcription/.env
    echo "./services/net/transcription/.env created"
fi
