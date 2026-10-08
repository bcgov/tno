# Implementation Notes

Status of the plan in this folder as built on the `content-analysis` branch (2026-09-30). The phase
documents describe the design; this records what was built, where it deviates, how to deploy it,
and what still needs a decision.

## Delivery

All phases and both independent workstreams shipped with database migration `1.5.6` and one
Elasticsearch migration. Moving analysis work from a database job table to Kafka followed with
database migration `1.5.7` (which also covers moving bulk topic rescore to an Event Handler work
order) and Kafka topic migration `1.0.1`.

| Migration | Contents |
| --- | --- |
| EF `1.5.6` (`libs/net/dal/Migrations/20260930220328_1.5.6.cs`) | `llm` limits; `report_ai_result`; topic scoring columns, `topic_rescore_job`, score check constraint; analysis tables (`analysis_job`, `content_analysis`, `content_field_ownership`, `analysis_topic`, `analysis_backfill`), quote provenance; `content.projection_revision`; history indexes. SQL: `Migrations/1.5.6/Up/PreUp/00-TopicScoring.sql`, `Up/PostUp/00-HistoryRetention.sql`, `Up/PostUp/01-TopicScoring.sql`, `Down/PreDown/*`. |
| EF `1.5.7` (`libs/net/dal/Migrations/20261007235520_1.5.7.cs`) | Drops `analysis_job` and `analysis_backfill` (Kafka is the source of analysis work; backfills are work orders) and `topic_rescore_job` (bulk rescores are work orders); adds `content.metadata` (`jsonb`, run history under `analysis`). SQL: `Migrations/1.5.7/Up/PostUp/00-ContentAnalysisMetadata.sql` (partial index `IX_content_analysis_failed`), `Down/PreDown/*`. |
| Kafka `1.0.1` (`db/kafka/migrations/V1.0.1.sh`) | Topics `analysis-backfill`, `analysis-retry`, `analysis-dlq`, `work-order`. |
| Elasticsearch `1.0.11` (`tools/elastic/migration/Migrations/1.0.11*`) | Rebuilds the content indexes **from the database** with external versions (projection revision), adds `analysis` (`dynamic: strict`), `topics.isSystem`, `projectionRevision`, `index.gc_deletes: 1h`, and creates the evidence index. See [Elasticsearch migration safety](#elasticsearch-migration-safety). |

## Status

| Document | Built |
| --- | --- |
| 01 Report synthesis | `TNO.AI` tokens, chunker, synthesizer; `ReportAISectionGenerator`; `report_ai_result` store (API + reporting service); LLM limits in the admin form; AI scope / source sections / output mode on AI sections (editor and subscriber). Analyzed stories contribute evidence (summary, facts, entities, quotes), ordered by topic so related stories share a request. |
| 02 Content analysis | `services/net/content-analysis` consuming the Kafka analysis topics (lifecycle, retry, and backfill consumers; dead-letter topic) and running the processes it is configured for; analysis requests recorded by the DAL save hook and sent by `AnalysisRequestFilter`; run history in `content.metadata`; ownership, population, topic registry; services/editor/admin endpoints; editor Analysis tab; `/admin/content-analysis`. Quote Extraction and NLP services removed. |
| 03 Indexing reliability | Projection revisions incremented in the save's transaction; index requests sent to Kafka by the API before it responds (a Kafka failure fails the request); versioned writes with retries in the indexing service, `tools/indexer`, and the migration tool; subscriber delete, `tools/indexer` transcript leak, and `MaxFailLimit` fixed. No reconciliation (out of scope). |
| 04 Backfill | Backfill work orders (`AnalysisBackfillService`), run a page at a time by the Event Handler (`ContentAnalysisBackfillHandler`); admin endpoints and panel. |
| 05 Topic scoring | Calculator, save hook, bulk rescore work orders (run by the Event Handler), rebuilt `/admin/topic-scores`, system topic flag. |
| 06 History retention | Purge service, `Purge History` schedule (02:00 daily), admin panel; purged report links show Not Found. |

## Deviations and decisions made while building

- **Ownership default.** A value with no `content_field_ownership` record is human-owned, so the
  migration inserts nothing for existing content. Records are written for analysis-populated
  values, automation writes, and human clears.
- **Index action for derived requests.** Call sites that know only the content ID
  (`RequestIndex(id)`) take the action from the saved status. Content in `Unpublish` status
  therefore gets an `Unpublish` request where the old code sent `Index` (topics save, score reset,
  rescore). Unpublishing is idempotent in the indexer.
- **Bulk editor actions** now always send hub messages (before, they were sent only when the index
  topic was configured).
- **Bulk rescore is a work order** (`WorkOrderType.TopicRescore`) the Event Handler runs a page at a
  time (`TopicRescoreHandler`); each page is rescored by `TopicScoreService.RescorePage` in a services
  API request, so a page and its index requests commit together. EF `1.5.7` drops
  `topic_rescore_job`.
- **Content-Analysis processes.** There is no on/off or shadow mode. The service's
  `Service:Processes` (config map `PROCESSES`) lists what it runs — `Metadata` (key facts, people
  and organizations, places, events, topics), `Summary`, `Quotes`, `Tags`, `Contributor`,
  `Topics` — and submits that list with each result; only those fields are applied (still only
  when empty and not set by a person). Empty runs nothing. Only `Contributor` needs no LLM. The
  processes are recorded in the analysis (`validation.processes`). Changing them does not
  reanalyze existing stories; use a forced backfill.
- **Index requests are sent immediately.** The save increments the content's projection revision
  in its transaction; a global filter (and the rescore job, per batch) sends the saved requests to
  Kafka once the transaction commits, before the API responds. Kafka not accepting them fails the
  request with a 500 (the change itself is saved). Requests from a rolled-back transaction are
  never sent. Analysis requests follow the same rule.
- **Failed index writes.** The indexing service retries each Elasticsearch write
  `Service:IndexRetryLimit` times (config map `INDEX_RETRY_LIMIT`, default 3) with a growing delay,
  then logs the failure and moves on. There is no reconciliation.
- **Expired report links.** `GET subscriber/report/instances/{id}/view` answers `204` when the
  instance no longer exists; the "view on web" page shows Not Found.
- **CoreNLP removed** (compose, `tools/corenlp`, OpenShift `core-nlp`, CI, scripts). A local
  `tno-corenlp` container from before is no longer managed by `make`; remove it with
  `docker rm -f tno-corenlp`.
- **Unapproved transcripts.** `ContentModel.ToPublishedDocument()` blanks the body *and* the
  analysis of unapproved AudioVideo content; every published-index writer uses it. Report synthesis
  ignores evidence that is not approved.
- **Evidence index** is written on every cluster (on-prem and Elastic Cloud).
- **One pass over every story.** Synthesis reads all of a section's stories together, ordered by the
  topic on each evidence document so related stories share a request; topic-summary headings come
  from the findings. Synthesizing each topic on its own sent a request per story (a 1,000-story
  report: 606 requests, 19 minutes; now 28 requests, under a minute). Map and reduce requests are
  capped at `MaxBatchInputTokens` and run `MaxConcurrentRequests` at a time. Evidence sets are read
  with PIT and `search_after` (`ReportEvidenceService`).
- **Retention setting disabled** (`is_enabled = false`) or `0` disables that purge.

## Configuration

| Where | Key | Default |
| --- | --- | --- |
| API | `Kafka:Producer:MessageTimeoutMs` | 10000 (fail fast when Kafka is down) |
| API | `API:NotificationPublishedBeforeOffset` | from the notification service config map (optional) |
| API / reporting | `Reporting:Synthesis:*` | margin 10%, depth 8, 8 concurrent, 16,000 input / 8,000 output tokens per map or reduce request, 3 attempts, 900s claim, 300s timeout |
| API | `Kafka:AnalysisTopic`, `AnalysisBackfillTopic`, `AnalysisRetryTopic`, `AnalysisDeadLetterTopic`, `AnalysisConsumerGroup`, `WorkOrderTopic` | `analysis`, `analysis-backfill`, `analysis-retry`, `analysis-dlq`, `ContentAnalysis`, `work-order` |
| DAL (API) | `TopicScore:TimeZone` | `Pacific Standard Time` |
| Indexing service | `Elastic:EvidenceIndex`, `Service:IndexRetryLimit`, `Service:IndexRetryDelayMs` | `content_evidence`, 3, 1000 |
| Content-Analysis service | `Service:Processes` | `Metadata,Summary,Quotes,Tags,Contributor,Topics` |
| Content-Analysis service | `Service:Topics`, `BackfillTopic`, `RetryTopic`, `DeadLetterTopic` (empty consumes none, e.g. a backfill-only instance) | `analysis`, `analysis-backfill`, `analysis-retry`, `analysis-dlq` |
| Content-Analysis service | `Service:QuietPeriodSeconds`, `MaxAttempts`, `RetryDelaySeconds`, `MaxRetryDelaySeconds`, `BackfillShare` | 120, 5, 30, 3600, 0.2 |
| Content-Analysis service | `Kafka:Consumer` `EnableAutoCommit`, `MaxThreads` | `false`, 1 (one request at a time per consumer; scale with partitions and replicas) |
| Event Handler | `Service:WorkOrderTopics`, `AnalysisBackfillTopic`, `AnalysisBackfillPageSize`, `TopicRescorePageSize` | `work-order`, `analysis-backfill`, 500, 200 |
| Settings table | `ContentAnalysisLLMId`, excluded media types/sources, `TopicPopulationMode`, `ReportRetentionDays`, `NotificationRetentionDays` | — / — / ExistingOnly / 90 / 30 |

## Deployment order

1. Create the Kafka topics: `make kafka-update` locally, `cd openshift && make kafka-update e=<env>`
   on OpenShift (partitions per `db/kafka/environments/<env>.conf`). Deploy the API, indexing service, reporting, event-handler, scheduler, and
   content-analysis images; apply EF `1.5.6` and `1.5.7` (`make db-update` locally). Requests still
   in `analysis_job` when `1.5.7` drops it are not carried over; run a missing-or-stale backfill over
   the affected range afterwards.
2. Run Elasticsearch `1.0.11` on **each** cluster, with the indexing service's index names:
   - on-prem: `Elastic__ContentIndex=unpublished_content`, `Elastic__PublishedIndex=content`;
   - Elastic Cloud: `Elastic__ContentIndex=content`, `Elastic__PublishedIndex=published_content`,
     and — if its `migrations` index has no history — `Elastic__BaselineVersion=1.0.10` (the tool
     refuses to run otherwise).
   The evidence index (`content_evidence`) is created on both.
   Then delete the kept indexes the log names once search is confirmed.
3. Choose the Content-Analysis LLM (with limits) on `/admin/content-analysis`, and set the
   service's `PROCESSES`.

## Elasticsearch migration safety

`1.0.11` changes nothing users see until its last step, and deletes nothing.

1. Creates the new indexes (an unaliased index of the same name left by a failed run is replaced).
2. Copies the field mappings the old indexes created dynamically (`source`, `mediaType`, ...), so
   field types do not change.
3. Rebuilds every content item from the database.
4. Verifies: every content item is in the new content index at its current projection revision
   (so changes made while it ran are caught up), and every published item in the new published
   index, with nothing else. Differences are repaired once; any that remain stop the
   migration with the aliases untouched.
5. Reports documents in the old indexes whose content no longer exists in the database (they are
   not carried over, and stay in the kept old index).
6. Moves all aliases in one atomic request. An alias name held by a concrete index (a cluster that
   never used versioned indexes) is first cloned to `{name}-backup-{time}`.
7. Verifies again, repairing changes made during the switch, and logs the kept old indexes.

A cluster whose indexes exist but whose `migrations` index is empty is refused unless
`Elastic__BaselineVersion` says which version it is at, so earlier migrations never run against it.

**Rollback** (`Elastic__MigrationVersion=1.0.10`) rebuilds `{alias}_v1.0.10-rollback-{time}`
indexes from the database with the same verification and atomic switch, and keeps the 1.0.11
indexes. Roll Elasticsearch back **before** the code, while changes still increment projection revisions.

Verified locally on copies of the local indexes: up, rollback, a failed run and its rerun, and a
cluster with concrete indexes and no history (refused without a baseline; backed up with one).
Then applied to the local cluster (`make elastic-update`): 24,913 stories verified, 258 published;
the old `*_v1.0.10` indexes are kept.

## Testing

- `dotnet test libs/net/tests/ai` — synthesis, chunking, AI sections, content analyzer (fake LLM).
- `dotnet test libs/net/tests/dal` — scoring, retention rules, and, with
  `TNO_TEST_POSTGRES="Host=localhost:40000;Database=tno;Username=admin;Password=..."`, database
  integration tests of index requests (revisions, rollback), analysis claims and submission (processes applied), AI
  result claims, and purges (they add and remove their own content).
- Corpus quality evaluation (summary faithfulness, quote precision) is a manual step: backfill a
  representative week and review a sample of analyses in the editor's Analysis tab.
