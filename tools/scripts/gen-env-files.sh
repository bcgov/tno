#!/bin/bash

echo ""
echo "*************************************"
echo "Setting up Docker Configuration Files"
echo "*************************************"

varKeycloak=$(grep -Po 'KEYCLOAK_USER=\K.*$' ./auth/keycloak/.env)
if [ -z "$varKeycloak" ]
then
    echo 'Enter a username for the keycloak realm administrator'
    read -p 'Username: ' varKeycloak
else
    echo "Your keycloak username: $varKeycloak"
fi

varDbUser=$(grep -Po 'POSTGRES_USER=\K.*$' ./db/postgres/.env)
if [ -z "$varDbUser" ]
then
    echo 'Enter a username for the database.'
    read -p 'Username: ' varDbUser
else
    echo "Your database username: $varDbUser"
fi

varElastic=$(grep -Po 'ELASTIC_USERNAME=\K.*$' ./db/elasticsearch/.env)
if [ -z "$varElastic" ]
then
    echo 'Enter a username for the Elasticsearch.'
    read -p 'Username: ' varElastic
else
    echo "Your Elasticsearch username: $varElastic"
fi

varAzureCognitiveServiceKey=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=\K.*$' ./api/editor/api-editor/src/main/resources/.env)
if [ -z "$varAzureCognitiveServiceKey" ]
then
    echo 'Enter your Azure Cognitive Service subscription key.'
    read -p 'Key: ' varAzureCognitiveServiceKey
else
    echo "Your Azure Cognitive Service subscription key: $varAzureCognitiveServiceKey"
fi

varAzureCognitiveServiceRegion=$(grep -Po 'COGNITIVE_SERVICES_SPEECH_REGION=\K.*$' ./api/editor/api-editor/src/main/resources/.env)
if [ -z "$varAzureCognitiveServiceRegion" ]
then
    echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
    read -p 'Region: ' varAzureCognitiveServiceRegion
else
    echo "Your Azure Cognitive Service region: $varAzureCognitiveServiceRegion"
fi

varAzureVideoAnalyzerKey=$(grep -Po 'AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=\K.*$' ./api/editor/api-editor/src/main/resources/.env)
if [ -z "$varAzureVideoAnalyzerKey" ]
then
    echo 'Enter your Azure Video Analyzer subscription key.'
    read -p 'Key: ' varAzureVideoAnalyzerKey
else
    echo "Your Azure Video Analyzer subscription key: $varAzureVideoAnalyzerKey"
fi

varAzureVideoAccountId=$(grep -Po 'AZURE_VIDEO_ANALYZER_ACCOUNT_ID=\K.*$' ./api/editor/api-editor/src/main/resources/.env)
if [ -z "$varAzureVideoAccountId" ]
then
    echo 'Enter your Azure Video Analyzer account ID.'
    read -p 'Account ID: ' varAzureVideoAccountId
else
    echo "Your Azure Video Analyzer account ID: $varAzureVideoAccountId"
fi

varAzureVideoLocation=$(grep -Po 'AZURE_VIDEO_ANALYZER_LOCATION=\K.*$' ./api/editor/api-editor/src/main/resources/.env)
if [ -z "$varAzureVideoLocation" ]
then
    echo 'Enter your Azure Video Analyzer location (i.e. trial).'
    read -p 'Location: ' varAzureVideoLocation
else
    echo "Your Azure Video Analyzer location: $varAzureVideoLocation"
fi

# Only required if the Azurite docker container doesn't allow for local domain names.
# Workaround is to either use 'mcr.microsoft.com/azure-storage/azurite:3.14.0', or use the IP address.
# echo 'Enter the IP of your local host.docker.internal.'
# read -p 'IP: ' varHostDockerInternal

varPassword=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/.env)
azureKey=$(date +%s | sha256sum | base64 | head -c 29)

if [ -z "$varPassword" ]
then
  # Generate a random password that satisfies password requirements.
  echo 'A password is randomly being generated.'
  varPassword=$(date +%s | sha256sum | base64 | head -c 29)A8!
  echo "Your generated password is: $varPassword"
else
  echo "Your password is: $varPassword"
fi

varDbName="tno"

###########################################################################
# TNO Configuration
###########################################################################

# Docker Compose
if test -f "./.env"; then
    echo "./.env exists"
