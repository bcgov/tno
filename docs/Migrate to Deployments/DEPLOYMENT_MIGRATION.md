# DeploymentConfig to Deployment Migration

## Overview

This document describes the migration of all OpenShift DeploymentConfigs to Kubernetes Deployments for the TNO project. This migration was necessary because DeploymentConfigs are deprecated in OpenShift and the standard Kubernetes Deployment resource is the recommended approach going forward.

## Migration Status

- **Dev Environment**: ✅ COMPLETE (December 2025)
- **Test Environment**: 🔄 READY (Preparation complete)
- **Prod Environment**: ⏳ PENDING (Awaiting test validation)

## What Was Done

### 1. File Renaming (32 files)

All existing `deploy.yaml` files were renamed to `deployConfig.yaml` to preserve the original DeploymentConfig definitions:

```
openshift/kustomize/*/base/deploy.yaml → openshift/kustomize/*/base/deployConfig.yaml
```

This ensures we have a backup of the original configurations.

### 2. Deployment Conversion (32 files)

New `deploy.yaml` files were created as Kubernetes Deployments with the following changes:

#### API Version & Kind

- **Before**: `kind: DeploymentConfig`, `apiVersion: apps.openshift.io/v1`
- **After**: `kind: Deployment`, `apiVersion: apps/v1`

#### Selector Format

- **Before**:
  ```yaml
  selector:
    part-of: tno
    component: api
  ```
- **After**:
  ```yaml
  selector:
    matchLabels:
      part-of: tno
      component: api
  ```

#### Strategy

- **Before**:
  ```yaml
  strategy:
    type: Rolling
    rollingParams:
      maxSurge: 25%
      maxUnavailable: 25%
  ```
- **After**:
  ```yaml
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 25%
      maxUnavailable: 25%
  ```

#### Triggers Removed

The `triggers` section was removed from the spec, but trigger functionality was preserved (see below).

#### Test Field Removed

The `test: false` field was removed as it's not applicable to Deployments.

### 3. Trigger Preservation (28 services)

**Critical**: DeploymentConfigs used triggers to automatically redeploy when ImageStreamTags were updated. This functionality was preserved by converting triggers to annotations.

Services with automatic rollout triggers now have the `image.openshift.io/triggers` annotation:

```yaml
metadata:
  annotations:
    image.openshift.io/triggers: '[{"from": {"kind": "ImageStreamTag", "name": "api:dev", "namespace": "9b301c-tools"}, "fieldPath": "spec.template.spec.containers[?(@.name==\"api\")].image"}]'
```

#### Services with Trigger Annotations (28 total):

- api-services
- app/editor
- app/subscriber
- charts-api
- corenlp
- nginx
- nginx-reverse-proxy (not currently used)
- oracle (not currently used)
- tools/psql (only used in dev)
- services/ches-retry
- services/content (content-service pod)
- services/content-historic (not currently used)
- services/contentmigration-current (not currently used)
- services/contentmigration-historic (not currently used)
- services/event-handler (event-handler-service pod)
- services/extract-quotes
- services/ffmpeg
- services/filemonitor
- services/folder-collection
- services/image
- services/indexing
- services/nlp
- services/notification
- services/reporting
- services/scheduler
- services/syndication
- services/transcription

#### Services Without ImageChange Triggers (4 total):

These services only had ConfigChange triggers and don't need annotations:

- kafka/kowl
- postgres/crunchy
- tekton
- tools/indexer

### 4. Kustomization Updates (79 files)

All kustomization overlay files were updated:

- Changed target kind from `DeploymentConfig` to `Deployment`
- Removed patches targeting `/spec/triggers` paths (no longer applicable)

Files updated in:

```
openshift/kustomize/*/overlays/{dev,test,prod}/kustomization.yaml
```

### 5. Production Configuration Sync

After comparing production pod YAMLs with the new Deployment files, the following configuration drift was identified and corrected:

#### Missing Environment Variables Added

**api-services** (3 variables):

```yaml
- name: Elastic__ContentIndex
  valueFrom:
    configMapKeyRef:
      name: indexing-service-cloud
      key: CONTENT_INDEX
- name: Elastic__PublishedIndex
  valueFrom:
    configMapKeyRef:
      name: indexing-service-cloud
      key: PUBLISHED_INDEX
- name: Elastic__ApiKey
  valueFrom:
    secretKeyRef:
      name: elastic-cloud
      key: API_KEY
```

**api** (1 variable):

```yaml
- name: Kafka__Consumer__GroupId
  value: api-validator
```

**indexing-service** (2 variables):

