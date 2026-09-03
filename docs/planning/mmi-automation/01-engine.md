# The Automation Engine

This document describes the automation execution engine as it is implemented
(`services/net/automation/Engine/AutomationEngine.cs`) and the definition document it executes.
It replaces the original engine-v2 design proposal; the design shipped, and the v1 engine it was
written against has been removed.

## Contents

- [Run context](#run-context)
- [Collections and digests](#collections-and-digests)
- [Phases and step sources](#phases-and-step-sources)
- [Analyses](#analyses)
- [Prompt library and tokens](#prompt-library-and-tokens)
- [Gates, conditions, and value sources](#gates-conditions-and-value-sources)
- [Actions](#actions)
- [The save model](#the-save-model)
- [Runs and observability](#runs-and-observability)
- [Editor](#editor)

## Run context

A run holds one context:

```text
$run.<name>     run-scoped collections; mutations are serialized
$item           the item the current process-step iteration is working on
$item.<name>    per-iteration values (drafts); isolated per parallel item
```

Items within a step are processed in parallel, so scoping is explicit: a draft named
`$item.digest` is per-item, a collection named `$run.digests` is lock-guarded on write.

Entries are **shared**: the same content item appearing in two collections is one entry, so a
change made in one step is visible everywhere, including in later prompts.

## Collections and digests

A collection is a named, ordered array of content entries — **references and digests, never full
content models**. Each entry is `existing` (has a database id) or `draft` (created during the run,
gets an id when saved).

The digest is projected from Elasticsearch on ingest (`search` actions / step filter sources).
Default fields: `id`, `headline`, `byline`, `summary`, `body`, `publishedOn` (rendered as a bare
`yyyy-MM-dd` so dedupe compares stories by date), `publishedOnUtc` (the full timestamp, so copies
preserve the original date exactly), `source`, `otherSource`, `source.name`, `source.code`,
`section`, `page`, `edition`, `status`, `contentType`, `sourceId`, `licenseId`, `mediaTypeId`,
`uid`, `mediaType.name`, `series.name`, `contributor.name`, `labels`, `topics` (JSON array of
`{name, score}`), `sentiment` (the default tone pool value), `tags` (JSON array of codes), and
`actions` (JSON array of the *applied* content action names — Boolean value `"true"`, or a
non-empty String value).

Default truncation caps `headline` at 300 and `summary` at 500 characters; **the body is never
capped by default** (a truncated ingest would destroy story text when an action writes the body
back). A per-action `truncate` config can cap fields deliberately.

Each entry also carries a **working copy**: the digest plus the deltas actions have accumulated
(field updates, tags, sentiment, contributor, content actions, publish/unpublish). Conditions and
prompts always see the working copy, whether or not anything has been written to the database.

## Phases and step sources

```text
init      runs once, before any iteration (no content subject)
process   runs once per item of the step's source collection
complete  runs once, after every process step; iterates only when it declares a source
```

A `process` step declares one source — a named collection (`source.from: "collection"`) or a
filter the step runs itself (`source.from: "filter"`) — plus optional `include`/`exclude` filter
id gates. Each distinct gate filter resolves **once per run** to an id set via an id-only
Elasticsearch projection; membership is a hash lookup per item.

## Analyses

An analysis is one prompt plus one declared result shape; actions consume results by
`analysisName.key`.

```jsonc
"analyses": [
  { "name": "triage", "prompt": { "ref": "editorial-rules" },
    "returns": { "sentiment": "int(-5..5)", "tags": "string[]", "publish": "bool" } },
  { "name": "extract", "prompt": { "ref": "extract", "override": "<p>…step-specific delta…</p>" },
    "returns": { "byline": "string", "headline": "string" } },
  { "name": "raw-check", "prompt": { "text": "…inline…" }, "raw": true }
]
```

- **Lazy** — an analysis runs only when a reachable action consumes it; items excluded by
  conditions cost nothing.
- **One analysis, several properties** — one LLM call shared by every consuming action.
- **`chain`** continues an earlier analysis as a conversation when an answer must see previous
  answers.
- **`raw`** returns the unparsed response (consumed via `name.value` and confirmation matching).
- Per-analysis, per-step, and per-action `llmId` overrides the profile LLM.

## Prompt library and tokens

Shared prompt text lives once in the definition's `prompts` map and is referenced with
`{ "ref": "name", "override": "…" }` — the override is layered onto the shared text. Prompts are
authored in a WYSIWYG editor (stored as HTML); the engine converts HTML to text at composition.

**The prompt is exactly what is sent to the LLM.** Tokens mark where data is inserted, replaced
once per item:

| Token | Meaning |
| ----- | ------- |
| `{content}` | The item's full working copy as JSON |
| `{content.<field>}` | One working-copy field (any digest field; `story` = summary, or body when empty) |
| `{lookup:tags}` etc. | Reference lists as JSON — code, name, description of the enabled records (tags, contributors, sources, mediaTypes, actions, topics); `[code,description]` selects fields. Identical for every item |
| `{candidates}` | Detect Duplicate only: the compared stories digest (batch mode) |
| `{candidate.<field>}` | Detect Duplicate only: one field of the single candidate (iterate mode) |

An analysis prompt containing no content token gets the story appended as a final `## News Story`
section; any `{content…}` token disables the append. The engine's built-in dedupe prompt can be
overridden by creating a library entry named `default-dedupe`. The validator warns on unknown
tokens.

## Gates, conditions, and value sources

Whether an action runs is decided by one of:

- **Always run.**
- **An LLM confirmation** — `confirm: "[PUBLISH CONTENT]"` matched against a named analysis's
  response; `{value}` compiles to a capture group for value-bearing confirmations.
- **A condition** — a declarative object over analysis results and working-copy fields:
  `from` (a boolean result such as a dedupe's `name.isDuplicate`), `field`/`op`/`value`, combined
  with `all`, `any`, `not`. List values escape literal commas as `\,` and backslashes as `\\`;
  quotes are taken literally.

A failing condition sends no prompt — that is where most saved runtime comes from.

Where an action needs a value, it comes from a fixed set of sources: an analysis result or
working-copy field (`{ "from": "extract.byline" }`), a literal, or a token template
(`{ "template": "[{content.source.code}] {content.headline}" }`).

## Actions

The **action catalog** (`libs/net/models/Areas/Admin/Automation/ActionCatalog.cs`) is the single
source of truth: the validator checks definitions against it, the engine dispatches by it, and the
editor renders each action's configuration form from its descriptor
(`GET /api/admin/automation/descriptors`). Adding an action type means one descriptor plus one
engine case.

Highlights by category:

- **search** — `search` runs a saved filter and writes digests into a collection (`fields`,
  `max`, `truncate` configure the projection).
- **collection** — `create`, `add`, `remove`, `move`, `copy`, `union`/`except`/`intersect`,
  `filter`, `sortBy`, `take`, `distinctBy`, and **`collection.save`** (see the save model; its
  `index` option controls whether Elasticsearch receives the saved content, default on).
- **content** — `content.update` (field from a value source), `content.tags`,
  `content.sentiment`, `content.contributor` (its `create` option adds a missing contributor),
  `content.action` (apply a content action such as Alert or Top Story), `content.publish` /
  `content.unpublish` (status deltas applied at save), **`content.create`** (a draft later
  actions target by its `as` name; `copyFrom` seeds it from the original item, `copyFields`
  limits the copy — the `*` sentinel / "all fields" checkbox copies everything the item carries —
  and `set` fills fields from value sources; a derived `uid` keeps the copy distinct), and
  **`content.save`** (write one item immediately; `index` option as above). A created draft with
  a pending publish is written with its final status in a single create, so the indexing pass
  publishes and requests notifications in one motion.
- **analysis** — `score` records per-objective scores; `select-top` takes the top N for an
  objective into a collection and can apply a content action to the winners.
- **detect duplicate** — a **pure detector**: it compares the subject against a collection
  (`iterate` = one prompt per candidate, `batch` = up to `batchSize` per prompt) and publishes
  `<name>.isDuplicate` and `<name>.matchedId` for ordinary gated actions to route on. Its
  `remember` option records confirmed pairs as `content_link` rows (value `duplicate`, read in
  both directions) so re-runs skip the LLM for known pairs; the `/contents/:id` page lists these
  links behind the headline's duplicates icon.
- **flow** — `exclude` (drop the item from the rest of the run; changes already made are kept),
  `abort` (stop the remaining actions of this step for this item).
- **distribute** — `report.run` and `notification.run` publish a report/notification by id.
  Note: profile **import does not remap these ids** — review them after importing into another
  environment.

## The save model

Nothing writes to the database implicitly. Changes accumulate on working copies and are written
only by:

- **`collection.save`** — flushes every changed item (and unsaved draft) of one collection; or
- **`content.save`** — writes one item immediately (needed when a later step requires the
  database id).

A flush applies all of an item's deltas in one update (one fetch, one update, one index). At the
end of a run the engine reports every unwritten change — the pending fields and which collections
contain the item — so a missing Save action is visible in the run outcome rather than silent.

## Runs and observability

- Runs are queued via Kafka and **atomically claimed** (Draft → Running), so redeliveries and
  scaled-out instances never execute a run twice. Invalid definitions fail before the engine
  starts; drafts may save with validation errors but cannot run.
- A **startup sweep** fails runs orphaned by a service restart, and a watchdog fails runs whose
  decision log goes quiet. Deleting a run mid-execution makes the run's logger abandon, which
  halts the engine for that run.
- The **decision log** records every decision, not only every prompt: exchanges (with prompt,
  response, and token counts) and outcomes (`executed`, `confirmed`, `not-confirmed`,
  `condition-failed`, `skipped`, `excluded`, `aborted`, `flushed`, `failed`). Prompts are always
  recorded; entries are retained for the current date only and pruned daily. Run history itself
  is pruned after `RunRetentionDays` (default 7).
- **Dry run** executes everything and writes nothing — full log, full intended change set.
  **Comparison runs** execute a candidate definition alongside the saved one and report the
  differences.
- The editor shows run history with an outcome/decision-log modal, a **Live Log** tab streaming
  the current run, and a **Debugging** tab: a conversation with the profile's LLM seeded with how
  the profile works (rendered from the definition), the most recent run's outcome counts and log
  tail, and optionally one content item (found by id or headline).

## Editor

The `/admin/automations` page works only with definition (schema version 2) profiles. The
designer edits the document directly: a prompt library (WYSIWYG, insertable tokens), collapsible
step groups, per-phase step forms, and per-action forms rendered from the catalog descriptors.
Edit modals confirm with **Done** — nothing persists until the profile is saved.

**Export** downloads the profile with the definitions of every referenced filter and LLM (no API
keys), so the file is self-contained. **Import** loads the file as a new unsaved profile,
matching filters and LLMs by name (creating them when missing — an imported LLM has no key) and
rewriting their ids throughout the definition. Report and notification ids are *not* remapped —
see the known gap in the [README](README.md).
