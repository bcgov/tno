# Unbounded Media Analytics Reports

Status: **Phases 1-3 implemented** on `feature/large-report-aggregations` (2026-11-04), with the
differences listed under "Implementation notes". Phase 4 stays parked. Where a product decision was
required, the **default** below applies unless the product owner changes it.

## Problem

Reports originally ran the reporting worker out of memory. PR #2690 bounded the pipeline at
10,000,000 estimated characters (`libs/net/core/Reporting/ReportSizeLimit.cs`) and shows
"Report too large" in the subscriber app. That stops the crash but turns the failure into a hard
ceiling, and the reports that hit it most exist only to feed **media analytics sections** (charts
of story counts and sentiment over time). Those reports do not need to list a single story. They
need to count them.

Goal: a chart section works for **any number of matching stories**. Sections that display stories
stay bounded (an email cannot render 100,000 stories).

## How reports run today

### Pipeline

1. A `ReportRequestModel` (`libs/net/kafka/Models/ReportRequestModel.cs`) arrives on the
   `reporting` Kafka topic, keyed `report-{reportId}`. Each pod handles one message at a time
   (`Kafka.Consumer.MaxThreads: 1`, `IsLongRunningJob`). Prod: 3 pods, **525Mi** memory limit
   (`openshift/kustomize/services/reporting/overlays/prod/kustomization.yaml`).
2. `ReportingManager.PopulateReportInstance` (`services/net/reporting/ReportingManager.cs`)
   calls `GET /api/services/reports/{id}/content` →
   `ReportService.FindContentWithElasticsearchAsync` (`libs/net/dal/Services/ReportService.cs`).
   For every section with a `FilterId` it runs the filter's stored Elasticsearch query, with the
   report's exclusions appended by the `JsonDocument` extensions in
   `libs/net/dal/Extensions/JsonDocumentExtensions.cs` (`AddExcludeContent`, `AddExcludeSources`,
   `AddExcludeMediaTypes`, `IncludeOnlyLatestPosted`, `ExcludeBCUpdates`), and returns **every hit
   as a full `ContentModel`** (headline, summary, **body**, tone pools, topics, tags, labels,
   actions, file references). Sections with a `FolderId` load the same from PostgreSQL with 13
   `Include`s. `MediaAnalytics` sections with their own filter or folder are **materialized the
   same way** even though the report template never lists their stories.
3. The worker deserializes everything into `TNO.TemplateEngine.Models.ContentModel`, writes one
   `report_instance_content` row per story, renders the Razor body, then for each enabled
   `MediaAnalytics` section builds a `ChartEngineContentModel`
   (`libs/net/template/Models/Reports/ChartEngineContentModel.cs`) whose `Content` is the
   concatenation of every `Content`-type section (`UseAllContent`), or the section's own / linked
   content, and runs the chart's Razor template (`ReportEngine.GenerateReportBodyAsync`,
   `libs/net/template/ReportEngine.cs` ~lines 483–530). The template groups in memory through
   `ReportExtensions.GroupContent`, `GetLabels`, `ExtractDatasetValues`, `GetColors`
   (`libs/net/template/ReportExtensions.cs`), producing Chart.js JSON. `GenerateBase64ImageAsync`
   posts that JSON to the Node charts API (`api/node`, `POST /base64/{chartType}`) and splices
   the PNG into the body where `[[chart-{sectionId}-{chartId}]]` stands.
4. The body is saved to `report_instance.body` and emailed through CHES. A send of an existing
   instance re-fetches **all** instance content (`GET /api/services/report/instances/{id}/content`,
   bodies included), even when nothing displays it.
5. The API has the same exposure: `ReportHelper.GenerateReportAsync` /
   `PreviewReportInstanceAsync` (`api/net/Areas/Helpers/ReportHelper.cs`) and
   `ReportService.GenerateReportInstanceAsync` (subscriber `POST reports/{id}/generate`) run the
   same fetch in the API process.
6. The subscriber template tab (`/reports/:id/settings/sections`) renders charts live in the
   browser with Chart.js (`app/subscriber/src/features/my-reports/edit/settings/template/charts/ChartViewer.tsx`),
   grouping the instance content client-side (`utils/getChartData.ts`, `groupData.ts`,
   `convertToChart.ts`) — a second implementation of the Razor grouping that also needs the full
   content list, or random test data when the report has no instance.

### Where the memory goes

The same story is held several times over: ES `_source` → API `ContentModel` → HTTP JSON → worker
`ContentModel` → copied into every section it appears in plus the chart aggregate list → Razor
buffers → body string → JSON back to the API → EF. For a chart the useful information is one
number per bucket.

### The guard

