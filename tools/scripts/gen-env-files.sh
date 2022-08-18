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

#############################
# Applications
#############################

API_HTTP_PORT=$portApi
API_HTTPS_PORT=$portApiHttps

APP_EDITOR_HTTP_PORT=$portAppEditor
APP_EDITOR_HTTPS_PORT=$portAppEditorHttps

APP_SUBSCRIBER_HTTP_PORT=$portAppSubscriber
APP_SUBSCRIBER_HTTPS_PORT=$portAppSubscriberHttps

#############################
# Services
#############################

SYNDICATION_PORT=$portSyndication
NLP_PORT=$portNlp
INDEXING_PORT=$portIndexing
CAPTURE_PORT=$portCapture
CLIP_PORT=$portClip
CONTENT_PORT=$portContent
IMAGE_PORT=$portImage
TRANSCRIPTION_PORT=$portTranscription


#############################
# Kafka Configuration
#############################

KAFKA_ZOOKEEPER_PORT=$portKafkaZookeeper
KAFKA_BROKER_ADVERTISED_HOST_PORT=$portKafkaBrokerAdvertisedHost
KAFKA_BROKER_ADVERTISED_EXTERNAL_PORT=$portKafkaBorkerAdvertisedExternal
KAFKA_SCHEMA_REGISTRY_PORT=$portKafkaSchemaRegistry
KAFKA_REST_PROXY_PORT=$portKafkaRestProxy
KAFKA_CONNECT_PORT=$portKafkaConnect
KAFKA_KSQLDB_PORT=$portKafkaKsqlDb
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
KEYCLOAK_USER=$keycloakUser
KEYCLOAK_PASSWORD=$keycloakPassword
KEYCLOAK_IMPORT='/tmp/realm-export.json -Dkeycloak.profile.feature.scripts=enabled -Dkeycloak.profile.feature.upload_scripts=enabled'
KEYCLOAK_LOGLEVEL=WARN
ROOT_LOGLEVEL=WARN

DB_VENDOR=POSTGRES
DB_ADDR=database
DB_PORT=5432
DB_SCHEMA=public
DB_DATABASE=$keycloakDbName
DB_USER=$dbUser
DB_PASSWORD=$password" >> ./auth/keycloak/.env
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

keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
keycloak__ServiceAccount__Authority=http://host.docker.internal:$portKeycloak
keycloak__ServiceAccount__Secret=

ConnectionStrings__TNO=Host=host.docker.internal:$portDatabase;Database=$dbName;Include Error Detail=true;Log Parameters=true;

DB_POSTGRES_USERNAME=$dbUser
DB_POSTGRES_PASSWORD=$password

# Only required when running in devcontainer for debugging.
# Storage__UploadPath=uploads
# Storage__CapturePath=uploads
# Keycloak__Issuer=http://localhost:$portKeycloak/auth/realms/tno

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

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./api/net/.env
    echo "./api/net/.env created"
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
REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth" >> ./app/editor/.env
    echo "./app/editor/.env created"
fi

# APP - Subscriber
if test -f "./app/subscriber/.env"; then
    echo "./app/subscriber/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
#API_URL=http://api:80/
REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:$portKeycloak/auth" >> ./app/subscriber/.env
    echo "./app/subscriber/.env created"
fi

###########################################################################
# Kafka Configuration
###########################################################################

# Kafka zookeeper
if test -f "./db/kafka/zookeeper/.env"; then
    echo "./db/kafka/zookeeper/.env exists"
else
echo \
"ZOOKEEPER_TICK_TIME=2000
ZOOKEEPER_CLIENT_PORT=2181" >> ./db/kafka/zookeeper/.env
    echo "./db/kafka/zookeeper/.env created"
fi

# Kafka broker
if test -f "./db/kafka/broker/.env"; then
    echo "./db/kafka/broker/.env exists"
else
echo \
"KAFKA_BROKER_ID=1
KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181
KAFKA_LISTENERS=INTERNAL://broker:29092,HOST://broker:9092,EXTERNAL://broker:29094
KAFKA_ADVERTISED_LISTENERS=INTERNAL://broker:29092,HOST://localhost:$portKafkaBrokerAdvertisedHost,EXTERNAL://host.docker.internal:$portKafkaBorkerAdvertisedExternal
KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=INTERNAL:PLAINTEXT,HOST:PLAINTEXT,EXTERNAL:PLAINTEXT
KAFKA_INTER_BROKER_LISTENER_NAME=INTERNAL
KAFKA_AUTO_CREATE_TOPICS_ENABLE='true'
KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1
KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS=0

