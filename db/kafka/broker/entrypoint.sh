#!/bin/bash
export POD_INDEX=${POD_NAME##*-}

# ID must be int greater than 0.
export KAFKA_BROKER_ID=$((${POD_INDEX}+1))

# Each broker must have its own listener registered with zookeeper.
# value: INTERNAL://kafka-headless:29092,HOST://kafka-headless:9092,EXTERNAL://kafka-host:29094
export KAFKA_ADVERTISED_LISTENERS=INTERNAL://kafka-broker-${POD_INDEX}.kafka-headless:29092,HOST://kafka-broker-${POD_INDEX}.kafka-headless:9092,EXTERNAL://kafka-broker-${POD_INDEX}.kafka-host:29094

echo KAFKA_BROKER_ID=${KAFKA_BROKER_ID}
echo KAFKA_ADVERTISED_LISTENERS=${KAFKA_ADVERTISED_LISTENERS}

/etc/confluent/docker/run
