#!/bin/bash

echo 'Enter the consumer group name'
read -p 'Group: ' varGroup

echo 'Enter the topic name'
read -p 'Topic: ' varTopic

docker exec -it tno-broker bash -c "/bin/kafka-consumer-groups --bootstrap-server broker:29092 --group $varGroup --topic $varTopic --reset-offsets --to-earliest --execute"
