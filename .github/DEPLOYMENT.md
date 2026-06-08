# Deployment Workflows

This document explains how the orchestrated GitHub Actions deployment flow works for MMI.

## Entry Points

### Auto Deploy

`auto-deploy.yml` runs on pushes to:

- `dev`
- `master`

It only starts when the push changes one of these paths:

```text
api/**
app/**
services/**
libs/**
openshift/**
.github/workflows/auto-deploy.yml
.github/workflows/deploy-all.yml
.github/scripts/**
```

The workflow calls `deploy-all.yml` with `deploy_scope: changed`.

| Branch | Environment | Image tag | DB migration |
| ------ | ----------- | --------- | ------------ |
| `dev` | `dev` | `:dev` | Runs when detected |
| `master` | `test` | `:test` | Runs when detected |

Production is not wired into this flow yet.

### Manual Deploy

`deploy-all.yml` can also be run from the GitHub Actions UI.

Inputs:

- `environment`: `dev` or `test`
- `deploy_scope`: `changed` or `all`
- `skip_db_migration`: `true` or `false`

Use `deploy_scope: all` carefully. It builds and deploys every supported API,
service, and frontend image for the selected environment.

## Flow

`deploy-all.yml` runs in fixed phases:

```text
Detect Deploy Targets
-> Build Images
-> DB Migration
-> Deploy API
-> Deploy Services
-> Deploy Frontend
-> Deployment Summary
```

The build, service deploy, and frontend deploy phases use matrix jobs. Items
inside the same matrix can run in parallel, but each phase waits for the earlier
phase through `needs`.

The build phase uses the shared `actions/build-scan-push-image` action. Docker
build failures retry, and ACR login, backup, and push operations retry with
increasing waits.

OpenShift deploy phases use the shared `actions/install-oc`,
`actions/openshift-login`, and `actions/deploy-openshift-workload` actions.
`install-oc` is a local shell action that pins the OpenShift client version and
avoids the warning noise from `redhat-actions/openshift-tools-installer@v1`.

## Change Detection

Changed deploys are decided by:

```text
.github/scripts/detect_deploy_changes.py
```

The main function is:

```python
detect_deploy_changes(changed_files, scope)
```

It returns:

- `build_matrix`: images to build and push
- `service_matrix`: backend deployments to restart
- `frontend_matrix`: frontend deployments to restart
- `deploy_api`: whether to restart API workloads
- `run_db_migration`: whether to run DB migration

## Source Path Rules

These source paths build a new image and deploy the matching workload.

| Changed path | Result |
| ------------ | ------ |
| `api/net/**` | Build `api`, deploy API |
| `api/node/**` | Build/deploy `charts-api` |
| `app/editor/**` | Build/deploy `editor` |
| `app/subscriber/**` | Build/deploy `subscriber` |
| `services/net/content/**` | Build/deploy `content-service` |
| `services/net/indexing/**` | Build/deploy `indexing-service` |
| `services/net/transcription/**` | Build/deploy `transcription-service` |
| `services/net/nlp/**` | Build/deploy `nlp-service` |
| `services/net/extract-quotes/**` | Build/deploy `extract-quotes-service` |
| `services/net/reporting/**` | Build/deploy `reporting-service` |
| `services/net/notification/**` | Build/deploy `notification-service` |
| `services/net/scheduler/**` | Build/deploy `scheduler-service` |
| `services/net/ffmpeg/**` | Build/deploy `ffmpeg-service` |
| `services/net/event-handler/**` | Build/deploy `event-handler-service` |
| `services/net/ches-retry/**` | Build/deploy `ches-retry-service` |
| `services/net/syndication/**` | Build/deploy `syndication-service` |
| `services/net/filemonitor/**` | Build/deploy `filemonitor-service` |
| `services/net/image/**` | Build/deploy `image-service` |
| `services/net/auto-clipper/**` | Build/deploy `auto-clipper-service` |
| `services/net/contentmigration/**` | Build/deploy `contentmigration-service` |
| `services/net/folder-collection/**` | Build/deploy `folder-collection-service` |

## Shared .NET Rules

| Changed path | Result |
| ------------ | ------ |
| `libs/net/**` | Build API and all .NET services, then deploy API and all .NET services |
| `libs/net/dal/**` | Same as `libs/net/**`, plus DB migration |

`db/postgres/**` is not part of auto-deploy detection.

## OpenShift Manifest Rules

Manifest-only changes usually restart or redeploy existing workloads without
building a new image.

| Changed path | Result |
| ------------ | ------ |
| `openshift/kustomize/api/**` | Deploy API only |
| `openshift/kustomize/api-services/**` | Deploy API only |
| `openshift/kustomize/charts/**` | Build/deploy `charts-api` |
| `openshift/kustomize/app/editor/**` | Build/deploy `editor` |
| `openshift/kustomize/app/subscriber/**` | Build/deploy `subscriber` |
| `openshift/kustomize/services/<service>/**` | Deploy that service only |
| `openshift/kustomize/shared_resources/**` | Build API, all .NET services, and both frontends; deploy API, services, and frontends |

`openshift/kustomize/cron/**` is not currently handled by `deploy-all.yml`.
CronJobs still use their own workflows.

## DB Migration

In `changed` mode, DB migration runs only when `libs/net/dal/**` changes.

In `all` mode, DB migration runs unless `skip_db_migration` is `true`.

The migration job builds the EF migration bundle from `libs/net` and runs it as
a temporary OpenShift pod.

DB migration uses the same local `install-oc` and `openshift-login` actions as
the orchestrated deploy jobs. Its Docker build, Trivy scan, ACR backup, and ACR
push steps still live in `_reusable-db-migration-cd.yml`.

## API StatefulSet

The API deploy phase updates both:

- `deployment/api-services`
- `statefulset/api`

This keeps API StatefulSet pods on the same image tag as the existing API
deployment.

## Elasticsearch Migration

`deploy-all.yml` does not run Elasticsearch migrations yet.

Dev uses the OpenShift-hosted Elasticsearch instance. Test and prod use cloud
Elasticsearch. The migration code and environment wiring need another pass
before this belongs in the orchestrated deploy flow.

If that setup is standardized later, add Elastic migration by:

- Adding an Elastic migration output in `detect_deploy_changes.py`
- Adding a `deploy-all.yml` job that calls `_reusable-elastic-migration-cd.yml`
- Passing the right `elastic_secret_name`, `indexing_configmap`, and
  `elastic_apikey_enabled` values per environment
- Placing the job before API/service/frontend rollout if app pods depend on it
