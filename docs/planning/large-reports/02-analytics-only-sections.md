# Phase 2 — Analytics-Only Sections and Snapshots

Make a `MediaAnalytics` section with its own filter or folder chart **every** matching story
without fetching or storing a single one, persist the computed chart data per instance, and give
the subscriber UI a live preview that does not need content in the browser. This phase is what
lets the reports that fail today run.

Depends on phase 1 (`ReportChartDataService`, engine `ChartData`, aggregate flag).

## Scope

In:

- `Filter` and `Folder` sources in `ReportChartDataService`.
- Stop materializing `MediaAnalytics` sections everywhere content is fetched.
- Entity `ReportInstanceChart`, table `report_instance_chart`, one EF migration.
- Snapshot write on generate/regenerate; read on every later render.
- Services endpoints: read snapshot, (re)compute and save.
- Subscriber endpoints: draft chart data for the live preview; snapshot read for an instance.
- Subscriber UI: `ChartViewer` data from the API; stories-tab summary for analytics sections;
  section options; too-large guidance.
- Validation: aggregate-mode requirements for analytics-only sources.
- Retention counts.
- Tests.

Out: `byline`/`otherSeries` keyword fields and Razor buckets (phase 3); list-path changes (phase 4).

## Sources: `Filter` and `Folder`

### Filter

Extract from `ReportService.FindContentWithElasticsearchAsync` (`libs/net/dal/Services/ReportService.cs`
~lines 1182–1330) the per-section query construction into a reusable method on `ReportService`:

`BuildSectionQuery(Report report, ReportSection section, SectionQueryContext context)` →
`(string index, JsonDocument query)`

where `SectionQueryContext` carries what the loop computes once per report today:
`excludeHistoricalContentIds`, `excludeReportContentIds`, `excludeAboveSectionContentIds`,
`previousInstance?.PublishedOn`, `reportSettings`, `ownerId`. The method applies, in this order,
exactly as the loop does: `AddExcludeContent` (historical/report/above-section ids per
`OverrideExcludeHistorical` and `RemoveDuplicates`), `AddExcludeSources`,
`AddExcludeMediaTypes`, `IncludeOnlyLatestPosted` (when `OnlyNewContent`), `ExcludeBCUpdates`
(when `OmitBCUpdates`), and chooses `ContentIndex` when the filter's `SearchUnpublished` is true,
otherwise `PublishedIndex`. `FindContentWithElasticsearchAsync` then calls this method itself, so
the two paths cannot drift.

`ReportChartDataService` with `Source == Filter` calls `BuildSectionQuery`, then applies the
phase 1 transformation (`size: 0`, `track_total_hits`, strip `sort`/`from`/`_source`/`highlight`/
`collapse`, attach `aggs`). The filter's `size` is ignored by construction.

Exclusion lists in `AddExcludeContent` can be large (`ExcludeHistorical` over up to 10
instances). They already exist in today's queries; no change, but the service logs the count of
excluded ids at Debug.

### Folder

`Source == Folder`: content ids from `FolderContents.Where(fc => fc.FolderId == section.FolderId)`
ordered by `SortOrder`, then the `InstanceIds` path (terms on `id`, chunked). Report-level
exclusions do not apply to folders today beyond `RemoveDuplicates`/`OverrideExcludeHistorical`,
which are id exclusions; apply the same exclusion ids by set difference before querying.

### `reportSection` with analytics-only sources

Each source section remains one query; an analytics-only section contributes its filter query as
its bucket. `RemoveDuplicates` between two analytics-only sections is not applied (decision 1).

## Stop materializing `MediaAnalytics` sections

Every place that fetches or stores section content skips sections whose
`SectionType == MediaAnalytics`:

