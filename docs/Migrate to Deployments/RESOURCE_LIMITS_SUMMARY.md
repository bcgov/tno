# Resource Limits Summary - Production

## Overview

Applied recommended resource limits to production services based on current resource requests with appropriate buffers for burst capacity and growth.

## Methodology

- **CPU Limits:** 2.5x current requests (allows burst capacity)
- **Memory Limits:**
  - 1.5x for stable services
  - 2.0x for variable workload services (transcription, ffmpeg, indexing)

## Applied Limits

| Service               | CPU Request | CPU Limit | Memory Request | Memory Limit | Notes                               |
| --------------------- | ----------- | --------- | -------------- | ------------ | ----------------------------------- |
| content-service       | 25m         | **63m**   | 150Mi          | **225Mi**    | 1.5x multiplier                     |
| indexing-service      | 20m         | **50m**   | 100Mi          | **200Mi**    | 2.0x multiplier (variable workload) |
| transcription-service | 20m         | **50m**   | 80Mi           | **160Mi**    | 2.0x multiplier (variable workload) |
| reporting-service     | 20m         | **50m**   | 350Mi          | **525Mi**    | 1.5x multiplier                     |
| notification-service  | 20m         | **50m**   | 350Mi          | **525Mi**    | 1.5x multiplier                     |
| ffmpeg-service        | 20m         | **50m**   | 100Mi          | **200Mi**    | 2.0x multiplier (variable workload) |
| ches-retry-service    | 20m         | **50m**   | 100Mi          | **150Mi**    | 1.5x multiplier                     |

## Replica Adjustments Needed

Two services have replica mismatches that should be addressed:

| Service              | Current Live | Kustomization File | Recommendation                               |
| -------------------- | ------------ | ------------------ | -------------------------------------------- |
| indexing-service     | 3            | 4                  | Update kustomization to 3 or scale live to 4 |
| notification-service | 3            | 6                  | Update kustomization to 3 or scale live to 6 |

## Rationale

### Why Add Limits?

Per OpenShift team: "At one point OpenShift removed the requirement to add limits. Which was great for us, but now they've added them back. They apply defaults if we don't do it."

### Benefits:

1. **Prevents Resource Hogging:** Services can't consume unlimited cluster resources
2. **Predictable Performance:** Limits prevent noisy neighbor problems
3. **Better Scheduling:** Kubernetes can make better placement decisions
4. **Cost Control:** Prevents unexpected resource consumption

### Conservative Approach:

- Limits are set well above current requests (1.5-2.5x)
- Allows for burst capacity and growth
- Services with variable workloads get extra headroom (2x memory)
- Can be adjusted based on actual usage patterns

## Files Modified

### Kustomization Files:

- `openshift/kustomize/services/content/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/indexing/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/transcription/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/reporting/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/notification/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/ffmpeg/overlays/prod/kustomization.yaml`
- `openshift/kustomize/services/ches-retry/overlays/prod/kustomization.yaml`

### Deployment Update Files:

All corresponding `prod-deployment-update.yaml` files regenerated with new limits.

## Verification

Run verification script:

```bash
./tools/scripts/verify-deployment-resources.sh prod
```

Expected: 7 services will show limit mismatches (this is correct - we're adding limits)

## Deployment Impact

When these deployment-update files are applied:

1. Services will have resource limits enforced
2. Pods may be rescheduled if current usage exceeds new limits (unlikely given generous buffers)
3. OpenShift will no longer apply default limits

## Monitoring Recommendations

After deployment, monitor for:

1. **CPU throttling:** If services hit CPU limits frequently, increase limits
2. **OOM kills:** If pods are killed for exceeding memory limits, increase limits
3. **Performance degradation:** Compare response times before/after

## Rollback Plan

If issues occur:

1. Scale back to DeploymentConfigs: `oc scale dc/<service> --replicas=<original> -n 9b301c-prod`
2. Or remove limits from kustomization.yaml and redeploy
3. Or increase limits if specific service needs more resources

## Next Steps

1. ✅ Limits applied to kustomization files
2. ✅ Deployment-update files regenerated
3. ⏳ Review changes: `git diff`
4. ⏳ Test in dev/test environments first
5. ⏳ Deploy to production during maintenance window
6. ⏳ Monitor resource usage for 1-2 weeks
7. ⏳ Adjust limits based on actual usage patterns