```yaml
- name: Kafka__Producer__ClientId
  valueFrom:
    configMapKeyRef:
      name: indexing-service
      key: KAFKA_CLIENT_ID
- name: Kafka__Producer__BootstrapServers
  valueFrom:
    configMapKeyRef:
      name: services
      key: KAFKA_BOOTSTRAP_SERVERS
```

**reporting-service** (1 variable):

```yaml
- name: Service__UseMailMerge
  value: 'false'
```

#### Naming Inconsistencies Fixed

**transcription-service**:

- Changed `S3_*` (single underscore) to `S3__*` (double underscore) to match .NET configuration format:
  - `S3_ACCESS_KEY` → `S3__AccessKey`
  - `S3_SECRET_KEY` → `S3__SecretKey`
  - `S3_BUCKET_NAME` → `S3__BucketName`
  - `S3_SERVICE_URL` → `S3__ServiceUrl`
- Changed `Logging__LogLevel__TNO` → `Serilog__MinimumLevel__Override__TNO`

#### Resource Limits Updated

**folder-collection-service**:

```yaml
resources:
  limits:
    cpu: 75m # was 50m
    memory: 150Mi # unchanged
  requests:
    cpu: 20m # was 25m
    memory: 100Mi # was 80Mi
```

## How Automatic Deployments Work

### ImageStream Tagging Per Environment

The trigger annotations reference ImageStreamTags in the `9b301c-tools` namespace with environment-specific tags:

- **Dev**: `image-name:dev`
- **Test**: `image-name:test`
- **Prod**: `image-name:prod`

When an ImageStreamTag is updated (e.g., via CI/CD pipeline), OpenShift automatically:

1. Detects the change via the `image.openshift.io/triggers` annotation
2. Updates the container image in the Deployment spec
3. Triggers a rolling update of the pods

### Example Workflow

1. CI/CD pipeline builds a new image
2. Pipeline tags the image: `oc tag api:latest api:dev -n 9b301c-tools`
3. OpenShift detects the tag change
4. Deployment in dev namespace automatically updates
5. Pods roll out with the new image

## Replica Configuration

Base `deploy.yaml` files have `replicas: 1` by default. Production replica counts are managed via Kustomize overlays:

```yaml
# openshift/kustomize/*/overlays/prod/kustomization.yaml
patches:
  - target:
      kind: Deployment
      name: service-name
    patch: |-
      - op: replace
        path: /spec/replicas
        value: 3
```

### Production Replica Counts

- api-services: 3
- content-service: 3
- editor: 3
- event-handler: 3
- extract-quotes: 3
- ffmpeg: 3
- folder-collection: 3
- indexing: 4
- nginx: 3
- notification: 6
- reporting: 3
- transcription: 6

## Verification Steps

To verify the migration was successful:

1. **Check Deployment status**:

   ```bash
   oc get deployments -n <namespace>
   ```

2. **Verify trigger annotations**:

   ```bash
   oc get deployment <name> -n <namespace> -o jsonpath='{.metadata.annotations.image\.openshift\.io/triggers}'
   ```

3. **Test automatic rollout**:

   ```bash
   # Tag a new image
   oc tag <image>:latest <image>:dev -n 9b301c-tools

   # Watch for automatic rollout
   oc rollout status deployment/<name> -n <namespace>
   ```

4. **Compare with production**:

   ```bash
   # Get current production config
   oc get deployment <name> -n 9b301c-prod -o yaml > prod-current.yaml

   # Compare with new deployment
   diff prod-current.yaml openshift/kustomize/*/base/deploy.yaml
   ```

## Files Modified

### Created/Modified

- 32 new `deploy.yaml` files (Deployment format)
- 32 renamed `deployConfig.yaml` files (backup of originals)
- 79 updated `kustomization.yaml` files in overlays

### Temporary Files (Cleaned Up)

- `convert_deployments.py`
- `convert_deployments_v2.py`
- `convert_deployments_v3.py`
- `convert_final.py`
- `convert_with_triggers.py`
- `add_missing_triggers.py`
- `remove_trigger_patches.py`
- `verify_production_configs.py`
- `production_config_analysis.md`

## Important Notes

1. **Backwards Compatibility**: The old `deployConfig.yaml` files are preserved and can be used to roll back if needed.

2. **Environment Variables**: All environment variables now match production. Any future changes should be made to the base `deploy.yaml` files or via Kustomize overlays.

3. **Resource Limits**: Resource limits now match production values. Adjust via Kustomize overlays for environment-specific needs.

4. **Trigger Functionality**: The automatic rollout behavior is identical to DeploymentConfigs. No changes to CI/CD pipelines are required.

5. **Testing**: Thoroughly test in dev and test environments before deploying to production.

## Rollback Procedure

If issues arise, you can roll back to DeploymentConfigs:

