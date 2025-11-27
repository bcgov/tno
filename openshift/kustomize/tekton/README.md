# Tekton Pipeline Tasks

This directory contains Tekton pipeline tasks for deploying and managing the TNO application in OpenShift.

## Overview

There are **two versions** of key Tekton tasks to support both DeploymentConfig (legacy) and Deployment (current) resource types during the migration period.

## Task Versions

### DeploymentConfig Tasks (Legacy - for prod until migrated)

Use these tasks for environments still running DeploymentConfigs:

- **`deploy-all`** - Orchestrates deployment of all services using DeploymentConfigs
- **`oc-backup`** - Database backup task using `dc/` syntax
- **`oc-deploy-with-tag`** - Image tagging and deployment using DeploymentConfigs

### Deployment Tasks (Current - for dev/test/migrated environments)

Use these tasks for environments migrated to Kubernetes Deployments:

- **`deploy-all-deployment`** - Orchestrates deployment of all services using Deployments
- **`oc-backup-deployment`** - Database backup task using pod selectors
- **`oc-deploy-with-tag-deployment`** - Image tagging and deployment using Deployments

## Usage

### Deploying Tasks to OpenShift

**For DeploymentConfig environments (e.g., prod before migration):**

```bash
oc apply -f base/tasks/deploy-all.yaml
oc apply -f base/tasks/oc-backup.yaml
oc apply -f base/tasks/oc-deploy-with-tag.yaml
```

**For Deployment environments (e.g., dev, test, or migrated prod):**

```bash
oc apply -f base/tasks/deploy-all-deployment.yaml
oc apply -f base/tasks/oc-backup-deployment.yaml
oc apply -f base/tasks/oc-deploy-with-tag-deployment.yaml
```

### Running Pipelines

**Deploy All Services (DeploymentConfig version):**

```bash
tkn task start deploy-all \
  --param PROJECT_SHORTNAME=9b301c \
  --param IMAGE_TAG=latest \
  --param ENV=prod \
  --param DEPLOY=all \
  --workspace name=conditions,claimName=pipeline-workspace
```

**Deploy All Services (Deployment version):**

```bash
tkn task start deploy-all-deployment \
  --param PROJECT_SHORTNAME=9b301c \
  --param IMAGE_TAG=latest \
  --param ENV=dev \
  --param DEPLOY=all \
  --workspace name=conditions,claimName=pipeline-workspace
```

**Database Backup (DeploymentConfig version):**

```bash
tkn task start oc-backup \
  --param PROJECT_NAMESPACE=9b301c-prod \
  --param DEPLOYMENT_CONFIG=backup \
  --param ARGS="-1"
```

**Database Backup (Deployment version):**

```bash
tkn task start oc-backup-deployment \
  --param PROJECT_NAMESPACE=9b301c-dev \
  --param DEPLOYMENT_NAME=backup \
  --param ARGS="-1"
```

**Deploy Single Service with Tag (DeploymentConfig version):**

```bash
tkn task start oc-deploy-with-tag \
  --param PROJECT=9b301c \
  --param IMAGE_NAME=editor \
  --param IMAGE_TAG=latest \
  --param ENV=prod
```

**Deploy Single Service with Tag (Deployment version):**

```bash
tkn task start oc-deploy-with-tag-deployment \
  --param PROJECT=9b301c \
  --param IMAGE_NAME=editor \
  --param IMAGE_TAG=latest \
  --param ENV=dev \
  --param OBJECT=deployment
```

## Key Differences Between Versions

