# Deployment Documentation

This directory contains all documentation related to the DeploymentConfig to Deployment migration for TNO services.

## Migration Status

- **Dev Environment**: ✅ COMPLETE - All 24 services migrated
- **Test Environment**: 🔄 READY - Preparation complete, ready to migrate
- **Prod Environment**: ⏳ PENDING - Awaiting test validation

## Quick Links

### 🚀 Getting Started

- **[QUICK_START_DEV.md](./QUICK_START_DEV.md)** - Migration quick start guide
  - How to use the migration script
  - Manual deployment steps
  - Testing and verification
  - Useful command reference

### 📋 Migration Guide

- **[DEPLOYMENT_MIGRATION.md](./DEPLOYMENT_MIGRATION.md)** - Complete migration documentation
  - What changed (DeploymentConfig → Deployment)
  - All 32 services migrated
  - Trigger mechanism preservation
  - Production configuration sync
  - Verification steps

### 📅 Deployment Plan

- **[DEPLOYMENT_PLAN.md](./DEPLOYMENT_PLAN.md)** - Actual migration process and results
  - Service categories and deployment strategies
  - Dev environment completion (all 24 services)
  - Test environment preparation
  - Production migration plan
  - Issues encountered and fixes
  - Rollback procedures

### 🔧 Scripts and Tools

- **[migrate-deployments.sh](../../tools/scripts/migrate-deployments.sh)** - Interactive migration script

  - Migrates DeploymentConfigs to Deployments with validation at each step
  - Usage: `./tools/scripts/migrate-deployments.sh <dev|test|prod> [service-name]`
  - Examples:
    - `./tools/scripts/migrate-deployments.sh dev` - Migrate all services in dev
    - `./tools/scripts/migrate-deployments.sh prod nginx` - Migrate only nginx in prod
  - Features:
    - Interactive prompts for safety
    - Automatic health checks
    - YAML comparison
    - Log inspection
    - Scales down old DeploymentConfigs
    - Handles Kafka consumers carefully

- **[compare_deployments.sh](./compare_deployments.sh)** - Compare deployed services with production
  - Usage: `./compare_deployments.sh <service-name> <dev|test|prod>`
  - Example: `./compare_deployments.sh corenlp dev`

## Quick Command Reference

### Migrate All Services (Recommended)

```bash
# Login to OpenShift
oc login

# Switch to environment
oc project 9b301c-dev    # or 9b301c-test, 9b301c-prod

# Run migration script
./tools/scripts/migrate-deployments.sh dev    # or test, prod
```

### Migrate Single Service

```bash
# Migrate just one service
./tools/scripts/migrate-deployments.sh dev nginx

# Or manually
oc apply -k openshift/kustomize/nginx/overlays/dev
oc rollout status deployment/nginx -n 9b301c-dev
oc scale dc/nginx --replicas=0 -n 9b301c-dev
```

### Common Commands

```bash
# View pods
oc get pods -n 9b301c-dev

# View logs
oc logs deployment/<service-name> -n 9b301c-dev

# Check deployment status
oc get deployments -n 9b301c-dev

# Rollback if needed
oc rollout undo deployment/<service-name> -n 9b301c-dev
```

## Service Categories

### Category 1: Stateless Services (10 services)

Rolling update safe - old and new pods can coexist:

- nginx, editor, subscriber, charts-api, api-services
- corenlp, nlp-service, ffmpeg-service, extract-quotes-service, transcription-service

### Category 2: Kafka Consumers

**Stateless (7 services)** - Rolling update safe:

- content-service, folder-collection-service, indexing-service
- event-handler-service, notification-service, reporting-service, ches-retry-service

**Single-Instance (4 services)** - Scale down first:

- scheduler-service, filemonitor-service, syndication-service, image-service

### Category 3: Supporting Services (4 services)

- psql, kowl, nginx-editor, nginx-subscriber

## Migration Results

### Dev Environment ✅

- **Duration**: ~2 hours
- **Services**: All 24 migrated successfully
- **Method**: Single automated run with migration script
- **Issues**: Fixed image patches, ConfigMap keys, resource limits
- **Status**: All services running on Deployments, old DCs scaled to 0

### Test Environment 🔄

- **Preparation**: Complete
- **Ready to migrate**: Yes
- **Expected duration**: ~2 hours
- **Command**: `./tools/scripts/migrate-deployments.sh test`

### Production Environment ⏳

- **Status**: Awaiting test validation
- **Prerequisites**: 48 hours stable test operation
- **Approach**: Single automated run (recommended) or staged deployment

## Need Help?

- Check [QUICK_START_DEV.md](./QUICK_START_DEV.md) for common issues
- Review [DEPLOYMENT_PLAN.md](./DEPLOYMENT_PLAN.md) for detailed procedures
- Use `oc <command> --help` for command syntax

## Related Documentation

- [Main README](../../README.md)
- [Development Guide](../DEVELOPMENT.md)
- [OpenShift Configuration](../../openshift/README.md)