| Code | Change |
| --- | --- |
| `ReportService.FindContentWithElasticsearchAsync(Report, …)` | `continue` for `MediaAnalytics` sections; they add nothing to `excludeAboveSectionContentIds` |
| `ReportService.FindContentWithElasticsearchAsync(ReportInstance, ReportSection, …)` (section regenerate) | return empty for a `MediaAnalytics` section |
| `ReportService.GenerateReportInstanceAsync` | no `ReportInstanceContent` rows for them |
| `ReportService.RegenerateReportInstanceSectionAsync` | for a `MediaAnalytics` section, recompute and save its snapshot instead (see below) |
| `ReportingManager.PopulateReportInstance` | the dictionary built from `searchResults` has no entry for them; `ReportInstanceContent` rows are not created; the image-fetch loop ignores them |
| `ReportHelper.GenerateReportAsync` | same |
| `ReportInstanceService.EstimateReportSize` / `ReportSizeValidator` | no change needed: they only count rows and `section.Content`, which are now empty for these sections. Add a test asserting an analytics section with a 100,000-story filter estimates 0 |

Existing instances keep their old `report_instance_content` rows for `MediaAnalytics` sections.
They are harmless: nothing reads them after this phase, and the aggregation ignores them. No data
migration deletes them.

## Entity and migration

`libs/net/entities/ReportInstanceChart.cs`, `AuditColumns`:

| Column | Type | Notes |
| --- | --- | --- |
| `instance_id` | bigint | PK part, FK → `report_instance.id`, cascade delete |
| `section_id` | int | PK part, FK → `report_section.id`, cascade delete |
| `chart_template_id` | int | PK part, FK → `chart_template.id`, cascade delete |
| `section_name` | varchar(100) | denormalized for lookups by name, as `report_instance_content` does |
| `data` | jsonb | the `ChartDataModel` JSON |
| `doc_count` | bigint | stories counted |
| `is_truncated` | boolean | any terms overflow |
| `generated_on` | timestamptz | |

Configuration `libs/net/dal/Configuration/ReportInstanceChartConfiguration.cs` modelled on
`ReportInstanceContentConfiguration`. `ReportInstance` gains `ICollection<ReportInstanceChart>
Charts`. `TNOContext` gains `DbSet<ReportInstanceChart> ReportInstanceCharts`.

Migration: the single EF migration of this PR (next version after the phase 1 one), generated
with `dotnet ef migrations add`, inheriting `SeedMigration`. No SQL scripts needed.

## Snapshot lifecycle

| Event | Action |
| --- | --- |
| Instance generated (worker `PopulateReportInstance` after `AddReportInstanceAsync`; API `ReportController.Generate` when it creates or regenerates) | compute every aggregate-mode chart of every enabled `MediaAnalytics` section and **save** rows |
| Section regenerate (`POST reports/{id}/sections/{sectionId}/regenerate`) on a `MediaAnalytics` section | recompute and overwrite that section's rows |
| Instance regenerated (`Generate(regenerate: true)` on an unsent instance) | recompute and overwrite all rows |
| Render (worker send, API preview of an instance, resend, retry) | **read** rows; a chart without a row (instance predates this phase, or source unsupported) is computed on the fly and, when the instance exists, saved |
| Owner edits instance content (add/remove stories) and previews | listed-section charts (`useAllContent`, linked) are recomputed from the current ids on the next generate/regenerate, exactly like story content; the preview reads the stored rows. Document this in the UI copy: "Charts refresh when the report is regenerated." |
| Instance purged by `HistoryRetentionService` | cascade; `HistoryRetentionService` adds `report_instance_chart` to the per-table counts it reports beside `report_instance_content` |

Writes go through `ReportInstanceService.SaveCharts(long instanceId, IEnumerable<ReportInstanceChart>)`
(delete-then-insert for the affected `(instance, section)` pairs, in one transaction).

## API: services area

`api/net/Areas/Services/Controllers/ReportInstanceController.cs`:

| Verb and path | Behaviour |
| --- | --- |
| `GET {id}/charts?requestorId=` | (phase 1 endpoint) now returns stored rows; computes and saves missing ones |
| `POST {id}/charts?requestorId=` | recompute and save all rows for the instance; returns them. Called by the worker after it creates the instance |

Both require the instance's `Report` with sections and chart templates loaded
(`ReportInstanceService.FindById(long id)` already includes
`Report.Sections.ChartTemplatesManyToMany.ChartTemplate`; see its `Include` chain around line 154).

`IApiService`: `GenerateReportInstanceChartsAsync(long instanceId, int? requestorId)` (POST) beside
the phase 1 GET.

