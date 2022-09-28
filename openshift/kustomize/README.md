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
