#!/bin/bash

# Logging topics
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic logs-nlp --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic logs-elastic --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic logs-syndication --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic logs-audio --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic logs-video --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"

# Topics for ingesting news
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic news-ghi --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic news-hth --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"

# Topic for the results of NLP process
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic news-nlp --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"

# Topics for media capture
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --topic media-cbckam --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
