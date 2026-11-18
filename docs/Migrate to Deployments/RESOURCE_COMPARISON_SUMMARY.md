# Resource Comparison Summary

## Overview

This document summarizes the comparison between current DeploymentConfig resources and generated Deployment resources for prod and test environments.

## Prod Environment

### Status: ✅ **REPLICAS MATCH - READY FOR MIGRATION**

All services now have matching replica counts between DeploymentConfigs and Deployments.

### Perfect Matches (4 services)

- ✅ **corenlp** - All resources match perfectly
- ✅ **nlp-service** - All resources match (replicas: 0)
- ✅ **extract-quotes-service** - All resources match (replicas: 3)
- ✅ **editor** - All resources match (replicas: 3)

### Replicas Match, Minor Resource Differences (16 services)

These services have correct replica counts but some resource limits differ from prod. The differences are acceptable as they're mostly where prod has no limits set and deployments have conservative limits.

| Service                       | Replicas | Notes                                                  |
| ----------------------------- | -------- | ------------------------------------------------------ |
| **api-services**              | 3 ✓      | Uses indexing-service-cloud, requests match            |
| **subscriber**                | 3 ✓      | All resources match                                    |
| **charts-api**                | 3 ✓      | Minor resource differences                             |
| **content-service**           | 3 ✓      | Prod has no limits, deployment has conservative limits |
| **indexing-service**          | 4 ✓      | Prod has no limits, deployment has conservative limits |
| **transcription-service**     | 6 ✓      | Prod has no limits, deployment has conservative limits |
| **reporting-service**         | 3 ✓      | Prod has no limits, deployment has conservative limits |
| **notification-service**      | 6 ✓      | Minor resource differences                             |
| **scheduler-service**         | 1 ✓      | Minor resource differences                             |
| **folder-collection-service** | 3 ✓      | Minor resource differences                             |
| **ffmpeg-service**            | 3 ✓      | Prod has no limits, deployment has conservative limits |
| **event-handler-service**     | 3 ✓      | Minor resource differences                             |
| **ches-retry-service**        | 1 ✓      | Prod has no limits, deployment has conservative limits |
| **syndication-service**       | 1 ✓      | Minor memory request difference                        |
| **filemonitor-service**       | 1 ✓      | All resources match                                    |
| **nginx-editor**              | 3 ✓      | All resources match                                    |

### Actions Taken for Prod

1. ✅ Created backup of all prod DeploymentConfigs
2. ✅ Generated prod overlay patches from backups
3. ✅ Updated all replica counts to match prod
4. ✅ Verified api-services uses indexing-service-cloud
5. ✅ All 21 deployment YAMLs generated successfully

## Test Environment

### Status: ✅ **MOSTLY READY - MINOR ADJUSTMENTS NEEDED**

Test environment has 12 perfect matches and 7 services with minor resource differences.

### Perfect Matches (12 services)

- ✅ **editor** - All resources match
- ✅ **subscriber** - All resources match
- ✅ **content-service** - All resources match
- ✅ **indexing-service** - All resources match
- ✅ **transcription-service** - All resources match
- ✅ **nlp-service** - All resources match
- ✅ **ffmpeg-service** - All resources match
- ✅ **event-handler-service** - All resources match
- ✅ **syndication-service** - All resources match
- ✅ **filemonitor-service** - All resources match
- ✅ **image-service** - All resources match
- ✅ **corenlp** - All resources match

### Minor Differences (7 services)

| Service                       | Issue          | DC      | Deployment |
| ----------------------------- | -------------- | ------- | ---------- |
| **api-services**              | Replicas       | 2       | 1          |
| **api-services**              | CPU Limit      | 500m    | none       |
| **api-services**              | Memory Limit   | 1536Mi  | none       |
| **charts-api**                | Replicas       | 1       | 3          |
| **charts-api**                | Resources      | Various | Various    |
| **reporting-service**         | CPU Limit      | 50m     | 100m       |
| **reporting-service**         | Memory Limit   | 250Mi   | 200Mi      |
| **notification-service**      | CPU Limit      | 50m     | 100m       |
| **notification-service**      | Memory Limit   | 250Mi   | 200Mi      |
| **notification-service**      | Memory Request | 100Mi   | 80Mi       |
| **scheduler-service**         | CPU Limit      | 50m     | 100m       |
| **scheduler-service**         | Memory Limit   | 100Mi   | 150Mi      |
| **folder-collection-service** | CPU Limit      | 75m     | 100m       |
| **folder-collection-service** | CPU Request    | 20m     | 50m        |
| **folder-collection-service** | Memory Request | 50Mi    | 80Mi       |
| **ches-retry-service**        | Memory Request | 100Mi   | 50Mi       |

