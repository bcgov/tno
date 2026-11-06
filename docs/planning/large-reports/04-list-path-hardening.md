# Phase 4 — List-Path Hardening (optional, parked)

Raise the ceiling for sections that **display** stories, without removing the 10M-character
output limit. Not needed for chart-only reports; schedule only if a display report legitimately
needs more than 10,000 stories or the worker's memory is still a problem after phases 1–2.

Depends on nothing in phases 1–3, but is worthless before phase 2 (chart content would still be
fetched).

## Changes

### 1. `_source` filtering

`ReportService.BuildSectionQuery` (phase 2) adds `_source.excludes` per section:

| Section renders | Excluded fields |
| --- | --- |
| neither `ShowFullStory` nor `ShowImage` | `body` |
| not `ShowImage` | nothing extra (images are fetched separately by id today) |

`ReportInstanceService.GetContentForInstanceAsync` (send path) takes the set of section names
that show the full story and projects `Body` to `""` for the rest (EF `Select` into the entity, or
a split query), so a send never loads bodies that are not rendered. The AI section input
(`ReportAISectionGenerator`) still needs bodies for its source sections; pass its section names
in the "full story" set when any AI section is enabled.

### 2. Paged section searches

For a section whose filter `size` is above 10,000, or when the report setting
`Content.AllowLargeSections` (new, default `false`) is on, replace the single `_search` with a
point-in-time + `search_after` loop: open a PIT on the index, page by 1,000 sorted by
`publishedOn desc, id asc` (plus the filter's own sort first when present), stop at the filter's
`size` or when a page is short, close the PIT. Hits are appended to the same `SearchResultModel`
so callers do not change.

### 3. Per-section rendering

`ReportEngine.GenerateReportBodyAsync` renders the body as today; the Razor template is one
unit. Splitting it per section means a template change for every report template and is only
worth it if the single body string is measured as the dominant allocation. Measure first with
`dotnet-counters` on the reporting pod while rendering the largest allowed report; proceed only if
the Razor buffer is more than half of the peak.

### 4. Limit review

After 1–3 ship, re-measure peak working set for a report at the 10M-character limit on the prod
memory limit (525Mi). If headroom allows, raise `ReportSizeLimit.MaximumCharacters` in one step
and record the measurement in this document.

## Tests

- Query JSON includes the right `_source.excludes` per section settings.
- `GetContentForInstanceAsync` returns empty bodies for non-full-story sections and full bodies
  for AI source sections.
- Paged search: mocked `ITNOElasticClient` returns three pages; the merged result preserves order
  and respects `size`; the PIT is closed on success and on failure.

## Acceptance

- Sending a 10,000-story headline-only report loads no bodies (log the total characters fetched).
- A filter with `size: 25,000` and `AllowLargeSections` lists 25,000 stories in a headline-only
  section under the character limit.
