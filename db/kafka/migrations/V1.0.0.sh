#!/bin/bash

# Topics for media capture
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic hub --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic notify --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic index --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic reporting --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic transcribe --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic request-clips --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic ffmpeg --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic event-schedule --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"

## Manually add topics
# /bin/kafka-topics --create --if-not-exists --topic hub --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic notify --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic index --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic reporting --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic transcribe --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic request-clips --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic ffmpeg --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic event-schedule --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
