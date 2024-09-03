#!/bin/bash

echo ""
echo "*************************************"
echo "Setting up Docker Configuration Files"
echo "*************************************"

. ./tools/scripts/variables.sh

###########################################################################
# TNO Configuration
###########################################################################

# Docker Compose
if test -f "./.env"; then
    echo "./.env exists"
else
echo \
"#############################
# Network
#############################

NGINX_HTTP_EDITOR_PORT=$portNginxEditor
NGINX_HTTP_SUBSCRIBER_PORT=$portNginxSubscriber
NGINX_HTTPS_PORT=$portNginxHttps

#############################
# Data Storage
#############################

DATABASE_PORT=$portDatabase

KEYCLOAK_HTTP_PORT=$portKeycloak
KEYCLOAK_HTTPS_PORT=$portKeycloakHttps

ELASTIC_HTTP_PORT=$portElastic
ELASTIC_COM_PORT=$portElasticCom
ELASTIC_DEJAVU_HTTP_PORT=$portDejavu

AZURE_BLOB_PORT=$portAzureBlob
AZURE_QUEUE_PORT=$portAzureQueue
AZURE_TABLE_PORT=$portAzureTable

ORACLE_DATABASE_PORT=$portOracleDatabase

#############################
# Applications
#############################

API_HTTP_PORT=$portApi
API_HTTPS_PORT=$portApiHttps

CSS_HTTP_PORT=$portCssApi
CSS_HTTPS_PORT=$portCssApiHttps

CHARTS_HTTP_PORT=$portChartsApi
CHARTS_HTTPS_PORT=$portChartsApiHttps

APP_EDITOR_HTTP_PORT=$portAppEditor
APP_EDITOR_HTTPS_PORT=$portAppEditorHttps

APP_SUBSCRIBER_HTTP_PORT=$portAppSubscriber
APP_SUBSCRIBER_HTTPS_PORT=$portAppSubscriberHttps

#############################
# Services
#############################

SYNDICATION_PORT=$portSyndication
CAPTURE_PORT=$portCapture
CLIP_PORT=$portClip
IMAGE_PORT=$portImage
FILE_PORT=$portFile
CONTENT_PORT=$portContent
CONTENTMIGRATION_PORT=$portContentMigration
INDEXING_PORT=$portIndexing
IMAGE_PORT=$portImage
TRANSCRIPTION_PORT=$portTranscription
NLP_PORT=$portNlp
CORENLP_PORT=$portCoreNlp
FILECOPY_PORT=$portFileCopy
NOTIFICATION_PORT=$portNotification
REPORTING_PORT=$portReporting
FOLDER_COLLECTION_PORT=$portFolderCollection
FFMPEG_PORT=$portFFmpeg
SCHEDULER_PORT=$portScheduler
EVENTHANDER_PORT=$portEventHandler
EXTRACT_QUOTES_PORT=$portExtractQuotes

#############################
# Kafka Configuration
#############################

KAFKA_BROKER_ADVERTISED_HOST_PORT=$portKafkaBrokerAdvertisedHost
KAFKA_BROKER_ADVERTISED_EXTERNAL_PORT=$portKafkaBrokerAdvertisedExternal
KAFKA_KOWL_PORT=$portKafkaKowl" >> ./.env
    echo "./.env created"
fi

# Database - PostgreSQL DockerHub Image
if test -f "./db/postgres/docker/.env"; then
    echo "./db/postgres/docker/.env exists"
else
echo \
"POSTGRES_USER=$dbUser
POSTGRES_PASSWORD=$password
POSTGRES_DB=$dbName

KEYCLOAK_DB=$keycloakDbName" >> ./db/postgres/docker/.env
    echo "./db/postgres/docker/.env created"
fi

# Database - PostgreSQL Redhat Image
if test -f "./db/postgres/rhel8/.env"; then
    echo "./db/postgres/rhel8/.env exists"
else
echo \
"POSTGRESQL_USER=$dbUser
POSTGRESQL_PASSWORD=$password
POSTGRESQL_DATABASE=$dbName
POSTGRESQL_ADMIN_PASSWORD=$password

KEYCLOAK_DATABASE=$keycloakDbName" >> ./db/postgres/rhel8/.env
    echo "./db/postgres/rhel8/.env created"
fi

# Database - Oracle Docker Image
if test -f "./db/oracle/docker/.env"; then
    echo "./db/oracle/docker/.env exists"
