# Deployment Plan: DeploymentConfig to Deployment Migration

## Overview

This document outlines the actual migration process used to migrate from DeploymentConfigs to Deployments across dev, test, and production environments.

## Migration Status

### ✅ Dev Environment - COMPLETE

- All 24 services successfully migrated
- Old DeploymentConfigs scaled to 0 (kept for rollback)
- All services running on Deployments

### 🔄 Test Environment - READY

- `.env` files extracted from cluster
- Deployment YAMLs generated
- Kustomize overlays updated with correct resource limits
- Ready to migrate

### ⏳ Prod Environment - PENDING

- Awaiting test validation

## Quick Start: Using the Migration Script

The migration script provides an automated, interactive migration process:

```bash
# Migrate all services in an environment
./src/tools/scripts/migrate-deployments.sh dev
./src/tools/scripts/migrate-deployments.sh test
./src/tools/scripts/migrate-deployments.sh prod

# Migrate a single service
./src/tools/scripts/migrate-deployments.sh dev nginx
./src/tools/scripts/migrate-deployments.sh test content-service

# The script will:
# 1. Check if DeploymentConfig exists and get current replicas
# 2. Show pre-deployment resource check (compare DC vs planned Deployment)
# 3. Deploy new Deployment from dev-deployment-update.yaml
# 4. Wait for rollout to complete
# 5. Compare YAML configurations (env vars and resources)
# 6. Show recent logs for health check
# 7. Prompt for confirmation
# 8. Scale down old DeploymentConfig (if not already at 0)
# 9. Verify final state
```

**Features:**

- ✅ Interactive prompts at each step for safety
- ✅ Pre-deployment resource limit validation
- ✅ Automatic health checks and log inspection
- ✅ YAML comparison to catch config differences
- ✅ Handles single-instance Kafka consumers (scales down DC first)
- ✅ Handles stateless services (rolling update)
- ✅ Skips unnecessary operations (e.g., scale-down if already at 0)
- ✅ Can migrate all services or just one
- ✅ Keeps old DeploymentConfigs for easy rollback

## Actual Migration Process Used

### Preparation Steps

1. **Extract secrets from cluster:**

   ```bash
   ./src/tools/scripts/extract-cluster-env-files.sh dev
   ./src/tools/scripts/extract-cluster-env-files.sh test
   ./src/tools/scripts/extract-cluster-env-files.sh prod
   ```

   - Creates `.env` files for kustomize secretGenerators
   - Prevents secrets from being overwritten during migration

2. **Generate deployment YAMLs:**

   ```bash
   ./src/tools/scripts/generate-all-deployment-yamls.sh dev
   ./src/tools/scripts/generate-all-deployment-yamls.sh test
   ./src/tools/scripts/generate-all-deployment-yamls.sh prod
   ```

   - Creates `{env}-deployment-update.yaml` files
   - Contains only Deployment, ConfigMap, and Service resources
   - Excludes Routes, Secrets, PVCs (managed separately)

3. **Fix kustomize overlays:**

   - Add missing image patches (many services had empty image fields)
   - Update resource limits to match actual running DeploymentConfigs
   - Fix ConfigMap references (e.g., ELASTIC_URIS key issues)
   - Add missing ConfigMap keys (e.g., ALWAYS_BCC, SEND_TO_ALL_BEFORE_FAILING)

4. **Regenerate deployment YAMLs after fixes:**
   ```bash
   ./src/tools/scripts/generate-all-deployment-yamls.sh dev
   ```

### Migration Execution

**Dev Environment (Completed):**

```bash
./src/tools/scripts/migrate-deployments.sh dev
```

- Migrated all 24 services in one run
- Script handled each service category appropriately:
  - **Stateless services**: Rolling update (new pods start, then old DC scaled down)
  - **Single-instance Kafka consumers**: Old DC scaled down first, then new Deployment deployed
  - **Supporting services**: Standard deployment

**Issues Encountered and Fixed:**

1. Missing image patches → Added to kustomize overlays
2. ConfigMap key mismatches → Fixed base YAML files
3. Resource limit differences → Updated overlays to match DCs
4. Volume attachment delays → Script waits appropriately
5. Missing ConfigMap keys → Added to overlay config-map.yaml files

## Service Categories

### Category 1: Stateless, Multi-Replica Safe (Blue-Green Deployable)

These services can run multiple instances simultaneously without issues. Safe for standard rolling updates.

