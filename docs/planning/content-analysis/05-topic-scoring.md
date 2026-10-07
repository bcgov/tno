# Topic Scoring Refactor

Fix and refactor how topic scores are calculated from the rules on `/admin/topic-scores`, and
rebuild that page. Independent of the other phases; it must ship before Phase 2 enables automatic
topic population, because populated topics are scored by these rules.

## What a score means

A topic score is a prominence weight — front page, image, lead time slot. Event of the Day sums
`topics.score` by topic type and topic name over the last day and reports each topic's share. The
Event of the Day filter and report helpers exclude the "Not Applicable" topic and scores of 0.

## Current behavior

- `topic_score_rule` rows (about 44 across roughly 15 sources) are matched by
  `TopicScoreHelper.GetScore` (`api/net/Areas/Helpers/TopicScoreHelper.cs`): rules for the content's
  source, in `SortOrder`, first match wins. No match returns nothing and the score stays 0.
- Scoring runs on editor add (always overwrites), editor update (only when a topic arrives without
  a score), and services add/ingest (always overwrites, including incoming legacy scores). It never
  runs when page, section, series, body, or publish time change, and `PUT editor/contents/{id}/topics`
  stores whatever the client sends.
- Editors can see and change a score only on the Event of the Day page, where the score is capped at
  the live rule result (`MaxTopicScore`).
- The admin page is one Formik form holding every rule in a CSS grid, saved in a single bulk `PUT`.

### Defects

| # | Defect | Location |
| --- | --- | --- |
| 1 | Edits that change only series or times are silently not saved: `TopicScoreRule.Equals` ignores `SeriesId`, `TimeMin`, `TimeMax`, and `Update` skips "equal" rules. | `libs/net/entities/TopicScoreRule.cs`, `libs/net/dal/Services/TopicScoreRuleService.cs:29-40` |
| 2 | `HasImage` is not evaluated (TODO), so image/no-image rule pairs always take the first. | `TopicScoreHelper.cs:118` |
| 3 | A rule without a page never matches a lettered page ("A1"), so catch-all print rules are dead. | `TopicScoreHelper.cs:139-195` |
| 4 | Content without a page number passes page-range checks; prefix comparison is case-sensitive. | `TopicScoreHelper.cs:139-195` |
| 5 | Content with no series or section matches series- or section-specific rules. | `TopicScoreHelper.cs:107-110` |
| 6 | Time ranges that wrap midnight never match; conversion treats unspecified `DateTimeKind` as server local. | `TopicScoreHelper.cs:223-241` |
| 7 | Character counts include HTML markup. | `TopicScoreHelper.cs:203` |
| 8 | Scoring checks only `Source.UseInTopics`; the editor form also honours `Series.UseInTopics` and hides topics for Image content. | Editor and Services `ContentController` |
| 9 | "Not Applicable" is identified by ID 1 in code and by name in reports and the Event of the Day filter. | `TopicScoreHelper.cs:51,74`, `Topic.tsx:37`, filter SQL |
| 10 | Admin page: `sortOrder` is a global row index and can duplicate; deleting an unsaved row sends `Delete` for ID 0; series is not cleared when the source changes; page inputs have no length limit; validation checks only trailing page digits. | `app/editor/src/features/admin/topic-score-rules/` |
| 11 | Event of the Day page: the sort compares an item's page with itself; `handleAddOrUpdate` can wipe or duplicate topics; `handleTopicChange` can throw on empty topics; switching from "Not Applicable" to a topic leaves score 0. | `app/editor/src/features/admin/event-of-the-day/` |
| 12 | The full rule table ships in the editor and subscriber lookups, though only the admin page uses it. | `Editor/Controllers/LookupController.cs:165` |
| 13 | No unit tests for scoring or the rule service. | — |

## Decisions

| Topic | Decision |
| --- | --- |
| Precedence | First matching rule by sort order within its source, with the defects fixed. |
| No match | Each source has a default score used when no rule matches. |
| Rescoring | Recalculate whenever a scoring input changes, unless the score is overridden. |
| Overrides | Set on the Event of the Day page, and by ingest when content arrives with a non-zero score. The content form does not show or edit scores. |
| Has image | True when the content has an attached image file or an `<img>` in its body. |
| Admin page | Grouped by source, per-rule editing with reorder, rule tester, and bulk rescore. |
| Topic population controls | A settings panel on the topics admin page, stored in the `setting` table. |

## Scoring service

Move scoring from `api/net/Areas/Helpers/TopicScoreHelper.cs` into a DAL service
(`TopicScoreService`) so the API, ingest, Content-Analysis acceptance, and bulk rescore share one
implementation. It returns the score, the matched rule ID (or "source default"), and, for the
tester, why each other rule did not match.

### Eligibility

Content is scored when its source or its series has `UseInTopics` and it is not Image content —
the same rule the editor form uses.

### Rule conditions

