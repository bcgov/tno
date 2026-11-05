# Phase 3 — `byline.keyword`, `otherSeries.keyword`, and Buckets for Razor

Lift the two phase 1 restrictions: `byline` grouping in aggregate mode, and custom Razor chart
templates on aggregated data.

Depends on phases 1 and 2.

## Part A — Elasticsearch migration 1.0.12

### Mapping change

Both content indexes (`index-content.json` and `index-published.json`, copied from
`tools/elastic/migration/Migrations/1.0.11/indexes/`) gain keyword sub-fields:

| Field | Today | Added |
| --- | --- | --- |
| `byline` | `text` | `fields: { keyword: { type: "keyword", ignore_above: 256 } }` |
| `otherSeries` | `text` | `fields: { keyword: { type: "keyword", ignore_above: 256 } }` |

Everything else is identical to 1.0.11. Adding a sub-field to a `text` field is a mapping update
Elasticsearch accepts in place, but existing documents are not re-indexed for it: the keyword
value appears only for documents indexed after the change. A chart over a multi-year range needs
the values on every document, so the migration re-indexes.

### Migration mechanics

`tools/elastic/migration/Migrations/1.0.12.cs` (`[Migration("1.0.12")]`, `: TNOMigration`) with
`SupportsSteps => true` and the same four steps as 1.0.11 (`prepare`, `copy`, `verify`,
`cutover`), reusing its native `_reindex` copy and alias cutover. Differences from 1.0.11:

- `prepare` creates `content_v1.0.12` and `unpublished_content_v1.0.12` from the new index JSON,
  carrying over the sources' dynamic mappings (`source`, `mediaType`, `series`, `owner` objects)
  as 1.0.11 does.
- `verify` compares document counts and a sample of `byline.keyword` values; there is no analysis
  rehydration step.
- Follow the live procedure in `tools/elastic/migration/README.md`: a second indexing service
  writes to the destinations between `prepare` and `cutover`; writers are never paused
  (memory rule: no maintenance-window migrations).
- `down/` recreates the 1.0.11 aliases on the previous indexes as 1.0.10's `down/pre` scripts do.

Test on `it_*` indexes locally before DEV (memory rule: run the built dll from `bin/` so the
`.env` does not clobber `Elastic__*` overrides).

### Aggregation changes after cutover

`ReportChartDataService`:

| Value | Aggregation | Label | Empty |
| --- | --- | --- | --- |
| `byline` | `terms` on `byline.keyword` | key with `<mark>`/`</mark>` stripped (today's `GroupContent` strips them) | missing or empty → `Unknown` (`missing: ""` then relabel) |
| `series` | `terms` on `seriesId`, plus for `seriesId` 0 a sub-`terms` on `otherSeries.keyword` | series name, else `otherSeries`, else `None` | restores today's `Series?.Name ?? OtherSeries ?? "None"` |

Remove the `byline` rejection in the service, the save-time validation, and the UI option hiding
added in phases 1–2. Gate on a configuration flag `Reports:ByLineAggregation` (default `false`)
flipped to `true` once the cutover is done in each environment, so the API and worker can deploy
before the index.

## Part B — Buckets for custom Razor chart templates

Lets `Event of the Day`, `Topic Analysis - Topic Score` and user-authored templates run without
stories.

### What the template receives

`ChartEngineContentModel` gains `ChartAggregations` (`Dictionary<string sectionName,
AggregationRootModel>`), filled by a new `ReportChartDataService.AggregateAsync(request,
JsonDocument aggs)` that runs a template-declared `aggs` block per source section and converts
the response with the existing `SearchAggregationHelper` into
`TNO.TemplateEngine.Models.Reports.AggregationRootModel` (buckets with `Key`, `DocCount`,
`AggregationSum`, `ChildAggregation`). `ReportSectionModel.Aggregations` and
`ReportExtensions.GetTotalScoresByTopicType(aggregations)` /
`GetTopicScoresByTopicTypeAndName(aggregations)` already consume this shape.

### Declaring the aggregation

`ChartTemplateSettingsModel` gains `JsonDocument? Aggregations`: the Elasticsearch `aggs` block
the template needs (for example a `nested` `topics` → `terms` on `topics.topicType` with a `sum`
on `topics.score`). When set, the chart runs in **bucket mode**: the service executes that block
per source section (filter query, ids, or folder as in phase 2), sets `ChartAggregations`, and the
Razor template renders the Chart.js JSON from it. `Content` is empty in bucket mode.

A template is therefore in one of three modes, checked in this order:

1. `settings.aggregate == true` → built-in converter, no Razor (phase 1).
2. `settings.aggregations != null` → bucket mode, Razor over `ChartAggregations`.
3. neither → legacy, Razor over `Content` (listed sections and linked reports only).

Save-time validation from phase 2 accepts modes 1 and 2 for analytics-only sources.

### Rewriting the seeded templates

- `Event of the Day` (`1.0.153/Up/PostUp/01-ChartTemplate-EventOfTheDay.sql`): declares a
  `nested` `topics` aggregation with `terms` on `topicType` and `name`, `sum` on `score`, and a
  `filter` excluding `name: "Not Applicable"`; the Razor body is rewritten over
  `ChartAggregations`. Output must match the current output for the same stories; keep the
  current template text in the migration's Down script.
- `Topic Analysis - Topic Score`: same approach.

One EF migration with PostUp SQL updating `chart_template.template` and `settings` for both, and
Down restoring them.

### Admin editor

The editor app's chart template form (`app/editor/src/features/admin/charts`) gains a JSON editor
for `settings.aggregations` next to the Razor template, and the admin preview
(`ChartTemplateController` `POST preview/json` / `POST preview/base64`,
`ChartPreviewRequestModel`) accepts `Aggregations` so an administrator can see buckets while
writing the template. `ChartPreviewRequestModel` already carries `Filter`/`FilterId`/`FolderId`/
`LinkedReportId`; in bucket mode the controller runs `AggregateAsync` against them instead of
`FindContentAsync`.

## Tests

- Migration: unit test on the 1.0.12 index JSON (sub-fields present, everything else equal to
  1.0.11); the `verify` step on a local `it_*` index.
- `ReportChartDataServiceTest`: `byline` query and `Unknown` relabel; `series` with
  `otherSeries.keyword` fallback; `AggregateAsync` converts a canned nested response into
  `AggregationRootModel`.
- Template tests (`libs/net/tests/ai`): rewritten `Event of the Day` renders identical JSON from
  buckets as the old template did from a fixture content list.
- Flag off/on behaviour for `Reports:ByLineAggregation`.

## Acceptance

- After cutover, `byline` and `series` charts over an own-filter section render in aggregate mode.
- `Event of the Day` renders for an own-filter section with no stories fetched.
- Rollback (`down`) restores the 1.0.11 aliases and the previous template text.
