#!/bin/bash

docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic hub --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic notify --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic index --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic reporting --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic transcribe --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic request-clips --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic ffmpeg --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic event-schedule --bootstrap-server $bootstrap"
