# Tekton

## Examples

- [Official Documentation](https://tekton.dev/docs/getting-started/)
- [bcgov/pipeline-templates](https://github.com/bcgov/pipeline-templates)
- [tecktoncd](https://github.com/tektoncd)
- [Tutorial](https://www.arthurkoziel.com/tutorial-tekton-triggers-with-github-integration/)

Tekton is a powerful and flexible open-source framework for creating CI/CD systems, allowing developers to build, test, and deploy across cloud providers and on-premise systems.

## Overview

Tekton pipelines provides a native Kubernetes implementation of CI/CD processes.

A Pipeline is composed of Tasks.
Each Task is composed of sequential Steps.
Each Step is run within its own container.
To automate Pipelines use Triggers.

## Pipelines

| Name                                          | Trigger | Description                                                                |
| --------------------------------------------- | ------- | -------------------------------------------------------------------------- |
| [DEV:app-editor](#build-deploy-with-template) | Git     | Builds the `dev` branch app-editor and deploys to the **DEV** environment. |
| [DEV:api-editor](#build-deploy-with-template) | Git     | Builds the `dev` branch api-editor and deploys to the **DEV** environment. |

## GitHub Triggers

To enable automation GitHub interceptors must be setup.
The TriggerBinding is generic and simply extracts parameters from the webhook.

| Template           | Types          | Description                                        | Install                                                                                  |
| ------------------ | -------------- | -------------------------------------------------- | ---------------------------------------------------------------------------------------- |
| git-webhook-secret | Secret         | A template that will generate a Secret             | `oc process -f ./triggers/git-webhook-secret.yaml \| oc create --save-config=true -f -`  |
| git-binding        | TriggerBinding | A template that generates a generic TriggerBinding | `oc process -f ./tasks/oc-build-with-template.yaml \| oc create --save-config=true -f -` |

### Build Deploy with Template

This pipeline uses native OpenShift BuildConfig and DeployConfig objects to build and deploy applications from a GitHub repository.
When a GIT Webhook event is triggered the EventListener will create a PipelineRun object that uses the `build-deploy-with-template` Pipeline.
The EventListener filters each webhook to ensure a PipelineRun object is only created when appropriate files have been added/modified/removed from a specific branch.

| Template                   | Types                          | Description                                                                     | Install                                                                                                                   |
| -------------------------- | ------------------------------ | ------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------- |
| react-nginx                | Template                       | A template that will be used to generate a BuildConfig within the pipeline task | `oc create -f ./build/react-nginx.yaml`                                                                                   |
| oc-build-with-template     | Task                           | A Task that runs the above template and then runs the generated BuildConfig     | `oc create -f ./tasks/oc-build-with-template.yaml`                                                                        |
| oc-deploy-with-tag         | Task                           | A Task that runs a DeployConfig                                                 | `oc create -f ./tasks/oc-deploy-with-tag.yaml`                                                                            |
| owasp-scanner              | Task                           | A Task that runs the OWASP ZAP scan against a target URL                        | `oc create -f /tasks/owasp-scanner.yaml`                                                                                  |
| build-deploy-with-template | Pipeline                       | A Pipeline that runs the above two Tasks                                        | `oc create -f ./pipelines/build-deploy-with-template.yaml`                                                                |
| git-ingress                | Ingress                        | An Ingress that is used by the webhook for each app                             | `oc process -f ./triggers/git-ingress.yaml \| oc create --save-config=true -f -`                                          |
| git-pipeline-with-template | TriggerTemplate, EventListener | A TriggerTemplate and EventListener for the above Pipeline                      | `oc process -f ./triggers/git-pipeline-with-template.yaml --param-file=${paramFile} \| oc create --save-config=true -f -` |
