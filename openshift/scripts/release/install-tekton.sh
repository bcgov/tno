#!/usr/bin/env bash
# Reconcile the manual fallback and disable automatic Tekton push releases.
# Pass --dry-run=server to validate without changing the cluster.
set -euo pipefail
root=$(cd "$(dirname "$0")/../../.." && pwd)
base="$root/openshift/kustomize/tekton/base"
# Disable duplicate triggers first, before installing executable tasks.
for file in triggers/dev/listener.yaml triggers/test/listener.yaml \
  tasks/pipeline-wait.yaml tasks/git-conditions.yaml tasks/build-all.yaml \
  tasks/db-migration-conditional.yaml tasks/deploy-all.yaml \
  pipelines/buildah-all.yaml pipelines/deploy-all.yaml; do
  oc apply -n 9b301c-tools -f "$base/$file" "$@"
done
