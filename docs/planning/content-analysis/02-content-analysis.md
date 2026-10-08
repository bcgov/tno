# Phase 2 — Content-Analysis Service

A new service, `services/net/content-analysis`, analyzes content after it is added or updated,
persists structured analysis, populates empty editorial fields, and replaces Quote Extraction.

## Service shape

- Follows the existing service conventions (`libs/net/services` runner, `ApiService`, health,
  configuration, Docker Compose and OpenShift deployment).
- Calls models through `TNO.AI` `LlmDirectClient` with Azure AI deployments from the `llm` table,
  using the limits added in Phase 1.
- Never touches the database directly; inputs, results, and run outcomes go through services API
  endpoints. Its work arrives through Kafka.

## Eligibility

All content is eligible except administrator-configured excluded media types and sources.

- Content without usable text produces a metadata-only result.
- AudioVideo content awaiting transcript approval (`IsApproved=false`) stays pending; approval
  changes the input identity and schedules analysis.

## Triggering

Kafka is the source of analysis work; nothing is queued in the database.

A DAL hook in `TNOContext.SaveChanges` detects inserts and updates to analysis inputs and records an
analysis request (request ID, content ID, input fingerprint, reason, requested time). The API sends
the requests whose transaction committed to the `analysis` topic before it responds, keyed by
content ID (`AnalysisRequestFilter`, the same rule as index requests: when Kafka is down the request
fails). Editor reanalysis and administrator replay send a forced request the same way.

Analysis inputs (the input fingerprint):

- headline, body, byline;
- summary only when human-owned — a generated summary is never an input to its own analysis;
- `IsApproved` for AudioVideo content;
- source, media type, and publication date.

Workflow changes (status, actions, publication) and population by analysis itself do not change
the fingerprint and send no request. Series merge updates content with `ExecuteUpdate`, bypassing
the hook; series is not an analysis input, so no request is needed.

## Kafka topics

| Topic | Sent by | Consumed by | Purpose |
| --- | --- | --- | --- |
| `analysis` | API | Content-Analysis | Added and changed content, reanalysis, replay |
| `analysis-backfill` | Event Handler | Content-Analysis | Backfill (its own consumer, so it never delays new content) |
| `analysis-retry` | Content-Analysis | Content-Analysis | Failed requests waiting out their backoff |
| `analysis-dlq` | Content-Analysis | nobody (inspect in Kowl) | Requests whose retries are exhausted |
| `work-order` | API | Event Handler | Work orders, including backfills |

Every analysis message is keyed by content ID, so requests for one content item are handled in
order, and instances scale with the topics' partitions (consumer group `ContentAnalysis`).

## Consuming

Each topic has its own consumer, handling one message at a time and committing it once its outcome
is recorded (`EnableAutoCommit: false`). For each request:

- **Quiet period.** A lifecycle request waits until `requestedOn + QuietPeriodSeconds` (default 2
  minutes). Every request waits the same period, so a partition's requests become due in order.
  A retry waits until its `notBefore`; backfill, reanalysis, and replay are due at once.
- **Latest input wins.** The current input is fetched by content ID. A request whose fingerprint no
  longer matches the content is skipped — a newer request for the changed input follows it — so a
  burst of edits is analyzed once. A request whose content already has a current analysis for the
  same fingerprint is skipped unless it is forced.
- **Ineligible** content (excluded, or a transcript awaiting approval) is recorded as `Skipped`; its
  approval changes the fingerprint and sends a new request.
- **Failures.** A transient failure is sent to `analysis-retry` with exponential backoff and jitter
  (`RetryDelaySeconds`, doubling, up to `MaxRetryDelaySeconds`) and recorded as `Retrying`; after
  `MaxAttempts` (default 5), or for a permanent failure, it is sent to `analysis-dlq` and recorded
  as `Failed`, for explicit replay.
- **Not the content's fault.** A misconfigured LLM, or an unreachable API or Kafka, is not
  committed: the consumer returns to the message and the failure counts toward the service
  sleeping, so nothing is lost and no attempt is spent.
- A cancelled backfill's requests are skipped (the work order's status is cached briefly).
- Provider rate limits from the `llm` table bound throughput; backfill requests are held to
  `BackfillShare` of them. The limiter is per instance.

## Run history

The outcome of each request is kept in `content.metadata` under `analysis`: the newest five runs
(request ID, reason, status, fingerprint, requested and finished times, attempts, backfill work
order, analysis ID, error), ordered by when their request was made, and the newest run's status.
Retries of a request update its run. An older request that finishes after a newer one never
replaces the newer outcome as the status.

The metadata is written only by targeted SQL under a row lock (`ContentAnalysisService.RecordRun`),
never through the entity: it does not change the content's version or projection revision, and a
stale editor form cannot overwrite it (the EF property is ignored on insert and update). A partial
index (`IX_content_analysis_failed`) finds content whose newest run failed.

## Processing

1. Fetch the content's current input.
2. Normalize text, keeping offsets into the source.
3. Copy authoritative metadata (source, dates, byline).
4. Split oversized text on paragraph/sentence boundaries within the model budget; overlap only for
   boundary context and deduplicate overlap results. Never truncate.
5. Extract structured information from every chunk.
6. Validate the schema, source spans, and that quotes are verbatim.
7. Merge duplicate findings; keep ambiguous identities separate.
8. Produce a bounded summary from validated facts.
9. Submit the result with the input hash and the request's run.

## Analysis record

`content_analysis` stores, per content ID and input hash:

- normalization, schema, prompt, and model versions;
- summary and supporting key facts;
- people, organizations, aliases, and roles;
- places and their roles in the story;
- topics, primary topic, suggested tags, and suggested contributor;
- reported events (actor, action, date, location) — distinct from workflow `Actions`; extraction
  never executes actions;
