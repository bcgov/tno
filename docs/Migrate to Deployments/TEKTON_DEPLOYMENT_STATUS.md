# Tekton Pipeline Deployment Status

## Summary

Comparison between live Tekton resources in OpenShift and local files for Deployment migration.

## Files to Deploy

### ✅ Already Created (Ready to Deploy)

| File                                                                    | Status   | Notes                                                     |
| ----------------------------------------------------------------------- | -------- | --------------------------------------------------------- |
| `openshift/kustomize/tekton/base/pipelines/buildah-all-deployment.yaml` | ✅ Ready | Matches live structure, uses `deploy-all-deployment` task |
| `openshift/kustomize/tekton/base/tasks/deploy-all-deployment.yaml`      | ✅ Ready | Uses `deployment` types instead of `dc`                   |
| `openshift/templates/tekton/pipelines/backup-database-deployment.yaml`  | ✅ Ready | Uses `oc-backup-deployment` task                          |
| `openshift/templates/tekton/tasks/oc-backup-deployment.yaml`            | ✅ Ready | Uses pod selector instead of `dc/`                        |
| `openshift/templates/tekton/tasks/oc-deploy-with-tag-deployment.yaml`   | ✅ Ready | Handles `deployment` type                                 |

### 📋 Comparison with Live

#### 1. buildah-all Pipeline

**Live:** Uses `deploy-all` task (DeploymentConfig)
**New:** Uses `deploy-all-deployment` task (Deployment)
**Differences from live:**

- ✅ Same structure (no db-migration, no OWASP)
- ✅ Same maintenance mode services (`nginx`, `nginx-editor`, `nginx-subscriber`)
- ✅ Same default URLs
- ✅ Only difference: task name

#### 2. deploy-all Task

**Live:** All components use `type]="dc"` (DeploymentConfig)
**New:** All components use `type]="deployment"` (Deployment)
**Key changes:**

- API: `type]="sts"` → stays `sts` (StatefulSet)
- All others: `type]="dc"` → `type]="deployment"`
- Replicas match live values
- Component list matches live (including `indexing-service-cloud`, `ches-retry-service`)

#### 3. backup-database Pipeline

**Live:** Uses `oc-backup` task with `dc/` syntax
**New:** Uses `oc-backup-deployment` task with pod selector
**Differences:**

- Old: `oc rsh -n $(params.PROJECT_NAMESPACE) dc/$(params.DEPLOYMENT_CONFIG)`
- New: `oc rsh -n $(params.PROJECT_NAMESPACE) deployment/$(params.DEPLOYMENT_NAME)`

#### 4. oc-deploy-with-tag Task

**Live:** Supports `dc` and `sts`
**New:** Supports `dc`, `sts`, and `deployment`
**Changes:**

- Added `deployment` handling (same as `sts` - requires rollout restart)

## Deployment Commands

### Deploy All New Tekton Resources

```bash
# Deploy to tools namespace
oc apply -n 9b301c-tools \
  -f openshift/kustomize/tekton/base/tasks/deploy-all-deployment.yaml \
  -f openshift/kustomize/tekton/base/pipelines/buildah-all-deployment.yaml \
  -f openshift/templates/tekton/pipelines/backup-database-deployment.yaml \
  -f openshift/templates/tekton/tasks/oc-backup-deployment.yaml \
  -f openshift/templates/tekton/tasks/oc-deploy-with-tag-deployment.yaml
```

### Verify Deployment

```bash
# Check pipelines
oc get pipelines -n 9b301c-tools | grep deployment

# Check tasks
oc get tasks -n 9b301c-tools | grep deployment

# Expected output:
# buildah-all-deployment
# deploy-all-deployment
# backup-database-deployment
# oc-backup-deployment
# oc-deploy-with-tag-deployment
```

## Testing

### Test buildah-all-deployment Pipeline

```bash
tkn pipeline start buildah-all-deployment \
  --param DEPLOY_TO=dev \
  --param COMPONENT="editor" \
  --workspace name=source,claimName=repo \
  --workspace name=conditions,claimName=repo \
  --workspace name=build,emptyDir="" \
  --workspace name=owasp-settings,emptyDir="" \
  -n 9b301c-tools
```

### Test backup-database-deployment Pipeline

```bash
tkn pipeline start backup-database-deployment \
  --param PROJECT_NAMESPACE=9b301c-dev \
  --param DEPLOYMENT_NAME=backup \
  --param ARGS="-1" \
  -n 9b301c-tools
```

## Migration Strategy

1. ✅ **Phase 1: Deploy new resources** (current)

   - Deploy all `-deployment` versions alongside existing resources
   - Both old (DC) and new (Deployment) pipelines coexist

2. **Phase 2: Test in dev**

   - Run `buildah-all-deployment` pipeline in dev
   - Verify all services deploy correctly
   - Test backup pipeline

3. **Phase 3: Migrate test/prod**

   - Run in test environment
   - Validate thoroughly
   - Run in prod with maintenance window

4. **Phase 4: Cleanup** (future)
   - Once stable, optionally remove old DC-based pipelines
   - Update default pipelines in UI/triggers

## Notes

- All new resources have `-deployment` suffix to avoid conflicts
- Original pipelines remain unchanged and functional
- Can switch between DC and Deployment pipelines as needed
- No downtime required for deployment of new pipelines
