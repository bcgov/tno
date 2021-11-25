# PostgreSQL Singleton Database

PostgreSQL is a powerful, open source object-relational database system with over 30 years of active development that has earned it a strong reputation for reliability, feature robustness, and performance.

More information on PostgreSQL > [here](https://www.postgresql.org/)

## How to Setup in Openshift

Create a Permanent Volume Claim (PVC) to enable future backup of the database.
The PVC is a separate file so that it doesn't get deleted by accident when making changes to the deploy.

```bash
# The backup PVC is stored on a storage type that is constantly backed up by the Exchange Lab.
# The database is not stored on this volume, but once configured it will place backups on this volume.
oc process -f 01-pvc.yaml --param-file=${paramFile:-01-pvc.dev.env} | oc create --save-config=true -f -
```

Create a secrets configuration for Postgres settings.
Secrets are a separate file so that they are not regenerated when the deploy is updated.

```bash
oc process -f 02-secrets.yaml --param-file=${paramFile:-02-secrets.dev.env} | oc create --save-config=true -f -
```

Now you can install the PostgreSQL database.

```bash
# This installs a service to communicate with the database and the database itself.
oc process -f 03-deploy.yaml --param-file=${paramFile:-03-deploy.dev.env} | oc create --save-config=true -f -
```
