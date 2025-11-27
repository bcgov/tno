# Resource Limits - Environment Comparison

## Overview

Resource limits differ between dev, test, and prod environments. Kustomize overlays have been updated to match the actual running DeploymentConfigs in each environment.

## Dev Environment - COMPLETE ✅

All dev overlays updated to match running DeploymentConfigs. Migration script validates resource limits before deployment.

## Test Environment - COMPLETE ✅

**Resource Limit Differences from Dev:**

Services with higher limits in test:

- **content-service**: CPU limit 75m (dev: none), memory request 100Mi (dev: 250Mi)
- **indexing-service**: CPU limit 75m (dev: 40m)
- **transcription-service**: CPU limit 75m (dev: 50m)
- **event-handler-service**: CPU limit 75m (dev: 50m)
- **editor**: CPU limit 100m (dev: 40m)
- **subscriber**: CPU limit 100m (dev: 40m)
- **nginx**: CPU limit 50m (dev: 40m)

All test overlays have been updated with correct values.

## Production Environment - PENDING ⏳

Production likely has higher resource limits than test. Will need to:

1. Extract actual resource limits from running DCs
2. Update prod overlays to match
3. Regenerate deployment YAMLs

## How to Check Resource Limits

```bash
# Get DC resources
oc get dc <service-name> -n 9b301c-<env> -o jsonpath='{.spec.template.spec.containers[0].resources}' | jq

# Check overlay patch
grep -A 15 "resources" openshift/kustomize/<path>/overlays/<env>/kustomization.yaml

# Compare all services in an environment
for service in content-service indexing-service notification-service; do
  echo "=== $service ==="
  oc get dc $service -n 9b301c-dev -o jsonpath='{.spec.template.spec.containers[0].resources}' 2>/dev/null | jq -c '.'
done
```

## Migration Script Validation

The migration script automatically:

1. Compares DC resources with planned Deployment resources
2. Shows differences before deployment
3. Prompts for confirmation if mismatches found
4. Helps catch configuration drift early