else
echo \
"ORACLE_PWD=$oraclePassword
ORACLE_CHARACTERSET=AL32UTF8
ORACLE_EDITION=standard
" >> ./db/oracle/docker/.env
    echo "./db/oracle/docker/.env created"
fi

# Database - DAL
if test -f "./libs/net/dal/.env"; then
    echo "./libs/net/dal.env exists"
else
echo \
"ConnectionStrings__TNO=Host=host.docker.internal:$portDatabase;Database=$dbName;Include Error Detail=true;Log Parameters=true;
DB_POSTGRES_USERNAME=$dbUser
DB_POSTGRES_PASSWORD=$password" >> ./libs/net/dal/.env
    echo "./libs/net/dal/.env created"
fi

# Keycloak
if test -f "./auth/keycloak/.env"; then
    echo "./auth/keycloak/.env exists"
else
echo \
"PROXY_ADDRESS_FORWARDING=true
KEYCLOAK_LOGLEVEL=WARN
ROOT_LOGLEVEL=WARN

KC_DB=postgres
KC_DB_URL=jdbc:postgresql://database/$keycloakDbName
KC_DB_USERNAME=$dbUser
KC_DB_PASSWORD=$password
KC_HOSTNAME=localhost
KEYCLOAK_ADMIN=$keycloakUser
KEYCLOAK_ADMIN_PASSWORD=$keycloakPassword" >> ./auth/keycloak/.env
    echo "./auth/keycloak/.env created"
fi

# Nginx
if test -f "./network/nginx/.env"; then
    echo "./network/nginx/.env exists"
else
echo \
"" >> ./network/nginx/.env
    echo "./network/nginx/.env created"
fi

# Azure Storage
if test -f "./db/azure-storage/.env"; then
    echo "./db/azure-storage/.env exists"
else
echo \
"AZURITE_ACCOUNTS=devaccount1:$azureKey" >> ./db/azure-storage/.env
    echo "./db/azure-storage/.env created"
fi

# Elasticsearch
if test -f "./db/elasticsearch/.env"; then
    echo "./db/elasticsearch/.env exists"
else
echo \
"NETWORK_HOST=0.0.0.0
CLUSTER_NAME=tno-es-cluster
CLUSTER_INITIAL_MASTER_NODES=$dbName
NODE_NAME=$dbName
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password
DISCOVERY_TYPE=single-node
DISCOVERY_SEED_HOSTS=
DISCOVERY_SEED_PROVIDERS=
BOOTSTRAP_MEMORY_LOCK=true
ES_JAVA_OPTS='-Xms512m -Xmx512m'" >> ./db/elasticsearch/.env
    echo "./db/elasticsearch/.env created"
fi

# API - .NET
if test -f "./api/net/.env"; then
    echo "./api/net/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080
TZ=America/Los_Angeles

ConnectionStrings__TNO=Host=host.docker.internal:$portDatabase;Database=$dbName;Include Error Detail=true;Log Parameters=true;

DB_POSTGRES_USERNAME=$dbUser
DB_POSTGRES_PASSWORD=$password

# Elastic__Url=http://localhost:$portElastic
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password

Reporting__ViewContentUrl=http://localhost:$portNginxSubscriber/view/
Charts__Url=http://charts:8080
# Charts__Url=http://localhost:$portChartsApi

AZURE_STORAGE_CONTAINER_NAME=$dbName
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devaccount1;AccountKey=$azureKey;BlobEndpoint=http://host.docker.internal:$portAzureBlob/devaccount1;

COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=$azureCognitiveServiceKey
COGNITIVE_SERVICES_SPEECH_REGION=$azureCognitiveServiceRegion

AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=$azureVideoAnalyzerKey
AZURE_VIDEO_ANALYZER_ACCOUNT_ID=$azureVideoAccountId
AZURE_VIDEO_ANALYZER_LOCATION=$azureVideoLocation

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBrokerAdvertisedExternal

################################################
# Only required when running in devcontainer for debugging.
################################################
# Storage__UploadPath=uploads
# Storage__CapturePath=uploads
# Keycloak__Issuer=http://localhost:$portKeycloak/auth/realms/mmi

################################################
# Remote CSS
################################################
# Keycloak__Authority=https://dev.loginproxy.gov.bc.ca/auth/realms/standard
# Keycloak__Audience=media-monitoring-mmia-3671,mmia-service-account-3994
# Keycloak__Issuer=media-monitoring-mmia-3671,mmia-service-account-3994
# CSS__IntegrationId=3671
# CSS__ClientId=service-account-team-795-4127
# CSS__Secret={https://bcgov.github.io/sso-requests}

