#!/bin/bash

# Logging topics
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic logs-nlp --zookeeper $zookeeper"
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic logs-elastic --zookeeper $zookeeper"
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic logs-rss --zookeeper $zookeeper"
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic logs-atom --zookeeper $zookeeper"

# Topics for ingesting news
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic news-ghi --zookeeper $zookeeper"
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic news-hth --zookeeper $zookeeper"

# Topic for the results of NLP process
docker exec -it tno-broker bash -c "/bin/kafka-topics --delete --topic news-nlp --zookeeper $zookeeper"