**Services:**

- `nginx` (reverse proxy)
- `nginx-reverse-proxy`
- `src/app/editor` (frontend)
- `src/app/subscriber` (frontend)
- `charts` (API)
- `api` (API)
- `api-services` (API)
- `core-nlp` (NLP processing)
- `src/services/nlp` (NLP service)
- `src/services/image` (image processing)
- `src/services/extract-quotes` (quote extraction)
- `src/services/ffmpeg` (video processing)
- `src/services/transcription` (transcription service)

**Deployment Strategy:** Standard rolling update

- Old and new versions can coexist during rollout
- No special considerations needed

### Category 2: Kafka Consumers

These services consume from Kafka topics. **Important discovery:** Kafka partition limits prevent duplicate processing even with multiple consumers.

**Single-Instance Services (Scale Down First):**

These services require the old DC to be scaled down before the new Deployment starts:

- `src/services/scheduler` (scheduled tasks)
- `src/services/filemonitor` (file monitoring with PVC)
- `src/services/syndication` (syndication)
- `src/services/image` (image processing)

**Deployment Strategy:** Scale down first

```bash
# Migration script automatically:
1. Scales old DC to 0
2. Waits for pod termination
3. Deploys new Deployment
4. Waits for rollout
Result: Brief downtime (~30-60 seconds), clean migration
```

**Stateless Kafka Consumers (Rolling Update Safe):**

These services can safely run old and new pods simultaneously:

- `src/services/content` (content processing)
- `src/services/event-handler` (event processing)
- `src/services/notification` (notifications)
- `src/services/reporting` (report generation)
- `src/services/indexing` (Elasticsearch indexing)
- `src/services/folder-collection` (folder collection)
- `src/services/ches-retry` (email retry)

**Deployment Strategy:** Rolling update

```bash
# Migration script automatically:
1. Deploys new Deployment
2. Waits for new pods to be ready
3. Prompts for confirmation
4. Scales down old DC
Result: Zero downtime, smooth transition
```

**Why This Works:**

- Kafka partitions limit concurrent processing
- Consumer groups handle rebalancing automatically
- No duplicate processing risk
- Faster deployments for most services

### Category 3: Supporting Services

These services support the infrastructure but aren't part of the main application flow.

**Services:**

- `kafka/kowl` (Kafka UI)
- `nginx-editor` (Nginx reverse proxy for editor)
- `nginx-subscriber` (Nginx reverse proxy for subscriber)
- `src/tools/psql` (PostgreSQL CLI utility - dev only)
- `oracle` (Oracle connector - not in dev)

**Deployment Strategy:** Standard deployment

- Less critical, can be deployed with minimal risk
- `psql` has PVC attachment, may take extra time for volume to detach/attach
- `nginx-editor` and `nginx-subscriber` need image patches in kustomize overlays

### ⚠️ EXCLUDED: Operator-Managed Services (DO NOT MIGRATE)

**PostgreSQL (Crunchy)**

- ❌ **DO NOT MIGRATE** using this process
- Managed by **PostgreSQL Operator**
- Uses **StatefulSets** (not DeploymentConfigs or Deployments)
- Has **PersistentVolumeClaims** for data storage
- Migration risks:
  - Data loss
  - Orphaned volumes
  - Application-wide downtime (all services depend on DB)
- **If migration needed:** Coordinate with DBA team and Crunchy operator documentation

**Kafka Cluster**

- ❌ **DO NOT MIGRATE** using this process
- Managed by **Strimzi Operator**
- Uses **StatefulSets** for brokers
- Has **PersistentVolumeClaims** for message storage
- Migration risks:
  - Message loss
  - Consumer group corruption
  - All Kafka-dependent services affected
- **If migration needed:** Coordinate with Kafka team and Strimzi operator documentation

**Why These Are Different:**

- Operators manage the full lifecycle (creation, updates, backups, scaling)
- StatefulSets provide stable network identities and persistent storage
- Custom resources (CRDs) define the desired state
- Manual intervention can conflict with operator reconciliation loops

## Actual Deployment Process

### Dev Environment - COMPLETED ✅

**Date:** December 2024
**Method:** Single automated run using migration script
**Duration:** ~2 hours (including fixes and validation)

**Command Used:**

```bash
./src/tools/scripts/migrate-deployments.sh dev
```

**Services Migrated (in order):**

1. **Category 1: Stateless Services**

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

