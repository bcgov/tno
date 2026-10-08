#!/bin/bash
# Sourced by db/kafka/scripts/migration.sh; deletes the topics and every message in them.

delete_topic analysis-backfill
delete_topic analysis-retry
delete_topic analysis-dlq
delete_topic work-order
