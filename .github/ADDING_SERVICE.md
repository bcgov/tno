# Adding a New Service

Use this checklist for a normal .NET service under `src/services`.

## 1. Add the Service Files

Create the service code and Dockerfile:

```text
src/services/<service>/
src/services/<service>/Dockerfile
```

Example:

```text
src/services/my-service/
src/services/my-service/Dockerfile
```

## 2. Add OpenShift Manifests

Create the OpenShift kustomize files:

```text
openshift/kustomize/services/<service>/
```

The deployment name and image name should match the service name when possible.


## 3. Add a GitHub Workflow

Create a workflow:

```text
.github/workflows/<service>-cicd.yml
```

For a normal .NET service, follow the existing service workflows and call:

```text
.github/workflows/_reusable-dotnet-cicd.yml
```

Set the service project path, Dockerfile, deployment name, and dev/test
kustomize overlay paths.

## 4. Add It to Auto Deploy Detection

Update:

```text
.github/scripts/detect_deploy_changes.py
```

Add the new service to these sections:

```text
BUILD_TARGETS
SERVICE_TARGETS
DOTNET_SERVICE_NAMES
SERVICE_PATHS
KUSTOMIZE_SERVICE_PATHS
```

For `my-service`, the entries should point to:

```text
src/services/my-service/
src/services/my-service/Dockerfile
openshift/kustomize/services/my-service/
```

## 5. Update Tests

Update:

```text
.github/scripts/test_detect_deploy_changes.py
```

At minimum, make sure:

- `src/services/<service>/**` builds and deploys the service.
- `openshift/kustomize/services/<service>/**` deploys the service.
- `deploy_scope: all` includes the service.

If the new service changes the all-scope counts, update those expected numbers.

## 6. Verify

Run:

```powershell
python .github/scripts/test_detect_deploy_changes.py
actionlint .github/workflows/auto-deploy.yml .github/workflows/deploy-all.yml .github/workflows/<service>-cicd.yml
git diff --check
```

After the workflow is on GitHub, use `Deploy All (Orchestrated)` with
`deploy_scope: changed` to test the service deployment.
