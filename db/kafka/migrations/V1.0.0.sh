#!/bin/bash

# Logging topics
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic logs-nlp --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic logs-elastic --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic logs-rss --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic logs-atom --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"

# Topics for ingesting news
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic news-ghi --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic news-hth --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"

# Topic for the results of NLP process
docker exec -it tno-broker bash -c "/bin/kafka-topics --create --topic news-nlp --zookeeper $zookeeper --partitions $partitions --replication-factor $replication"
