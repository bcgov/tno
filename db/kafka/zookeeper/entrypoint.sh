#!/bin/bash
export ZOOKEEPER_SERVER_ID=$((${POD_NAME##*-}+1))
echo ZOOKEEPER_SERVER_ID=${ZOOKEEPER_SERVER_ID}
echo ${ZOOKEEPER_SERVER_ID} > ${ZOOKEEPER_DATA_DIR}/myid
/etc/confluent/docker/run