- Preflight estimate: `ReportSizeValidator` (`libs/net/template/ReportSizeValidator.cs`),
  `ReportInstanceService.EstimateReportSize`, `api/net/Areas/Helpers/ReportSizeValidation.cs`:
  256 characters + headline + summary per story per section (+ body when `ShowFullStory`, + image
  when `ShowImage`). A section that only feeds charts still pays per story, so a chart-only report
  trips the limit at roughly 20,000–40,000 stories.
- Exact bound on rendered output (`ReportSizeLimit.Validate` while Razor writes,
  `ReportSizeLimit.Replace` on chart splice).
- `ReportTooLargeException` → HTTP 400 → "Report too large" in `ReportView.tsx` /
  `ReportInstanceView.tsx` (`useReportInstances.ts` matches `data.type === 'ReportTooLargeException'`).
  The worker does not requeue it (`AddRequestToQueue`).

### A second, silent limit

Each section filter is one `_search` with the filter's `size` (`FilterSettingsModel.Size`,
default 10; `generateQuery.ts` copies it into the stored query). Elasticsearch caps `from + size`
at `index.max_result_window` (default 10,000; `index-content.json` does not override it). A chart
built from hits already **undercounts** once a filter matches more than its `size`. Aggregations
count every matching document and remove this limit as a side effect.

## Design

### Principle: charts aggregate, lists materialize

| Purpose | Data path | Bound |
| --- | --- | --- |
| Display stories (Content, Gallery, AI, TableOfContents) | fetch hits, store `report_instance_content`, render | 10M-character limit, unchanged |
| Chart data (MediaAnalytics) | Elasticsearch **aggregations** (`size: 0`) → Chart.js datasets | O(buckets), independent of story count |

Full specification of the aggregation mapping, bucket conversion and labels is in
[01-aggregation-chart-data.md](01-aggregation-chart-data.md).

### Chart data sources

The editor already lets a `MediaAnalytics` section pick a filter, folder or linked report, or
"Content In This Report" (`app/subscriber/src/features/my-reports/components/DataSources.tsx`,
`settings/data-sources/ReportSectionMediaAnalytics.tsx` sets `settings.useAllContent`). The
schema needs nothing new for sources.

| Source | Aggregation input | Phase |
| --- | --- | --- |
| Content in this report (`useAllContent`) | `terms` on `id` with the **instance's content ids** of each enabled `Content`-type section, one query per section | 1 |
| Linked report (`linkedReportId`) | `terms` on `id` with the linked report's current instance content ids | 1 |
| Own filter (`filterId`) | the filter query, built exactly as for a content section, `size: 0`. **No hits fetched, nothing stored in `report_instance_content`.** | 2 |
| Own folder (`folderId`) | `terms` on `id` with the folder's content ids | 2 |

### Mixed reports: listed stories plus charts

A chart over a listed section must **not** re-run the section's filter at render time. The instance
is a snapshot: the owner may have removed or added stories, de-duplication already ran, folder
contents moved on, new content has been indexed. Charts over listed sections therefore aggregate
over the instance's content ids. That is exact for what is listed, carries every edit and
de-duplication, needs no story bodies, and is bounded by the story limit itself (≤ ~39,000 ids
under the 10M limit, below `index.max_terms_count` 65,536; the service chunks at 10,000 regardless).

| Report shape | Chart input | Outcome |
| --- | --- | --- |
| Story sections + chart `useAllContent` | instance ids per content section | Matches the listed stories exactly |
| Story sections + chart with own filter | filter aggregation | Unbounded chart beside bounded stories |
| Chart-only report (own filter) | filter aggregation | No content rows at all |
| Chart over a linked report | ids of the linked instance | Bounded because that instance was |
| `dataset`/`groupBy = reportSection` across both kinds | one query per section | Each section is a bucket |
| Story section that exists only to feed a chart and exceeds the limit | owner converts the chart to an own-filter source and removes the story section | Chart unbounded; stories no longer listed |
| Custom Razor chart walking `Content` | legacy in-memory path (phase 1); bucket mode (phase 3) | Keeps working for listed sections |
| Edit → preview → send of an unsent instance | snapshot recomputed on generate/regenerate | Same staleness rules as stories today |
| Resend, retry, history | `report_instance_chart` snapshot | Identical output later |

### Snapshot per instance

Instances are snapshots. With no content rows to replay for analytics-only sources, the computed
chart dataset is the snapshot: table `report_instance_chart`, one row per instance × section ×
chart template, holding the Chart.js JSON. Specified in
[02-analytics-only-sections.md](02-analytics-only-sections.md).

### Live chart preview in the template tab

