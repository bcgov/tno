# Manual Tekton releases

GitHub Actions owns automatic releases: `dev` → `9b301c-dev`, `master` →
`9b301c-test`. Tekton is a manually invoked fallback for those environments.
The `git-webhook-dev` and `git-webhook-master` EventListeners now reject push
notifications through a constant-false CEL filter. Their definitions remain in
Git so reapplying them cannot accidentally re-enable duplicate releases.

## Reconcile installed definitions after merge

From the repository root, while no release is active:

```bash
bash openshift/scripts/release/install-tekton.sh --dry-run=server
bash openshift/scripts/release/install-tekton.sh
```

The script disables automatic listeners first and installs the current git,
build, migration, deployment tasks and `buildah-all`/`deploy-all` pipelines.
It uses the cluster-provided `openshift/cli:latest` image stream, matching the live
tasks; the retired ACR `openshift-cli:latest` tag is not used. It does not start a
release or change application workloads. Existing service
accounts, registry secrets, bindings, and storage must already exist.

The previous live build task used retired `services/net/...` Dockerfile paths.
The current task uses `src/...`, resolves Dockerfiles against their actual build
contexts, and checks out the requested Git commit rather than silently using the
latest branch head. It does not trace registry credentials. Rotate the registry
credential previously exposed by the old task's shell tracing using the normal
credential-management procedure; the installer does not rotate credentials.

## Start a manual release

Use the OpenShift Pipelines UI to start **buildah-all**, or copy
`base/pipeline-runs/manual-release.yaml`, replace `GIT_REF` with the exact merged
commit SHA, select the matching branch/environment, and run `oc create -f` on the
edited file. Use `dev`/`dev` or `master`/`test`. The default example targets dev.
Do not run it concurrently with GitHub Actions deployment to the same environment.

`COMPONENT=all` builds the supported components and runs the migration. `*` selects
components changed in the chosen commit; a component ID selects that component in
addition to detected changes. For a multi-commit recovery release, prefer `all`.
The source workspace uses the existing `repo` PVC; the conditions workspace gets
its own PVC. Use an image tag unique to the release instead of reusing `latest`.

Release order:

1. Check out the exact commit and identify changes.
2. Build images and push to ACR.
3. Run the required database migration, using `montford` in dev or `mooncrest` in
   test. A failure blocks the pipeline.
4. Promote selected images, stop/update/start StatefulSet `api`, and roll Deployment
   `api-services`. Both APIs must become ready before any other service starts.
5. Roll the remaining selected Deployments in parallel, preserving their replica
   counts. Any failed rollout fails the task.

The deployment task runs `openshift/scripts/release/rollout.sh` from the checked-out
commit. Therefore the fallback must use a commit containing the updated scripts.
Test uses `indexing-service-cloud`; both frontends use the single `app` image.
The API StatefulSet is the only non-rolling workload in this release path.

`deploy-all` is a deployment-only pipeline for already-built images and uses the
same migration/API gates. Legacy pipeline examples elsewhere in this directory
are not installed by the reconciliation script. Production and Elasticsearch
migration orchestration are outside this dev/test fallback.

## Verification

```bash
python3 -m unittest discover -s .github/scripts -v
python3 -m unittest discover -s openshift/scripts/tests -p 'test_release*.py' -v
```

Tests use fake OpenShift/Buildah commands to exercise failure handling, migration
timeouts, both API gates, replica preservation, and build paths. Server-side dry
run checks the installed Tekton API schema. An actual dev release is still needed
to validate cluster capacity, image pulls, migrations, and application readiness.