## API: subscriber area

`api/net/Areas/Subscriber/Controllers/ReportController.cs` (route base `api/[area]/reports`):

| Verb and path | Behaviour |
| --- | --- |
| `POST {id}/sections/{sectionId}/charts/{chartTemplateId}/data` | Body: `ChartDataPreviewRequestModel`. Computes chart data for the section with the **draft** settings and source in the body; nothing is stored. Authorization: report owner, or `report.IsPublic`. Returns `ChartDataModel` (200), or 204 when the source is `useAllContent`/linked and no instance exists yet (the UI then shows test data). 400 for `byline` in aggregate mode or a legacy template with an analytics-only source |

`ChartDataPreviewRequestModel` (`libs/net/models/Areas/Subscriber/Models/Report/`):

| Member | Meaning |
| --- | --- |
| `SectionSettings` | `ChartSectionSettingsModel`, the form's current values |
| `UseAllContent` | bool |
| `FilterId`, `FolderId`, `LinkedReportId` | the form's current data source (nullable) |
| `InstanceId` | optional; the unsent instance to take ids from for `useAllContent` (default: current instance of the report for this user) |

`api/net/Areas/Subscriber/Controllers/ReportInstanceController.cs`:

| Verb and path | Behaviour |
| --- | --- |
| `GET {id}/charts` | stored rows for the instance as `ReportInstanceChartModel` (subscriber-area copy of the model) |

## Validation (save time)

In `ReportService.AddAndSave`/`UpdateAndSave` validation of a report (where sections are
validated today, or a new `ReportValidator` in `libs/net/dal`), for every `MediaAnalytics` section
with `FilterId` or `FolderId`:

- every attached chart template must have `Settings.Aggregate == true`, else
  `BadRequestException("Section '{label}': chart '{name}' needs a template that supports
  aggregation when the section has its own filter or folder.")`;
- `SectionSettings.GroupBy`/`Dataset` must not be `byline` (until phase 3), else
  `BadRequestException("Section '{label}': grouping by byline is not available for a section with
  its own filter or folder.")`.

The subscriber editor hides the `byline` option and non-aggregate templates for such sections
(`ReportSectionMediaAnalyticsChartWizard.tsx` / `ChartPicker.tsx` / `ConfigureDataset.tsx`), and
shows the API message if the save is rejected anyway.

## Subscriber UI

All under `app/subscriber/src/features/my-reports/edit/`.

### Live chart preview (template tab)

- `libs/npm/core/src/hooks/api/subscriber/useApiSubscriberReports.ts`: add
  `getSectionChartData(reportId, sectionId, chartTemplateId, body)` → `IChartData`.
  `app/subscriber/src/store/hooks/subscriber/useReports.ts`: expose it.
- `settings/template/ReportSectionMediaAnalyticsChart.tsx`: replace the `convertToChart(...)`
  effect with a debounced (400 ms) call to `getSectionChartData` whenever `section.filterId`,
  `section.folderId`, `section.linkedReportId`, `section.settings.useAllContent` or
  `chart.sectionSettings` change. A 200 sets `data`; a 204 sets `data` to
  `convertToChart(section, chart, testData, values.sections)` and shows the existing "Use test
  data" toggle in its on state; an error shows the message in place of the chart. The "Use test
  data" toggle keeps working (it forces the test-data branch).
- `ChartViewer.tsx`: unchanged (it already takes `data: IChartData`).
- Delete the client-side grouping once nothing imports it: `utils/getChartData.ts`,
  `groupData.ts`, `getDatasets.ts`, `separateDatasets.ts`, `calcDataPoint.ts`,
  `calcAverageSentiment.ts`, `isStoryCount.ts`. Keep `convertToChart.ts` only if the test-data
  branch needs it; otherwise generate test `IChartData` directly in `ReportEditContext.tsx`
  (`testDataConfig`, `regenerateTestData`) and delete `convertToChart.ts` too. Keep
  `generateChartOptions.ts`, `calcAutoSize.ts`, `shouldResizeChart.ts`, colour helpers.
- Update `app/subscriber/src/test-vitest` tests that import the removed utils.

