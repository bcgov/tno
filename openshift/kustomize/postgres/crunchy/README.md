# CrunchyDB HA Cluster

Original information source from Exchange Lab [here](https://github.com/bcgov/how-to-workshops/tree/master/crunchydb/high-availablility).

To install the CrunchyDB HA Cluster in the **dev** namespace, run the following command.

`oc kustomize ./overlays/dev | oc create -f -`

## Manually Backup

To perform an adhoc backup of the database run the following command.

```
oc annotate -n 9b301c-test postgrescluster postgres-cluster \
  postgres-operator.crunchydata.com/pgbackrest-backup="$( date '+%F_%H:%M:%S' )"
```

Subsequent runs will require the `--overwrite` attribute.

```
oc annotate -n 9b301c-test postgrescluster postgres-cluster --overwrite \
  postgres-operator.crunchydata.com/pgbackrest-backup="$( date '+%F_%H:%M:%S' )"
```

# Change Fix

The Crunchy DB Operator is getting an upgrade! We are moving from our current 5.0 version to 5.3! This will provide a few new much-requested features, and is required for the 4.12 Openshift upgrade.

When?

The operator will be upgraded on Thursday May 25th.

Will there be an impact on the Platform apps?

Most Crunchy databases running images compatible with our current v5.0 operator will break once the upgrade is performed! Fortunately, you can upgrade your images to a 5.3 compatible version before the operator upgrade. These images have been tested as compatible with both the current and new versions of the operator. If you upgrade before Thursday May 25th, you should see no impact to your application.

Please upgrade your images as follows:

Postgres 13: no update
**Postgres 14: crunchy-postgres:ubi8-14.7-0**
Postgres 14 GIS 3.1: crunchy-postgres-gis:ubi8-14.7-3.1-0
Postgres 14 GIS 3.2: crunchy-postgres-gis:ubi8-14.7-3.2-0
Postgres 14 GIS 3.3: crunchy-postgres-gis:ubi8-14.7-3.3-0
Postgres 15: crunchy-postgres:ubi8-15.2-0
Postgres 15 GIS 3.3: crunchy-postgres-gis:ubi8-15.2-3.3-0
PGAdmin: crunchy-pgadmin4:ubi8-4.30-10
**PGBackRest: crunchy-pgbackrest:ubi8-2.41-4**
**PGBouncer: crunchy-pgbouncer:ubi8-1.18-0**
**PGExporter: crunchy-postgres-exporter:ubi8-5.3.1-0**

- Add configuration  `spec.backups.pgbackrest.sidecars.pgbackrestConfig.resources`.
