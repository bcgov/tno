#!/usr/bin/env bash

kafka_action() {
  local command=$1 default_environment=$2 script args=()
  case "$command" in
    kafka-update) allow_options e n r b p f d y; script=migration ;;
    kafka-topic-add) allow_options e t b p r c i d; script=topic-add ;;
    kafka-topic-delete) allow_options e t b i d y; script=topic-delete ;;
    *) die "Unknown Kafka command '$command'" ;;
  esac
  args=(-e "${e:-$default_environment}")
  [[ -z "$b" ]] || args+=(-z "$b")
  [[ -z "$d" ]] || args+=(-d)
  case "$command" in
    kafka-update)
      [[ -z "$n" ]] || args+=(-n "$n")
      [[ -z "$r" ]] || args+=(-r)
      [[ -z "$p" ]] || args+=(-p "$p")
      [[ -z "$f" ]] || args+=(--replication "$f")
      [[ -z "$y" ]] || args+=(-y) ;;
    kafka-topic-add|kafka-topic-delete)
      [[ -z "$t" ]] || args+=(-t "$t")
      [[ -z "$i" ]] || args+=(-i "$i")
      if [[ "$command" == kafka-topic-add ]]; then
        [[ -z "$p" ]] || args+=(-p "$p")
        [[ -z "$r" ]] || args+=(-r "$r")
        [[ -z "$c" ]] || args+=(-c "$c")
      else
        [[ -z "$y" ]] || args+=(-y)
      fi ;;
  esac
  bash "dependencies/kafka/scripts/$script.sh" "${args[@]}"
}