### Stories tab (`content/stories/ReportSectionMediaAnalytics.tsx`)

Today it lists `sectionContent` for a section with `filterId`/`folderId`. Replace with a summary
card per chart from `GET report/instances/{id}/charts` (`useReportInstances.getReportInstanceCharts`):
chart name, `docCount` stories counted, `generatedOn`, and a "Truncated" badge when
`isTruncated`. Remove the empty-content messages for these sections.

### Data-source options (`settings/data-sources/ReportSectionMediaAnalytics.tsx`)

When `filterId` or `folderId` is set, hide "Remove duplicate stories" and "Remove duplicate titles
from the last 3 days" (they do not apply, decision 1) and show the sentence "Charts count every
story the filter matches. Stories in this section are not listed in the report."

### "Report too large" copy

`ReportTooLargeException.UserMessage` (`libs/net/core/Exceptions/ReportTooLargeException.cs`)
becomes: "This report has grown too large to run. Remove stories or narrow the date range and
filters, then refresh the report before previewing or sending it again. If a section only feeds a
chart, point the media analytics section at the filter directly and remove the story section; charts
have no size limit. Existing stories have not been removed." Update the two tests that assert on
the message (`libs/net/tests/api/ReportSizeValidationTest.cs`, subscriber vitest files matching
`Report too large`).

## Worker changes

`ReportingManager.PopulateReportInstance`: after `AddReportInstanceAsync` / `UpdateReportInstanceAsync`
succeeds, call `Api.GenerateReportInstanceChartsAsync(instance.Id, request.RequestorId)` and keep
the rows for `GenerateReportOutputAsync`, which sets `ChartData` as in phase 1. For
`GenerateAndSendReportAsync(request, instance)` (existing instance), call
`GetReportInstanceChartsAsync` (reads rows, computes missing). Failures surface as
`ReportingException` with `ReportingErrors.FailedToGenerateOutput` and follow the existing retry
rules.

## Tests

- `libs/net/tests/dal/Services/ReportChartDataServiceTest.cs`: `Filter` source applies every
  exclusion `BuildSectionQuery` applies (compare the query sent with the one
  `FindContentWithElasticsearchAsync` sends for the same section, minus `size`/`sort`/`from`,
  plus `aggs`); `Folder` source ids; `reportSection` buckets across one listed and one
  analytics-only section.
- `libs/net/tests/dal/Services/ReportServiceTest.cs`: `FindContentWithElasticsearchAsync` and
  `GenerateReportInstanceAsync` skip `MediaAnalytics` sections; `RegenerateReportInstanceSectionAsync`
  on a `MediaAnalytics` section rewrites its snapshot rows only.
- `libs/net/tests/dal/Services/ReportSizeEstimateTest.cs`: analytics-only section estimates 0.
- `libs/net/tests/dal/Integration/`: `report_instance_chart` cascade on instance delete; retention
  count line.
- `libs/net/tests/api/`: subscriber draft endpoint authorization (owner, public, other user → 403),
  204 without instance, 400 for `byline` and for a legacy template with an own filter; save-time
  validation messages.
- `libs/net/tests/services/`: worker calls `GenerateReportInstanceChartsAsync` once per generated
  instance and `GetReportInstanceChartsAsync` for an existing instance; a chart without a row falls
  back to legacy.
- Subscriber vitest: `ReportSectionMediaAnalyticsChart` debounces and calls the endpoint, handles
  200/204/error; stories-tab summary renders `docCount`; too-large copy.

## Acceptance

- A report with only `MediaAnalytics` sections, each on a filter matching more than 100,000
  stories, generates, previews, sends and resends; `report_instance_content` has no rows for it;
  worker memory stays flat (check the pod's working set during the run).
- A mixed report (story sections + `useAllContent` chart + own-filter chart) renders the story
  chart from the instance ids and the filter chart from the aggregation, and its preflight counts
  only the story sections.
- Regenerating a section or an instance refreshes the stored rows; resend reuses them.
- The template tab shows the same numbers as the emailed PNG for the same instance.
- `make db-update` applies the migration; `dotnet test` green; `yarn lint` and `yarn test` green in
  `app/subscriber`.
