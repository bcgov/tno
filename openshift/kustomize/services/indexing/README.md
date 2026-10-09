# Indexing Service

Kustomize for the indexing service, which consumes the Kafka `index` topic and writes content to Elasticsearch.

| Overlay | Deployment | Writes to | Replicas |
| ------- | ---------- | --------- | -------- |
| `overlays/dev` | `indexing-service` | DEV OpenShift Elasticsearch | 2 |
| `overlays/test` | `indexing-service` | TEST OpenShift Elasticsearch (retired; Cloud is primary) | 0 |
| `overlays/prod` | `indexing-service` | PROD OpenShift Elasticsearch (retired; Cloud is primary) | 0 |
| `overlays/cloud-dev` | `indexing-service-cloud` | Scaffold: DEV has no Elastic Cloud cluster yet. Set its URL and create the `elastic-cloud` Secret first. | 0 |
| `overlays/cloud-test` | `indexing-service-cloud` | TEST Elastic Cloud; also updates status and sends alerts, notifications and `folder` messages | 3 |
| `overlays/cloud-prod` | `indexing-service-cloud` | PROD Elastic Cloud; as TEST | 3 |
| `overlays/migration-dev` | `indexing-service-migration` | The new indexes of a DEV Elasticsearch migration | 0 until set up |
| `overlays/migration-test` | `indexing-service-migration` | The new indexes of a TEST Cloud migration | 0 until set up |
| `overlays/migration-prod` | `indexing-service-migration` | The new indexes of a PROD Cloud migration | 0 until set up |

The cloud and migration overlays share their changes through components:

- [`components/cloud`](./components/cloud): renames everything to `indexing-service-cloud`, uses the `IndexingCloud`
  consumer group, the Cloud index names (`content`, `published_content`), `INDEX_ONLY=false` (it is the only indexing
  service once the OpenShift one is retired) and the `elastic-cloud` credentials. Each `cloud-<environment>` overlay sets the cluster URL, replicas, resources and image.
- [`components/migration`](./components/migration): renames everything to `indexing-service-migration`, with its own
  `IndexingMigration` consumer group starting at the latest message, `INDEX_ONLY=true`, placeholder index names and no
  pods. `migration-dev` builds on `dev`, and `migration-test` and `migration-prod` on the cloud overlays. How to use
  it is in the [Elasticsearch migration runbook](../../elastic-migration/README.md#second-indexing-service).

Kustomize follows a renamed ConfigMap into every reference to it, so the components never patch environment
variables by position.

```bash
oc kustomize openshift/kustomize/services/indexing/overlays/cloud-test        # render
oc apply --dry-run=server -k openshift/kustomize/services/indexing/overlays/cloud-test
oc apply -k openshift/kustomize/services/indexing/overlays/cloud-test
```