2. **Category 2: Kafka Consumers (Stateless)**

   - content-service
   - folder-collection-service
   - indexing-service
   - event-handler-service
   - notification-service
   - reporting-service
   - ches-retry-service

3. **Category 2: Kafka Consumers (Single-Instance)**

   - scheduler-service
   - filemonitor-service
   - syndication-service
   - image-service

4. **Category 3: Supporting Services**
   - psql
   - kowl
   - nginx-editor
   - nginx-subscriber

**Issues Fixed During Migration:**

1. **Missing Image Patches**

   - Problem: Many services had empty `image:` fields in deployment YAMLs
   - Solution: Added image patches to kustomize overlays
   - Services affected: transcription, folder-collection, content, indexing, event-handler, notification, reporting, nginx-editor, nginx-subscriber

2. **ConfigMap Key Mismatches**

   - Problem: Base YAMLs referenced wrong ConfigMap keys
   - Solution: Fixed base YAML files
   - Examples:
     - `ELASTIC_URIS` → `ELASTICSEARCH_URI` (folder-collection, notification)
     - Added missing keys: `ALWAYS_BCC`, `SEND_TO_ALL_BEFORE_FAILING`

3. **Resource Limit Differences**

   - Problem: Kustomize overlays didn't match running DeploymentConfigs
   - Solution: Updated overlays to match actual DC resources
   - Services affected: content, indexing, transcription, event-handler

4. **Volume Attachment Delays**
   - Problem: PVC-based services (filemonitor, psql) took time to attach volumes
   - Solution: Script waits appropriately, no code changes needed

**Validation:**

- ✅ All 24 services migrated successfully
- ✅ All pods running and healthy
- ✅ No duplicate processing detected
- ✅ All routes accessible
- ✅ Application functionality verified

### Test Environment - READY 🔄

**Preparation Completed:**

1. ✅ Extracted `.env` files from test cluster
2. ✅ Generated all deployment YAMLs
3. ✅ Updated kustomize overlays with correct resource limits
4. ✅ Fixed image patches for all services
5. ✅ Added kowl, nginx-editor, nginx-subscriber to migration script

**Resource Limit Differences from Dev:**

- content-service: CPU limit 75m (dev: none), memory request 100Mi (dev: 250Mi)
- indexing-service: CPU limit 75m (dev: 40m)
- transcription-service: CPU limit 75m (dev: 50m)
- event-handler-service: CPU limit 75m (dev: 50m)
- editor: CPU limit 100m (dev: 40m)
- subscriber: CPU limit 100m (dev: 40m)
- nginx: CPU limit 50m (dev: 40m)

**Ready to Migrate:**

```bash
./src/tools/scripts/migrate-deployments.sh test
```

**Expected Duration:** ~2 hours (same as dev)

**Validation Plan:**

- Integration testing
- Performance testing
- 48 hours of stable operation before prod

### Production Environment - PENDING ⏳

**Prerequisites:**

- ✅ Dev environment fully validated
- ⏳ Test environment validation (48 hours minimum)
- ⏳ Change management approval
- ⏳ Maintenance window scheduled
- ⏳ Rollback plan documented
- ⏳ Team on standby

**Preparation Steps:**

1. Extract `.env` files from prod cluster
2. Generate all deployment YAMLs
3. Review resource limits (likely higher than test)
4. Update kustomize overlays if needed
5. Regenerate deployment YAMLs

**Migration Approach:**

**Option 1: Single Automated Run (Recommended)**

```bash
./src/tools/scripts/migrate-deployments.sh prod
```

- Same process as dev and test
- Script handles all service categories appropriately
- Interactive prompts for safety
- Expected duration: ~2 hours

**Option 2: Staged Deployment**
If more caution is desired:

```bash
# Stage 1: Stateless services (off-peak hours)
./src/tools/scripts/migrate-deployments.sh prod nginx
./src/tools/scripts/migrate-deployments.sh prod editor
./src/tools/scripts/migrate-deployments.sh prod subscriber
# ... etc

# Stage 2: Kafka consumers (maintenance window)
./src/tools/scripts/migrate-deployments.sh prod content-service
./src/tools/scripts/migrate-deployments.sh prod indexing-service
# ... etc
```

**Monitoring:**

- Watch pod status continuously
- Monitor Kafka consumer lag
- Check application logs
- Verify no duplicate processing
- Monitor for 24 hours minimum

