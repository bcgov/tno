#!/bin/bash
# Elasticsearch snapshot operations for MMI. See ../SNAPSHOTS.md.
#
#   snapshots.sh [--local] <command> [arguments]
#
# Targets the OpenShift cluster by default (namespace $NS, default 9b301c-dev; `oc login` first).
# --local targets the docker-compose container ($LOCAL_CONTAINER, default tno-elastic) and reads
# ELASTIC_PASSWORD from dependencies/elasticsearch/.env when it is not already set.
#
# Elasticsearch snapshot repository (works on 7.17 and 8.x):
#   setup                     Register the 'snapshots' repository, verify every node can write to it,
#                             and create/replace the 'nightly' snapshot lifecycle policy.
#   now [name]                Take a snapshot now (default name: manual-<UTC timestamp>).
#   wait <name>               Wait for a snapshot to finish; fails unless it succeeded.
#   list                      List snapshots in the repository.
#   status [name]             Show progress of running snapshots, or of the named snapshot.
#   policy                    Show the nightly policy, its last run and next run.
#   run-policy                Run the nightly policy immediately.
#   restore <name> [indices]  Restore a snapshot's indices as restored_<index> (nothing is overwritten).
#                             Aliases and global state are not restored.
#   restore-in-place <name> <indices>
#                             Restore indices under their original names. Each must not exist (delete it first).
#   delete <name>             Delete a snapshot.
#
# Whole-volume CSI snapshots of data-elastic-N (OpenShift only; namespace quota allows 5 in total):
#   volume <label>            Create VolumeSnapshot data-elastic-N-<label> for every data volume.
#   volume-list               List VolumeSnapshots in the namespace.
#   volume-delete <label>     Delete the VolumeSnapshots created with <label>.
set -euo pipefail

NS="${NS:-9b301c-dev}"
POD="${POD:-elastic-0}"
LOCAL_CONTAINER="${LOCAL_CONTAINER:-tno-elastic}"
REPOSITORY="snapshots"
LOCATION="/usr/share/elasticsearch/snapshots"
POLICY="nightly"
# Every day at 09:30 UTC (01:30 Pacific standard time).
SCHEDULE="${SCHEDULE:-0 30 9 * * ?}"
# Snapshot only the application indices; system indices and cluster state are not needed to recover them.
INDICES="${INDICES:-content*,unpublished_content*,migrations}"
RETAIN_DAYS="${RETAIN_DAYS:-14d}"
RETAIN_MIN="${RETAIN_MIN:-1}"
RETAIN_MAX="${RETAIN_MAX:-1}"

LOCAL=false
if [ "${1:-}" = "--local" ]; then
  LOCAL=true
  shift
fi