else
echo \
"" >> ./.env
    echo "./.env created"
fi

# Database - PostgreSQL
if test -f "./db/postgres/.env"; then
    echo "./db/postgres/.env exists"
else
echo \
"POSTGRES_USER=$varDbUser
POSTGRES_PASSWORD=$varPassword
POSTGRES_DB=$varDbName
KEYCLOAK_DB=keycloak" >> ./db/postgres/.env
    echo "./db/postgres/.env created"
fi

# Database - DAL
if test -f "./libs/java/dal/db/.env"; then
    echo "./libs/java/dal/db/.env exists"
else
echo \
"DB_URL=jdbc:postgresql://host.docker.internal:50002/$varDbName
DB_USERNAME=$varDbUser
DB_PASSWORD=$varPassword" >> ./libs/java/dal/db/.env
    echo "./libs/java/dal/db/.env created"
fi

# Keycloak
if test -f "./auth/keycloak/.env"; then
    echo "./auth/keycloak/.env exists"
else
echo \
"PROXY_ADDRESS_FORWARDING=true
DB_VENDOR=POSTGRES
DB_ADDR=database
DB_PORT=5432
DB_DATABASE=keycloak
DB_SCHEMA=public
DB_USER=$varDbUser
DB_PASSWORD=$varPassword
KEYCLOAK_USER=$varKeycloak
KEYCLOAK_PASSWORD=$varPassword
KEYCLOAK_IMPORT='/tmp/realm-export.json -Dkeycloak.profile.feature.scripts=enabled -Dkeycloak.profile.feature.upload_scripts=enabled'
KEYCLOAK_LOGLEVEL=WARN
ROOT_LOGLEVEL=WARN" >> ./auth/keycloak/.env
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
CLUSTER_INITIAL_MASTER_NODES=$varDbName
NODE_NAME=$varDbName
ELASTIC_USERNAME=$varElastic
ELASTIC_PASSWORD=$varPassword
DISCOVERY_TYPE=single-node
DISCOVERY_SEED_HOSTS=
DISCOVERY_SEED_PROVIDERS=
BOOTSTRAP_MEMORY_LOCK=true
ES_JAVA_OPTS='-Xms512m -Xmx512m'" >> ./db/elasticsearch/.env
    echo "./db/elasticsearch/.env created"
fi

# API - Editor
if test -f "./api/editor/.env"; then
    echo "./api/editor/.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/

DB_URL=jdbc:postgresql://host.docker.internal:50002/$varDbName
DB_USERNAME=$varDbUser
DB_PASSWORD=$varPassword

ELASTIC_URIS=host.docker.internal:50007
ELASTIC_USERNAME=$varElastic
ELASTIC_PASSWORD=$varPassword

AZURE_STORAGE_CONTAINER_NAME=$varDbName
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devaccount1;AccountKey=$azureKey;BlobEndpoint=http://host.docker.internal:50020/devaccount1;

COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=$varAzureCognitiveServiceKey
COGNITIVE_SERVICES_SPEECH_REGION=$varAzureCognitiveServiceRegion

AZURE_VIDEO_ANALYZER_SUBSCRIPTION_KEY=$varAzureVideoAnalyzerKey
AZURE_VIDEO_ANALYZER_ACCOUNT_ID=$varAzureVideoAccountId
AZURE_VIDEO_ANALYZER_LOCATION=$varAzureVideoLocation

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:50019" >> ./api/editor/.env
    echo "./api/editor/.env created"
fi

# APP - Editor
if test -f "./app/editor/.env"; then
    echo "./app/editor/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
#API_URL=http://api-editor:8080/
REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://localhost:50000/auth" >> ./app/editor/.env
    echo "./app/editor/.env created"
fi

# APP - Subscriber
if test -f "./app/subscriber/.env"; then
    echo "./app/subscriber/.env exists"
