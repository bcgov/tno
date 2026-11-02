#!/bin/bash
# Elasticsearch snapshot operations for MMI. See ../SNAPSHOTS.md.
#
#   snapshots.sh [--local] <command> [arguments]
#
# Targets the OpenShift cluster by default (namespace $NS, default 9b301c-dev; `oc login` first).
# --local targets the docker-compose container ($LOCAL_CONTAINER, default tno-elastic) and reads
# ELASTIC_PASSWORD from db/elasticsearch/.env when it is not already set.
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
RETAIN_MIN="${RETAIN_MIN:-3}"
RETAIN_MAX="${RETAIN_MAX:-14}"

LOCAL=false
if [ "${1:-}" = "--local" ]; then
  LOCAL=true
  shift
fi

if $LOCAL && [ -z "${ELASTIC_PASSWORD+x}" ]; then
  ENV_FILE="$(git rev-parse --show-toplevel 2>/dev/null || pwd)/db/elasticsearch/.env"
  if [ -f "$ENV_FILE" ]; then
    ELASTIC_PASSWORD="$(grep -E '^ELASTIC_PASSWORD=' "$ENV_FILE" | cut -d= -f2- | tr -d "'\"")"
  fi
fi

# es <METHOD> <path> [json body] - call Elasticsearch from inside the container/pod.
# Prints the response body and fails (exit 1) when Elasticsearch answers with HTTP 400 or above.
es() {
  local method="$1" path="$2" body="${3:-}" output status
  local args=(curl -sS -m 600 -w '\n%{http_code}' -X "$method" -H 'Content-Type: application/json' "http://localhost:9200$path")
  [ -n "$body" ] && args+=(-d "$body")
  if $LOCAL; then
    if [ -n "${ELASTIC_PASSWORD:-}" ]; then
      # Pass the credentials through the environment so they never appear in a process list.
      output="$(docker exec -e "ES_AUTH=elastic:$ELASTIC_PASSWORD" "$LOCAL_CONTAINER" sh -c 'exec "$@" -u "$ES_AUTH"' sh "${args[@]}")"
    else
      output="$(docker exec "$LOCAL_CONTAINER" "${args[@]}")"
    fi
  else
    output="$(oc exec -n "$NS" "$POD" -c elastic -- "${args[@]}")"
  fi
  status="${output##*$'\n'}"
  printf '%s\n' "${output%$'\n'*}"
  if [ "$status" -ge 400 ] 2>/dev/null; then
    echo "ERROR: $method $path returned HTTP $status" >&2
    return 1
  fi
}

# Wait for a snapshot to finish; fail unless it succeeded.
wait_for() {
  local name="$1" state
  while true; do
    state="$(es GET "/_snapshot/$REPOSITORY/$name?filter_path=snapshots.state" | grep -o '"state":"[A-Z_]*"' | cut -d'"' -f4 || true)"
    case "$state" in
      SUCCESS) echo "Snapshot '$name' succeeded."; return 0 ;;
      IN_PROGRESS|"") sleep 10 ;;
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
    sed -n '2,31p' "$0" | sed 's/^# \{0,1\}//'
    exit 1
    ;;
esac