# env_value <file> <NAME> - the value of NAME=value in an env file. One pair of matching quotes around the
# whole value is removed, as docker-compose does; any other quote character is part of the value.
env_value() {
  local value
  value="$(sed -n "s/^$2=//p" "$1" | tail -n 1)"
  value="${value%$'\r'}"
  if [ "${#value}" -ge 2 ]; then
    case "$value" in
      \"*\" | \'*\') value="${value:1:${#value}-2}" ;;
    esac
  fi
  printf '%s' "$value"
}

if $LOCAL && [ -z "${ELASTIC_PASSWORD+x}" ]; then
  ENV_FILE="$(git rev-parse --show-toplevel 2>/dev/null || pwd)/dependencies/elasticsearch/.env"
  if [ -f "$ENV_FILE" ]; then
    ELASTIC_PASSWORD="$(env_value "$ENV_FILE" ELASTIC_PASSWORD)"
  fi
fi

# curl_quote <text> - quote a value for a curl config file (curl -K).
curl_quote() {
  local text="$1"
  text="${text//\\/\\\\}"
  text="${text//\"/\\\"}"
  text="${text//$'\t'/\\t}"
  text="${text//$'\n'/\\n}"
  text="${text//$'\r'/\\r}"
  printf '"%s"' "$text"
}

# es <METHOD> <path> [json body] - call Elasticsearch from inside the container/pod.
# Prints the response body. Returns 1 when Elasticsearch answers with HTTP 400 or above, and 2 when no complete
# response arrived (the container or pod is not running, the connection is refused, the call times out).
es() {
  local method="$1" path="$2" body="${3:-}" output status code=0
  local args=(curl -sS -m 600 -w '\n%{http_code}' -X "$method" -H 'Content-Type: application/json' "http://localhost:9200$path")
  [ -n "$body" ] && args+=(-d "$body")
  if $LOCAL; then
    if [ -n "${ELASTIC_PASSWORD:-}" ]; then
      # The credentials reach curl as a config file on standard input (curl -K -), so they are in no argument
      # list and no environment, on this host or in the container.
      output="$(printf 'user = %s\n' "$(curl_quote "elastic:$ELASTIC_PASSWORD")" \
        | docker exec -i "$LOCAL_CONTAINER" "${args[@]}" -K -)" || code=$?
    else
      output="$(docker exec "$LOCAL_CONTAINER" "${args[@]}")" || code=$?
    fi
  else
    output="$(oc exec -n "$NS" "$POD" -c elastic -- "${args[@]}")" || code=$?
  fi
  # curl writes the status after the body. It is 000, or missing, when no response arrived.
  status="${output##*$'\n'}"
  if [ "$code" -ne 0 ] || ! [[ "$status" =~ ^[1-5][0-9][0-9]$ ]]; then
    echo "ERROR: $method $path got no complete response from Elasticsearch (exit $code)." >&2
    return 2
  fi
  printf '%s\n' "${output%$'\n'*}"
  if [ "$status" -ge 400 ]; then
    echo "ERROR: $method $path returned HTTP $status" >&2
    return 1
  fi
}

# Wait for a snapshot to finish; fail unless it succeeded. It also fails when Elasticsearch says the snapshot
# cannot be read (a wrong name, a missing repository), and when its state cannot be read three times in a row.
wait_for() {
  local name="$1" response state code unread=0
  while true; do
    code=0
    response="$(es GET "/_snapshot/$REPOSITORY/$name?filter_path=snapshots.state")" || code=$?
    if [ "$code" -eq 1 ]; then
      printf '%s\n' "$response" >&2
      echo "ERROR: snapshot '$name' cannot be read from repository '$REPOSITORY'. Check the name with 'list'." >&2
      return 1
    fi
    state="$(grep -o '"state":"[A-Z_]*"' <<<"$response" | cut -d'"' -f4 || true)"
    case "$state" in
      SUCCESS) echo "Snapshot '$name' succeeded."; return 0 ;;
      IN_PROGRESS) unread=0; sleep 10 ;;
      "")
        unread=$((unread + 1))
        if [ "$unread" -ge 3 ]; then
          echo "ERROR: the state of snapshot '$name' could not be read $unread times in a row. It may still be running: run 'wait $name' again once Elasticsearch answers." >&2
          return 1
        fi
        sleep 10
        ;;
      *) echo "ERROR: snapshot '$name' finished with state $state." >&2; es GET "/_snapshot/$REPOSITORY/$name" >&2; return 1 ;;
    esac
  done
}

require() {
  if [ -z "${1:-}" ]; then
    echo "ERROR: $2" >&2
    exit 1
  fi
}