KAFKA_JMX_PORT=9101
KAFKA_JMX_HOSTNAME=host.docker.internal
KAFKA_CONFLUENT_SCHEMA_REGISTRY_URL=http://schema-registry:8081

# KAFKA_METRIC_REPORTERS=io.confluent.metrics.reporter.ConfluentMetricsReporter
# KAFKA_CONFLUENT_LICENSE_TOPIC_REPLICATION_FACTOR=1
# KAFKA_CONFLUENT_BALANCER_TOPIC_REPLICATION_FACTOR=1
# KAFKA_TRANSACTION_STATE_LOG_MIN_ISR=1
# KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR=1

# CONFLUENT_METRICS_REPORTER_BOOTSTRAP_SERVERS=broker:29092
# CONFLUENT_METRICS_REPORTER_TOPIC_REPLICAS=1
# CONFLUENT_METRICS_ENABLE='true'
# CONFLUENT_SUPPORT_CUSTOMER_ID='anonymous'

# Rest API v3
# KAFKA_CONFLUENT_METRICS_REPORTER_BOOTSTRAP_SERVERS=broker:29092
# KAFKA_CONFLUENT_METRICS_REPORTER_ZOOKEEPER_CONNECT=zookeeper:2181
# KAFKA_CONFLUENT_METRICS_REPORTER_TOPIC_REPLICAS=3
# KAFKA_CONFLUENT_METRICS_ENABLE='true'
# KAFKA_CONFLUENT_SUPPORT_CUSTOMER_ID=anonymous
# KAFKA_KAFKA_REST_ADVERTISED_LISTENERS=http://host.docker.internal:$portKafkaKowl
# KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_ORIGIN='*'
# KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_METHODS=GET,POST,PUT,DELETE
# KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_HEADERS=origin,content-type,accept,authorization" >> ./db/kafka/broker/.env
    echo "./db/kafka/broker/.env created"
fi

# Kafka schema-registry
if test -f "./db/kafka/schema-registry/.env"; then
    echo "./db/kafka/schema-registry/.env exists"
else
echo \
"SCHEMA_REGISTRY_HOST_NAME=schema-registry
SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS='broker:9092'
SCHEMA_REGISTRY_LISTENERS=http://0.0.0.0:8081" >> ./db/kafka/schema-registry/.env
    echo "./db/kafka/schema-registry/.env created"
fi

# Kafka rest-proxy
if test -f "./db/kafka/rest-proxy/.env"; then
    echo "./db/kafka/rest-proxy/.env exists"
else
echo \
"KAFKA_REST_HOST_NAME=rest-proxy
KAFKA_REST_BOOTSTRAP_SERVERS='broker:9092'
KAFKA_REST_LISTENERS='http://0.0.0.0:8082'
KAFKA_REST_SCHEMA_REGISTRY_URL='http://schema-registry:8081'" >> ./db/kafka/rest-proxy/.env
    echo "./db/kafka/rest-proxy/.env created"
fi

# Kafka connect
if test -f "./db/kafka/connect/.env"; then
    echo "./db/kafka/connect/.env exists"
else
echo \
"CONNECT_BOOTSTRAP_SERVERS='broker:9092'
CONNECT_REST_ADVERTISED_HOST_NAME=connect
CONNECT_REST_PORT=8083
CONNECT_GROUP_ID=compose-connect-group
CONNECT_CONFIG_STORAGE_TOPIC=docker-connect-configs
CONNECT_CONFIG_STORAGE_REPLICATION_FACTOR=1
CONNECT_OFFSET_FLUSH_INTERVAL_MS=10000
CONNECT_OFFSET_STORAGE_TOPIC=docker-connect-offsets
CONNECT_OFFSET_STORAGE_REPLICATION_FACTOR=1
CONNECT_STATUS_STORAGE_TOPIC=docker-connect-status
CONNECT_STATUS_STORAGE_REPLICATION_FACTOR=1
CONNECT_KEY_CONVERTER=org.apache.kafka.connect.storage.StringConverter
CONNECT_VALUE_CONVERTER=io.confluent.connect.avro.AvroConverter
CONNECT_VALUE_CONVERTER_SCHEMA_REGISTRY_URL=http://schema-registry:8081
# CLASSPATH required due to CC-2422
CLASSPATH=/usr/share/java/monitoring-interceptors/monitoring-interceptors-6.2.1.jar
CONNECT_PRODUCER_INTERCEPTOR_CLASSES='io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor'
CONNECT_CONSUMER_INTERCEPTOR_CLASSES='io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor'
CONNECT_PLUGIN_PATH='/usr/share/java,/usr/share/confluent-hub-components'
CONNECT_LOG4J_LOGGERS=org.apache.zookeeper=ERROR,org.I0Itec.zkclient=ERROR,org.reflections=ERROR" >> ./db/kafka/connect/.env
    echo "./db/kafka/connect/.env created"
