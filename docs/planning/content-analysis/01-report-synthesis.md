# Phase 1 — Bounded Report Synthesis

Replace the single unbounded AI request per report section with bounded, token-budgeted synthesis,
and generate each AI result once. Ships before Content-Analysis exists, using the full body of each
story.

## Current behavior

- `ReportEngine.GenerateReportAISectionsAsync` (`libs/net/template/ReportEngine.cs:867-1112`)
  builds one JSON list of every story in the report (full `Body`, `Summary` only when the body is
  empty) and sends it, plus each previous instance, in one chat-completions request per AI
  section. There is no token counting, output limit, or truncation.
- The historical-instance count is the maximum `IncludePreviousReports` across all AI sections.
- AI runs on every API preview and view (`ReportHelper.GenerateReportAsync` always passes
  `viewOnWebOnly=false`), on the admin "prime cache" job, and again at send. The subscriber edit
  view regenerates whenever `updatedOn` changes. Output persists only inside `report_instance.body`.
- Link-only rendering already skips AI.

## Scope

- Direct-model AI sections only. Agent-backed sections (`llm.AgentName`) keep today's behavior.
- Non-AI reports are unchanged.
- Reports never wait for analysis and never request it.

## Story input

| Story state | Input sent to synthesis |
| --- | --- |
| Phase 1 (no analysis exists) | Headline, metadata, and the full body, split into budget-sized chunks on paragraph/sentence boundaries |
| Phase 2+, analyzed | Analysis summary, key facts, entities, and quotes |
| Phase 2+, not yet analyzed | Same as Phase 1 |

Every included story contributes. Nothing is silently truncated; a story too large for one request
is chunked, and its chunks are reduced before joining its section.

## Model limits

Add to the `llm` table (new EF migration):

- context window (tokens)
- maximum output tokens
- token estimation strategy
- requests-per-minute and tokens-per-minute limits

A direct-model LLM used by an AI section without these values is a configuration error reported
on the section. Surface the new fields in the admin LLM form (`app/editor/src/features/admin/llms/`).

For every request:

```text
input allowance =
  context window
  − reserved output
  − instructions
  − protocol overhead
  − safety margin (default 10%)
```

Historical-instance inputs count against the same allowance.

## Synthesis

For each AI section:

1. Pin a manifest: section ID, content IDs (and, from Phase 2, analysis IDs), prompts, model,
   settings, and the historical instances included.
2. Group stories. Phase 1 groups within the section only. From Phase 2, group by topic — the
   matched staff `Topic` when present, otherwise the analysis topic registry label.
3. Send stories in batches that fit the input allowance; each batch returns structured findings
   with the evidence handles of the stories behind them.
4. Reduce findings recursively within each group until they fit one request (maximum depth 8,
   configurable).
5. Assemble the final output in application code: headings, bullets, source lists from recorded
   provenance, and HTML sanitization.

Rules:

- Use compact evidence handles in prompts; keep full provenance outside the model context.
- Never send all groups through one unbounded final request.
- On a context-length rejection, split the batch and retry. On truncated output, retry with
  smaller batches. If synthesis cannot complete within the depth limit, record the failure on the
  section instead of sending partial or unbounded output.
- Honour each AI section's own historical-instance count, and process each historical instance
  separately in chronological order.

## Result reuse

Persist AI results (new table) keyed by a hash of the manifest, prompts, section settings, model,
and pipeline version. Reuse a matching result for previews, views, link-only and full-text
rendering, retries, and resends. Record token usage and duration with each result.

The editor, subscriber, and admin preview/view endpoints and the reporting service all call one
orchestration component, so none of them triggers a second generation for an unchanged manifest.

## Section settings

Extend `ReportSectionSettingsModel` for AI sections:

- scope: per section (default for new sections) or whole report — existing sections keep their
  current whole-report behavior until changed;
- source sections: which content sections feed the AI section;
- output mode: free text, or topic summary (headings with bullet statements and source lists).

Update the AI section editors in both apps
(`app/subscriber/src/features/my-reports/edit/settings/template/ReportSectionAI.tsx`,
`app/editor/src/features/admin/reports/components/ReportSectionAI.tsx`).

## Reporting service

- Generation stays inside the reporting service's handler; there is no readiness wait.
- Measure generation time for large reports. Because the service processes one report at a time,
  set `MaxThreads` and replicas so large AI reports do not delay scheduled reports.

## Bug fix

- `ChoiceIndex == -1` builds the joined choices but never assigns them to `sectionData.Data`
  (`libs/net/template/ReportEngine.cs:1062-1071`). Assign them.

## Tests

- Reports of 1, 100, 1,000, and 10,000 stories: every story is accounted for and every request fits
  its budget.
- Long articles and transcripts are chunked, not truncated.
- Context rejection and truncated output split and retry; depth-limit failure is reported.
- Previews, views, and sends of an unchanged manifest reuse one stored result.
- Section isolation, historical-instance handling, source provenance, and HTML sanitization.
- Agent-backed sections behave exactly as before.
- Quality evaluation of synthesis against a reviewed corpus of past reports.
