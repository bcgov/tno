#!/bin/bash
# Sourced by db/kafka/scripts/migration.sh; see db/kafka/scripts/kafka.sh for the functions.
# A topic without --partitions or --replication-factor uses the environment's defaults.

ensure_topic hub
ensure_topic notify
ensure_topic index
ensure_topic reporting
ensure_topic transcribe
ensure_topic request-clips
ensure_topic ffmpeg
ensure_topic event-schedule
ensure_topic automation
ensure_topic analysis
