# Elasticsearch

Local and DEV target **9.5.7** as of 2026-11-27 and are upgraded from 8.19.25 with the plan below. Apply
`overlays/dev` to an 8.19.25 cluster only as that plan's phase 4. Local uses
`dependencies/elasticsearch/Dockerfile`; DEV pins the image in `overlays/dev/kustomization.yaml`.
TEST and PROD use Elastic Cloud. Their StatefulSets are scaled to 0, and `overlays/test` and `overlays/prod` pin
the image to 8.19.21 so that a change to the shared base image does not reach them.

The 2026-11-19 rollout to 8.19.25 completed with all three DEV nodes green. Local retained its
single-node yellow status with all primary shards active. Application alias searches passed
in both environments; the `pre-8-19-25-20261119` and `post-8-19-25-20261119` snapshots succeeded.

- [Upgrade 8.19.25 → 9.5.7 (Local and DEV), planned](UPGRADE-9.5.7.md): needs 7.x-created indexes cleared first
  with `scripts/upgrade-9.sh` (check, rebuild, delete, migrate system indexes)
- [Upgrade 7.17.9 → 8.19.21 (Local and DEV)](UPGRADE-8.19.21.md)
- [Snapshots](SNAPSHOTS.md): `scripts/snapshots.sh` (backup and restore), `scripts/rolling-update.sh` (one node at a time)

## External access (DEV)

DEV Elasticsearch has security disabled, so the `elastic` route
(`https://tno-dev.apps.silver.devops.gov.bc.ca/elastic/`) is restricted to an IP allow-list. The addresses are
set on the live route only and are not committed, because this repository is public:

```bash
oc annotate route elastic -n 9b301c-dev haproxy.router.openshift.io/ip_whitelist='<ip> [<ip> ...]' --overwrite
oc get route elastic -n 9b301c-dev -o jsonpath='{.metadata.annotations.haproxy\.router\.openshift\.io/ip_whitelist}'
```

Applying the overlay keeps the annotation, because it is not part of the manifest. Other addresses get an empty
reply. Applications are unaffected: they use the in-cluster service `http://elastic:9200`. Without the
allow-list, use `oc port-forward -n 9b301c-dev svc/elastic 9200:9200` and `http://localhost:9200`.

Generate a password file.

```bash
echo PASSWORD=$(< /dev/urandom tr -dc 'A-Z-a-z-0-9@#$%&_+=' | head -c${1:-32}) > ./base/secret.env
```

Install Elasticsearch cluster in specified environment.

```bash
oc kustomize ./overlays/dev | oc create -f -
```

View Elasticsearch status [https://tno-dev.apps.silver.devops.gov.bc.ca/elastic](https://tno-dev.apps.silver.devops.gov.bc.ca/elastic)

## Helpful Examples

- [Blog](https://portworx.com/blog/run-ha-elasticsearch-elk-red-hat-openshift/)
- [Blog](https://blog.knoldus.com/deploying-elasticsearch-on-kubernetes/)
- [GitHub](https://github.com/pires/kubernetes-elasticsearch-cluster/blob/master/es-master.yaml)
- [Helm](https://spot.io/blog/kubernetes-tutorial-successful-deployment-of-elasticsearch/)
