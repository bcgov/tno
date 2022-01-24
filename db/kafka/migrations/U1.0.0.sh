#!/bin/bash

# Logging topics
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-nlp --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-elastic --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-rss --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-atom --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-audio --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic logs-video --bootstrap-server $bootstrap"

# Topics for ingesting news
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-ghi --bootstrap-server $bootstrap"
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-hth --bootstrap-server $bootstrap"

# Topic for the results of NLP process
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic news-nlp --bootstrap-server $bootstrap"

# Topics for media capture
docker exec -i tno-broker bash -c "/bin/kafka-topics --delete --topic media-cbckam --bootstrap-server $bootstrap"