- verbatim quotes with speaker and source offsets;
- validation results, timestamps, and token usage.

Flexible payloads are validated JSONB beside relational keys and status. Store the input hash, not a
copy of the input. Inferred values are marked as inferred; missing dates, locations, or attribution
are left empty rather than invented.

Analysis records, including superseded ones, last as long as their content: the foreign key to
`content` cascades on delete, and the content's evidence index documents are removed with it.

## Acceptance

The API accepts a submission only when the content exists and is eligible and the input hash is
current. A duplicate submission returns the existing result; a stale one is rejected without
populating anything (a newer request for the changed input follows it).

In one transaction, acceptance:

1. stores the analysis;
2. applies allowed population (below);
3. increments `Version` when editorial fields changed;
4. records the index request, which the API sends to Kafka after commit.

After commit it sends a hub `ContentUpdated` message with reason `analysis`, and the editor form
merges populated fields and the new version the way it handles `quotes` today
(`app/editor/src/features/content/form/hooks/useContentForm.ts`). Acceptance never sends another
analysis request.

## Population and ownership

New `content_field_ownership` records who owns each populated field or collection value: `human`,
`analysis`, or `automation`, plus a cleared marker when a human removed a value.

- Migration marks every existing populated value `human`, including past Quote Extraction quotes
  and automation writes.
- Editor and subscriber saves mark changed values `human`; clearing a field records the cleared
  marker. Automation saves mark values `automation`.
- Analysis fills a field only when it is empty and not cleared, and refreshes or removes only
  values it owns when their supporting analysis changes. It never changes `human` or `automation`
  values.

| Field | Population rule |
| --- | --- |
| Summary | Fill an empty summary with the generated summary. |
| Tags | Add matches to existing enabled `Tag` codes when the content has none; never create tags. |
| Contributor | Set when empty and the byline/columnist matches an existing `Contributor` (including aliases); never create contributors. |
| Quotes | Add validated verbatim quotes with speaker, deduplicated against existing quotes; preserve quote IDs and `IsRelevant` choices. |
| Topics | Governed by the topic controls below. |

Unmatched tag and contributor suggestions stay in the analysis record.

`Quote` gains provenance columns (owner, analysis ID, source offsets).

## Topics

The staff-managed `Topic` list is important to users, so population is controlled:

- the Content-Analysis service assigns topics only when it runs its `Topics` process;
- mode (`TopicPopulationMode`, on the topics admin page): assign existing active topics only, or
  allow creating topics;
- each assigned topic is scored by the topic score rules, like any other calculated score. Topic population depends on the
[topic scoring refactor](05-topic-scoring.md), which must ship first.

Extracted topics that match no staff topic are kept in a versioned analysis topic registry with
stable IDs and deterministic aliases (no fuzzy-only merging). Report synthesis uses the staff topic
when matched, otherwise the registry label.

## API

Services area (service account):

- fetch a content item's input, submit a result, record a run (skipped, retrying, failed);
- read a backfill's next page;
- publish analysis requests and work orders (Kafka producer area).

Editor/admin area:

- read a content item's analysis, field ownership, and recent runs;
- request reanalysis;
- read the requests waiting in each topic (consumer-group lag), list failed content and replay it;
- manage excluded media types/sources and topic population settings.

## Elasticsearch

Migrations run against both clusters: on-prem (local, DEV) and Elastic Cloud (TEST, PROD), where
the indexing service's index names differ.

- Add an `analysis` object to the content mappings with `dynamic: strict` on that object only.
  The rest of the document keeps dynamic mapping, which `source`, `mediaType`, `owner`, `series`,
  and `quotes` depend on today.
- Field types: `keyword` for IDs, topics, entities, versions, and status; `text` for summaries and
  evidence; `date` for timestamps; `nested` for quote, entity, and event relationships;
  `geo_point` only for verified coordinates.
- Add an evidence index for report synthesis. Each document carries `sourceId`, `mediaTypeId`,
  `isApproved`, and publication status so the existing source and media-type exclusions apply and
  unapproved transcripts never reach subscriber output. Documents are removed or replaced with
  their content.
- Retrieve complete evidence sets with PIT and `search_after`. Stories are grouped by the topic on
  each evidence document, so no topic aggregation is needed.
- The evidence index is written on every cluster.

## Replacing Quote Extraction and NLP

1. Fix the quotes endpoint to send an index request (ships immediately, independent of the rest).
2. Remove `services/net/extract-quotes` and its deployment. Content-Analysis extracts and applies
   quotes when its service is configured to run the `Quotes` process.
5. Retire `services/net/nlp` (not deployed today): the service, its `nlp` topic, the editor NLP
   work-order request, and its OpenShift folder.

## Tests

- New content and relevant updates record one analysis request; unrelated changes, rolled-back
  saves, and analysis acceptance do not.
- Analysis proceeds while Elasticsearch is unavailable.
- Stale results and duplicate submissions cannot overwrite current data; an older request's run
  never replaces a newer one's status; saving content never overwrites its run history.
- Human edits, clears, and automation values survive population.
- An open editor form merges populated fields without a conflict on its next save.
- Processes: only the configured processes run, and only their results are applied.
- Topic controls: mode, and scores from topic score rules.
- Long articles, transcripts, empty text, invalid output, ambiguous entities, and quote evidence.
- Unapproved transcripts are neither analyzed nor exposed through the evidence index.
- Quality evaluation against a reviewed corpus.
- Duplicate alert rate on analysis re-index (see [accepted risks](README.md#accepted-risks)).
