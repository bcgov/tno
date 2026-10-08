#!/bin/bash

# Topics for Content-Analysis (analysis requests are keyed by content ID) and Event Handler work orders.
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic analysis-backfill --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic analysis-retry --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic analysis-dlq --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"
docker exec -i tno-broker bash -c "/bin/kafka-topics --create --if-not-exists --topic work-order --bootstrap-server $bootstrap --partitions $partitions --replication-factor $replication"

## Manually add topics
# /bin/kafka-topics --create --if-not-exists --topic analysis-backfill --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic analysis-retry --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic analysis-dlq --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
# /bin/kafka-topics --create --if-not-exists --topic work-order --bootstrap-server kafka-headless:29092 --partitions 6 --replication-factor 3
