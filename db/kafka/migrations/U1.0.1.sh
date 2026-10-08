#!/bin/bash

docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic analysis-backfill --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic analysis-retry --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic analysis-dlq --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic work-order --bootstrap-server $bootstrap"
