#!/bin/bash
export POD_INDEX=${POD_NAME##*-}

# ID must be int greater than 0.
export KAFKA_NODE_ID=$((${POD_INDEX}+1))

# Each broker must have its own listener registered with zookeeper.
# value: INTERNAL://kafka-headless:29092,HOST://kafka-headless:9092,EXTERNAL://kafka-host:29094
export KAFKA_ADVERTISED_LISTENERS=INTERNAL://kafka-broker-${POD_INDEX}.kafka-headless:29092,HOST://kafka-broker-${POD_INDEX}.kafka-headless:9092,EXTERNAL://kafka-broker-${POD_INDEX}.kafka-host:29094
export KAFKA_CONTROLLER_QUORUM_VOTERS=1@kafka-broker-0.kafka-headless:29093,2@kafka-broker-1.kafka-headless:29093,3@kafka-broker-2.kafka-headless:29093,4@kafka-broker-3.kafka-headless:29093

echo ${POD_NAME}
echo KAFKA_NODE_ID=${KAFKA_NODE_ID}
echo KAFKA_ADVERTISED_LISTENERS=${KAFKA_ADVERTISED_LISTENERS}
echo KAFKA_CONTROLLER_QUORUM_VOTERS=${KAFKA_CONTROLLER_QUORUM_VOTERS}

/etc/confluent/docker/run