A rule matches when every condition it sets matches; an unset condition matches anything.

| Condition | Match rule |
| --- | --- |
| Series | Content series equals the rule series. Content without a series does not match a series rule. |
| Section | Trimmed, case-insensitive equality. Content without a section does not match a section rule. |
| Page | Parsed as prefix + number. Min and max must share a prefix (enforced on save). Case-insensitive prefix; number within the inclusive range. Content without a page number does not match a page rule. |
| Has image | Attached image file or `<img>` in the body, compared with the rule's yes/no. |
| Time | Publish time converted from UTC to the configured time zone. A range whose minimum is after its maximum wraps midnight. Content without a publish time does not match a time rule. |
| Characters | Length of the body's plain text (HTML stripped), inclusive range. |

Rules are evaluated in `SortOrder` within the source; the first match wins. With no match, the
source's default score applies; with no default, the score is 0.

### Not Applicable

Add a system flag to `topic` and mark "Not Applicable" with it. Code, the Event of the Day filter,
and report helpers identify it by the flag instead of ID 1 or its name.

## Score storage and rescoring

`content_topic` gains:

- `score_rule_id` — the rule that produced the score (null for source default or override);
- `is_score_overridden` — the score was set by an editor or arrived with ingest.

A `SaveChanges` hook recalculates the scores of non-overridden content topics whenever a scoring
input changes: topic, source, series, section, page, attached images, body, or publish time.

- Event of the Day page: choosing a score different from the calculated one marks it overridden;
  "Reset" clears the override and recalculates. The existing cap at the calculated score stays.
- `PUT editor/contents/{id}/topics`: a score equal to the calculated score is stored as calculated;
  any other score is an override. Add the missing version check.
- Ingest: an incoming non-zero score is stored as an override; otherwise it is calculated.
- Content-Analysis topic population ([Phase 2](02-content-analysis.md#topics)) stores calculated
  scores.
- Rename `MaxTopicScore` to `CalculatedTopicScore` in the folder content endpoints.

## Admin page (`/admin/topic-scores`)

Two-pane layout built from tno-core components.

**Sources pane**

- Lists sources that use topics (source or series `UseInTopics`), with rule count and default
  score.
- Filter by name or code; the default score is edited inline.

**Rules pane (selected source)**

- Ordered rule table: series, section, page range, image, time range, character range, score.
- Drag to reorder; saves the new order for that source only.
- Filter by series or section.
- Add, edit, and delete one rule at a time in a form drawer, each saved immediately.
  - Series select limited to the source's series, cleared when the source changes.
  - Section as a combobox of known sections.
  - Page as prefix plus number fields (at most 5 characters total).
  - Image as Any / Yes / No; time pickers; number inputs for characters and score.
  - Validation: min ≤ max, matching page prefixes, whole-number score ≥ 0.

**Rule tester**

- Enter a content ID, or fill in source, series, section, page, image, publish time, and length.
- Shows the matched rule and score, the fallback to the source default when nothing matches, and
  for each other rule the first condition that failed.

**Bulk rescore**

- Choose a date range and optional sources.
- Preview the number of stories whose calculated score would change (overrides excluded).
- Run as a background job that updates scores and re-indexes changed content, showing progress
  and failures.

## Topics admin page

Add a population settings panel to `/admin/topics` for Content-Analysis:

- `TopicPopulationEnabled` — off by default;
- `TopicPopulationMode` — existing active topics only (default), or allow creating topics.

Remove the orphaned `TopicForm.tsx`, which no route renders.

## API

Admin area:

- `GET` sources with topic rule summaries; `PUT` a source's default score;
- `GET` a source's rules; `POST`, `PUT`, `DELETE` a single rule; `PUT` a source's rule order;
- `POST` test (content ID or fields) → match explanation;
- `POST` rescore preview and rescore job; `GET` job status.

Remove the bulk-array rule `PUT` once the new page ships, and drop `topic_score_rules` from the
editor and subscriber lookups.

## Migration

- EF migration: `source.topic_default_score` (nullable), `content_topic.score_rule_id` (FK, set
  null on delete), `content_topic.is_score_overridden`, `topic.is_system`, and a check constraint
  that `score >= 0`.
- PostUp SQL: renumber `sort_order` per source, preserving current order; mark "Not Applicable" as
  the system topic.
- Existing `content_topic` scores are marked overridden, so no existing story's score changes until
  an editor resets it or a bulk rescore is run.

## Tests

- Unit tests for every condition, precedence, the source default, midnight wrap, lettered pages,
  missing page, series, and section, and has-image by attachment and by body.
- Rescoring on each input change; overrides preserved; reset recalculates.
- Rule saves persist series and time changes; per-source ordering; deletion of unsaved rules.
- Tester explanations match actual scoring.
- Bulk rescore preview count equals the rows changed, and overrides are untouched.
- Event of the Day page: sorting, topic switching, and reset.