Stays real-time and client-rendered with Chart.js. Its data comes from a subscriber endpoint that
runs the same aggregation service for the section's **draft** (unsaved) chart settings, for both
listed and analytics-only sources, so the preview equals the emailed PNG. Client-side grouping is
retired; random test data stays for reports without an instance. Specified in
[02-analytics-only-sections.md](02-analytics-only-sections.md).

### Where the code lives

- Query building and bucket → dataset conversion: **`libs/net/dal`** (new
  `ReportChartDataService`), beside `ReportService`, reusing its exclusion builders. Registered
  automatically (`ServiceCollectionExtensions.AddTNOServices` registers every `IBaseService`
  implementation by convention; the class needs an `IReportChartDataService` interface). Testable
  from `libs/net/tests/dal` (which may not reference `services/net`, see memory rule).
- API: services-area endpoints for the worker, subscriber-area endpoints for the UI. No new
  background work in the API.
- Worker: `ReportingManager` fetches chart data through `IApiService`
  (`libs/net/services/Helpers/ApiService.cs`) and hands it to the engine; the engine renders a chart
  from pre-computed data when present.
- Charts API (`api/node`): unchanged. Only the producer of its input changes.

## Decisions

Defaults the implementation follows. Change them here before starting the affected phase.

| # | Question | Default |
| --- | --- | --- |
| 1 | Counting semantics for analytics-only sources (own filter/folder) | Count every matching story. No 3-day title de-duplication, no cross-section de-duplication between two analytics-only sections. Listed sections are unaffected (their charts use the instance ids). |
| 2 | Sentiment source | All `tonePools` entries of a story (`nested` aggregation over `tonePools.value`, no owner filter). Today's code takes `TonePools.FirstOrDefault()`; stories almost always carry one pool. |
| 3 | Date gaps and bucket density | Match today: only dates with content (`min_doc_count: 1`), no automatic interval coarsening, chart width left to `AutoResize`. `terms` aggregations use `size: 1000`; exceeding it sets `truncated` on the result and is logged. |
| 4 | Custom Razor chart templates | Phase 1: they keep the legacy content path (listed sections and linked reports only). Phase 3: buckets exposed to Razor so they can opt into aggregate mode. An analytics-only source combined with a legacy template is rejected at save time (see 02). |
| 5 | Snapshot | Yes: `report_instance_chart`, written once per generate/regenerate, read by every later render. |
| 6 | Migrating existing failing reports | Owners convert them in the editor (remove the story section, point the chart at the filter). The "Report too large" message explains this. No admin bulk conversion. |
| 7 | List-path hardening (phase 4) | Parked. Specified in [04-list-path-hardening.md](04-list-path-hardening.md); schedule only if a display report needs more than 10,000 stories. |
| 8 | `byline` grouping before `byline.keyword` exists | Phase 1–2: `byline` charts stay on the legacy content path; an analytics-only source may not select `byline` (API 400, option hidden in the UI). Phase 3 lifts this. |
| 9 | `series` label for stories without a `seriesId` | Aggregate mode labels them `None` (today: `OtherSeries ?? "None"`; `otherSeries` is `text` and not aggregatable). |

## Implementation notes

Where the build differs from the phase documents, because the repository required it:

| Topic | Built | Why |
| --- | --- | --- |
| Migrations | One EF migration, 1.5.16, for phases 1-3 | All phases share one branch and PR (one migration per PR). |
| Flagged templates | Only `Custom` | `Count`, `Sentiment Count`, `Average Sentiment` and `Sentiment` group by hand, so they fail the rule in phase 1 and their output differs from the converter. |
| Which sections stop fetching stories | A `MediaAnalytics` section with its own filter or folder, **only if every chart aggregates** and the filter declares no `aggs` of its own | Saved reports use legacy templates with their own filter, Event of the Day among them. Skipping them would chart nothing, and report templates read `section.Aggregations`. |
| Save-time rejection | Not built; decision 4's rejection and decision 8's rejection do not apply | With the rule above those sections keep working on the legacy path, bounded by the size limit. Rejecting them would stop existing reports from being saved. |
| Client-side grouping | Kept for templates that do not aggregate and for test data | They have no server data to show. |
| Shared types | `ChartDataModel`, `ChartDatasetModel` and the colour helpers moved to `libs/net/models` | `libs/net/dal` cannot reference `libs/net/template`. `ChartDatasetModel.Label` also serialized as `labels`; it is now `label`. |
| Bucket templates | The API runs the Razor template and stores the resulting Chart.js JSON | The snapshot is always Chart.js JSON, so a resend reads one shape, and `libs/net/dal` cannot run Razor. |
| `None` sentiment bucket | A `filter` aggregation for stories without a tone pool | Exact, and works inside a second dimension, where subtracting counts cannot. |
| Series fallback | A sibling `filter` on missing `seriesId`, then `terms` on `otherSeries.keyword` | Same result as a sub-aggregation of bucket 0, with no bucket 0 to special-case. |
| Stale charts | An unsent instance recomputes a chart whose section or chart settings changed after it was computed | Otherwise a saved settings change would not show in previews until a regenerate. Sent instances never recompute. |
| Report without an instance | Added `GET services/reports/{id}/charts` | The worker can run with `GenerateInstance = false`, and analytics-only sections have no stories then. |
| Draft endpoint | Also checks the body's filter, folder and linked report belong to the user or the report | The body names what to count. |
| ES 1.0.12 copy | Added `keepAnalysis` to `NativeReindex` | The 1.0.11 script removes `analysis` because 1.0.11 rebuilt it from the database. 1.0.12 has no such step and would have lost it. |
| ES 1.0.12 rollback | `rollback/` index files, through the same steps as 1.0.11 | Matches the current tool rather than the older `down/` scripts. |
| Tno-core | App code calls `useApi` directly, and reads the new settings with a local type | The interface change in `libs/npm/core` is unpublished, and the apps build against the published package. |

