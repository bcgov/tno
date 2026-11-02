#!/bin/bash
# Roll the Elasticsearch StatefulSet onto its current pod template one node at a time, the way
# Elasticsearch requires for a rolling restart or a rolling upgrade. See ../UPGRADE-8.19.21.md.
#
#   NS=9b301c-dev scripts/rolling-update.sh hold     Pause rollouts (partition = replicas) BEFORE applying a change.
#   NS=9b301c-dev scripts/rolling-update.sh run      Move each pod (highest ordinal first) onto the new template.
#   NS=9b301c-dev scripts/rolling-update.sh status   Show pod revisions, node versions and cluster health.
#   NS=9b301c-dev scripts/rolling-update.sh config   Apply only the ConfigMap rendered from overlays/$OVERLAY (default dev).
#                                                    Pods read it on their next restart; nothing restarts now.
#   NS=9b301c-dev scripts/rolling-update.sh restart  Rolling restart onto the same template (e.g. after `config`).
#
# For every pod `run` will:
#   1. stop replica allocation (cluster.routing.allocation.enable=primaries) and flush,
#   2. lower the StatefulSet partition so only that pod is recreated,
#   3. wait for the pod to run the new revision, become Ready and rejoin the cluster,
#   4. re-enable allocation and wait for the cluster to return to green.
# It stops at the first failure and leaves allocation as it was at that point; rerunning resumes.
set -euo pipefail

DIR="$(cd "$(dirname "$0")/.." && pwd)"
OVERLAY="${OVERLAY:-dev}"
NS="${NS:-9b301c-dev}"
STS="${STS:-elastic}"
GREEN_TIMEOUT="${GREEN_TIMEOUT:-60m}"
POD_TIMEOUT="${POD_TIMEOUT:-900}"

replicas="$(oc get statefulset "$STS" -n "$NS" -o jsonpath='{.spec.replicas}')"

# es <pod> <METHOD> <path> [body] - call Elasticsearch through a pod that is not being restarted.
es() {
  local pod="$1" method="$2" path="$3" body="${4:-}"
  local args=(curl -sS -m 3700 -X "$method" -H 'Content-Type: application/json' "http://localhost:9200$path")
  [ -n "$body" ] && args+=(-d "$body")
  oc exec -n "$NS" "$pod" -c elastic -- "${args[@]}"
  echo
}

set_partition() {
  oc patch statefulset "$STS" -n "$NS" --type merge \
    -p "{\"spec\":{\"updateStrategy\":{\"type\":\"RollingUpdate\",\"rollingUpdate\":{\"partition\":$1}}}}" >/dev/null
}

status() {
  local update
  update="$(oc get statefulset "$STS" -n "$NS" -o jsonpath='{.status.updateRevision}')"
  echo "StatefulSet $STS: partition=$(oc get statefulset "$STS" -n "$NS" -o jsonpath='{.spec.updateStrategy.rollingUpdate.partition}') updateRevision=$update"
  oc get pods -n "$NS" -l statefulset=elastic-cluster \
    -o custom-columns='POD:.metadata.name,REVISION:.metadata.labels.controller-revision-hash,READY:.status.containerStatuses[0].ready,IMAGE:.spec.containers[0].image'
  local via
  via="$(oc get pods -n "$NS" -l statefulset=elastic-cluster -o jsonpath='{.items[0].metadata.name}')"
  es "$via" GET '/_cat/nodes?v&h=name,version,node.role,master&s=name'
  es "$via" GET '/_cluster/health?filter_path=status,number_of_nodes,relocating_shards,initializing_shards,unassigned_shards'
}