################################################
# Custom Keycloak Realm
################################################
# Keycloak__Authority=https://dev.loginproxy.gov.bc.ca/auth/realms/mmi
# Keycloak__Audience=mmi-app
# Keycloak__Issuer=mmi-app,mmi-service-account
# Keycloak__ServiceAccount__Authority=https://dev.loginproxy.gov.bc.ca
# Keycloak__ServiceAccount__Secret={GET KEYCLOAK mmi-service-account CLIENT SECRET}
# Keycloak__ClientId={GET KEYCLOAK mmi-app CLIENT ID}

################################################
# Local Keycloak
################################################
keycloak__Authority=http://host.docker.internal:$portKeycloak/realms/mmi
Keycloak__Audience=mmi-app,mmi-service-account
Keycloak__Issuer=mmi-app,mmi-service-account
Keycloak__ServiceAccount__Authority=http://host.docker.internal:$portKeycloak
Keycloak__ServiceAccount__RealmPath=/realms/
Keycloak__ServiceAccount__AdminPath=/admin/realms/
Keycloak__ServiceAccount__Secret={GET KEYCLOAK mmi-service-account CLIENT SECRET}
Keycloak__ClientId={GET KEYCLOAK mmi-app CLIENT ID}

# CSS__ApiUrl=http://host.docker.internal:$portCssApi/api
# CSS__Authority=http://host.docker.internal:$portCssApi
# CSS__TokenPath=/api/v1/token
# CSS__ClientId=service-account-team-795-4127
# CSS__Secret={GET KEYCLOAK mmi-service-account CLIENT SECRET}

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={GET CHES USERNAME}
CHES__Password={GET CHES PASSWORD}
CHES__EmailAuthorized=true" >> ./api/net/.env
    echo "./api/net/.env created"
fi

# API - CSS
if test -f "./tools/css-api/.env"; then
    echo "./tools/css-api/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8080

# Keycloak API
Keycloak__RealmPath=/realms/
Keycloak__AdminPath=/admin/realms/
Keycloak__Secret={GET KEYCLOAK SERVICE ACCOUNT}" >> ./tools/css-api/.env
    echo "./tools/css-api/.env created"
fi

# API - Charts
if test -f "./api/node/.env"; then
    echo "./api/node/.env exists"
else
echo \
"PORT=8080" >> ./api/node/.env
    echo "./api/node/.env created"
fi

# APP - Editor
if test -f "./app/editor/.env"; then
    echo "./app/editor/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
WDS_SOCKET_PORT=$portNginxEditor
#API_URL=http://api:80/
#REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth" >> ./app/editor/.env
    echo "./app/editor/.env created"
fi

# APP - Subscriber
if test -f "./app/subscriber/.env"; then
    echo "./app/subscriber/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
WDS_SOCKET_PORT=$portNginxSubscriber
#API_URL=http://api:80/
#REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth" >> ./app/subscriber/.env
    echo "./app/subscriber/.env created"
fi

###########################################################################
# Kafka Configuration
###########################################################################

# Kafka broker
if test -f "./db/kafka/broker/.env"; then
    echo "./db/kafka/broker/.env exists"
else
echo \
"CLUSTER_ID='MkU3OEVBNTcwNTJENDM2Qk'
KAFKA_NODE_ID=1
KAFKA_LISTENER_SECURITY_PROTOCOL_MAP='CONTROLLER:PLAINTEXT,INTERNAL:PLAINTEXT,HOST:PLAINTEXT,EXTERNAL:PLAINTEXT'
KAFKA_ADVERTISED_LISTENERS='INTERNAL://broker:29092,HOST://localhost:9092,EXTERNAL://host.docker.internal:40102'
KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1
KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS=0
KAFKA_TRANSACTION_STATE_LOG_MIN_ISR=1
KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR=1
KAFKA_JMX_PORT=9101
KAFKA_JMX_HOSTNAME=host.docker.internal
KAFKA_PROCESS_ROLES='broker,controller'
KAFKA_CONTROLLER_QUORUM_VOTERS='1@broker:29093'
KAFKA_LISTENERS='INTERNAL://broker:29092,CONTROLLER://broker:29093,HOST://0.0.0.0:9092,EXTERNAL://0.0.0.0:29094'
KAFKA_INTER_BROKER_LISTENER_NAME='INTERNAL'
KAFKA_CONTROLLER_LISTENER_NAMES='CONTROLLER'
KAFKA_AUTO_CREATE_TOPICS_ENABLE='true'
KAFKA_TOOLS_LOG4J_LOGLEVEL=WARN
KAFKA_LOG4J_LOGGERS=org.apache.zookeeper=ERROR,org.apache.kafka=ERROR,kafka=ERROR,kafka.cluster=ERROR,kafka.controller=ERROR,kafka.coordinator=ERROR,kafka.log=ERROR,kafka.server=ERROR,kafka.zookeeper=ERROR,state.change.logger=ERROR
KAFKA_LOG4J_ROOT_LEVEL=WARN
KAFKA_HEAP_OPTS='-Xmx8G -Xms6G'
KAFKA_JVM_PERFORMANCE_OPTS='-server -XX:MetaspaceSize=96m  -XX:G1HeapRegionSize=16M -XX:MinMetaspaceFreeRatio=50 -XX:MaxMetaspaceFreeRatio=80 -XX:+UseG1GC -XX:MaxGCPauseMillis=20 -XX:InitiatingHeapOccupancyPercent=35 -XX:+ExplicitGCInvokesConcurrent -Djava.awt.headless=true'
KAFKA_NUM_PARTITIONS=3
KAFKA_DEFAULT_REPLICATION_FACTOR=1" >> ./db/kafka/broker/.env
    echo "./db/kafka/broker/.env created"
