#!/bin/bash

export KAFKA_BROKER_ID=${HOSTNAME##*-}
echo "KAFKA_BROKER_ID=$KAFKA_BROKER_ID"

/etc/confluent/docker/run
