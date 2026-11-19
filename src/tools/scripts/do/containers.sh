#!/usr/bin/env bash

compose() {
  local operation=$1 profile=${p:-} services=() compose_cmd=() args=()
  shift
  [[ -z "$n" ]] || services=("$n")
  if [[ "$CONTAINER_ENGINE" == podman ]]; then
    compose_cmd=(podman compose)
  elif docker compose version >/dev/null 2>&1; then
    compose_cmd=(docker compose)
  elif command -v docker-compose >/dev/null 2>&1; then
    compose_cmd=(docker-compose)
  else
    die 'Install Docker Compose (docker compose or docker-compose).'
  fi
  [[ "$operation" != up ]] || args+=(--env-file .env)
  args+=(-f docker-compose.yml -f docker-compose.override.yml
    -f dependencies/kafka/docker-compose.yml -f src/services/docker-compose.yml)
  if [[ -z "$profile" ]]; then
    case "$operation" in stop|down|rm) profile=all ;; *) [[ -z "$n" ]] || profile=all ;; esac
  fi
  [[ -z "$profile" ]] || args+=(--profile "$profile")
  # Down operates on the project, never an individual service.
  [[ "$operation" != down ]] || services=()
  "${compose_cmd[@]}" "${args[@]}" "$operation" "$@" "${services[@]}"
}

container_action() {
  local command=$1
  shift
  case "$command" in
    build|up|stop|restart|refresh|remove) allow_options n p ;;
    down) allow_options p v y ;;
    nuke) allow_options y ;;
    refresh-keycloak|renew|db-nuke|elastic-update|reset-consumer-offset) allow_options ;;
    db-*) database_action "$command" "$@"; return ;;
    kafka-*) kafka_action "$command" local; return ;;
    *) die "Unknown $group command '$command'" ;;
  esac
  case "$command" in
    build) compose build --force-rm ;;
    up) compose up -d ;;
    stop) compose stop ;;
    down)
      if [[ -n "$v" ]]; then
        bash src/tools/scripts/confirm.sh "This deletes every $CONTAINER_ENGINE volume in the project - the database, elasticsearch, kafka and api data are permanently lost." "$y"
        compose down -v
      else
        compose down
        echo "Volumes kept. Use 'bash do $group down -v' to also delete all local data."
      fi ;;
    restart) compose stop; compose up -d ;;
    refresh) compose stop; compose rm -f; compose build --force-rm; compose up -d ;;
    remove) compose rm -s -v ;;
    refresh-keycloak)
      n=keycloak; compose stop; compose rm -f
      bash src/tools/scripts/refresh-keycloak.sh
      compose build --force-rm; compose up -d ;;
    renew)
      n=indexing; compose stop
      local service
      for service in api editor subscriber; do
        n=$service; container_action refresh
      done
      bash src/tools/scripts/db-update.sh
      bash src/tools/scripts/elastic-update.sh
      n=indexing; container_action refresh ;;
    elastic-update) bash "src/tools/scripts/$command.sh" ;;
    db-nuke)
      n=keycloak; compose stop
      n=database; compose stop
      "$CONTAINER_ENGINE" rm -f -v tno-database
      "$CONTAINER_ENGINE" volume rm -f tno-database-data
      compose up -d
      bash src/tools/scripts/db-update.sh
      n=keycloak; compose up -d ;;
    reset-consumer-offset) bash dependencies/kafka/scripts/reset-consumer-offset.sh ;;
    nuke)
      v=1; option_keys='v y'; container_action down
      bash src/tools/scripts/nuke.sh ;;
  esac
}