fi

# Kafka Kowl
if test -f "./db/kafka/kowl/.env"; then
    echo "./db/kafka/kowl/.env exists"
else
echo \
"# See: https://github.com/cloudhut/kowl/tree/master/docs/config for reference config files.
# This is a YAML file because of Kowl's lack of configuration options.
# Keep this as a .env so it doesn't get committed to source.
kafka:
  brokers:
    - broker:29092

# server:
  # listenPort: 8080" >> ./db/kafka/kowl/.env
    echo "./db/kafka/kowl/.env created"
fi

###########################################################################
# .NET Services Configuration
###########################################################################

## Syndication Ingest Service
if test -f "./services/net/syndication/.env"; then
    echo "./services/net/syndication/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/syndication/.env
    echo "./services/net/syndication/.env created"
fi

## Capture Ingest Service
if test -f "./services/net/capture/.env"; then
    echo "./services/net/capture/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__VolumePath=../data

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/capture/.env
    echo "./services/net/capture/.env created"
fi

## Clip Ingest Service
if test -f "./services/net/clip/.env"; then
    echo "./services/net/clip/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__VolumePath=../data

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/clip/.env
    echo "./services/net/clip/.env created"
fi

## Image Ingest Service
if test -f "./services/net/image/.env"; then
    echo "./services/net/image/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__VolumePath=../data

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/image/.env
    echo "./services/net/image/.env created"
fi

## File Ingest Service
if test -f "./services/net/filemonitor/.env"; then
    echo "./services/net/filemonitor/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__VolumePath=../data

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/filemonitor/.env
    echo "./services/net/filemonitor/.env created"
fi

## Content Service
if test -f "./services/net/content/.env"; then
    echo "./services/net/content/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Kafka__Admin__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal
Kafka__Producer__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal
Kafka__Consumer__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__TranscriptionTopic=transcription

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=" >> ./services/net/content/.env
    echo "./services/net/content/.env created"
fi

## Content Migration Service
if test -f "./services/net/contentmigration/.env"; then
    echo "./services/net/contentmigration/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__DataLocation=Openshift
Service__SupportedImportMigrationTypes=Historic,Any,Recent,Current

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=" >> ./services/net/contentmigration/.env
    echo "./services/net/contentmigration/.env created"
fi

## Transcription Service
if test -f "./services/net/transcription/.env"; then
    echo "./services/net/transcription/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__AzureCognitiveServicesKey={ENTER A VALID AZURE KEY}

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/transcription/.env
    echo "./services/net/transcription/.env created"
fi

## Indexing Service
if test -f "./services/net/indexing/.env"; then
    echo "./services/net/indexing/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__ElasticsearchUri=http://host.docker.internal:$portElastic
Service__ElasticsearchUsername=$elasticUser
Service__ElasticsearchPassword=$password

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/indexing/.env
    echo "./services/net/indexing/.env created"
fi

## NLP Service
if test -f "./services/net/nlp/.env"; then
    echo "./services/net/nlp/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/nlp/.env
    echo "./services/net/nlp/.env created"
fi

## FileCopy Service
if test -f "./services/net/filecopy/.env"; then
    echo "./services/net/filecopy/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__EmailAuthorized=true
