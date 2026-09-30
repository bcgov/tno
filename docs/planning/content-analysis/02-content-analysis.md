# Phase 2 — Content-Analysis Service

A new service, `services/net/content-analysis`, analyzes content after it is added or updated,
persists structured analysis, populates empty editorial fields, and replaces Quote Extraction.

## Service shape

- Follows the existing service conventions (`libs/net/services` runner, `ApiService`, health,
  configuration, Docker Compose and OpenShift deployment).
- Calls models through `TNO.AI` `LlmDirectClient` with Azure AI deployments from the `llm` table,
  using the limits added in Phase 1.
- Never touches the database directly; claims, inputs, and results go through new services API
  endpoints.

## Eligibility

All content is eligible except administrator-configured excluded media types and sources.

- Content without usable text produces a metadata-only result.
- AudioVideo content awaiting transcript approval (`IsApproved=false`) stays pending; approval
  changes the input identity and schedules analysis.

## Triggering

A DAL hook in `TNOContext.SaveChanges` detects inserts and updates to analysis inputs and upserts one
`analysis_job` per content ID in the same transaction, with `due_at = now + quiet period`
(default 2 minutes). A further change inside the quiet period moves `due_at` forward, so rapid
edits produce one analysis of the latest input.

Analysis inputs (the input fingerprint):

- headline, body, byline;
- summary only when human-owned — a generated summary is never an input to its own analysis;
- `IsApproved` for AudioVideo content;
- source, media type, and publication date.

Workflow changes (status, actions, publication) and population by analysis itself do not change
the fingerprint. Deleting content, including EF cascade deletes, cascades to its jobs. Series merge
updates content with `ExecuteUpdate`, bypassing the hook; series is not an analysis input, so no
job is needed.

## Work queue

`analysis_job` is the source of truth: content ID, input hash, reason (`lifecycle`, `reanalysis`,
`backfill`), priority, backfill ID, status, attempts, next attempt time, lease expiry, and fencing
token.

- Workers claim due jobs through the API (`FOR UPDATE SKIP LOCKED`), ordered by priority then due
  time. Lifecycle work outranks backfill.
- Each claim gets a lease and a new fencing token; workers renew long leases.
- A Kafka `analysis` topic message wakes idle workers after a job is created; workers also poll, so
  a lost message delays work but never loses it. Messages are committed on receipt.
- Transient failures retry with exponential backoff and jitter; after 5 attempts (configurable) the
  job fails and is shown for explicit replay.
- Expired leases return jobs to the queue.
- Provider rate limits from the `llm` table bound concurrency.

## Processing

1. Claim a job and fetch its current input.
2. Normalize text, keeping offsets into the source.
3. Copy authoritative metadata (source, dates, byline).
4. Split oversized text on paragraph/sentence boundaries within the model budget; overlap only for
   boundary context and deduplicate overlap results. Never truncate.
5. Extract structured information from every chunk.
6. Validate the schema, source spans, and that quotes are verbatim.
7. Merge duplicate findings; keep ambiguous identities separate.
8. Produce a bounded summary from validated facts.
9. Submit the result with the input hash and fencing token.

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

The API accepts a submission only when the content exists and is eligible, the input hash is
current, and the claim and fencing token are valid. A duplicate submission returns the existing
result; a stale one is rejected without populating anything.

In one transaction, acceptance:

1. stores the analysis;
2. applies allowed population (below);
3. increments `Version` when editorial fields changed;
4. records the index request (after commit until Phase 3's outbox exists).

After commit it sends a hub `ContentUpdated` message with reason `analysis`, and the editor form
merges populated fields and the new version the way it handles `quotes` today
(`app/editor/src/features/content/form/hooks/useContentForm.ts`). Acceptance never creates another
analysis job.

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

- global switch (`TopicPopulationEnabled`), off by default;
- mode (`TopicPopulationMode`): assign existing active topics only, or allow creating topics;
- each assigned topic is scored by the topic score rules, like any other calculated score.

Both settings live on the topics admin page. Topic population depends on the
[topic scoring refactor](05-topic-scoring.md), which must ship first.

Extracted topics that match no staff topic are kept in a versioned analysis topic registry with
stable IDs and deterministic aliases (no fuzzy-only merging). Report synthesis uses the staff topic
when matched, otherwise the registry label.

## API

Services area (service account):

- claim job, renew lease, fetch input, submit result, record failure.

Editor/admin area:

- read a content item's analysis and field ownership;
- request reanalysis;
- list failed jobs and replay them;
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
- Retrieve complete evidence sets with PIT and `search_after`, and enumerate topic buckets with
  composite aggregation paging.

## Replacing Quote Extraction and NLP

1. Fix the quotes endpoint to send an index request (ships immediately, independent of the rest).
2. Run Content-Analysis in shadow mode — analysis stored and compared, no population.
3. Disable the Quote Extraction writer, then enable population.
4. Remove `services/net/extract-quotes` and its deployment.
5. Retire `services/net/nlp` (not deployed today): the service, its `nlp` topic, the editor NLP
   work-order request, and its OpenShift folder.

## Tests

- New content and relevant updates create one job after the quiet period; unrelated changes and
  analysis acceptance do not.
- Analysis proceeds while Elasticsearch is unavailable.
- Stale results, expired leases, duplicate submissions, and concurrent workers cannot overwrite
  current data.
- Human edits, clears, and automation values survive population.
- An open editor form merges populated fields without a conflict on its next save.
- Topic controls: switch, mode, and scores from topic score rules.
- Long articles, transcripts, empty text, invalid output, ambiguous entities, and quote evidence.
- Unapproved transcripts are neither analyzed nor exposed through the evidence index.
- Shadow comparison against Quote Extraction output; quality evaluation against a reviewed corpus.
- Duplicate alert rate on analysis re-index (see [accepted risks](README.md#accepted-risks)).
