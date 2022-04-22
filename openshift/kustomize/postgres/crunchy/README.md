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
