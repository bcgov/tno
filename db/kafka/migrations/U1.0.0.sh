#!/bin/bash
# Sourced by db/kafka/scripts/migration.sh; deletes the topics and every message in them.

delete_topic hub
delete_topic notify
delete_topic index
delete_topic reporting
delete_topic transcribe
delete_topic request-clips
delete_topic ffmpeg
delete_topic event-schedule
delete_topic automation
delete_topic analysis