1. Rename files:

   ```bash
   mv deploy.yaml deploy-new.yaml
   mv deployConfig.yaml deploy.yaml
   ```

2. Update kustomization.yaml files to target `DeploymentConfig` instead of `Deployment`

3. Apply the changes:
   ```bash
   oc apply -k openshift/kustomize/<service>/overlays/<env>
   ```

## Lessons Learned from Dev Deployment

### Issue 1: Missing Image Specification

**Problem**: Base `deploy.yaml` files had `image: ""` (empty string), causing deployment failures.

**Error**:

```
The Deployment "extract-quotes-service" is invalid: spec.template.spec.containers[0].image: Required value
```

**Solution**: Set a default image in base `deploy.yaml`:

```yaml
image: image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/<service-name>:latest
```

Then override per environment in overlays using patches:

```yaml
patches:
  - target:
      kind: Deployment
      name: service-name
    patch: |-
      - op: replace
        path: /spec/template/spec/containers/0/image
        value: image-registry.apps.silver.devops.gov.bc.ca/9b301c-tools/<service-name>:latest
```

### Issue 2: Secret Generation vs. Existing Secrets

**Problem**: Kustomize `secretGenerator` tried to create secrets from local `.env` files that don't exist in the repository (gitignored).

**Error**:

```
error: loading KV pairs: env source files: [llm-api-keys.env]: evalsymlink failure
```

**Solution**: Remove `secretGenerator` sections from overlays when secrets already exist in the cluster. The Deployment references existing secrets via `secretKeyRef`, which works correctly:

```yaml
# REMOVE this from kustomization.yaml:
secretGenerator:
  - name: llm-api-keys
    type: Opaque
    env: llm-api-keys.env

# The deploy.yaml already references the existing secret correctly:
env:
  - name: Service__PrimaryApiKeys
    valueFrom:
      secretKeyRef:
        name: llm-api-keys # This secret already exists in cluster
        key: PRIMARY_API_KEYS
```

## Command Format Clarification

Both command formats work for deploying with Kustomize:

```bash
# Method 1: Direct apply (recommended for updates)
oc apply -k openshift/kustomize/<service>/overlays/<env>

# Method 2: Pipe to apply (useful for reviewing first)
oc kustomize openshift/kustomize/<service>/overlays/<env> | oc apply -f -

# Method 3: Pipe to create (only for initial creation)
oc kustomize openshift/kustomize/<service>/overlays/<env> | oc create -f -
```

**Use `oc apply`** instead of `oc create` because:

- `apply` creates OR updates resources (idempotent)
- `create` fails if resources already exist
- `apply` is safe to run multiple times

## Successfully Deployed Services

### Dev Environment - COMPLETE ✅

**All 24 services migrated successfully:**

**Category 1: Stateless Services (10)**

- nginx
- editor
- subscriber
- charts-api
- api-services
- corenlp
- nlp-service
- ffmpeg-service
- extract-quotes-service
- transcription-service

**Category 2: Kafka Consumers - Stateless (7)**

- content-service
- folder-collection-service
- indexing-service
- event-handler-service
- notification-service
- reporting-service
- ches-retry-service

**Category 2: Kafka Consumers - Single Instance (4)**

- scheduler-service
- filemonitor-service
- syndication-service
- image-service

**Category 3: Supporting Services (4)**

- psql
- kowl
- nginx-editor
- nginx-subscriber

**Migration Method:**

```bash
./tools/scripts/migrate-deployments.sh dev
```

**Duration:** ~2 hours (including fixes and validation)

**Validation:**

- ✅ All pods running and healthy
- ✅ All routes accessible
- ✅ No duplicate processing detected
- ✅ Application functionality verified
- ✅ Old DeploymentConfigs scaled to 0 (kept for rollback)

### Test Environment - READY 🔄

**Preparation Complete:**

- ✅ `.env` files extracted from cluster
- ✅ Deployment YAMLs generated
- ✅ Kustomize overlays updated with correct resource limits
- ✅ Image patches added for all services

**Ready to migrate:**

```bash
./tools/scripts/migrate-deployments.sh test
```

### Production Environment - PENDING ⏳

**Prerequisites:**

- ⏳ Test environment validation (48 hours minimum)
- ⏳ Change management approval
- ⏳ Maintenance window scheduled

## Next Steps

1. ✅ Deploy all services to dev environment - COMPLETE
2. ✅ Verify functionality and fix issues - COMPLETE
3. 🔄 Deploy to test environment - READY
4. ⏳ Integration and performance testing in test
5. ⏳ Deploy to production with appropriate change management
6. ⏳ After successful production deployment, archive `deployConfig.yaml` files