wait_for_pod() {
  local pod="$1" update revision ready deadline=$((SECONDS + POD_TIMEOUT))
  update="$(oc get statefulset "$STS" -n "$NS" -o jsonpath='{.status.updateRevision}')"
  while true; do
    revision="$(oc get pod "$pod" -n "$NS" -o jsonpath='{.metadata.labels.controller-revision-hash}' 2>/dev/null || true)"
    ready="$(oc get pod "$pod" -n "$NS" -o jsonpath='{.status.containerStatuses[0].ready}' 2>/dev/null || true)"
    if [ "$revision" = "$update" ] && [ "$ready" = "true" ]; then return 0; fi
    if [ $SECONDS -ge $deadline ]; then
      echo "ERROR: $pod did not become Ready on revision $update within ${POD_TIMEOUT}s." >&2
      oc describe pod "$pod" -n "$NS" | tail -20 >&2
      oc logs "$pod" -n "$NS" -c elastic --tail=40 >&2 || true
      return 1
    fi
    sleep 10
  done
}

case "${1:-}" in
  config)
    # Render the overlay and keep only the ConfigMap document.
    oc kustomize "$DIR/overlays/$OVERLAY" \
      | awk 'BEGIN { RS = "\n---\n" } /(^|\n)kind: ConfigMap\n/ { print "---"; print }' \
      | oc apply -n "$NS" -f -
    ;;
  restart)
    set_partition "$replicas"
    oc patch statefulset "$STS" -n "$NS" --type merge \
      -p "{\"spec\":{\"template\":{\"metadata\":{\"annotations\":{\"kubectl.kubernetes.io/restartedAt\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"}}}}}" >/dev/null
    exec "$0" run
    ;;
  hold)
    set_partition "$replicas"
    echo "Rollouts paused (partition=$replicas). Apply the change now, then run: $0 run"
    ;;
  run)
    update="$(oc get statefulset "$STS" -n "$NS" -o jsonpath='{.status.updateRevision}')"
    for ((n = replicas - 1; n >= 0; n--)); do
      pod="$STS-$n"
      via="$STS-$(( n == 0 ? 1 : 0 ))"
      if [ "$(oc get pod "$pod" -n "$NS" -o jsonpath='{.metadata.labels.controller-revision-hash}')" = "$update" ]; then
        # Already moved (e.g. a resumed run): still make sure it rejoined and the cluster recovered.
        echo "== $pod already runs revision $update"
        set_partition "$n"
      else
        echo "== $pod: stop replica allocation and flush (via $via)"
        es "$via" PUT /_cluster/settings '{"persistent":{"cluster.routing.allocation.enable":"primaries"}}'
        es "$via" POST /_flush
        echo "== $pod: recreate on revision $update"
        set_partition "$n"
        wait_for_pod "$pod"
      fi
      # Capture output before matching: `grep -q` in a pipeline exits early, and with pipefail the
      # resulting SIGPIPE makes the whole pipeline look like a failure.
      until nodes="$(es "$via" GET "/_cat/nodes?h=name")" && [[ $'\n'"$nodes"$'\n' == *$'\n'"$pod"$'\n'* ]]; do sleep 5; done
      echo "== $pod: in the cluster; re-enable allocation and wait for green (timeout $GREEN_TIMEOUT)"
      es "$via" PUT /_cluster/settings '{"persistent":{"cluster.routing.allocation.enable":null}}'
      health="$(es "$via" GET "/_cluster/health?wait_for_status=green&wait_for_no_initializing_shards=true&wait_for_no_relocating_shards=true&timeout=$GREEN_TIMEOUT&filter_path=status,timed_out,unassigned_shards")"
      echo "$health"
      if [[ "$health" != *'"status":"green"'* ]]; then
        # While versions are mixed, replicas of a primary on an upgraded node cannot be allocated to an
        # older node, so yellow is expected until the last node is upgraded (all primaries are assigned).
        versions="$(es "$via" GET '/_cat/nodes?h=version' | sort -u | awk 'NF { n++ } END { print n + 0 }')"
        if [[ "$health" == *'"status":"yellow"'* ]] && [ "$versions" -gt 1 ]; then
          echo "WARNING: cluster is yellow while node versions are mixed (expected during an upgrade); continuing."
        else
          echo "ERROR: cluster is not green after $pod. Investigate before continuing (rerun resumes)." >&2
          exit 1
        fi
      fi
    done
    echo "All pods run revision $update."
    status
    ;;
  status)
    status
    ;;
  *)
    sed -n '2,18p' "$0" | sed 's/^# \{0,1\}//'
    exit 1
    ;;
esac
