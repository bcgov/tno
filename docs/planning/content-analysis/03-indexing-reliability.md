# Phase 3 — Indexing Reliability

Make Kafka failures visible and Elasticsearch writes ordered, so a redelivered or out-of-order
request can never let an older projection replace a newer one.

## Current behavior

- Controllers publish `IndexRequestModel` to the `index` topic **after** the database commit. A
  crash or Kafka failure in between loses the request silently.
- The indexing service reads the latest content from the API and writes full documents with no
  versioning. It commits the Kafka offset even when the Elasticsearch write fails.
- Two indexers run in PROD: the on-prem indexer and an `IndexOnly` indexer writing Elastic Cloud
  (consumer group `IndexingCloud`, index names `content` and `published_content`).
- `tools/indexer` and the Elasticsearch migration tool also write documents.

## Index requests

- Every place that publishes an index request today still does, in the same API request as the
  change. Paths that do not index today keep not indexing, except the bug fixes below.
- Content gains a `projection_revision` column, incremented in the same transaction as the change
  each index request follows.
- The API sends the requests to the `index` topic, keyed by content ID, once the transaction
  commits and before it responds. When Kafka does not accept them the request fails (HTTP 500):
  Kafka being down means the system is down. The API's producer gives up after
  `Kafka:Producer:MessageTimeoutMs` (10 seconds) rather than Kafka's 5-minute default.
- There is no queue of unsent requests and no background dispatcher.
- Hub (SignalR) messages stay post-commit and best effort.

`IndexRequestModel` gains `ProjectionRevision` and `Reason`. The indexer's side effects
(notifications, folder forwarding) are unchanged for every reason; `Reason` is recorded for metrics.

## Ordered Elasticsearch writes

- Every write passes `version_type=external_gte` with the content's projection revision, taken
  from the content the indexer fetched (the latest), not from the message. A version conflict means
  a newer projection is already indexed and counts as success.
- Deletes and unpublish deletes pass the same version. Raise `index.gc_deletes` so a delete's
  version outlives the longest redelivery delay.
- Every writer adopts this: the indexing service (on-prem and cloud), `tools/indexer`, and the
  migration tool's reindex (destination `version_type: external`).
- Enable versioning only on indexes rebuilt from the database by an Elasticsearch migration, so no
  document carries an internal version. Run it on both clusters.

## Failed writes

The indexing service retries each Elasticsearch write a configured number of times
(`Service:IndexRetryLimit`, default 3) and logs a write that still fails. A separate service that
checks and repairs missed writes is out of scope.

## Mutation paths that do not index today

Unchanged by this phase unless listed under bug fixes; recorded so the behavior is explicit.

| Path | Today |
| --- | --- |
| Subscriber content add and update | No index request |
| Services `PUT contents/{id}` without `?index=true` (transcription, auto-clipper) | No index request |
| Services content status, actions, and links endpoints | No index request |
| Notification service resetting the alert action | No index request |
| Report-instance saves updating private content and `Versions` | No index request |
| Series merge (`ExecuteUpdate`) | No index request, no `Version` increment |
| EF cascade deletes (source, media type, license, series, contributor) | Documents stay in Elasticsearch |

## Bug fixes

- **Subscriber delete** (`api/net/Areas/Subscriber/Controllers/ContentController.cs`): record a
  `Delete` index request so the document is removed.
- **`tools/indexer` transcript leak** (`tools/indexer/IndexerManager.cs:158-159`): blank the body of
  unapproved AudioVideo content before building the published model, using the same rule as the
  indexing service.
- **`MaxFailLimit`**: rename the key to `MaxFailureLimit` in every service's appsettings and config
  maps so it binds.

## Tests

- A request whose index requests Kafka does not accept fails; a rolled-back change sends none.
- Redelivered and out-of-order index messages cannot regress or resurrect a document.
- Both clusters (on-prem and Elastic Cloud index names) receive ordered writes.
- A failed Elasticsearch write is retried, then logged.
- Subscriber delete removes the document; `tools/indexer` never publishes an unapproved transcript.
