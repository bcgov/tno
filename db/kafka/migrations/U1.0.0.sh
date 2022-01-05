#!/bin/bash

# Logging topics
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-nlp --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-elastic --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-rss --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-atom --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-audio --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-video --zookeeper $zookeeper"

# Topics for ingesting news
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-ghi --zookeeper $zookeeper"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-hth --zookeeper $zookeeper"

# Topic for the results of NLP process
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-nlp --zookeeper $zookeeper"

# Topics for media capture
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic media-cbckam --zookeeper $zookeeper"