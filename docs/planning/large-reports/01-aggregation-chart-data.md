# Phase 1 — Aggregation Chart Data

Produce every built-in chart's data from Elasticsearch aggregations instead of from materialized
stories. After this phase the worker and the API preview no longer hold chart content in memory
for `useAllContent` and linked-report charts, counts are exact (no `max_result_window` cap), and
the groundwork for analytics-only sources (phase 2) exists.

## Scope

In:

- `ReportChartDataService` in `libs/net/dal` with its request/result models.
- Services-area API endpoints the worker calls.
- `ReportEngine` renders a chart from pre-computed data when present.
- `ReportingManager` and `ReportHelper` compute chart data for `MediaAnalytics` sections whose
  chart templates are in aggregate mode and whose source is `useAllContent` or a linked report.
- Seeded chart templates `Count`, `Sentiment Count`, `Average Sentiment`, `Sentiment` and
  `Custom` flagged `aggregate` after verification (see "Chart template flag").
- Tests.

Out (later phases): own-filter/folder sources without materialization, snapshot table, subscriber
endpoints, UI, `byline`, custom Razor templates on buckets.

## Behaviour to reproduce

The aggregate path must produce the same `labels`/`datasets` JSON the `Custom` chart template
produces today (`libs/net/dal/Migrations/1.0.163/Up/PostUp/00-Chart.sql`) for the same stories.
The reference implementation is `ReportExtensions` (`libs/net/template/ReportExtensions.cs`):
`GroupContent` (grouping keys and empty-value names), `GetLabels`/`GetLabel` (label formatting),
`ExtractDatasetValues` (values), `GetColors`/`GetSentimentColorScript` (colours). Keep those
helpers; the legacy path still uses them.

### Dimensions

`ChartSectionSettingsModel.GroupBy` produces the labels (x axis); `Dataset` produces one dataset
per bucket; `DatasetValue` is `count` or `sentiment`. Each dimension value maps to one aggregation:

