# Kustomize

To setup monitoring of your PostgreSQL HA cluster you will need to install [Kustomize](https://kustomize.io/).

Information on installation [here](https://kubectl.docs.kubernetes.io/installation/kustomize/).

For Windows installation. Open a command prompt as an administrator.

`choco install Kustomize`

## Kustomize Command

```bash
oc kustomize ./overlays/dev | oc create -f -
```

## Solution Installation

| Service                                                 | Description                                               |
| ------------------------------------------------------- | --------------------------------------------------------- |
| [CrunchyDB HA Cluster](./postgres/crunchy/README.md)    | High availability PostgreSQL database cluster.            |
| [CrunchyDB HA Monitoring](./postgres/monitor/README.md) | High availability PostgreSQL database cluster monitoring. |

## Setup Environment

To setup each environment start with the Kafka installation.
This will create a service-account in the environment that is required.
If you have issues with the Kafka broker, you may need to delete the log files.
On a new environment the easiest way to do this is to delete the pods and pvc for each broker.

```bash
# Data
oc kustomize kafka/broker/overlays/test | oc create -f -
oc kustomize kafka/postgres/crunchy/overlays/test | oc create -f -
oc kustomize elastic/overlays/test | oc create -f -
oc kustomize postgres/crunchy/overlays/test | oc create -f -
```

Once the database, Kafka, and Elasticsearch are running successfully.
Deploy the application components.

```bash
oc kustomize nginx/overlays/test | oc create -f -
oc kustomize api/overlays/test | oc create -f -
oc kustomize app/editor/overlays/test | oc create -f -
```

Once the application is running you will need to do the following.

- Ensure Keycloak is setup correctly in the environment
- Update the `tno` database

Keycloak needs to be setup manually.

- Realm requires roles, groups, clients, and mappings to be configured appropriately.
- Ensure the `api` and `editor` are configured with the correct keycloak realm settings.

The simplest way to update the database requires the following steps.

- Update the `/libs/net/dal/.env` with the appropriate configuration for the environment
- Run the `make db-connect e=test` command in the `/openshift` folder.
- Run the `make db-update` command in the `/libs/net` folder.

Deploy the services below.

```bash
oc kustomize services/syndication/overlays/test | oc create -f -
oc kustomize services/capture/overlays/test | oc create -f -
oc kustomize services/clip/overlays/test | oc create -f -
oc kustomize services/image/overlays/test | oc create -f -
oc kustomize services/filemonitor/overlays/test | oc create -f -
oc kustomize services/content/overlays/test | oc create -f -
oc kustomize services/indexing/overlays/test | oc create -f -
oc kustomize services/transcription/overlays/test | oc create -f -
oc kustomize services/nlp/overlays/test | oc create -f -
```

Deploy the tools below.

```bash
oc kustomize cron/overlays/test | oc create -f -
```

To update either the `services` or `ches` shared configMaps.

```bash
oc kustomize shared_resources/overlays/dev/ | oc apply -f -
```