| Feature              | DeploymentConfig Version                        | Deployment Version                                                               |
| -------------------- | ----------------------------------------------- | -------------------------------------------------------------------------------- |
| **Task Names**       | `deploy-all`, `oc-backup`, `oc-deploy-with-tag` | `deploy-all-deployment`, `oc-backup-deployment`, `oc-deploy-with-tag-deployment` |
| **Resource Type**    | `dc` (DeploymentConfig)                         | `deployment` (Deployment)                                                        |
| **Backup Parameter** | `DEPLOYMENT_CONFIG`                             | `DEPLOYMENT_NAME`                                                                |
| **Pod Access**       | `dc/$(params.DEPLOYMENT_CONFIG)`                | Pod selector: `-l name=$(params.DEPLOYMENT_NAME)`                                |
| **Rollout Command**  | `oc rollout status dc/...`                      | `oc rollout status deployment/...`                                               |
| **API Service**      | `sts` (StatefulSet)                             | `sts` (StatefulSet) - same for both                                              |

## Component Types in deploy-all Tasks

Both `deploy-all` and `deploy-all-deployment` manage the following services:

### Applications (3 services)

- **charts-api** - Charts API service
- **editor** - Editor web application
- **subscriber** - Subscriber web application

### Processing Services (13 services)

- **content-service** - Content processing
- **indexing-service** - Content indexing
- **indexing-service-cloud** - Cloud-based indexing (test/prod only)
- **transcription-service** - Audio/video transcription
- **nlp-service** - Natural language processing
- **extract-quotes-service** - Quote extraction
- **reporting-service** - Report generation
- **notification-service** - Notification handling
- **scheduler-service** - Task scheduling
- **folder-collection-service** - Folder monitoring
- **ffmpeg-service** - Media processing
- **event-handler-service** - Event processing
- **ches-retry-service** - CHES retry logic

### Ingest Services (3 services)

- **syndication-service** - Content syndication
- **filemonitor-service** - File monitoring
- **image-service** - Image processing

### API Service (1 service)

- **api** - Main API (StatefulSet)

## Migration Strategy

1. **Current State**: Prod uses DeploymentConfig tasks, dev/test use Deployment tasks
2. **During Migration**: Both task versions are deployed to OpenShift
3. **After Migration**: Once all environments use Deployments, DeploymentConfig tasks can be deprecated
4. **Cleanup**: Remove DeploymentConfig task versions after successful migration

## Parameters

### Common Parameters (both versions)

- **PROJECT_SHORTNAME** - Project namespace prefix (default: `9b301c`)
- **IMAGE_TAG** - Image tag to deploy (default: `latest`)
- **ENV** - Target environment (`dev`, `test`, or `prod`)
- **TIMEOUT** - Deployment timeout (default: `600s`)
- **DEPLOY** - Component to deploy: `*` (current), `all`, or specific component name
- **WAIT** - Wait for pods to scale up: `yes` or `no` (default: `no`)

### Backup-Specific Parameters

**DeploymentConfig version:**

- **DEPLOYMENT_CONFIG** - Name of the DeploymentConfig running backup service

**Deployment version:**

- **DEPLOYMENT_NAME** - Name of the Deployment running backup service

### Deploy-with-Tag Parameters

**Deployment version only:**

- **OBJECT** - Kubernetes object type: `deployment` or `sts` (default: `deployment`)

## Workspaces

The `deploy-all` and `deploy-all-deployment` tasks require a workspace:

- **conditions** - Workspace containing `build.env` file with build flags

Example `build.env`:

```bash
BUILD_API=true
BUILD_EDITOR=true
BUILD_SUBSCRIBER=false
# ... etc for each component
```

## Templates

Template versions of tasks are also available in `/openshift/templates/tekton/`:

- `tasks/oc-backup.yaml` - DeploymentConfig version
- `tasks/oc-backup-deployment.yaml` - Deployment version
- `tasks/oc-deploy-with-tag.yaml` - DeploymentConfig version
- `tasks/oc-deploy-with-tag-deployment.yaml` - Deployment version
- `pipelines/backup-database.yaml` - DeploymentConfig version
- `pipelines/backup-database-deployment.yaml` - Deployment version

## Support

For issues or questions about Tekton tasks, refer to:

- [OpenShift Pipelines Documentation](https://docs.openshift.com/container-platform/latest/cicd/pipelines/understanding-openshift-pipelines.html)
- [Tekton Documentation](https://tekton.dev/docs/)
