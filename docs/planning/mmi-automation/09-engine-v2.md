# Engine v2: Run Context, Collections, and Content Pipeline

This document proposes the next generation of the automation execution engine. It is written
against the engine as it exists today (`services/net/automation/AutomationManager.cs`) and against
the two profiles configured in the database, so every claim below can be checked.

The goal is not new features for their own sake. It is to make the capabilities the profiles
already need — searching, collecting, analyzing, creating, excluding, updating — expressible
without copying configuration, and to stop paying for LLM calls that answer questions the
database can answer for free.

## Contents

- [Why](#why)
- [Concepts](#concepts)
- [Configuration schema](#configuration-schema)
- [Worked example: Morning Process](#worked-example-morning-process)
- [Memory and performance model](#memory-and-performance-model)
- [Compatibility and migration](#compatibility-and-migration)
- [Stories](#stories)
- [Delivery phases](#delivery-phases)
- [Risks and open decisions](#risks-and-open-decisions)

## Why

### The configuration duplicates itself, and the copies have drifted

Profile 3 (`Morning Process`) has 8 steps and 89 actions. Six of those steps — one per
newspaper — carry **byte-identical** step prompts (5282 characters, `md5 fd29895e…`). The actions
inside them are copies too, and the copies no longer agree with each other:

| Action name | Copies | Distinct prompt variants |
| ----------- | -----: | -----------------------: |
| columnist   |     12 |                        2 |
| page        |      6 |                        6 |
| publish     |      6 |                        4 |
| body        |      6 |                        3 |
| body-size   |      6 |                        3 |
| byline      |      6 |                        2 |
| tags        |      6 |                        2 |
| top-story   |      6 |                        2 |
| commentary  |      7 |                        3 |
| sentiment   |      6 |                        1 |

Roughly 76 of the 89 actions are copies of about a dozen logical actions. `page` has six copies
and six different prompts. Nobody can tell which differences are intentional tuning and which are
copy-paste rot, and a change to shared wording has to be made six times.

### LLM calls are spent on questions the database can answer

Two actions in every paper step are pure property checks sent to a language model:

```text
body-size → "If the content.body is less than 100 characters respond with [IGNORE CONTENT]"
page      → "If content.page is not A1, A2 … NP1 respond [IGNORE CONTENT].
             If content.section is Scene or Food respond [IGNORE CONTENT]."
```

`page` is action index 7. A story on page D5 therefore pays for byline, columnist, body,
sentiment, columnist again, and a 3534-character tag prompt — eight LLM round trips — before the
model is asked something a length and a set membership test could have answered instantly.

### Reference data is pasted into prompts

The `tags` action prompt embeds the full tag list (code, name, description) as 3534 characters of
HTML, stored six times in the profile. The database has 35 enabled tags. Adding a tag in admin
does not reach any prompt until somebody hand-edits six actions.

The service already fetches a lookup bundle once per run (`GetLookupsAsync`) containing `Tags`
with `Code`, `Name`, `Description`, and `IsEnabled`, and already uses it to validate tag codes in
the response. The data is in memory; the prompt just cannot see it.

### Run state is a set of special cases

Everything a run remembers lives in purpose-built dictionaries wired to specific action types:
`executedContentByAction`, `collectionsByAction`, `scores`, `extractedData`, `createdContents`.
Each new capability adds another one, and each is threaded by hand through
`ExecuteStepInstanceAsync` (thirteen parameters today). There is no way to name a set of content
and hand it to a later step, no way to remove an item from a run, and no way for a created item to
outlive the step instance that created it.

### Dispatch is a single switch

`AutomationManager.cs` is 3046 lines and dispatches every action type through one `switch`. There
is no handler contract, so there is no per-type validation, no schema for the editor to render
from, and no unit boundary to test against. `AutomationProfileForm.tsx` mirrors the same switch by
hand.

## Concepts

### Requirement coverage

| Requirement                                    | Mechanism                                        |
| ---------------------------------------------- | ------------------------------------------------ |
| Execute searches for content                   | `search` action writing to a named collection    |
| Named dictionaries containing content arrays   | Run context collections and `collection.*` verbs |
| Initialization steps and actions               | `init` phase                                     |
| Completion steps and actions                   | `complete` phase                                 |
| Reduce memory footprint                        | Content refs plus deltas, streaming hydration    |
| Minimize runtime                               | Property conditions, lazy analyses, one flush    |
| Perform analysis on content                    | Declared `analyses` per step                     |
| Update content                                 | `content.*` actions on the working copy          |
| Create content populated by multiple actions   | Drafts and draft collections                     |
| Request a notification to run                  | `notification.run`                               |
| Request a report to run                        | `report.run`                                     |
| Full prompt and response logs in the UI        | Run log with same-day retention and a log viewer |
| Dry run without database changes               | `dryRun` mode reporting the intended change set  |
| Ask why something happened, and how to improve | Explain-and-improve assistant over a log entry   |

### Run context

A run holds one context, replacing the five ad-hoc dictionaries:

```text
$run.<name>     run-scoped collections and variables; mutations are serialized
$item           the item the current iteration is processing
$item.<name>    per-iteration values (analysis results, drafts); isolated per parallel item
```

Scoping is explicit because items within a step are processed in parallel. A draft named
`$item.digest` is safe; a collection named `$run.digests` is lock-guarded on write.

### Collections

A collection is a named, ordered array of content entries:

```json
{ "kind": "existing", "id": 12345, "digest": { "headline": "…", "publishedOn": "2026-08-19" } }
{ "kind": "draft", "tempKey": "digest-7", "digest": { … }, "deltas": { … } }
```

Collections hold **references and digests, never full content models**. The digest carries only
the fields the profile declares it needs. Drafts — content created during the run and not yet
saved — are first-class members, so a step can iterate items that do not exist in the database
yet.

Operations: `create`, `add`, `remove`, `move`, `union`, `except`, `intersect`, `filter`, `sortBy`,
`take`, `distinctBy`.

### Phases

```text
init      runs once, before any iteration
process   runs once per item of the step's resolved content
complete  runs once, after every process step
```

Phases are independent of whether the profile has a filter. This removes the current
`target`/`hasProfileFilter` validity matrix and the dead branch that reports
`"target 'x' is not valid for this profile configuration"`.

### Step content source

Every `process` step declares exactly one source, plus optional filter gates:

```jsonc
"source": { "from": "profile", "include": [12], "exclude": [20] }  // profile results, gated
"source": { "from": "filter", "filter": 13 }                       // the step runs its own search
"source": { "from": "collection", "collection": "$run.digests" }   // from the dictionary
```

`include` and `exclude` are sets of filter ids. Each distinct filter resolves **once per run** to
an id set using an id-only Elasticsearch projection, cached run-wide, regardless of how many steps
reference it. Membership is then a hash lookup per item.

### The subject rule

> Within a step, every action applies to the item the step is iterating. To act on something else,
> iterate a different collection.

This is the whole answer to "how does an action know whether it is working on the original item or
a new one". A step over `$run.inbox` has wire stories as its subject; a step over `$run.digests`
has created items as its subject. The action configuration is identical.

The one exception is the step that creates an item, where both exist at once. There, a draft is
addressed by name:

```jsonc
{ "type": "content.create", "as": "$item.digest", … },
{ "type": "content.update", "target": "$item.digest", "field": "headline", … },  // explicit
{ "type": "content.sentiment", "value": { "from": "triage.sentiment" } }         // no target → subject
```

Beyond a field or two, the recommended pattern is to push the draft into a collection and let the
next step iterate it. That retires `worksOn`, `createIdentifier`, `createClone`, and the rule that
create/extract actions require `sendSeparatePrompts` — four special cases replaced by two general
primitives.

### Analyses

An analysis is one prompt plus one declared result shape. A step declares as many as it needs, and
actions reference results by name.

```jsonc
"analyses": [
  {
    "name": "triage",
    "prompt": { "ref": "editorial-rules" },
    "returns": { "sentiment": "int(-5..5)", "tags": "string[]", "publish": "bool" }
  },
  {
    "name": "columnist",
    "prompt": { "ref": "columnist-rules" },
    "returns": { "name": "string?" }
  },
  {
    "name": "scoring",
    "prompt": { "ref": "score-rules" },
    "chain": "triage",
    "returns": { "topStory": "int", "featured": "int", "commentary": "int" }
  }
]
```

- **One analysis covering several properties** is one call. Two actions consuming
  `triage.sentiment` and `triage.tags` share it; today they would be two calls.
- **One analysis per property** is available when a prompt is too complex to share. Set the
  granularity per property, permanently — some questions merge well, some do not.
- **Lazy.** An analysis runs only when an action that consumes it is actually reachable. An item
  excluded by a property condition, or whose consuming actions are all gated off, costs nothing.
- **`chain`** continues an earlier analysis as a conversation, preserving today's chat-completions
  behaviour where an answer needs to see previous answers. Unchained analyses are independent
  single calls.

Each existing action maps to an analysis returning one field, which is why the migration is
risk-free: the same prompts produce the same call count and the same behaviour until somebody
chooses to merge them.

### Conditions

Whether an action runs is decided by one of two things, and never by an expression language.

**A language-model answer** — today's confirmation statement, unchanged:

```jsonc
{ "type": "content.publish", "confirm": "[PUBLISH CONTENT]" }
{ "type": "content.publish", "when": { "from": "triage.publish" } }   // structured equivalent
```

**A content property condition** — a declarative object:

```jsonc
{ "type": "exclude", "when": { "field": "body", "op": "lengthLessThan", "value": 100 } }

{ "type": "exclude", "when": { "any": [
  { "field": "page", "op": "notIn", "value": ["A1", "A1 / Front", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A10", "NP1"] },
  { "field": "section", "op": "in", "value": ["Scene", "Food"] }
] } }
```

Operators: `exists`, `isEmpty`, `equals`, `notEquals`, `in`, `notIn`, `contains`, `startsWith`,
`matches`, `lengthLessThan`, `lengthGreaterThan`, `greaterThan`, `lessThan`, `hasTag`, `hasAction`,
`statusIs`. Combinators: `all`, `any`, `not`.

Both may appear on one action. **The property condition is evaluated first, and when it fails no
prompt is sent.** That is where most of the saved runtime comes from.

Values, where an action needs one, come from a fixed set of sources — there is nothing to compute:

```jsonc
"value": { "from": "triage.sentiment" }                   // an analysis result
"value": { "from": "content.byline" }                     // the current working copy
"value": { "literal": "Wire" }
"value": { "template": "DIGEST: {content.headline}" }     // the token substitution prompts already use
```

### Exclusion

Three verbs, with different reach. None of them ever discards accumulated changes.

| Verb                | Later actions in this step | Later steps | Stays in collections | Pending changes written |
| ------------------- | -------------------------- | ----------- | -------------------- | ----------------------- |
| `abort`             | stop                       | still run   | yes                  | yes                     |
| `exclude`           | stop                       | skipped     | yes, marked excluded | yes                     |
| `collection.remove` | continue                   | depends     | no                   | yes                     |

`exclude` is positional. Actions ordered before it have already applied and will be saved; actions
after it are skipped. This supports "apply sentiment to everything the search returned, publish
only the items the analysis selects, then stop carrying the rest":

```jsonc
{ "type": "content.sentiment", "value": { "from": "triage.sentiment" } },  // all items
{ "type": "content.tags", "value": { "from": "triage.tags" } },            // all items
{ "type": "content.publish", "confirm": "[PUBLISH CONTENT]" },             // the subset
{ "type": "exclude" }                                                      // stop carrying it forward
```

Excluded items land in `$run.excluded` with their reason, so the run summary accounts for them
rather than having them silently disappear.

`dedupe` accepts `onDuplicate: exclude | abort | remove`.

### Working copy and flush

Each item carries a **working copy**: its projected digest plus the deltas actions have
accumulated. Every `content.*` action writes to the deltas, and every later prompt renders the
working copy — so a step sees what earlier steps changed, whether or not those changes have been
written to the database. This is today's `SerializeTargetView` behaviour promoted from per-step to
per-run.

Deltas are kept rather than full models deliberately: holding a full `ContentModel` (43 properties,
10 nested collections) for every touched item until the end of a run is exactly the memory problem
this design is meant to avoid.

```jsonc
"saveMode": "end-of-run"                                  // profile default
{ "phase": "process", "saveMode": "end-of-step", … }      // per-step override
```

|                         | end-of-run                   | end-of-step                      |
| ----------------------- | ---------------------------- | -------------------------------- |
| Writes per item         | 1 fetch + 1 update + 1 index | one set per step that touched it |
| Database ids downstream | only after the run           | immediately                      |
| Exposure on crash       | all unwritten changes        | the current step only            |

Use `end-of-step` on a step whose created content later steps must reference by database id, or
where partial results should survive a failure. Use `end-of-run` everywhere else.

### Lookup tokens

Reference data is resolved by the engine at prompt composition from the lookup bundle already
fetched once per run:

```text
{lookup:tags}              CODE | Name — Description, enabled only, sorted by code
{lookup:tags[code,name]}   declared columns only
{lookup:contributors}
{lookup:sources}
{lookup:mediaTypes}
{lookup:actions}
{lookup:topics}
```

The same data validates the response afterwards, so the prompt and the validation can no longer
disagree. Lookup blocks belong in the step prompt rather than per action: the block is identical
for every item, so it sits at the front of the payload where providers can cache the prefix across
hundreds of items.

Lists are size-guarded — a declared maximum, with truncation recorded in the run summary rather
than a silently partial list. Subsets use fixed parameters (`{lookup:tags(group=ministries)}`),
never expressions.

### Observability and debugging

Three capabilities share one substrate: a run log that records every decision, not only every
prompt.

**The run log.** Each entry records what the engine was doing, what it sent, what came back, and
what it did about it:

```text
run, step, action, analysis, content id, attempt
prompt, response, tokens (prompt/completion), duration
outcome (confirmed | not confirmed | condition failed | skipped | failed | excluded)
resulting change, if any
```

Prompts are recorded **always**, not behind a configuration flag. The flag exists today because
prompts embed content bodies and the log shares the seven-day run-history retention. Separating
the two retentions removes the reason for the flag: run history keeps its current retention, and
log entries are kept for the **current date only** and pruned on the daily sweep.

**Decisions, not just exchanges.** An entry is written when a property condition excludes an item,
when an analysis is skipped because nothing consumes it, when a `maxCalls` budget is exhausted, and
when an item is excluded or aborted. Without these, the questions that matter most — "why was this
story ignored?", "why did nothing publish?" — have no data behind them, because increasingly the
answer will be a condition that never reached the model.

**Dry run.** `dryRun` executes everything, writes nothing: no content updates, no reports, no
notifications. It produces a complete log and the full intended change set per item. This is the
loop for tuning a profile — dry run, read the log, ask why, adjust, dry run again — with no risk
to published content.

**Explain and improve.** Any log entry can be opened as a conversation with the LLM, seeded with
the exact prompt, the exact response, the parsed outcome, the action configuration, and the content
digest. It answers two kinds of question:

- _Why did this happen?_ — why the confirmation matched or did not, which part of the response the
  engine parsed, which condition failed.
- _How do I improve it?_ — a suggested revision of the prompt, shown as a diff against the current
  text.

Suggestions are never applied automatically. An admin reviews the diff and chooses to save it to
the profile or the prompt library. The assistant conversation is itself logged, so a tuning session
is auditable.

### Prompt library

Shared prompt text lives once and is referenced:

```jsonc
"prompt": { "ref": "editorial-rules", "override": "Sun bylines appear as …" }
```

A step or action stores only its delta from the shared text. This is what stops the six-way drift:
a difference between two papers becomes visible as an override instead of hiding inside six
near-identical blobs.

### Handler registry

Each action type becomes a registered handler with a descriptor:

```csharp
Type, RequiresSubject, RequiresPersistedId, Reads[], Writes[], UsesLLM, ConfigSchema
```

The registry replaces the switch. The descriptors give the editor a schema to render from instead
of a hand-maintained mirror of the same switch, give profile save a validation contract, and give
each action type a unit test boundary.

## Configuration schema

```jsonc
{
  "schemaVersion": 2,
  "name": "Morning Process",
  "llm": 2,
  "saveMode": "end-of-run",
  "dryRun": false,

  "prompts": {
    "editorial-rules": "…shared 5282-character text, stored once…",
    "columnist-rules": "…",
    "score-rules": "…",
    "tag-rules": "Review the headline, summary and body …\n\nAvailable tags:\n{lookup:tags}",
  },

  "steps": [
    {
      "name": "Load collections",
      "phase": "init",
      "actions": [
        {
          "type": "search",
          "filter": 11,
          "into": "$run.inbox",
          "fields": [
            "id",
            "headline",
            "byline",
            "source",
            "section",
            "page",
            "publishedOn",
            "body",
          ],
          "max": 2000,
        },
        { "type": "search", "filter": 18, "into": "$run.cpnews", "max": 500 },
        { "type": "collection.create", "name": "$run.digests" },
      ],
    },
    {
      "name": "Vancouver Sun",
      "phase": "process",
      "source": { "from": "profile", "include": [12] },
      "saveMode": "end-of-run",
      "analyses": [
        {
          "name": "triage",
          "prompt": { "ref": "editorial-rules" },
          "returns": { "byline": "string", "sentiment": "int(-5..5)", "publish": "bool" },
        },
        { "name": "tagging", "prompt": { "ref": "tag-rules" }, "returns": { "tags": "string[]" } },
        {
          "name": "columnist",
          "prompt": { "ref": "columnist-rules" },
          "returns": { "name": "string?" },
        },
        {
          "name": "scoring",
          "prompt": { "ref": "score-rules" },
          "chain": "triage",
          "returns": { "topStory": "int", "featured": "int", "commentary": "int" },
        },
      ],
      "actions": [
        {
          "type": "exclude",
          "when": {
            "any": [
              {
                "field": "page",
                "op": "notIn",
                "value": ["A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A10", "NP1"],
              },
              { "field": "section", "op": "in", "value": ["Scene", "Food"] },
            ],
          },
        },
        { "type": "exclude", "when": { "field": "body", "op": "lengthLessThan", "value": 100 } },
        { "type": "content.update", "field": "byline", "value": { "from": "triage.byline" } },
        { "type": "content.contributor", "value": { "from": "columnist.name" } },
        { "type": "content.sentiment", "value": { "from": "triage.sentiment" } },
        { "type": "content.tags", "value": { "from": "tagging.tags" } },
        {
          "type": "dedupe",
          "against": "$run.cpnews",
          "mode": "batch",
          "batchSize": 25,
          "onDuplicate": "exclude",
        },
        { "type": "content.publish", "when": { "from": "triage.publish" } },
        { "type": "score", "objective": "top-story", "value": { "from": "scoring.topStory" } },
      ],
    },
    {
      "name": "Select and distribute",
      "phase": "complete",
      "actions": [
        {
          "type": "select-top",
          "objective": "top-story",
          "take": 10,
          "into": "$run.topStories",
          "contentAction": 1,
        },
        { "type": "report.run", "report": 5, "using": "$run.topStories" },
        { "type": "notification.run", "notification": 2, "using": "$run.topStories" },
      ],
    },
  ],
}
```

## Worked example: Morning Process

### Structure

| Today                                       | v2                                                        |
| ------------------------------------------- | --------------------------------------------------------- |
| Profile filter + 8 steps + 89 actions       | `init` search + 6 scoped `process` steps + `complete`     |
| 6 identical 5282-char step prompts          | 1 library entry + per-paper overrides                     |
| 3534-char tag list stored 6 times           | `{lookup:tags}`                                           |
| `body-size` and `page` as LLM prompts       | property conditions, no call                              |
| Duplicate detection against a prior action  | `dedupe against $run.cpnews`                              |
| Created content dies with its step instance | drafts pushed to `$run.digests`, iterated by a later step |

### Cost per item, derived from the current configuration

| Case                         | Today                                 | v2                                  |
| ---------------------------- | ------------------------------------- | ----------------------------------- |
| Off-section story            | 8 calls                               | **0**                               |
| Body under 100 characters    | 1 call                                | **0**                               |
| Fully processed story        | 12 calls, plus 1 per dedupe candidate | 1–3 calls, plus 1 per 25 candidates |
| Database writes, 3-step item | 3 fetches + 3 updates + 3 reindexes   | 1 + 1 + 1                           |

The per-call payload also shrinks: `{content}` currently serializes all 43 properties and 10
nested collections of `ContentModel` on every turn, and in chat mode the whole conversation is
resent each time.

There is no measured baseline yet — the run history table is empty because runs are pruned after
seven days. Story MMI-AUTO-057 exists to capture one before any of this ships.

## Memory and performance model

| Concern                | Today                                                    | v2                                                      |
| ---------------------- | -------------------------------------------------------- | ------------------------------------------------------- |
| Iterated content       | Full models for the whole run, plus `contentById` growth | Refs and digests; full models hydrated per item         |
| `{results}` enrichment | One giant string of every hit including bodies           | Declared projection through `{collection:name[fields]}` |
| Content in prompts     | 43 properties and 10 collections                         | Declared field projection                               |
| Peak working set       | O(items)                                                 | O(parallelism)                                          |
| Filter queries         | One per step per behaviour                               | One per distinct filter per run, cached                 |
| LLM calls              | One per action per item                                  | One per reachable analysis per item                     |
| Content writes         | One per item per step that touched it                    | One per dirty item per flush boundary                   |

Streaming hydration is the mechanism behind the peak-working-set line: ids are paged, full models
are fetched for a bounded window of items being processed, and released when the item completes.

## Compatibility and migration

Profiles carry `schemaVersion`. Version 1 profiles continue to execute on the current code path
until they are migrated; nothing is forced.

Automatic mapping for a v1 profile:

| v1                                           | v2                                                          |
| -------------------------------------------- | ----------------------------------------------------------- |
| Profile `filterId`                           | `init` step with `search` into `$run.inbox`                 |
| Step `target: start`                         | `phase: init`                                               |
| Step `target: content`                       | `phase: process`, `source.from: profile`                    |
| Step `target: end`                           | `phase: complete`                                           |
| Step `target: none`                          | `phase: init` or `complete` by position                     |
| `applyToAutomationFilter: true`              | `source.include: [filterId]`                                |
| `iterateStepFilter: true`                    | `source.from: filter`                                       |
| `fetch-content` action                       | `search` into a named collection                            |
| `deduplicate` with `priorActionId`           | `dedupe against` the collection or the prior action's items |
| Each action prompt                           | One analysis returning one field                            |
| `sendSeparatePrompts` / `useChatCompletions` | Multiple analyses / `chain`                                 |
| `abort-step`                                 | `abort`                                                     |
| `worksOn` / `createIdentifier`               | Named draft plus draft collection                           |

Because each v1 action becomes a single-field analysis, a migrated profile issues the same prompts
in the same order and produces the same call count. Merging analyses, replacing prompts with
property conditions, and collapsing duplicated text into the prompt library are then deliberate,
reviewable edits rather than side effects of the migration.

## Stories

### Epic Group 9: Engine v2 foundation

#### MMI-AUTO-039 - Action Handler Registry

**Story**: As engineering, we need action types resolved through registered handlers so the engine
can be extended and tested without editing a single switch statement.

**Acceptance Criteria**

- Each action type is a handler registered in DI, with a descriptor declaring subject
  requirements, persisted-id requirements, context reads/writes, LLM use, and a config schema.
- `AutomationManager` dispatches through the registry; the existing `switch` is removed.
- Behaviour is unchanged for every existing profile; the change ships with no configuration edits.
- Each handler has unit tests covering its success, no-op, and failure paths.

#### MMI-AUTO-040 - Run Context and Scoped Names

**Story**: As engineering, we need one run context so state is not threaded by hand through the
execution methods.

**Acceptance Criteria**

- A `RunContext` type replaces `executedContentByAction`, `collectionsByAction`, `scores`,
  `extractedData`, and `createdContents`.
- `$run.*` names are run-scoped with serialized mutation; `$item.*` names are per-iteration and
  isolated across parallel items.
- `ExecuteStepInstanceAsync` takes the context instead of its current thirteen parameters.
- Parallel execution is covered by tests that assert no cross-item leakage.

#### MMI-AUTO-041 - Named Collections and Dictionary Operations

**Story**: As admins, we need named collections of content that actions can create and later
actions can modify.

**Acceptance Criteria**

- Collections are named entries in the run context holding content refs with declared digests.
- Verbs available as action types: `create`, `add`, `remove`, `move`, `union`, `except`,
  `intersect`, `filter`, `sortBy`, `take`, `distinctBy`.
- Collections may contain unsaved drafts alongside existing content.
- Collection sizes and mutations appear in the run summary.

#### MMI-AUTO-042 - Search Action and Projected Content Refs

**Story**: As admins, we need a step or action to run a filter and put the results into a named
collection without acting on them.

**Acceptance Criteria**

- `search` executes a filter and writes refs into the named collection.
- The Elasticsearch request projects only the declared fields; long text fields are truncated on
  ingest to configured limits.
- Paging respects `max` and the index `max_result_window`; truncation is reported in the summary.
- A filter used by more than one step or action executes once per run.

#### MMI-AUTO-043 - Lifecycle Phases

**Story**: As admins, we need explicit initialization and completion phases that do not depend on
whether the profile has a filter.

**Acceptance Criteria**

- Steps declare `phase` as `init`, `process`, or `complete`.
- `init` and `complete` steps run once and require no content subject.
- Phase order is enforced; `process` steps execute in configured order between them.
- The `target`/`hasProfileFilter` validity matrix and its skip branch are removed.

#### MMI-AUTO-044 - Step Content Source Resolution

**Story**: As admins, we need each step to declare where its content comes from, including a
collection built earlier in the run.

**Acceptance Criteria**

- `source.from` supports `profile`, `filter`, and `collection`.
- `include` and `exclude` filter id sets gate the resolved items.
- Each distinct gate filter resolves once per run to an id set using an id-only projection.
- Items are matched against gates by hash lookup, not by re-querying per step.
- The run summary reports resolved counts and gate exclusions per step.

### Epic Group 10: Decisions, analysis, and content lifecycle

#### MMI-AUTO-045 - Content Property Conditions

**Story**: As admins, we need actions gated on content property values so decisions the database
can make do not cost an LLM call.

**Acceptance Criteria**

- `when` accepts a declarative condition object with the documented operators and the `all`,
  `any`, and `not` combinators.
- Conditions evaluate against the working copy, so they see changes made earlier in the run.
- A failing condition prevents any prompt associated with that action from being sent.
- Conditions are validated at profile save against the content field list.

#### MMI-AUTO-046 - Declared Analyses

**Story**: As admins, we need to choose how many properties one prompt covers, so simple questions
share a call and complex ones stay isolated.

**Acceptance Criteria**

- A step declares zero or more named analyses, each with a prompt and a declared return shape.
- Actions reference analysis results by name through `value.from` and `when.from`.
- An analysis executes at most once per item and only when a consuming action is reachable.
- `chain` continues a named earlier analysis as a conversation.
- Structured responses are requested through the provider's JSON schema support where available,
  with confirmation-statement parsing retained as the fallback.
- Malformed or missing fields are recorded on the action summary and do not fail the run.

#### MMI-AUTO-047 - Exclusion and Abort Semantics

**Story**: As admins, we need to stop processing an item without losing the changes already made
to it.

**Acceptance Criteria**

- `abort` stops the remaining actions in the current step only.
- `exclude` stops the remaining actions and removes the item from all later steps.
- Neither verb discards accumulated changes; excluded items still flush.
- Excluded items are recorded in `$run.excluded` with a reason and appear in the run summary.
- `dedupe` supports `onDuplicate` values `exclude`, `abort`, and `remove`.

#### MMI-AUTO-048 - Working Copy and Flush Modes

**Story**: As editors, we need content updates visible to later steps immediately and written to
the database once.

**Acceptance Criteria**

- Each item carries a working copy of digest plus deltas; `content.*` actions write deltas.
- Prompts and conditions render the working copy, including changes from earlier steps.
- `saveMode` is configurable per profile and overridable per step, supporting `end-of-run` and
  `end-of-step`.
- A flush applies all of an item's deltas in one update with indexing.
- Unflushed changes at the point of failure are reported in the run summary.

#### MMI-AUTO-049 - Content Creation and Draft Collections

**Story**: As editors, we need to create content during a run, populate it across several actions,
and process it as the subject of a later step.

**Acceptance Criteria**

- `content.create` produces a named draft in the item scope, with a declared field copy list and a
  `set` map of value sources.
- The derived `uid` rule that keeps a created item distinct from its original is preserved.
- Drafts can be added to collections and iterated by a later step, where they become the subject.
- Actions requiring a persisted id are rejected at profile save when the source is a draft
  collection that has not been flushed.
- The run summary maps temporary keys to real ids after the flush.

#### MMI-AUTO-050 - Lookup Tokens in Prompts

**Story**: As admins, we need reference data such as the enabled tag list injected into prompts
instead of pasted into them.

**Acceptance Criteria**

- `{lookup:…}` tokens resolve for tags, contributors, sources, media types, actions, and topics.
- Only enabled records are rendered, in a stable order, with optional declared columns.
- Data comes from the lookup bundle already fetched once per run; no per-action request is made.
- Lists are size-guarded, and truncation is recorded in the run summary.
- The same data validates the response, so prompt and validation cannot disagree.

#### MMI-AUTO-051 - Prompt Library

**Story**: As admins, we need shared prompt text stored once so profiles stop drifting.

**Acceptance Criteria**

- Named prompt entries are stored at profile scope and referenced by `prompt.ref`.
- A step or action may declare an `override` that layers onto the referenced text.
- The editor shows which entries a change affects before it is saved.
- Migration extracts identical prompt text from existing profiles into library entries.

### Epic Group 11: Delivery, validation, and rollout

#### MMI-AUTO-052 - Report and Notification Actions over Collections

**Story**: As editors, we need a report or notification to run against a collection the run
produced.

**Acceptance Criteria**

- `report.run` and `notification.run` accept an optional `using` collection.
- Items in the collection without persisted ids fail validation at save, not at run time.
- The run summary records which collection was used and its size.

#### MMI-AUTO-053 - Profile Validation at Save

**Story**: As admins, we need configuration errors reported when we save, not discovered in a run.

**Acceptance Criteria**

- Each action validates against its handler's config schema.
- Cross-checks include unknown collection names, analyses referenced but not declared, conditions
  naming unknown fields, draft sources feeding id-requiring actions, and unreachable steps.
- Errors are returned per step and per action so the editor can highlight them in place.

#### MMI-AUTO-054 - Editor Configuration from Handler Descriptors

**Story**: As admins, we need the configuration UI to follow the engine automatically.

**Acceptance Criteria**

- The action modal renders fields from the handler descriptor rather than a hand-written switch.
- Adding an action type in the service requires no editor change to configure it.
- Condition builders, value-source pickers, and collection pickers are shared components.

#### MMI-AUTO-055 - Profile v1 to v2 Migration

**Story**: As admins, we need existing profiles moved to the new schema without changing what they
do.

**Acceptance Criteria**

- A migration converts a v1 profile using the documented mapping table.
- Each v1 action becomes a single-field analysis, preserving prompt order and call count.
- Identical prompt text is extracted into the prompt library, with differences kept as overrides.
- The engine runs v1 and v2 profiles side by side until every profile is migrated.

#### MMI-AUTO-056 - Shadow Run and Comparison Harness

**Story**: As admins, we need to compare a changed profile against the current one before trusting
it.

**Acceptance Criteria**

- A run can execute in comparison mode against a second profile version over the same content.
- The report shows per-item differences in decisions, field values, and action outcomes.
- Comparison runs never write content or send reports and notifications.

#### MMI-AUTO-057 - Run Instrumentation

**Story**: As admins, we need to see what a run actually costs.

**Acceptance Criteria**

- The run summary records LLM call count, prompt and completion token counts, wall time per step,
  and content writes.
- Counts are broken down per step and per analysis.
- A baseline is captured for `Morning Process` before engine v2 work begins.

#### MMI-AUTO-058 - Streaming Hydration and Memory Bounds

**Story**: As engineering, we need run memory bounded by concurrency rather than by result size.

**Acceptance Criteria**

- Iteration pages content ids and hydrates full models only for the items currently in flight.
- Hydrated models are released when the item completes.
- Peak working set is measured and reported for a run over at least 2000 items.

#### MMI-AUTO-059 - Dry Run Mode

**Story**: As admins, we need to validate a profile against real content without changing
anything.

**Acceptance Criteria**

- `dryRun` computes every decision and change but performs no content writes and creates no
  content.
- Reports and notifications are recorded as intended rather than sent.
- The run summary lists the full intended change set per item, including created drafts.
- A dry run produces the same complete run log as a live run, so it can be inspected and explained.
- Dry run is available from the editor as a run option, and dry runs are visibly marked in run
  history.

### Epic Group 12: Observability and debugging

#### MMI-AUTO-060 - Full Run Log Capture and Same-Day Retention

**Story**: As admins, we need every prompt and response from today's runs recorded, without
turning a flag on first.

**Acceptance Criteria**

- Prompts are always recorded; `IncludeLLMPromptsInSummary` is removed.
- Each entry records run, step, action, analysis, content id, attempt, prompt, response, prompt and
  completion tokens, duration, and outcome.
- Where an entry produced a change, the change is linked to it.
- Log retention is configured independently of run-history retention and defaults to the current
  date; older entries are pruned on the daily sweep.
- Truncation limits are configurable, and truncation is marked on the entry rather than silent.
- Entries are written incrementally during the run, so a failed run still has the log up to the
  failure.
- Indexes support retrieval by run, by content item, and by date for pruning.

#### MMI-AUTO-061 - Run Log Viewer

**Story**: As admins, we need to read today's logs in the editor rather than in the database.

**Acceptance Criteria**

- A run opens a log view listing entries in execution order.
- Filters by step, action, analysis, content item, and outcome; free-text search across prompt and
  response.
- An entry expands to the full prompt and response, with the parsed value and resulting change
  shown beside them.
- An entry links to its content item and to the change it produced.
- A run's log can be exported for offline review.
- The view handles a full day of entries without loading them all at once.

#### MMI-AUTO-062 - Decision Trace

**Story**: As admins, we need to see why something did not happen, not only why it did.

**Acceptance Criteria**

- Entries are recorded for property conditions that failed, including the field, operator, and
  compared values.
- Entries are recorded for analyses skipped as unreachable, exhausted `maxCalls` budgets, aborts,
  and exclusions with their reason.
- Every item in the run resolves to a trace explaining its outcome, including items that were
  excluded before any prompt was sent.
- Non-LLM entries are visually distinct in the viewer and carry no token cost.

#### MMI-AUTO-063 - Explain and Improve Assistant

**Story**: As admins, we need to ask why a decision was made and how to make the prompt better.

**Acceptance Criteria**

- Any log entry opens a conversation seeded with its prompt, response, parsed outcome, action
  configuration, and content digest.
- The assistant answers "why did this happen" with reference to the recorded response and the
  confirmation or condition that was evaluated.
- The assistant can propose a revised prompt, shown as a diff against the current text.
- A proposed revision is applied only by an explicit admin action, and applies to the profile or
  the prompt library entry.
- The assistant uses a configurable LLM, defaulting to the profile's.
- Assistant conversations are logged and attributed to the user who ran them.
- The assistant never writes content and never modifies configuration on its own.

## Delivery phases

### Phase A: Measure, log, and refactor

**Goal**: know the baseline, be able to read what a run did, and make the engine extensible
without changing behaviour.

- MMI-AUTO-057
- MMI-AUTO-060
- MMI-AUTO-061
- MMI-AUTO-039
- MMI-AUTO-040

**Exit criteria**

- A recorded baseline for `Morning Process`: calls, tokens, wall time, writes.
- Every prompt and response from today's runs is readable in the editor, filterable and
  searchable, with no flag to enable.
- All action types dispatch through registered handlers with unit tests.
- No configuration changes required; existing profiles behave identically.

Logging and instrumentation come first deliberately. Every later phase is judged by whether a run
got cheaper or better, and there is currently no run history to judge against.

### Phase B: Collections and phases

**Goal**: content sets become nameable, and lifecycle becomes explicit.

- MMI-AUTO-041
- MMI-AUTO-042
- MMI-AUTO-043
- MMI-AUTO-044

**Exit criteria**

- A profile can search into a named collection in `init` and iterate it in `process`.
- Step sources and gate filters resolve once per run.

### Phase C: Cheap decisions, safe exclusion, and dry run

**Goal**: stop paying for decisions the database can make, control what carries forward, and make
it safe to try changes.

- MMI-AUTO-045
- MMI-AUTO-047
- MMI-AUTO-048
- MMI-AUTO-062
- MMI-AUTO-059

**Exit criteria**

- `body-size` and `page` run as property conditions with zero LLM calls.
- Excluded items skip later steps and still have their changes written.
- `end-of-run` flushing verified against a multi-step profile.
- Every item in a run resolves to a trace explaining its outcome, including items excluded before
  any prompt was sent.
- A dry run over real content reports the full intended change set and writes nothing.

Dry run lands here rather than at the end because it depends only on flushing being a discrete
step, and because it is the safety net for everything that follows.

### Phase D: Analysis and prompt hygiene

**Goal**: choose call granularity deliberately, and stop duplicating prompt text.

- MMI-AUTO-046
- MMI-AUTO-050
- MMI-AUTO-051
- MMI-AUTO-063

**Exit criteria**

- A step can merge several properties into one analysis, with the comparison harness showing the
  effect.
- The tag list is injected from lookups; the pasted copies are deleted.
- An admin can open any log entry, ask why the decision was made, and receive a proposed prompt
  revision as a reviewable diff.

### Phase E: Content lifecycle

**Goal**: create content properly and route it through the pipeline.

- MMI-AUTO-049
- MMI-AUTO-052

**Exit criteria**

- A profile creates items in one step and processes them as subjects in the next.
- A report runs against a collection produced by the run.

### Phase F: Editor, validation, migration

**Goal**: make the new model configurable and move the existing profiles onto it.

- MMI-AUTO-053
- MMI-AUTO-054
- MMI-AUTO-055
- MMI-AUTO-056

**Exit criteria**

- Configuration errors surface at save with per-action detail.
- `Morning Process` and `BC Calendar` run on schema version 2 with comparison evidence.

### Phase G: Efficiency and confidence

**Goal**: bound memory at scale.

- MMI-AUTO-058

**Exit criteria**

- Peak working set is independent of result-set size.
- A run over at least 2000 items reports a measured peak working set.

## Risks and open decisions

### Risks

- **Merged analyses change model behaviour.** Twelve conversational turns, where each answer sees
  the previous ones, is not the same as one structured call. This is why merging is opt-in per
  property and why MMI-AUTO-056 exists. Do not merge without a comparison run over a real day of
  content.
- **`end-of-run` flushing widens the crash window.** A failure late in a run loses every unwritten
  change. The per-step override exists for the cases where that is unacceptable; a periodic or
  phase-boundary flush can be added if the exposure proves too large in practice.
- **Structured output support varies by deployment.** The client already speaks both the Responses
  API and chat completions; JSON schema support should be confirmed per configured LLM before
  Phase D, with confirmation parsing retained as the fallback.
- **Full prompt logging stores content bodies.** Every prompt embeds content, so a day of logs is
  a copy of a day of content. Estimated at roughly 10-40 MB per day at current volumes, which is
  manageable, but it is why retention is same-day and why the log is a separate table with its own
  pruning. Access follows the existing admin authorization on automation.
- **The explain assistant costs LLM calls and can be wrong.** It reasons about a recorded exchange,
  it does not re-run it. Its suggestions are proposals shown as diffs; nothing it produces reaches
  a profile without an admin saving it, and nothing it produces touches content.
- **Migration touches live profiles.** Both existing profiles run daily. Migration produces a new
  version rather than editing in place, and v1 execution stays available until v2 is proven.

### Open decisions

- **Action-level filter scope.** Steps scope to filters; actions may as well. Action-level scoping
  is more expressive but makes a step's behaviour vary per item, which is harder to read in the
  editor and harder to test. Confirm whether it is needed at delivery time.
- **Storage shape.** The action table currently carries fifteen sparse nullable columns, each
  meaningful to one action type. Version 2 could keep that shape, or move the step and action
  graph into a validated JSON document with a reference table preserving foreign-key integrity for
  filters, reports, and notifications. This affects the editor substantially and should be decided
  before Phase F.
- **Discarding changes.** `exclude` always preserves accumulated changes. If a case appears that
  needs "drop this item and roll back what we did to it", it needs a fourth verb and a defined
  rollback scope.
