#!/usr/bin/env bash
# Shared by GitHub Actions and the manual Tekton release.
set -euo pipefail
namespace=${1:?namespace required}
kind=${2:?kind required}
name=${3:?workload required}
timeout=${4:-600s}
image=${5:-}
container=${6:-$name}
resource="$kind/$name"

if [[ "$kind" == statefulset ]]; then
  # The API cannot overlap old/new instances. Preserve its configured capacity.
  workload=$(oc get "$resource" -n "$namespace" -o json)
  replicas=$(jq -r '.spec.replicas' <<< "$workload")
  (( replicas > 0 )) || { echo "$resource has no desired replicas; cannot verify API readiness" >&2; exit 1; }
  selector=$(jq -r '.spec.selector.matchLabels | to_entries | map(.key + "=" + .value) | join(",")' <<< "$workload")
  [[ -n "$selector" ]] || { echo 'Missing StatefulSet pod selector' >&2; exit 1; }
  oc scale "$resource" -n "$namespace" --replicas=0
  if [[ $(oc get pods -n "$namespace" -l "$selector" -o json | jq '.items | length') -gt 0 ]]; then
    oc wait pod -n "$namespace" -l "$selector" --for=delete --timeout="$timeout"
  fi
  # Leave it stopped on failure: never start downstream services after a partial API update.
  if [[ -n "$image" ]]; then
    oc set image "$resource" "$container=$image" -n "$namespace"
  fi
  oc scale "$resource" -n "$namespace" --replicas="$replicas"
  if (( replicas > 0 )); then
    # Works for both OnDelete and RollingUpdate StatefulSets after a full stop/start.
    seconds=${timeout%s}
    deadline=$((SECONDS + seconds))
    while :; do
      status=$(oc get "$resource" -n "$namespace" -o json)
      if jq -e --argjson replicas "$replicas" '
        .status.observedGeneration >= .metadata.generation and
        .status.readyReplicas == $replicas and
        .status.currentRevision == .status.updateRevision' <<< "$status" >/dev/null; then
        break
      fi
      (( SECONDS < deadline )) || { echo "$resource readiness timed out" >&2; exit 1; }
      sleep 5
    done
  fi
elif [[ "$kind" == deployment ]]; then
  if [[ "$name" == api-services ]]; then
    replicas=$(oc get "$resource" -n "$namespace" -o json | jq -r '.spec.replicas')
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
selector=$(oc get "$resource" -n "$namespace" -o json | jq -r '.spec.selector.matchLabels | to_entries | map(.key + "=" + .value) | join(",")')
oc get pods -n "$namespace" -l "$selector" \
  -o 'custom-columns=NAME:.metadata.name,READY:.status.containerStatuses[*].ready,IMAGE:.status.containerStatuses[*].imageID'
