# Patroni PostgreSQL Cluster Database

Patroni is a template for you to create your own customized, high-availability solution using Python and - for maximum accessibility - a distributed configuration store like ZooKeeper, etcd, Consul or Kubernetes. Database engineers, DBAs, DevOps engineers, and SREs who are looking to quickly deploy HA PostgreSQL in the datacenter-or anywhere else-will hopefully find it useful.

More information on Patroni > [here](https://patroni.readthedocs.io/en/latest/)

PostgreSQL is a powerful, open source object-relational database system with over 30 years of active development that has earned it a strong reputation for reliability, feature robustness, and performance.

More information on PostgreSQL > [here](https://www.postgresql.org/)

## How to Setup in Openshift

Install the service account.
This account is used by Patroni to create additional objects within Kubernetes.

```bash
# Create an `.env` file with the parameters for each namespace.
oc process -f 01-service-account.yaml --param-file=${paramFile:-01-service-account.dev.env} | oc create --save-config=true -f -
# Grant the new service account pull permission to the TOOLS namespace.
oc policy add-role-to-user system:image-puller system:serviceaccount:$(oc project --short):${nameOfServiceAccount:tno} -n 9b301c-tools
```

Create a secrets configuration for Postgres settings.
Secrets are a separate file so that they are not regenerated when the deploy is updated.

```bash
oc process -f 02-secrets.yaml --param-file=${paramFile:-02-secrets.dev.env} | oc create --save-config=true -f -
```

Create a Permanent Volume Claim (PVC) to enable future backup of the database.
The PVC is a separate file so that it doesn't get deleted by accident when making changes to the deploy.

```bash
# The backup PVC is stored on a storage type that is constantly backed up by the Exchange Lab.
# The database is not stored on this volume, but once configured it will place backups on this volume.
oc process -f 03-pvc.yaml --param-file=${paramFile:-03-pvc.dev.env} | oc create --save-config=true -f -
```

Now you can install the Patroni PostgreSQL statefulset cluster.
A satefulset is used to ensure each pod is consistently mapped to the same PVC and has the same identification information.

```bash
# This installs a service to communicate with the cluster and the cluster itself.
oc process -f 04-deploy.yaml --param-file=${paramFile:-04-deploy.dev.env} | oc create --save-config=true -f -
```
