# CrunchyDB HA Cluster

Original information source from Exchange Lab [here](https://github.com/bcgov/how-to-workshops/tree/master/crunchydb/high-availablility).

To install the CrunchyDB HA Cluster in the **dev** namespace, run the following command.

`oc kustomize ./overlays/dev | oc create -f -`

## Manually Backup

To perform an adhoc backup of the database run the following command.

```bash
oc annotate -n 9b301c-test postgrescluster postgres-cluster \
  postgres-operator.crunchydata.com/pgbackrest-backup="$( date '+%F_%H:%M:%S' )"
```

Subsequent runs will require the `--overwrite` attribute.

```bash
oc annotate -n 9b301c-test postgrescluster postgres-cluster --overwrite \
  postgres-operator.crunchydata.com/pgbackrest-backup="$( date '+%F_%H:%M:%S' )"
```

## Upgrade Information

To upgrade CrunchyDB following the information provided. Note that this implementation was completed before the Exchange Lab provided a Helm chart, which means the process is slightly different.

Official documentation here - [Postgres Major Version Upgrade](https://access.crunchydata.com/documentation/postgres-operator/5.5/guides/major-postgres-version-upgrade)

Version alignment here - [Release notes](https://access.crunchydata.com/documentation/postgres-operator/latest/releases/5.5.x)

Exchange Lab Helm chart here - [Github](https://github.com/bcgov/crunchy-postgres)

Artifactory Image Tags here - [JFrog Registry](https://artifacts.developer.gov.bc.ca/ui/repos/tree/General/bcgov-docker-local)

### Step 1: Shutdown Services

Turn everything off in the environment.

```bash
cd openshift
make stop e=dev
```

### Step 2: Apply hte latest template

This will remove all image specifications so that the `spec.postgresVersion: 14` version will be managed by the operator. This process may take a while as it updates to the latest minor version You may need to restart the cluster.

```bash
oc kustomize openshift/kustomize/postgres/crunchy/overlays/dev | oc apply -f -
```

### Step 3: Take a Full Backup

You will need to wait and confirm the backup has been completed, as this is just a request for it do it when it can.

```bash
oc annotate -n 9b301c-test postgrescluster postgres-cluster \
  postgres-operator.crunchydata.com/pgbackrest-backup="$( date '+%F_%H:%M:%S' )"
```

### Step 4: Configure PGUpgrade Object

```yaml
apiVersion: postgres-operator.crunchydata.com/v1beta1
kind: PGUpgrade
metadata:
  name: crunchy-upgrade
spec:
  postgresClusterName: crunchy
  fromPostgresVersion: 14
  toPostgresVersion: 15
```

### Step 5: Shutdown and Annotate the Cluster

Link the cluster to the update.

```bash
# Annotate the cluster
oc -n 9b301c-dev annotate postgrescluster crunchy postgres-operator.crunchydata.com/allow-upgrade="crunchy-upgrade"
```

Shutdown the cluster.

```bash
oc patch postgrescluster/crunchy -n 9b301c-dev --type merge --patch '{"spec":{"shutdown": true}}'
```

### Step 6: Watch and Wait

Check on status.

```bash
oc describe pgupgrade -n 9b301c-dev
```

### Step 7: Restart

Update `PostgresCluster` object with `spec.postgresVersion: 15`.

```bash
oc patch postgrescluster/crunchy -n 9b301c-dev --type merge --patch '{"spec":{"postgresVersion": 15}}'

# Start up again
oc patch postgrescluster/crunchy -n 9b301c-dev --type merge --patch '{"spec":{"shutdown": false}}'
```

### Other Commands

If you need to restart.

```bash
oc patch postgrescluster/crunchy -n 9b301c-dev --type merge --patch '{"spec":{"metadata":{"annotations":{"restarted":"'"$(date)"'"}}}}'
```
