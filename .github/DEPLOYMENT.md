# Deployment Workflows

This document explains how the orchestrated GitHub Actions deployment flow works for MMI.

## Entry Points

### Auto Deploy

`auto-deploy.yml` runs on pushes to:

- `dev`
- `master`

It only starts when the push changes one of these paths:

```text
src/api/**
src/charts-api/**
src/app/**
src/services/**
src/libs/**
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
- `allow_dev_branch_override`: permit a manual dev release from another branch

Use `deploy_scope: all` carefully. It builds and deploys every supported API,
service, and frontend image for the selected environment.

## Flow

`deploy-all.yml` runs in fixed phases:

```text
Detect Deploy Targets
-> Validate Selected Components
-> Build / Scan / Publish Images
-> Required DB Migration (must succeed)
-> API StatefulSet stop/update/start, then API Deployment rolling update (both must be ready)
-> Other Service and Frontend Deployments (parallel rolling updates)
-> Deployment Summary
```

The build, service deploy, and frontend deploy phases use matrix jobs. Items
inside the same matrix can run in parallel. Both the frontend and service matrices
wait for the migration and API gates, but do not wait for each other. Failed or
timed-out migration/API/rollout checks fail the release. A failed validation cannot
be mistaken for an intentionally skipped image build.

Selected React components run lint, formatting, type checking, unit tests, and build
before image publication. Selected .NET components run formatting and a fresh Release
build; Charts API runs its compile check. CodeQL and the separate .NET library test
workflow remain independent checks; they are not gates in this release workflow.

The build phase uses the shared `actions/build-scan-push-image` action. Docker
build failures retry.

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
| `src/api/**` | Build `api`, deploy API |
| `src/charts-api/**` | Build/deploy `charts-api` |
| `src/app/**` | Build `app`, deploy `editor` and `subscriber` (one image serves both; `APP_MODE` picks the design) |
| `src/services/content/**` | Build/deploy `content-service` |
| `src/services/indexing/**` | Build/deploy `indexing-service` |
| `src/services/transcription/**` | Build/deploy `transcription-service` |
| `src/services/content-analysis/**` | Build/deploy `content-analysis-service` |
| `src/services/reporting/**` | Build/deploy `reporting-service` |
| `src/services/notification/**` | Build/deploy `notification-service` |
| `src/services/scheduler/**` | Build/deploy `scheduler-service` |
| `src/services/ffmpeg/**` | Build/deploy `ffmpeg-service` |
| `src/services/event-handler/**` | Build/deploy `event-handler-service` |
| `src/services/ches-retry/**` | Build/deploy `ches-retry-service` |
| `src/services/syndication/**` | Build/deploy `syndication-service` |
| `src/services/filemonitor/**` | Build/deploy `filemonitor-service` |
| `src/services/image/**` | Build/deploy `image-service` |
| `src/services/auto-clipper/**` | Build/deploy `auto-clipper-service` |
| `src/services/contentmigration/**` | Build/deploy `contentmigration-service` |
| `src/services/folder-collection/**` | Build/deploy `folder-collection-service` |

## Shared .NET Rules

| Changed path | Result |
| ------------ | ------ |
| `src/libs/**` | Build API and all .NET services, then deploy API and all .NET services |
| `src/libs/dal/**` | Same as `src/libs/**`, plus DB migration |

`dependencies/postgres/**` is not part of auto-deploy detection.

## OpenShift Manifest Rules

Manifest-only changes usually restart or redeploy existing workloads without
building a new image.

| Changed path | Result |
| ------------ | ------ |
| `openshift/kustomize/api/**` | Deploy API only |
| `openshift/kustomize/api-services/**` | Deploy API only |
| `openshift/kustomize/charts/**` | Build/deploy `charts-api` |
| `openshift/kustomize/app/editor/**` | Deploy `editor` only |
| `openshift/kustomize/app/subscriber/**` | Deploy `subscriber` only |
| `openshift/kustomize/services/<service>/**` | Deploy that service only |
| `openshift/kustomize/shared_resources/**` | Build API, all .NET services, and both frontends; deploy API, services, and frontends |

`openshift/kustomize/cron/**` is not currently handled by `deploy-all.yml`.
CronJobs still use their own workflows.

## DB Migration

In `changed` mode, DB migration runs when `src/libs/dal/**` or `src/libs/Dockerfile` changes.

In `all` mode, DB migration always runs. Required migrations cannot be bypassed with a skip input.

The migration job builds the EF migration bundle from `src/libs` and runs it as
a temporary OpenShift pod.

DB migration uses the same local `install-oc` and `openshift-login` actions as
the orchestrated deploy jobs. Its Docker build, Trivy scan, ACR backup, and ACR
push steps still live in `_reusable-db-migration-cd.yml`.

## API StatefulSet

The API deploy phase updates both:

- `deployment/api-services`
- `statefulset/api`

Both use the API image. `statefulset/api` is a non-rolling exception: record its
replica count, scale to zero, wait for old pods to disappear, update the image,
restore the replica count, and require readiness. `deployment/api-services` then
uses a rolling update. No other selected service deploys until both complete.

The shared `openshift/scripts/release/rollout.sh` enforces `RollingUpdate` with
`maxUnavailable: 0` and `maxSurge: 1` for Deployments, preserves replica counts,
and fails on rollout errors. Ensure the namespace has capacity for the surge pod.
The API StatefulSet can remain stopped if an update fails; investigate and use the
manual rollback before resuming. Rollback also follows the non-rolling API rule.

General Kustomize changes still require deliberate manifest application. The release
script enforces rollout strategy but does not reconcile all application manifests,
secrets, routes, or replica settings. This avoids resetting live capacity from old
overlays. CronJobs, automation-service, reverse proxies, and Elasticsearch migrations
retain their separate operational workflows.

## Manual Tekton fallback

GitHub Actions owns automatic dev/test releases. Follow
[`openshift/kustomize/tekton/README.md`](../openshift/kustomize/tekton/README.md) to
reconcile the installed Tekton tasks and disable push EventListeners after merge.
Changing the files in Git alone does not update installed cluster resources.
Do not run the manual fallback concurrently with a GitHub release to the same
environment: both publish environment tags.

## Elasticsearch Migration

`deploy-all.yml` does not run Elasticsearch migrations yet.

Dev uses the OpenShift-hosted Elasticsearch instance. Test and prod use cloud
Elasticsearch. The migration code and environment wiring need another pass
before this belongs in the orchestrated deploy flow.

If that setup is standardized later, add Elastic migration by:

- Adding an Elastic migration output in `detect_deploy_changes.py`
- Adding a `deploy-all.yml` job that runs the migration Job (the **Elastic Migration CI/CD**
  workflow only builds and pushes the `elastic-migration` image)
- Passing the right `elastic_secret_name`, `indexing_configmap`, and
  `elastic_apikey_enabled` values per environment
- Placing the job before API/service/frontend rollout if app pods depend on it