| Value | Raw key | Aggregation | Label | Empty value |
| --- | --- | --- | --- | --- |
| `""` (no dimension) | — | none (single bucket) | dataset label `Story Count`, or `Average Sentiment` when `DatasetValue == "sentiment"` | — |
| `dayMonthYear`, `monthDay` | `yyyyMMdd` | `date_histogram` on `publishedOn`, `calendar_interval: "day"`, `format: "yyyyMMdd"`, `time_zone: "America/Vancouver"`, `min_doc_count: 1` | `dd-MMM-yyyy` / `MMM-dd` from the key | — |
| `monthYear` | `yyyyMM` | `date_histogram`, `calendar_interval: "month"`, `format: "yyyyMM"`, same zone | `MMM-yyyy` | — |
| `year` | `yyyy` | `date_histogram`, `calendar_interval: "year"`, `format: "yyyy"` | key | — |
| `source` / `otherSource` | `otherSource` | `terms` on `otherSource` | key | key is the stored string; stories with empty `otherSource` produce the `""` bucket, as today |
| `mediaType` | `mediaTypeId` | `terms` on `mediaTypeId` | media type name via `IMediaTypeService` lookup | no id or unknown id → `Other` |
| `series` | `seriesId` | `terms` on `seriesId`, `missing: 0` | series name via `ISeriesService` | id 0 → `None` (decision 9) |
| `contentType` | `contentType` | `terms` on `contentType` | key (`AudioVideo`, `PrintContent`, `Image`, `Internet`; today's `ContentType.ToString()`) | — |
| `sentiment` | integer | `nested` path `tonePools` → `terms` on `tonePools.value` | key as string | stories with no tone pool → bucket `None` (count via a `missing`-style sibling: top-level `doc_count` minus the nested `reverse_nested` sum; see "Nested and None") |
| `sentimentSimple` | `Negative` / `Neutral` / `Positive` | `nested` `tonePools` → `range` on `tonePools.value` with ranges `to: 0` (Negative), `from: 0, to: 1` (Neutral), `from: 1` (Positive) | key | `None` as above |
| `topicType` | `topics.topicType` | `nested` `topics` → `terms` on `topics.topicType` | key | stories with no topics are not counted in any bucket, as today |
| `topicName` | `topics.name` | `nested` `topics` → `terms` on `topics.name` | key | as above |
| `reportSection` | section name | one query per source section (no aggregation on this dimension) | `section.Settings.Label`, `Other` when empty | — |
| `byline` | — | **not supported in phase 1** (`byline` is `text`); the chart stays on the legacy path | — | — |

Ordering: `GroupByOrder` / `DatasetOrder` (`asc` default) order buckets by **raw key**
(`order: { "_key": "asc" | "desc" }`), which is what today's `OrderBy(group => group.Key)` does on
the raw key strings. `reportSection` keeps section `SortOrder`.

`ExcludeEmptyValues` removes buckets labelled `Unknown` (`byline`), `None` (`series`,
`sentiment`, `sentimentSimple`), and, when `DatasetValue == "sentiment"`, every bucket whose
sentiment `value_count` is 0.

Each `terms` aggregation uses `size: 1000`. If `sum_other_doc_count > 0` the result sets
`Truncated = true`; the caller logs a warning naming report, section and chart.

### Values

- `count`: the bucket's `doc_count`. For a nested dimension use the `reverse_nested` document
  count, not the nested `doc_count`, so a story with two topics of the same type counts once per
  type bucket (matches `GroupBy(... Topics.Any(...))` today).
- `sentiment`: inside every bucket add `nested` `tonePools` → `sum` and `value_count` on
  `tonePools.value`. Value = `Math.Round(sum / count, MidpointRounding.AwayFromZero)` as an
  integer; `null` when `count == 0`. (Use `sum` + `value_count`, never `avg`, so chunked queries
  can be merged.)

### Two dimensions

When both `Dataset` and `GroupBy` are set, the `Dataset` aggregation is the outer one and the
`GroupBy` aggregation is its sub-aggregation. When the outer dimension is nested (`sentiment`,
`sentimentSimple`, `topicType`, `topicName`), wrap the inner one in `reverse_nested: {}` first.
When the inner one is nested, it is itself a `nested` block. Labels are the union of inner keys
across all outer buckets, ordered by `GroupByOrder`; a dataset's `data` has one entry per label,
`0` for a missing count bucket and `null` for a missing sentiment bucket (today's
`ExtractDatasetValues` yields `0` / `null` in the same cases).

### Nested and None

Stories without a tone pool count toward the `None` bucket for `sentiment` / `sentimentSimple`.
Compute it as: documents matched by the query (`hits.total.value` with `track_total_hits: true`)
minus the sum of `reverse_nested` counts of the value buckets. Likewise for the second dimension
inside an outer bucket, using the outer bucket's `doc_count`. Stories without topics are not
counted anywhere for `topicType`/`topicName`, as today.

### Datasets JSON

Each dataset carries exactly the fields the `Custom` template writes:

| Field | Value |
| --- | --- |
| `label` | dataset label (table above; for `reportSection` the section label) |
| `data` | numbers / `null`, one per label |
| `backgroundColor` | the parsed JSON value of `ReportExtensions.GetColors(settings.DatasetColors, datasetIndex, settings.Dataset, rawDatasetKey, settings.DatasetValue)` |
| `borderColor` | the parsed JSON value of `GetColors(settings.DatasetBorderColors ?? settings.DatasetColors, datasetIndex, ...)` (today the template passes `DatasetColors` for both; keep that) |
| `spanGaps` | `true` |
| `borderWidth` | `2` |
| `minBarLength` | `settings.MinBarLength` or `null` |

`GetColors` returns a JSON fragment: an array of colour strings, the literal `null`, or (for
`Dataset == ""` with `DatasetValue == "sentiment"`) a **quoted JavaScript arrow function** from
`GetSentimentColorScript`. The charts API (`api/node`) evaluates that string as a function. The
converter stores whatever `GetColors` returns, parsed as JSON, so the string form is preserved.
Verify by rendering the fixture report in both modes and comparing PNGs byte for byte.

## `ReportChartDataService` (`libs/net/dal/Services/ReportChartDataService.cs`)

Interface `IReportChartDataService : IBaseService` so `AddTNOServices` registers it.

### Request

`ReportChartDataRequest` (`libs/net/dal/Models/Reports/` or beside the service):

| Member | Meaning |
| --- | --- |
| `Report` | `Entities.Report` with `Sections` and their `ChartTemplatesManyToMany.ChartTemplate` loaded |
| `Section` | the `MediaAnalytics` `ReportSection` |
| `Chart` | the `ReportSectionChartTemplate` (gives `ChartTemplate.Settings`, `ChartTemplate.Template` and the per-section `Settings` → `ChartSectionSettingsModel`) |
| `Source` | one of `InstanceIds`, `Filter`, `Folder`, `LinkedInstanceIds` |
| `SectionIds` | for `InstanceIds` / `LinkedInstanceIds`: `IDictionary<string sectionName, long[] contentIds>` in section `SortOrder`; the keys are the source sections (`useAllContent`: every enabled `Content`-type section of the report; linked: the linked report's enabled `Content`-type sections) |
| `RequestorId` | user whose source/media type exclusions apply (`UserSources`, `UserMediaTypes`), as `FindContentWithElasticsearchAsync` does |

Phase 1 implements `InstanceIds` and `LinkedInstanceIds`. `Filter` and `Folder` are added in
phase 2 but the enum and the `switch` default (throw `NotImplementedException`) exist now.

### Result

`ReportChartDataResult`:

| Member | Meaning |
| --- | --- |
| `Data` | `ChartDataModel` (`Labels`, `Datasets`) |
| `DocCount` | total stories counted across all source sections |
| `Truncated` | any `terms` bucket list overflowed |
| `Warnings` | strings for the caller to log |

### Algorithm

1. Deserialize `Chart.Settings` to `ChartSectionSettingsModel` (section settings) and
   `Chart.ChartTemplate.Settings` to `ChartTemplateSettingsModel`.
2. Reject `GroupBy == "byline"` or `Dataset == "byline"` with `InvalidOperationException("byline
   is not available in aggregate mode")`; the caller falls back to the legacy path (see engine).
3. For each source section (one for non-`reportSection` sources is still one query per section;
   results are merged by key, counts summed, sentiment `sum`/`value_count` summed):
   - Build the query. `InstanceIds`/`LinkedInstanceIds`: `{"query":{"terms":{"id":[...]}}}`
     against `ElasticOptions.ContentIndex` (the unpublished index holds every story, so an
     unpublished story listed in a report still counts), chunked at 10,000 ids per request.
   - Set `size: 0`, `track_total_hits: true`; remove `sort`, `from`, `_source`, `highlight`,
     `collapse` if present.
   - Attach the `aggs` block from the dimension table.
   - Execute through `ITNOElasticClient.SearchAsync<JsonElement>` (or add a
     `SearchRawAsync(index, JsonDocument)` returning `JsonDocument`; the existing generic result
     model only parses hits and a fixed aggregation shape, so parse the raw response for this).
     Use `RetryElasticsearchQueryAsync` semantics as `ReportService` does.
4. Convert buckets to `ChartDataModel` per the tables above. For `reportSection` the sections are
   the buckets and no section query carries that dimension.
5. Resolve `mediaTypeId` / `seriesId` keys to names in one call each (`IMediaTypeService.FindAll`,
   `ISeriesService.FindAll`, both cached in the API already).

### Determinism

Given the same ids and settings the output is byte-identical: ordering is fully specified, label
formatting uses `CultureInfo.InvariantCulture`, and numbers are integers or `null`.

## API: services area

Controller `api/net/Areas/Services/Controllers/ReportInstanceController.cs`
(route base `api/[area]/report/instances`):

| Verb and path | Purpose |
| --- | --- |
| `GET {id}/charts?requestorId=` | Compute chart data for every enabled `MediaAnalytics` section of instance `{id}` whose chart template is in aggregate mode and whose source is supported in this phase. Source ids come from `report_instance_content` of this instance (`useAllContent`) or of the linked report's current instance (`GetCurrentReportInstance(linkedReportId, ownerId)`). Returns `IEnumerable<ReportInstanceChartModel>`. Phase 2 makes this read the snapshot; in phase 1 it computes. |

`ReportInstanceChartModel` (`libs/net/models/Areas/Services/Models/ReportInstance/`):
`InstanceId`, `SectionName`, `SectionId`, `ChartTemplateId`, `Data` (`JsonDocument`, the
`ChartDataModel`), `DocCount`, `Truncated`, `GeneratedOn`.

`IApiService` / `ApiService` (`libs/net/services/Helpers/`): `GetReportInstanceChartsAsync(long
instanceId, int? requestorId)` → `IEnumerable<ReportInstanceChartModel>`, following the pattern of
`GetContentForReportInstanceIdAsync` (`RetryRequestAsync`, `OpenClient.GetAsync`).

## Engine: render from pre-computed data

`ChartEngineContentModel` gains `JsonDocument? ChartData`. In `ReportEngine.GenerateReportBodyAsync`
(the `MediaAnalytics` loop, `libs/net/template/ReportEngine.cs` ~lines 492–528):

- If `chart.ChartData != null`: build `new ChartRequestModel(chartModel, chart.ChartData)` and call
  `GenerateBase64ImageAsync` (the existing `ChartData` bypass already skips the Razor template).
  Do **not** build the `content` list or call `getReportAsync` for that chart.
- Else: today's behaviour, unchanged.

Build the `content` list and call `getReportAsync` lazily, only when at least one chart of the
section lacks `ChartData`, so a fully aggregated section touches no content.

## Chart template flag

`ChartTemplateSettingsModel` (`libs/net/models/Settings/ChartTemplateSettingsModel.cs`) gains
`bool Aggregate` (JSON `aggregate`, default `false`). A chart template may be flagged only if its
Razor body uses exclusively `GroupContent`, `GetLabels`/`GetLabel`, `ExtractDatasetValues`,
`GetColors`/`GetColor`, and the dataset fields in the "Datasets JSON" table. The implementer
inspects each seeded template (`Count`, `Sentiment Count`, `Average Sentiment`, `Sentiment`,
`Custom` in `libs/net/dal/Migrations/1.0.112/Up/PostUp/*.sql` and later updates) and flags only
those that qualify; `Event of the Day` and `Topic Analysis - Topic Score` are **not** flagged
(they filter and score topics by hand).

Migration: one EF migration (next version after `1.5.15`) with no model change and a PostUp SQL
script `00-ChartTemplate-Aggregate.sql`:
`UPDATE chart_template SET settings = settings || '{"aggregate": true}' WHERE name IN (...)`,
idempotent. The Down script removes the key. The admin chart template form in the editor app
(`app/editor/src/features/admin/charts`) gets a checkbox "Data comes from aggregations (no template)" bound to `settings.aggregate`.

## Worker: `ReportingManager`

In `SendReportAsync` → `GenerateReportOutputAsync`, before `GenerateReportBodyAsync` (full-text
body only; the link-only body renders no charts):

1. If any enabled `MediaAnalytics` section has a chart with `Settings.Aggregate` and a supported
   source, call `Api.GetReportInstanceChartsAsync(instance.Id, request.RequestorId)`.
2. For each returned row, set `ChartData` on the matching `ChartEngineContentModel`
   (`sectionContent[sectionName].ChartTemplates` where `Id == ChartTemplateId`).
3. A chart that comes back without a row (unsupported source, `byline`, legacy template) renders
   through the legacy path. Log at Information which path each chart used.

On resend/retry the body is reused, so no chart work happens (unchanged).

## API preview: `ReportHelper`

- `PreviewReportInstanceAsync` (instance exists): call `IReportChartDataService` directly with
  `InstanceIds` from `report_instance_content`, set `ChartData` as the worker does.
- `GenerateReportAsync(ReportModel, requestorId, ...)` (no instance, subscriber `POST
  reports/{id}/preview`): the content was just fetched; pass each section's hit ids as
  `SectionIds`. No instance row is needed.

## Tests

`libs/net/tests/dal/Services/ReportChartDataServiceTest.cs` with `ITNOElasticClient` mocked
(`services.AddMockSingleton<ITNOElasticClient>()` as `ReportServiceTest` does). Assert on the
**query JSON sent** and on the **conversion of canned responses**:

- One test per dimension value in the table: correct `aggs` block (field, nested path, interval,
  time zone, format, order, size).
- Two-dimension query with nested outer and non-nested inner, and the reverse.
- `sentiment` value uses `sum` + `value_count` and rounds `AwayFromZero`; `null` when count 0.
- `None` bucket arithmetic for `sentiment` / `sentimentSimple`.
- Id chunking at 10,000 with bucket merge (counts summed, sentiment recomputed from sums).
- `ExcludeEmptyValues` per label kind; `Truncated` on `sum_other_doc_count`.
- Label formatting: `20260104` → `04-Jan-2026`, `Jan-04`; `202601` → `Jan-2026`.
- `mediaType`/`series` name resolution and `Other`/`None` fallbacks.
- `byline` rejection.
- Colours: `GetColors` fragment is preserved as parsed JSON, including the sentiment script string.

`libs/net/tests/ai/` (template engine tests live here): `GenerateReportBodyAsync` renders a chart
from `ChartData` without touching content; a section with one aggregate and one legacy chart
builds content once.

`libs/net/tests/services/`: `ReportingManager` sets `ChartData` from the API rows and logs the
path per chart.

Fixture parity test (manual, documented in the PR): a report with the `Custom` template over a
small listed section rendered legacy and aggregate produces identical `ChartDataModel` JSON.

## Acceptance

- A report with story sections and `useAllContent` charts renders the same PNGs as before.
- The worker's `GET …/content` call still happens (stories are listed) but chart generation no
  longer enumerates `ContentModel` lists; verified by the Information log line per chart.
- `useAllContent` and linked-report charts still count exactly the listed stories (which the
  filter's `size` caps). Full counts for a filter that matches more than its `size` arrive with
  own-filter sources in phase 2.
- `dotnet test libs/net/tests/dal libs/net/tests/ai libs/net/tests/services` green; `dotnet format`
  clean.
