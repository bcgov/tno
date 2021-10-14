#!/bin/bash

echo 'Enter a username for the keycloak realm administrator'
read -p 'Username: ' varKeycloak

echo 'Enter a username for the API database.'
read -p 'Username: ' varApiDb

echo 'Enter a username for the Elasticsearch.'
read -p 'Username: ' varElastic

echo 'Enter your Azure Cognitive Service subscription key.'
read -p 'Key: ' varAzureCognitiveServiceKey

echo 'Enter your Azure Cognitive Service region (i.e. canadacentral).'
read -p 'Region: ' varAzureCognitiveServiceRegion

# Only required if the Azurite docker container doesn't allow for local domain names.
# Workaround is to either use 'mcr.microsoft.com/azure-storage/azurite:3.14.0', or use the IP address.
# echo 'Enter the IP of your local host.docker.internal.'
# read -p 'IP: ' varHostDockerInternal

passvar=$(grep -Po 'POSTGRES_PASSWORD=\K.*$' ./db/postgres/.env)
azureKey=$(date +%s | sha256sum | base64 | head -c 29)

if [ -z "$passvar" ]
then
  # Generate a random password that satisfies password requirements.
  echo 'A password is randomly being generated.'
  passvar=$(date +%s | sha256sum | base64 | head -c 29)A8!
  echo "Your generated password is: $passvar"
else
  echo "Your current password is: $passvar"
fi

###########################################################################
# TNO Configuration
###########################################################################

# Keycloak
if test -f "./auth/keycloak/.env"; then
    echo "./auth/keycloak/.env exists"
else
echo \
"PROXY_ADDRESS_FORWARDING=true
KEYCLOAK_USER=$varKeycloak
KEYCLOAK_PASSWORD=$passvar
KEYCLOAK_IMPORT='/tmp/realm-export.json -Dkeycloak.profile.feature.scripts=enabled -Dkeycloak.profile.feature.upload_scripts=enabled'
KEYCLOAK_LOGLEVEL=WARN
ROOT_LOGLEVEL=WARN" >> ./auth/keycloak/.env
fi

# Azure Storage
if test -f "./db/azure-storage/.env"; then
    echo "./db/azure-storage/.env exists"
else
echo \
"AZURITE_ACCOUNTS=devaccount1:$azureKey" >> ./db/azure-storage/.env
fi

# API Database - PostgreSQL
if test -f "./db/postgres/.env"; then
    echo "./db/postgres/.env exists"
else
echo \
"POSTGRES_USER=$varApiDb
POSTGRES_PASSWORD=$passvar
POSTGRES_DB=tno" >> ./db/postgres/.env
fi

# Elasticsearch
if test -f "./db/elasticsearch/.env"; then
    echo "./db/elasticsearch/.env exists"
else
echo \
"NETWORK_HOST=0.0.0.0
CLUSTER_NAME=tno-es-cluster
CLUSTER_INITIAL_MASTER_NODES=tno
NODE_NAME=tno
ELASTIC_USERNAME=$varElastic
ELASTIC_PASSWORD=$passvar
DISCOVERY_TYPE=single-node
BOOTSTRAP_MEMORY_LOCK=true
ES_JAVA_OPTS='-Xms512m -Xmx512m'" >> ./db/elasticsearch/.env
fi

# API - Editor
if test -f "./api/editor/api/src/main/resources/.env"; then
    echo "./api/editor/api/src/main/resources/.env exists"
else
echo \
"KEYCLOAK_AUTH_SERVER_URL=http://host.docker.internal:50000/auth/

DB_URL=jdbc:postgresql://host.docker.internal:50002/tno
DB_USERNAME=$varApiDb
DB_PASSWORD=$passvar

ELASTIC_URIS=host.docker.internal:50007
ELASTIC_USERNAME=$varElastic
ELASTIC_PASSWORD=$passvar

AZURE_STORAGE_CONTAINER_NAME=tno
AZURE_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=http;AccountName=devaccount1;AccountKey=$azureKey;BlobEndpoint=http://host.docker.internal:50020/devaccount1;

COGNITIVE_SERVICES_SPEECH_SUBSCRIPTION_KEY=$varAzureCognitiveServiceKey
COGNITIVE_SERVICES_SPEECH_REGION=$varAzureCognitiveServiceRegion" >> ./api/editor/api/src/main/resources/.env
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
fi

# Kafka broker
if test -f "./db/kafka/broker/.env"; then
    echo "./db/kafka/broker/.env exists"
else
echo \
"KAFKA_BROKER_ID=1
KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181
KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT
KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://broker:9092,PLAINTEXT_HOST://localhost:50012
KAFKA_METRIC_REPORTERS=io.confluent.metrics.reporter.ConfluentMetricsReporter
KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1
KAFKA_GROUP_INITIAL_REBALANCE_DELAY_MS=0
KAFKA_CONFLUENT_LICENSE_TOPIC_REPLICATION_FACTOR=1
KAFKA_CONFLUENT_BALANCER_TOPIC_REPLICATION_FACTOR=1
KAFKA_TRANSACTION_STATE_LOG_MIN_ISR=1
KAFKA_TRANSACTION_STATE_LOG_REPLICATION_FACTOR=1
KAFKA_JMX_PORT=9101
KAFKA_JMX_HOSTNAME=localhost
KAFKA_CONFLUENT_SCHEMA_REGISTRY_URL=http://schema-registry:8081
CONFLUENT_METRICS_REPORTER_BOOTSTRAP_SERVERS=broker:9092
CONFLUENT_METRICS_REPORTER_TOPIC_REPLICAS=1
CONFLUENT_METRICS_ENABLE='true'
CONFLUENT_SUPPORT_CUSTOMER_ID='anonymous'