# CHES__OverrideTo=

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal" >> ./services/net/filecopy/.env
    echo "./services/net/filecopy/.env created"
fi

## Notification Service
if test -f "./services/net/notification/.env"; then
    echo "./services/net/notification/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__OverrideTo={CHANGE THIS TO YOUR EMAIL ADDRESS}

# Elastic__Url=host.docker.internal:$portElastic
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password" >> ./services/net/notification/.env
    echo "./services/net/notification/.env created"
fi

## Reporting Service
if test -f "./services/net/reporting/.env"; then
    echo "./services/net/reporting/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
Charts__Url=http://charts:8080
# Charts__Url=http://localhost:$portChartsApi

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__OverrideTo={CHANGE THIS TO YOUR EMAIL ADDRESS}" >> ./services/net/reporting/.env
    echo "./services/net/reporting/.env created"
fi

## Scheduler Service
if test -f "./services/net/scheduler/.env"; then
    echo "./services/net/scheduler/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__OverrideTo={CHANGE THIS TO YOUR EMAIL ADDRESS}" >> ./services/net/scheduler/.env
    echo "./services/net/scheduler/.env created"
fi

## Folder Collection Service
if test -f "./services/net/folder-collection/.env"; then
    echo "./services/net/folder-collection/.env exists"
else
echo \
"# Local
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

###########################################
# Local
###########################################
Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

# Elastic__Url=host.docker.internal:$portElastic
ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password" >> ./services/net/folder-collection/.env
    echo "./services/net/folder-collection/.env created"
fi

## FFmpeg Service
if test -f "./services/net/ffmpeg/.env"; then
    echo "./services/net/ffmpeg/.env exists"
else
echo \
"# Local
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

###########################################
# Local
###########################################
Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__VolumePath=../data" >> ./services/net/ffmpeg/.env
    echo "./services/net/ffmpeg/.env created"
fi

## Event Handler Service
if test -f "./services/net/event-handler/.env"; then
    echo "./services/net/event-handler/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__OverrideTo={CHANGE THIS TO YOUR EMAIL ADDRESS}" >> ./services/net/event-handler/.env
    echo "./services/net/event-handler/.env created"
fi

## Extract Quotes Service
if test -f "./services/net/extract-quotes/.env"; then
    echo "./services/net/extract-quotes/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak
Auth__Keycloak__Audience=mmi-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}
Auth__OIDC__Token=/realms/mmi/protocol/openid-connect/token

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__CoreNLPApiUrl=http://host.docker.internal:$portCoreNlp
Service__ExtractQuotesOnIndex=false
Service__ExtractQuotesOnPublish=true

Kafka__BootstrapServers=host.docker.internal:$portKafkaBrokerAdvertisedExternal

CHES__AuthUrl=https://dev.loginproxy.gov.bc.ca/auth/realms/comsvcauth/protocol/openid-connect/token
CHES__HostUri=https://ches-dev.api.gov.bc.ca/api/v1
CHES__Username={YOU WILL NEED TO GET THIS FROM CHES}
CHES__Password={YOU WILL NEED TO GET THIS FROM CHES}
CHES__OverrideTo={CHANGE THIS TO YOUR EMAIL ADDRESS}" >> ./services/net/extract-quotes/.env
    echo "./services/net/extract-quotes/.env created"
fi



###########################################################################
# .NET Tools
###########################################################################

## Elastic Migration Tool
if test -f "./tools/elastic/migration/.env"; then
    echo "./tools/elastic/migration/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development

ConnectionStrings__TNO=Host=host.docker.internal:$portDatabase;Database=$dbName;Include Error Detail=true;Log Parameters=true;

DB_POSTGRES_USERNAME=$dbUser
DB_POSTGRES_PASSWORD=$password

ELASTIC_USERNAME=$elasticUser
ELASTIC_PASSWORD=$password
Elastic__Url=http://host.docker.internal:$portElastic
# Elastic__MigrationVersion=1.0.1" >> ./tools/elastic/migration/.env
    echo "./tools/elastic/migration/.env created"
fi


## Objects Backup
if test -f "./tools/objects-backup/.env"; then
    echo "./tools/objects-backup/.env exists"
else
echo \
"S3_ENDPOINT={Find in s3 portal}
S3_ACCESS_KEY={Your S3_ACCESS_KEY}
S3_SECRET_KEY={Your S3_SECRET_KEY}
S3_BUCKET={Your Bucket Name}" >> ./tools/objects-backup/.env
    echo "./tools/objects-backup/.env created"
fi