## Rollback Procedures

### For Stateless Services (Category 1)

```bash
# Quick rollback using Kubernetes
oc rollout undo deployment/<service-name> -n <namespace>

# Or revert to DeploymentConfig
oc delete deployment/<service-name> -n <namespace>
oc scale dc/<service-name> --replicas=<original-count> -n <namespace>
```

### For Kafka Consumer Services (Category 2)

```bash
# 1. Scale down new Deployment
oc scale deployment/<service-name> --replicas=0 -n <namespace>

# 2. Delete new Deployment
oc delete deployment/<service-name> -n <namespace>

# 3. Scale up old DeploymentConfig
oc scale dc/<service-name> --replicas=1 -n <namespace>

# 4. Monitor for recovery
oc logs -f dc/<service-name> -n <namespace>
```

## Monitoring During Deployment

### Key Metrics to Watch

1. **Pod Status**

   ```bash
   oc get pods -n <namespace> -w
   ```

2. **Deployment Status**

   ```bash
   oc get deployments -n <namespace>
   ```

3. **Application Logs**

   ```bash
   oc logs -f deployment/<service-name> -n <namespace>
   ```

4. **Events**

   ```bash
   oc get events -n <namespace> --sort-by='.lastTimestamp'
   ```

5. **Resource Usage**
   ```bash
   oc adm top pods -n <namespace>
   ```

### Red Flags - Rollback Immediately If:

- Pods are crash-looping (CrashLoopBackOff)
- Kafka consumer lag is growing rapidly
- Duplicate message processing detected
- Database connection errors
- Authentication failures
- 500 errors in application logs
- Memory or CPU usage spikes abnormally

## Post-Deployment Validation

### For Each Service

1. **Health Checks**

   ```bash
   # Check readiness
   oc get pods -l component=<service-name> -n <namespace>

   # All pods should be Running and Ready (1/1)
   ```

2. **Route Testing**

   ```bash
   # Get route
   oc get route <service-name> -n <namespace>

   # Test endpoint
   curl -I https://<route-url>/health
   ```

3. **Log Review**

   ```bash
   # Check for errors in last 10 minutes
   oc logs deployment/<service-name> -n <namespace> --since=10m | grep -i error
   ```

4. **Kafka Consumer Validation** (for Category 2 services)
   - Check consumer group lag
   - Verify messages are being processed
   - Confirm no duplicate processing
   - Monitor for 24 hours minimum

## Success Criteria

### Dev Environment

- ✅ All services deployed successfully
- ✅ All routes accessible
- ✅ Authentication working
- ✅ Basic functionality validated
- ✅ No errors in logs
- ✅ Kafka consumers processing messages

### Test Environment

- ✅ All dev criteria met
- ✅ Integration tests passing
- ✅ Performance acceptable
- ✅ No regression issues
- ✅ 48 hours of stable operation

### Production Environment

- ✅ All test criteria met
- ✅ Zero data loss
- ✅ No duplicate processing
- ✅ All business functions working
- ✅ User acceptance confirmed
- ✅ 1 week of stable operation

## Communication Plan

### Before Deployment

- Notify stakeholders 1 week in advance
- Schedule maintenance windows
- Prepare rollback plan
- Brief support team

### During Deployment

- Status updates every 30 minutes
- Immediate notification of issues
- Keep rollback team on standby

### After Deployment

- Summary report within 24 hours
- Lessons learned session
- Update documentation

## Emergency Contacts

- DevOps Lead: [Contact Info]
- Application Owner: [Contact Info]
- Database Admin: [Contact Info]
- On-Call Engineer: [Contact Info]

## Cleanup (After Successful Production Deployment)

After 2 weeks of stable production operation:

```bash
# Archive old DeploymentConfigs
for ns in 9b301c-dev 9b301c-test 9b301c-prod; do
  oc get dc -n $ns -o yaml > deploymentconfigs-backup-$ns.yaml
done

# Delete old DeploymentConfigs (optional - keep for safety)
# oc delete dc --all -n <namespace>
```

## Notes

- **Kafka Consumer Groups**: Ensure consumer group IDs are correctly configured
- **Database Connections**: Monitor connection pool usage during rollout
- **Elasticsearch**: Watch for indexing lag during indexing service deployment
- **File Storage**: Verify PVC mounts are working correctly
- **Secrets/ConfigMaps**: Ensure all referenced secrets exist in target namespace