fi

# Kafka ksqldb
if test -f "./db/kafka/ksqldb/.env"; then
    echo "./db/kafka/ksqldb/.env exists"
else
echo \
"KSQL_CONFIG_DIR='/etc/ksql'
KSQL_BOOTSTRAP_SERVERS='broker:29092'
KSQL_HOST_NAME=ksqldb
KSQL_LISTENERS='http://0.0.0.0:8088'
KSQL_CACHE_MAX_BYTES_BUFFERING=0
KSQL_KSQL_SCHEMA_REGISTRY_URL='http://schema-registry:8081'
KSQL_PRODUCER_INTERCEPTOR_CLASSES='io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor'
KSQL_CONSUMER_INTERCEPTOR_CLASSES='io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor'
KSQL_KSQL_CONNECT_URL='http://connect:8083'
KSQL_KSQL_LOGGING_PROCESSING_TOPIC_REPLICATION_FACTOR=1
KSQL_KSQL_SINK_REPLICAS=1
KSQL_KSQL_STREAMS_REPLICATION_FACTOR=1
KSQL_KSQL_STREAMS_NUM_STANDBY_REPLICAS=1

## Recommended production settings
# KSQL_KSQL_LOGGING_PROCESSING_TOPIC_AUTO_CREATE='true'
# KSQL_KSQL_LOGGING_PROCESSING_STREAM_AUTO_CREATE='true'
# KSQL_KSQL_STREAMS_PRODUCER_RETRIES=2147483647
# KSQL_KSQL_STREAMS_PRODUCER_CONFLUENT_BATCH_EXPIRY_MS=9223372036854775807
# KSQL_KSQL_STREAMS_PRODUCER_REQUEST_TIMEOUT_MS=300000
# KSQL_KSQL_STREAMS_PRODUCER_MAX_BLOCK_MS=9223372036854775807
# KSQL_KSQL_STREAMS_STATE_DIR=/mnt/data" >> ./db/kafka/ksqldb/.env
    echo "./db/kafka/ksqldb/.env created"
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
    - broker:9092

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

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api

Kafka__BootstrapServers=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./services/net/syndication/.env
    echo "./services/net/syndication/.env created"
fi

## Capture Ingest Service
if test -f "./services/net/capture/.env"; then
    echo "./services/net/capture/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__OutputPath=../data/capture

Kafka__BootstrapServers=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./services/net/capture/.env
    echo "./services/net/capture/.env created"
fi

## Clip Ingest Service
if test -f "./services/net/clip/.env"; then
    echo "./services/net/clip/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api
# Service__CapturePath=../data/capture
# Service__OutputPath=../data/clip

Kafka__BootstrapServers=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./services/net/clip/.env
    echo "./services/net/clip/.env created"
fi

## Image Ingest Service
if test -f "./services/net/image/.env"; then
    echo "./services/net/image/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__OutputPath=../data/image" >> ./services/net/image/.env
    echo "./services/net/image/.env created"
fi

## Content Service
if test -f "./services/net/content/.env"; then
    echo "./services/net/content/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__ContentTopics=news-ghi
Service__TranscriptionTopic=transcription

Kafka__BootstrapServers=host.docker.internal:$portKafkaBorkerAdvertisedExternal" >> ./services/net/content/.env
    echo "./services/net/content/.env created"
fi

## Transcription Service
if test -f "./services/net/transcription/.env"; then
    echo "./services/net/transcription/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__TranscriptionTopic=transcription
Service__AzureCognitiveServicesKey={ENTER A VALID AZURE KEY}

Kafka__BootstrapServers=host.docker.internal:40102" >> ./services/net/transcription/.env
    echo "./services/net/transcription/.env created"
fi

## Indexing Service
if test -f "./services/net/indexing/.env"; then
    echo "./services/net/indexing/.env exists"
else
echo \
"ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:8081

Auth__Keycloak__Authority=http://host.docker.internal:$portKeycloak/auth/realms/tno
Auth__Keycloak__Audience=tno-service-account
Auth__Keycloak__Secret={YOU WILL NEED TO GET THIS FROM KEYCLOAK}

Service__ApiUrl=http://host.docker.internal:$portApi/api
Service__ElasticsearchUri=http://host.docker.internal:$portElastic
Service__ElasticsearchUsername=$elasticUser
Service__ElasticsearchPassword=$password

Kafka__BootstrapServers=host.docker.internal:40102" >> ./services/net/indexing/.env
    echo "./services/net/indexing/.env created"
fi