# Rest API v3
KAFKA_CONFLUENT_METRICS_REPORTER_BOOTSTRAP_SERVERS=broker:9092
KAFKA_CONFLUENT_METRICS_REPORTER_ZOOKEEPER_CONNECT=zookeeper:2181
KAFKA_CONFLUENT_METRICS_REPORTER_TOPIC_REPLICAS=3
KAFKA_CONFLUENT_METRICS_ENABLE='true'
KAFKA_CONFLUENT_SUPPORT_CUSTOMER_ID=anonymous
KAFKA_KAFKA_REST_ADVERTISED_LISTENERS=http://localhost:50017
KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_ORIGIN='*'
KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_METHODS=GET,POST,PUT,DELETE
KAFKA_KAFKA_REST_ACCESS_CONTROL_ALLOW_HEADERS=origin,content-type,accept,authorization" >> ./db/kafka/broker/.env
fi

# Kafka schema-registry
if test -f "./db/kafka/schema-registry/.env"; then
    echo "./db/kafka/schema-registry/.env exists"
else
echo \
"SCHEMA_REGISTRY_HOST_NAME=schema-registry
SCHEMA_REGISTRY_KAFKASTORE_BOOTSTRAP_SERVERS='broker:9092'
SCHEMA_REGISTRY_LISTENERS=http://0.0.0.0:8081" >> ./db/kafka/schema-registry/.env
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
CONNECT_PRODUCER_INTERCEPTOR_CLASSES="io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor"
CONNECT_CONSUMER_INTERCEPTOR_CLASSES="io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor"
CONNECT_PLUGIN_PATH="/usr/share/java,/usr/share/confluent-hub-components"
CONNECT_LOG4J_LOGGERS=org.apache.zookeeper=ERROR,org.I0Itec.zkclient=ERROR,org.reflections=ERROR" >> ./db/kafka/connect/.env
fi

# Kafka control-center
if test -f "./db/kafka/control-center/.env"; then
    echo "./db/kafka/control-center/.env exists"
else
echo \
"CONTROL_CENTER_BOOTSTRAP_SERVERS='broker:9092'
CONTROL_CENTER_CONNECT_CONNECT_DEFAULT_CLUSTER='connect:8083'
CONTROL_CENTER_KSQL_KSQLDB1_URL="http://ksqldb-server:8088"
CONTROL_CENTER_KSQL_KSQLDB1_ADVERTISED_URL="http://localhost:50016"
CONTROL_CENTER_SCHEMA_REGISTRY_URL="http://schema-registry:8081"
CONTROL_CENTER_REPLICATION_FACTOR=1
CONTROL_CENTER_INTERNAL_TOPICS_PARTITIONS=1
CONTROL_CENTER_MONITORING_INTERCEPTOR_TOPIC_PARTITIONS=1
CONFLUENT_METRICS_TOPIC_REPLICATION=1
PORT=9021" >> ./db/kafka/control-center/.env
fi

# Kafka ksqldb-server
if test -f "./db/kafka/ksqldb-server/.env"; then
    echo "./db/kafka/ksqldb-server/.env exists"
else
echo \
"KSQL_CONFIG_DIR="/etc/ksql"
KSQL_BOOTSTRAP_SERVERS="broker:9092"
KSQL_HOST_NAME=ksqldb-server
KSQL_LISTENERS="http://0.0.0.0:8088"
KSQL_CACHE_MAX_BYTES_BUFFERING=0
KSQL_KSQL_SCHEMA_REGISTRY_URL="http://schema-registry:8081"
KSQL_PRODUCER_INTERCEPTOR_CLASSES="io.confluent.monitoring.clients.interceptor.MonitoringProducerInterceptor"
KSQL_CONSUMER_INTERCEPTOR_CLASSES="io.confluent.monitoring.clients.interceptor.MonitoringConsumerInterceptor"
KSQL_KSQL_CONNECT_URL="http://connect:8083"
KSQL_KSQL_LOGGING_PROCESSING_TOPIC_REPLICATION_FACTOR=1
KSQL_KSQL_LOGGING_PROCESSING_TOPIC_AUTO_CREATE='true'
KSQL_KSQL_LOGGING_PROCESSING_STREAM_AUTO_CREATE='true'" >> ./db/kafka/ksqldb-server/.env
fi

# Kafka rest-proxy
if test -f "./db/kafka/rest-proxy/.env"; then
    echo "./db/kafka/rest-proxy/.env exists"
else
echo \
"KAFKA_REST_HOST_NAME=rest-proxy
KAFKA_REST_BOOTSTRAP_SERVERS='broker:9092'
KAFKA_REST_LISTENERS="http://0.0.0.0:8082"
KAFKA_REST_SCHEMA_REGISTRY_URL='http://schema-registry:8081'" >> ./db/kafka/rest-proxy/.env
fi