case "${1:-}" in
  setup)
    es PUT "/_snapshot/$REPOSITORY" "{\"type\":\"fs\",\"settings\":{\"location\":\"$LOCATION\",\"compress\":true}}"
    es POST "/_snapshot/$REPOSITORY/_verify"
    indices_json="[\"${INDICES//,/\",\"}\"]"
    es PUT "/_slm/policy/$POLICY" "{
      \"schedule\": \"$SCHEDULE\",
      \"name\": \"<nightly-{now/d}>\",
      \"repository\": \"$REPOSITORY\",
      \"config\": { \"indices\": $indices_json, \"include_global_state\": false, \"ignore_unavailable\": true },
      \"retention\": { \"expire_after\": \"$RETAIN_DAYS\", \"min_count\": $RETAIN_MIN, \"max_count\": $RETAIN_MAX }
    }"
    ;;
  now)
    name="${2:-manual-$(date -u +%Y%m%d-%H%M%S)}"
    indices_json="[\"${INDICES//,/\",\"}\"]"
    es PUT "/_snapshot/$REPOSITORY/$name?wait_for_completion=false" \
      "{\"indices\":$indices_json,\"include_global_state\":false,\"ignore_unavailable\":true}"
    echo "Started snapshot '$name'. Wait for it with: $0 $($LOCAL && echo --local) wait $name"
    ;;
  wait)
    require "${2:-}" "wait <snapshot>"
    wait_for "$2"
    ;;
  list)
    es GET "/_cat/snapshots/$REPOSITORY?v&s=start_epoch&h=id,status,start_time,duration,indices,successful_shards,failed_shards,total_shards"
    ;;
  status)
    if [ -n "${2:-}" ]; then
      es GET "/_snapshot/$REPOSITORY/$2/_status?filter_path=snapshots.snapshot,snapshots.state,snapshots.shards_stats,snapshots.stats.total,snapshots.stats.incremental"
    else
      es GET "/_snapshot/_status"
    fi
    ;;
  policy)
    es GET "/_slm/policy/$POLICY?human&filter_path=*.policy.schedule,*.policy.retention,*.last_success,*.last_failure,*.next_execution,*.stats"
    ;;
  run-policy)
    es POST "/_slm/policy/$POLICY/_execute"
    ;;
  restore)
    require "${2:-}" "restore <snapshot> [indices]"
    indices="${3:-$INDICES}"
    es POST "/_snapshot/$REPOSITORY/$2/_restore" "{
      \"indices\": \"$indices\",
      \"ignore_unavailable\": true,
      \"include_global_state\": false,
      \"include_aliases\": false,
      \"rename_pattern\": \"(.+)\",
      \"rename_replacement\": \"restored_\$1\"
    }"
    echo "Restoring into restored_<index>. Check with: $0 $($LOCAL && echo --local) status"
    ;;
  restore-in-place)
    require "${2:-}" "restore-in-place <snapshot> <indices>"
    require "${3:-}" "restore-in-place <snapshot> <indices> (name the indices explicitly)"
    es POST "/_snapshot/$REPOSITORY/$2/_restore" "{\"indices\":\"$3\",\"include_global_state\":false,\"include_aliases\":false}"
    ;;
  delete)
    require "${2:-}" "delete <snapshot>"
    es DELETE "/_snapshot/$REPOSITORY/$2"
    ;;
  volume)
    require "${2:-}" "volume <label>"
    if $LOCAL; then echo "ERROR: volume snapshots are OpenShift only." >&2; exit 1; fi
    for pvc in $(oc get pvc -n "$NS" -o name | grep '/data-elastic-' | cut -d/ -f2); do
      cat <<EOF | oc apply -n "$NS" -f -
apiVersion: snapshot.storage.k8s.io/v1
kind: VolumeSnapshot
metadata:
  name: $pvc-$2
  labels:
    part-of: tno
    component: elastic
    snapshot-label: "$2"
spec:
  volumeSnapshotClassName: netapp
  source:
    persistentVolumeClaimName: $pvc
EOF
    done
    oc get volumesnapshot -n "$NS" -l "snapshot-label=$2"
    ;;
  volume-list)
    oc get volumesnapshot -n "$NS"
    ;;
  volume-delete)
    require "${2:-}" "volume-delete <label>"
    oc delete volumesnapshot -n "$NS" -l "snapshot-label=$2"
    ;;
  *)
    # The comment block at the top of this file.
    awk 'NR > 1 && /^#/ { sub(/^# ?/, ""); print; next } NR > 1 { exit }' "$0"
    exit 1
    ;;
esac