else
echo \
"NODE_ENV=development
CHOKIDAR_USEPOLLING=true
#API_URL=http://api-subscriber:8080/
REACT_APP_KEYCLOAK_AUTH_SERVER_URL=http://localhost:50000/auth" >> ./app/subscriber/.env
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
KAFKA_ADVERTISED_LISTENERS=INTERNAL://broker:29092,HOST://localhost:50012,EXTERNAL://host.docker.internal:50019
KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=INTERNAL:PLAINTEXT,HOST:PLAINTEXT,EXTERNAL:PLAINTEXT
KAFKA_INTER_BROKER_LISTENER_NAME=INTERNAL
KAFKA_AUTO_CREATE_TOPICS_ENABLE='false'
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
# KAFKA_KAFKA_REST_ADVERTISED_LISTENERS=http://host.docker.internal:50017
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
SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS='broker:29092'
SCHEMA_REGISTRY_LISTENERS=http://0.0.0.0:8081" >> ./db/kafka/schema-registry/.env
    echo "./db/kafka/schema-registry/.env created"
fi

# Kafka rest-proxy
if test -f "./db/kafka/rest-proxy/.env"; then
    echo "./db/kafka/rest-proxy/.env exists"
else
echo \
"KAFKA_REST_HOST_NAME=rest-proxy
KAFKA_REST_BOOTSTRAP_SERVERS='broker:29092'
KAFKA_REST_LISTENERS='http://0.0.0.0:8082'
KAFKA_REST_SCHEMA_REGISTRY_URL='http://schema-registry:8081'" >> ./db/kafka/rest-proxy/.env
    echo "./db/kafka/rest-proxy/.env created"
fi

# Kafka connect
if test -f "./db/kafka/connect/.env"; then
    echo "./db/kafka/connect/.env exists"
else
echo \
"CONNECT_BOOTSTRAP_SERVERS='broker:29092'
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
KSQL_KSQL_LOGGING_PROCESSING_TOPIC_AUTO_CREATE='true'
KSQL_KSQL_LOGGING_PROCESSING_STREAM_AUTO_CREATE='true'" >> ./db/kafka/ksqldb/.env
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
    - broker:29092

# server:
  # listenPort: 8080" >> ./db/kafka/kowl/.env
    echo "./db/kafka/kowl/.env created"
fi

###########################################################################
# Services Configuration
###########################################################################

## Syndication - ATOM
if test -f "./services/syndication/atom.env"; then
    echo "./ervices/syndication/atom.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/

DB_URL=jdbc:postgresql://host.docker.internal:50002/$varDbName
DB_USERNAME=$varDbUser
DB_PASSWORD=$varPassword

KAFKA_LOGS_TOPIC=atom-logs

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:50019
KAFKA_CLIENT_ID=atom-01

MAX_FAILED_ATTEMPTS=5

DATA_SOURCE_ID=GHI
DATA_SOURCE_TYPE=ATOM
DATA_SOURCE_URL=https://www.globalhungerindex.org/atom.xml
DATA_SOURCE_TOPIC=test" >> ./ervices/syndication/atom.env
    echo "./ervices/syndication/atom.env created"
fi

## Syndication - RSS
if test -f "./services/syndication/rss.env"; then
    echo "./ervices/syndication/rss.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/

DB_URL=jdbc:postgresql://host.docker.internal:50002/$varDbName
DB_USERNAME=$varDbUser
DB_PASSWORD=$varPassword

KAFKA_LOGS_TOPIC=rss-logs

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:50019
KAFKA_CLIENT_ID=rss-01

MAX_FAILED_ATTEMPTS=5

DATA_SOURCE_ID=HTH
DATA_SOURCE_TYPE=RSS
DATA_SOURCE_URL=https://www.howtohaven.com/howtohaven.xml
DATA_SOURCE_TOPIC=test" >> ./ervices/syndication/rss.env
    echo "./ervices/syndication/rss.env created"
fi

## NLP
if test -f "./services/nlp/.env"; then
    echo "./ervices/nlp/.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/

DB_URL=jdbc:postgresql://host.docker.internal:50002/$varDbName
DB_USERNAME=$varDbUser
DB_PASSWORD=$varPassword

KAFKA_LOGS_TOPIC=nlp-logs

KAFKA_BOOTSTRAP_SERVERS=host.docker.internal:50019
KAFKA_GROUP_ID=nlp-01
KAFKA_CONSUMER_TOPICS=test
KAFKA_POLL_TIMEOUT=5000
ENABLE_AUTO_COMMIT=true
AUTO_OFFSET_RESET=latest

KAFKA_CLIENT_ID=nlp-01
KAFKA_PRODUCER_TOPIC=nlp

MAX_FAILED_ATTEMPTS=5" >> ./ervices/nlp/.env
    echo "./ervices/nlp/.env created"
fi