Decided: **Event of the Day and Topic Analysis keep their Razor templates and are not rewritten.**
The `Event of the Day` report template, which `MMI Morning Media Analysis` uses, has its own two
charts (`Topic Analysis - Topic Score`, `Topic Type - last 28 days`). They stay on the legacy path
and under the size limit. The subscriber app only offers `Custom`, so no other template needs the
aggregate path.

### Rollout

1. Deploy the API and worker, then run `make db-update` (EF 1.5.16).
2. In each environment, run ES migration 1.0.12 live (`tools/elastic/migration/README.md`).
3. After cutover, set `Reports__ByLineAggregation=true` on the API. It only needs to be set there.

## Phases

Each phase ships on its own, in order. One EF migration per PR (see memory rule).

| Phase | Document | Delivers | Schema |
| --- | --- | --- | --- |
| 1 | [01-aggregation-chart-data.md](01-aggregation-chart-data.md) | `ReportChartDataService`; services endpoints; engine renders from pre-computed data; built-in chart templates flagged `aggregate`; worker and API preview use it for `useAllContent` and linked-report charts. Exact counts (no 10,000 cap); the worker stops holding chart content. | one EF migration: PostUp SQL flags the seeded chart templates; no DDL |
| 2 | [02-analytics-only-sections.md](02-analytics-only-sections.md) | Own-filter/folder `MediaAnalytics` sections no longer materialized; `report_instance_chart`; subscriber draft-chart-data endpoint; `ChartViewer` fetches data; stories-tab summary; too-large guidance. **Unblocks the failing reports.** | one EF migration: `report_instance_chart` |
| 3 | [03-byline-keyword-and-razor-buckets.md](03-byline-keyword-and-razor-buckets.md) | ES migration 1.0.12 adding `byline.keyword` and `otherSeries.keyword`; buckets exposed to Razor chart templates. | ES index version 1.0.12 |
| 4 | [04-list-path-hardening.md](04-list-path-hardening.md) | Optional: `_source` excludes, `search_after` paging, per-section rendering. | none |

## Cross-cutting rules

- No code in `services/net` that `libs/net/tests` would need; keep logic in `libs/net/dal` and
  `libs/net/template`.
- Work still arrives through the `reporting` Kafka topic; no polling, no job tables.
- `dotnet format --verify-no-changes`, `yarn lint` (zero warnings) and prettier on every frontend
  file touched.
- Rebuild map: `libs/net/**` changes → `make refresh n=api` and `make refresh n=reporting`;
  `app/subscriber/src` hot-reloads; `libs/npm/core` changes → re-pack into `app/subscriber`
  (`make pack n=subscriber`) then `make refresh n=subscriber`.
- Every new endpoint carries the same `[Route]` quadruple as its controller's siblings and
  Swagger attributes.

## Glossary

- **Listed section**: a section whose stories render in the report (type `Content`, `Gallery`,
  `AI`, `TableOfContents`). Has `report_instance_content` rows.
- **Analytics-only source**: a `MediaAnalytics` section with its own `filterId` or `folderId`.
  After phase 2 it has no `report_instance_content` rows.
- **Chart data**: the Chart.js JSON `{ labels: string[], datasets: [...] }` that the charts API
  renders. `ChartDataModel` / `ChartDatasetModel` in `libs/net/template/Models/Charts`.
- **Aggregate mode**: a chart whose data is produced by `ReportChartDataService` instead of its
  Razor template.