### Failed to Generate (1 service)

- ❌ **extract-quotes-service** - Missing secret file (llm-api-keys.env)

### Actions Needed for Test

1. ⚠️ Update api-services test overlay to set replicas: 2 and add limits
2. ⚠️ Update charts-api test overlay to set replicas: 1 and adjust resources
3. ⚠️ Create placeholder for extract-quotes-service llm-api-keys.env in test
4. ⚠️ Optionally adjust resource limits for the 5 other services (minor differences)

## Dev Environment

### Status: ✅ **ALREADY MIGRATED AND RUNNING**

Dev environment has been successfully migrated to Deployments and is running in production.

### Lessons Learned from Dev

1. ✅ Oracle service not deployed in dev (commented out in scripts)
2. ✅ Service count: 25 services (26 total - oracle)
3. ✅ `getPods` function returns "0" for empty results
4. ✅ Deployment YAMLs exclude secrets, routes, and PVCs (managed separately)
5. ✅ Migration scripts create backups before applying changes

## Secret Files Required

### All Environments

- `openshift/kustomize/services/image/base/secret.env` - SSH key
- `openshift/kustomize/services/filemonitor/base/secret.env` - SSH key

### Prod-Specific

- `openshift/kustomize/services/transcription/overlays/prod/azure.env` - Azure credentials
- `openshift/kustomize/services/extract-quotes/overlays/prod/llm-api-keys.env` - LLM API keys

### Test-Specific

- `openshift/kustomize/services/transcription/overlays/test/azure.env` - Azure credentials
- `openshift/kustomize/services/extract-quotes/overlays/test/llm-api-keys.env` - LLM API keys

## Scripts Available

### Comparison Scripts

- `./src/tools/scripts/compare-prod-resources-simple.sh` - Compare prod resources
- `./src/tools/scripts/compare-test-resources.sh` - Compare test resources

### Backup Scripts

- `./src/tools/scripts/backup-test-resources.sh` - Backup test DeploymentConfigs

### Sync Scripts

- `./src/tools/scripts/sync-overlays-from-backups.sh <env>` - Generate overlay patches from backups
- `./src/tools/scripts/update-prod-replicas-from-backup.sh` - Update replica counts

### Generation Scripts

- `./src/tools/scripts/generate-all-deployment-yamls.sh <env>` - Generate all deployment YAMLs
- `./src/tools/scripts/extract-cluster-env-files.sh <env>` - Extract secret files from cluster

### Migration Scripts

- `./src/tools/scripts/migrate-deployments.sh <env>` - Full migration (backup, apply, verify)

## Next Steps

### For Test

1. Run: `./src/tools/scripts/update-prod-replicas-from-backup.sh` (modify for test)
2. Create missing secret placeholder: `touch openshift/kustomize/services/extract-quotes/overlays/test/llm-api-keys.env`
3. Run: `./src/tools/scripts/generate-all-deployment-yamls.sh test`
4. Review and test in test environment

### For Prod

1. ✅ Prod overlays are ready
2. ✅ All replicas match
3. ✅ All deployment YAMLs generated
4. ⚠️ Extract secret files from prod cluster: `./src/tools/scripts/extract-cluster-env-files.sh prod`
5. ⚠️ Have manager run migration: `./src/tools/scripts/migrate-deployments.sh prod`

## Summary

| Environment | Status      | Services Ready       | Action Required                 |
| ----------- | ----------- | -------------------- | ------------------------------- |
| **Dev**     | ✅ Migrated | 25/25                | None - Running successfully     |
| **Test**    | ⚠️ Ready    | 12/20 perfect        | Minor adjustments to 7 services |
| **Prod**    | ✅ Ready    | 20/20 replicas match | Extract secrets, then migrate   |

**Overall Status: READY FOR PROD MIGRATION** 🚀

All critical replica counts match, resource limits are acceptable (mostly conservative), and all necessary scripts and backups are in place.
