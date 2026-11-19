#!/usr/bin/env bash
# Shared by GitHub Actions and the manual Tekton release. Requires only Bash and oc.
set -euo pipefail
namespace=${1:?namespace required}
kind=${2:?kind required}
name=${3:?workload required}
timeout=${4:-600s}
image=${5:-}
container=${6:-$name}
resource="$kind/$name"

pod_selector() {
  local selector
  selector=$(oc get "$resource" -n "$namespace" -o 'go-template={{range $key, $value := .spec.selector.matchLabels}}{{$key}}={{$value}},{{end}}')
  selector=${selector%,}
  [[ -n "$selector" ]] || { echo "$resource has no pod selector" >&2; return 1; }
  printf '%s' "$selector"
}

if [[ "$kind" == statefulset ]]; then
  # The API cannot overlap old/new instances. Preserve its configured capacity.
  replicas=$(oc get "$resource" -n "$namespace" -o 'jsonpath={.spec.replicas}')
  (( replicas > 0 )) || { echo "$resource has no desired replicas; cannot verify API readiness" >&2; exit 1; }
  selector=$(pod_selector)
  oc scale "$resource" -n "$namespace" --replicas=0
  if [[ -n $(oc get pods -n "$namespace" -l "$selector" -o name) ]]; then
    oc wait pod -n "$namespace" -l "$selector" --for=delete --timeout="$timeout"
  fi
  # Leave it stopped on failure: never start downstream services after a partial API update.
  if [[ -n "$image" ]]; then
    oc set image "$resource" "$container=$image" -n "$namespace"
  fi
  oc scale "$resource" -n "$namespace" --replicas="$replicas"
  # Works for both OnDelete and RollingUpdate StatefulSets after a full stop/start.
  seconds=${timeout%s}
  deadline=$((SECONDS + seconds))
  query_errors=$(mktemp)
  trap 'rm -f "$query_errors"' EXIT
  while :; do
    request_seconds=$((deadline - SECONDS))
    (( request_seconds <= 10 )) || request_seconds=10
    (( request_seconds > 0 )) || request_seconds=1
    if status=$(oc get "$resource" -n "$namespace" --request-timeout="${request_seconds}s" -o 'jsonpath={.metadata.generation}{"|"}{.status.observedGeneration}{"|"}{.status.readyReplicas}{"|"}{.status.currentRevision}{"|"}{.status.updateRevision}' 2>"$query_errors"); then
      cat "$query_errors" >&2
      IFS='|' read -r generation observed ready current_revision update_revision <<< "$status"
      if (( ${observed:-0} >= ${generation:-1} && ${ready:-0} == replicas )) &&
        [[ -n "$update_revision" && "$current_revision" == "$update_revision" ]]; then
        break
      fi
    else
      # A failed observation does not mean the workload failed. Retry transport
      # errors without replaying the stop/start, but keep permanent errors fatal.
      error=$(cat "$query_errors")
      printf '%s\n' "$error" >&2
      case "$error" in
        *Forbidden*|*Unauthorized*|*NotFound*|*'x509:'*) exit 1 ;;
        *'i/o timeout'*|*'TLS handshake timeout'*|*'connection reset by peer'*|*'connection refused'*|*'context deadline exceeded'*|*'Client.Timeout'*|*'unexpected EOF'*)
          echo "$resource readiness query lost connectivity; retrying within ${timeout}." >&2 ;;
        *) exit 1 ;;
      esac
    fi
    (( SECONDS < deadline )) || { echo "$resource readiness timed out" >&2; exit 1; }
    delay=$((deadline - SECONDS))
    (( delay <= 5 )) || delay=5
    sleep "$delay"
  done
elif [[ "$kind" == deployment ]]; then
  if [[ "$name" == api-services ]]; then
    replicas=$(oc get "$resource" -n "$namespace" -o 'jsonpath={.spec.replicas}')
    (( replicas > 0 )) || { echo "$resource has no desired replicas; cannot verify API readiness" >&2; exit 1; }
  fi
  # Merge patch removes Recreate and preserves replica counts and other settings.
  oc patch "$resource" -n "$namespace" --type=merge \
    -p '{"spec":{"strategy":{"type":"RollingUpdate","rollingUpdate":{"maxSurge":1,"maxUnavailable":0}}}}'
  if [[ -n "$image" ]]; then
    oc set image "$resource" "$container=$image" -n "$namespace"
  fi
  oc rollout restart "$resource" -n "$namespace"
  oc rollout status "$resource" -n "$namespace" --timeout="$timeout"
else
  echo "Unsupported workload kind: $kind" >&2
  exit 1
fi
oc get "$resource" -n "$namespace" -o wide
# Print the resolved running digests as release evidence, not just mutable tag names.
selector=$(pod_selector)
oc get pods -n "$namespace" -l "$selector" \
  -o 'custom-columns=NAME:.metadata.name,READY:.status.containerStatuses[*].ready,IMAGE:.status.containerStatuses[*].imageID'
