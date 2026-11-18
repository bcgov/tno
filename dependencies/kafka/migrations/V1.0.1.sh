#!/bin/bash
# Sourced by dependencies/kafka/scripts/migration.sh; see dependencies/kafka/scripts/kafka.sh for the functions.
# Content-Analysis requests (keyed by content ID) and Event Handler work orders. Partitions cap how
# many instances consume a topic in parallel; override per environment in dependencies/kafka/environments.

ensure_topic analysis-backfill
ensure_topic analysis-retry
ensure_topic analysis-dlq
ensure_topic work-order
