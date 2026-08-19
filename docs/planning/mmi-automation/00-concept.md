# Automation Concept

This document describes what MMI automation is and how a profile executes. It is the conceptual
reference behind the implementation stories in this folder.

## Automation Profile

An **Automation Profile** is a configuration for a scheduled (or manually run) process that uses AI to
inspect and act on content. A profile is composed of ordered **Steps**, and each step is composed of
ordered **Actions**.

A profile is configured with:

- An **LLM** — the model used to evaluate step and action prompts.
- An optional **filter** — an Elasticsearch query used to fetch the relevant content items the profile
  iterates over.
- Zero or more **schedules** — `event_schedule` rows (type `Automation`) fired by the scheduler
  service, each once per day at (or after) its run-at time on its selected week days (none
  selected = every day); without any the profile is manual-only. Schedules are managed in a grid
  on the profile form and reconciled on profile save. The automation service performs no schedule
  evaluation itself, so it can be horizontally scaled.

Profiles, their steps, and their actions are persisted in the database (`automation_profile`,
`automation_step`, `automation_action`).

## Execution Flow

1. **Content selection.** If the profile has a filter, its Elasticsearch query is run to fetch the
   candidate content items. The profile then iterates over each returned item and executes its steps in
   order for that item. If the profile has no filter, the steps still run (start/end steps do not need an
   iterated item).
2. **Steps run in order.** Steps execute by their configured order (`priority`), each completing
   before the next starts.
3. **Items run in parallel within a step.** Content items are independent, so a step processes
   them concurrently (bounded by `MaxParallelContentItems`, default 8). All run state (counts,
   scores, changes, responses, `maxCalls` budgets) is synchronized; `maxCalls` slots are reserved
   atomically so parallel items cannot exceed the cap.
4. **Actions run in order.** Within each step instance, actions execute in their configured array order.

## Steps

A **Step** performs the actual LLM prompt and handles the response.

Step fields: `name`, `description`, `prompt`, `target`, `priority` (execution order, ascending),
`isEnabled`, optional `filterId` with `applyToAutomationFilter` / `iterateStepFilter`, optional
`llmId` (overrides the profile LLM for this step), `sendSeparatePrompts`, `useChatCompletions`, and
its ordered `actions`.

A step with its own filter can use it in one of three ways:

- Run **its own Elasticsearch query** to gather a separate result set injected into the prompt at
  `{results}` (enrichment).
- **Apply its filter to the profile's query results** to identify which of the iterated content items it
  should process (a gate). Controlled by `applyToAutomationFilter`; only available for `content`
  targets.
- **Iterate over the step filter's results** (`iterateStepFilter`) — only available for `start`/`end`
  targets. The filter's hits become the step's iteration source: the step executes once per hit
  and its actions apply to each item, exactly like a `content` step but sourced from the step
  filter instead of the profile filter.

A step then **composes a prompt** to send to the LLM. The prompt contains **keyword tokens** that are
replaced at runtime with data from the iterated content item or from the step's search results (for
example `{content}`, `{content.headline}`, or `{results}` and selected result fields). Part of prompt
composition is injecting each of the step's **Actions** where the `{actions}` token appears (or
appending them when the token is absent). The result is a **single prompt per content item**; the LLM
response is then parsed for each action's confirmation statement to determine which actions to perform
on the content.

Steps also declare a **target** timing:

- `start` (labelled "Run once at start") — runs once before content iteration (requires a profile
  filter), or once per step-filter hit when `iterateStepFilter` is enabled.
- `content` — runs per iterated content item (requires a profile filter).
- `end` (labelled "Run once at end") — runs once after iteration completes (requires a profile
  filter), or once per step-filter hit when `iterateStepFilter` is enabled.
- `none` — runs once; the only valid target when the profile has **no** filter (there is no content
  iteration). The editor constrains the available targets and validates this on save.

### How a Step Prompts

A step sends its prompts in one of three modes. They are mutually exclusive and checked in this
order:

- **Use chat completions** (`useChatCompletions`, takes precedence) — the step runs as a
  conversation. The step prompt becomes the **system prompt** (its default omits the News Story and
  Actions sections) and each action is sent as its own **user message** appended to a shared
  message history, so the model retains its earlier answers and stays consistent across actions.
  Requires a deployment-based (API key) LLM — agent-mode LLMs are single-turn only. Per-action LLM
  overrides still apply because the history is maintained client-side.
- **Send separate prompt for each action** (`sendSeparatePrompts`) — one prompt per action (the
  step prompt plus that single action's prompt), sent at the action's position in the sequence. A
  confirmed abort therefore stops later actions _before_ their prompts are ever sent. Each action
  may use its own LLM (`llmId`). `extract-data` and `create-content` require this mode.
- **One composed prompt** (default) — the step prompt with every action's prompt injected at the
  `{actions}` token (appended when the token is absent), sent **once per content item**. The single
  response is then scanned with each action's confirmation statement independently.

Actions that compose no prompt — `deduplicate`, `fetch-content`, and any "Always run" action — are
excluded from all three. When a step has nothing left to prompt with, the LLM call is skipped
entirely.

## Actions

An **Action** contributes a prompt fragment that determines whether the action should be performed. The
action requires the LLM response to contain a specific **Confirmation Statement**. When that confirmation
statement is found in the response, a **coded action** is performed based on the action's **Action Type**.

Action fields: `name`, `prompt`, `actionType`, `maxCalls`, `confirmationStatement`, optional
`contentField` (update-content-field), optional `contentActionId` (add-action, select-top), optional
`reportId` (run-report), optional `notificationId` (run-notification), optional `filterId`
(fetch-content), optional `priorActionId` (deduplicate — an earlier action in this step or a prior
step), optional `objective` (score-content, select-top), optional `llmId` (this action's own LLM,
honoured when the step sends separate prompts or uses chat completions), optional `worksOn` and
`createIdentifier` / `createClone` (create-content), `abortIfNoConfirmation`, `autoExecute`
("Always run"), `isEnabled`. Each action may declare `maxCalls` to bound how many times it can
execute within a single profile run.

Action types that need more than a scalar carry it in a **`settings` JSON** column rather than a
dedicated field:

| Settings key  | Used by          | Shape                                                          |
| ------------- | ---------------- | -------------------------------------------------------------- |
| `extract`     | `extract-data`   | `[{ key, value }]` — one row per key to produce.               |
| `mapping`     | `create-content` | `{ contentProperty: extractedKey }`.                           |
| `collection`  | `fetch-content`  | `{ fields, maxItems, truncate: { headline, summary, body } }`. |
| `deduplicate` | `deduplicate`    | `{ mode, batchSize, maxComparisons }`.                         |

`abortIfNoConfirmation` stops the remaining actions on the step when _this_ action does not receive
its confirmation — the inverse of `abort-step`, useful for "if it was not published, do nothing
else". It is only meaningful for actions that require a confirmation (not "Always run").

An **"Always run"** action (`autoExecute`) executes unconditionally: its prompt is excluded from the
composed step prompt and no confirmation statement is required (the editor hides the Confirmation
Statement field). It is only offered for value-less action types (`publish-content`,
`unpublish-content`, `add-action`, `run-report`, `run-notification`). Position order still applies —
an earlier `abort-step`/`deduplicate` abort prevents later always-run actions from executing — and
`maxCalls` still caps executions. When every enabled action in a step is always-run (or
`deduplicate`/`fetch-content`, which compose no prompt), the engine skips the step's LLM call
entirely, enabling pure-mechanical steps. Action ids are round-tripped by the editor so profile
saves update actions in place — required for stable `priorActionId` references.

### Action Types

| Type                                  | Behaviour (service implementation)                                                                                                                                                                                                                                                                     |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `publish-content`                     | Sets the content status to Publish; the indexed content update sends the publish request that transitions it to Published.                                                                                                                                                                             |
| `unpublish-content`                   | Sets the content status to Unpublish; the indexed content update sends the unpublish request.                                                                                                                                                                                                          |
| `update-content-field`                | Applies the extracted `{value}` to the selected `contentField` (headline, byline, summary, body, edition, section, page).                                                                                                                                                                              |
| `add-action`                          | Applies the configured content action (`contentActionId`, e.g. Featured Story, Top Story, Commentary, Alert) to the content item.                                                                                                                                                                      |
| `add-tags`                            | Adds the extracted comma-separated tag codes/names (validated against existing tags) to the content item.                                                                                                                                                                                              |
| `add-sentiment`                       | Applies the extracted sentiment value (-5..5) to the default tone pool.                                                                                                                                                                                                                                |
| `select-columnist`                    | Matches the extracted name against contributor names and aliases and sets the contributor on the content item.                                                                                                                                                                                         |
| `extract-data`                        | Builds a per-item key/value dictionary for later actions in the step to consume; see below. Requires `sendSeparatePrompts`.                                                                                                                                                                            |
| `create-content`                      | Builds a **new** content item from extracted data (optionally cloned from the iterated item), registered under `createIdentifier`; see below. Requires `sendSeparatePrompts`.                                                                                                                          |
| `abort-step` (Stop Remaining Actions) | Position sensitive; see below.                                                                                                                                                                                                                                                                         |
| `score-content`                       | Records a per-item score for an objective in run state; see below.                                                                                                                                                                                                                                     |
| `select-top`                          | Applies a content action to the LLM-selected top candidates; see below.                                                                                                                                                                                                                                |
| `fetch-content`                       | Runs the action's filter (`filterId`) once per run and holds the results as a **collection** for later actions to consume; see below. It acts on no content and calls no LLM.                                                                                                                          |
| `deduplicate`                         | Compares the current item against the candidates a configured prior action (`priorActionId` — earlier in this step or an earlier step) supplies, in one of two configurable modes; see below. A response containing the confirmation statement marks a duplicate and aborts the step at this position. |
| `run-report`                          | Queues the configured report (`reportId`) with the reporting service (generates an instance and sends to subscribers).                                                                                                                                                                                 |
| `run-notification`                    | Publishes the configured notification (`notificationId`) via the editor notifications publish endpoint.                                                                                                                                                                                                |

### Confirmation Statements and Value Extraction

A confirmation statement is matched against the LLM response to decide whether the action executes.
Two forms are supported:

- **Literal** — a statement without tokens (e.g. `[PUBLISH CONTENT]`) is a case-sensitive substring
  match.
- **Pattern with `{value}`** — a statement containing the `{value}` token (e.g. `[SENTIMENT:{value}]`)
  is compiled by the engine into a regular expression: every literal character is regex-escaped and
  `{value}` becomes a capture group (`[\s\S]+?` — it may span multiple lines for rewritten text).
  When the pattern matches, the action executes and the captured value is passed to the action
  handler (e.g. the sentiment value for `add-sentiment`, the new field text for
  `update-content-field`).

Additional token and multiline rules:

- **`{field}`** — replaced with the action's selected content field (`contentField`) before the
  statement is compiled, so one statement works for any field.
- **Multiline values** — for large responses (rewritten body text, HTML, XML, special characters)
  the statement should bound `{value}` between start and end marker lines, e.g.:

  ```text
  [UPDATE FIELD START:{field}]
  {value}
  [UPDATE FIELD END:{field}]
  ```

  An end marker is required for reliable extraction — with only a prefix marker the engine cannot
  know where an arbitrary multiline value ends. When compiling a multiline statement the engine
  treats newlines flexibly (`\r?\n`) and captures everything between the markers verbatim.

Admins never author raw regular expressions — the `{value}`/`{field}` tokens keep statements simple
and safe while enabling data extraction.

Special action types:

- **Stop Remaining Actions** (`abort-step`) — position sensitive. When its confirmation statement is
  found in the LLM response, actions ordered _before_ it that were confirmed are still applied, but
  no actions ordered _after_ it execute for the current content item. Execution then continues with
  the next content item / step.
- **Score Content** (`score-content`) — records a numeric score per content item for the action's
  **objective** (e.g. `top-story`, `featured`, `commentary`). Scores live in run-scoped state and are
  echoed into the run summary; they are not written to content. One step prompt can request scores
  for multiple objectives at once because each score action has a distinct confirmation statement.
- **Select Top Content** (`select-top`) — used on an `end` step. The `{candidates:<objective>}` token
  injects a digest of all scored items ordered by score (contentId, score, headline, source,
  summary). The LLM responds with the selected content ids and the engine applies the
  action's configured content action to them, capped by `maxCalls`. The `{objective}` token in
  prompts/statements is replaced with the action's objective.
- **Extract Data** (`extract-data`) — produces a per-item dictionary of key/value pairs that later
  actions in the same step read (chiefly `create-content` through its `mapping`). Each
  `settings.extract` row is one key, and the row's value decides how it is filled:

  - a bare content token (e.g. `{content.headline}`) — copied straight from the iterated item, **no
    LLM call**. The aggregate fields (`tags`, `topics`, `sentiment`) resolve to the code/name-list
    or numeric form `create-content` can apply.
  - a `"double-quoted"` value — a literal constant, **no LLM call**.
  - anything else — an instruction. Every instruction row is gathered into a **single** prompt that
    asks for one `[UPDATE FIELD START:key] … [UPDATE FIELD END:key]` marker block per key, so N
    generated keys still cost one LLM call. The step prompt is used as the preamble; the content is
    appended automatically unless the step prompt already placed it with a `{content}` token.

  The dictionary is scoped to the content item (items run in parallel) and does not persist beyond
  the step. Requires `sendSeparatePrompts`.

- **Create Content** (`create-content`) — builds a **new** content item rather than editing the
  iterated one. It optionally clones the iterated item as its starting point (`createClone`), then
  applies `settings.mapping` (content property ← extracted key), which handles scalar fields, enums
  (`status`, `contentType`), foreign keys resolved by id/code/name (`source`, `mediaType`,
  `series`) and collections (`tags`, `topics`, `sentiment`). With no explicit mapping it defaults to
  an identity map over the standard fields. A clone is given a **fresh `uid`** derived from the
  original uid, the action name and the date, so the system does not treat it as the same story.
  The item is registered under `createIdentifier` (e.g. `c1`); later actions in the step target it
  by setting `worksOn` to that identifier, and it is persisted once at the end of the step — created
  as Draft to obtain an id, then published with indexing if a `publish-content` action targeted it.
  Requires `sendSeparatePrompts`.
- **Fetch Content Collection** (`fetch-content`) — runs the action's own filter (`filterId`) and
  keeps the results in run-scoped state so later actions can compare against them. It executes
  **once per run** no matter where it sits (a `start` step runs it once; on a `content` step the
  parallel items all share the single fetch), calls no LLM, requires no confirmation statement,
  and its prompt is excluded from the composed step prompt. Because a collection is held for the
  whole run it is deliberately bounded: the Elasticsearch `_source` is limited to the fields a
  comparison reads (`id`, `headline`, `byline`, `summary`, `body`, `publishedOn`, `source`,
  `otherSource`), long text is truncated on arrival, and the item count is capped (default 500).
  Both are configurable per action via `settings.collection`
  (`{ fields, maxItems, truncate: { headline, summary, body } }`). An unsaved action (id 0) cannot
  supply a collection because nothing can reference it yet.
- **Deduplication** (`deduplicate`) — position sensitive, and the only action type that is **not**
  confirmed by the main step response. Its prompt is excluded from the composed step prompt. When
  the step reaches it, the engine resolves the candidates from the configured prior action
  (`priorActionId`): a `fetch-content` action supplies the collection it gathered, any other prior
  action supplies the content items it successfully processed this run. The comparison is made on
  the headline, the story text (the summary, or the body when there is no summary), and the
  published **date** — the digest renders `publishedOn` as `yyyy-MM-dd` so two stories filed the
  same day compare as the same date regardless of the hour. `settings.deduplicate` selects the
  mode:

  - `iterate` (default) — one comparison prompt per candidate (`{content}` = the current item,
    `{previous}` = one candidate). Most precise, one LLM call per candidate.
  - `batch` — up to `batchSize` candidates (default 25) are digested into `{previous}` as a JSON
    array and compared in a single prompt, so a large collection costs `ceil(N / batchSize)` calls
    instead of `N`. Because one prompt covers many candidates the confirmation statement must name
    the match: it uses the `{value}` token to capture the duplicate's contentId
    (e.g. `[DUPLICATE:{value}]`). A confirmation whose captured value is not a content id in that
    batch is recorded in the run summary and skipped rather than aborting against an unknown item.

  `maxComparisons` (0 = unbounded) caps how many candidates are examined per content item in
  either mode. A response containing the confirmation statement marks the current item as a
  duplicate and aborts the step at this position (like `abort-step`); the run summary records a
  `duplicate` change with the matched content id.

## Run Execution

Runs are queued by the API (status `Draft`) and executed by the automation service. When a run is
queued the API also publishes an `AutomationRequestModel` message (run id + profile id) to the
Kafka **`automation`** topic. The automation service is a Kafka consumer (consumer group
`Automation`), so multiple service instances can run in parallel — Kafka assigns each message to
one instance, enabling horizontal scaling. Before executing, an instance atomically claims the
run (`Draft` -> `Running` via a conditional update) so redeliveries, reconciliation sweeps, and
scaled-out instances never execute the same run twice. A low-frequency reconciliation sweep
executes any queued (`Draft`) run older than `StalePendingRunMinutes` (default 5) to recover lost
messages, and marks runs stuck in `Running` longer than `AbandonedRunMinutes` (default 240) as
`Failed` (a crashed instance). Scheduled runs are queued the same way: the scheduler service
fires the profile's `Automation` event schedule and requests a run through the API, so scheduled
runs also flow through Kafka.

1. The service consumes an automation request (or reconciles a stale queued run), loads the profile
   (with filter queries embedded), and the profile's LLM configuration.
2. The profile filter query runs against Elasticsearch to load the content items to iterate. The
   engine pages through **all** matches — the filter's stored `size` is a UI page size, not a cap.
3. Steps execute in priority order. A gating step filter (`applyToAutomationFilter`) processes an
   iterated item only when the item appears in the filter's results (all matches, paged the same
   way); gate results are **cached per run** and only their content ids are kept, so steps sharing
   a gate filter reuse one query and no document bodies are held. An enrichment filter's results
   are all injected at `{results}`, and are fetched only when a prompt actually references the
   token. A `start`/`end` step with **iterate over content from step filter** enabled instead uses
   its step filter's results as the iteration source: the step executes once per hit and its
   actions apply to each item (no gate, no `{results}` injection). Enrichment and iteration fetch
   full models and are deliberately _not_ cached run-wide — each behaviour owns its memory. The one
   result set held for the whole run is a `fetch-content` action's collection, which is why that
   one is projected and capped.
4. Each step execution prompts the LLM in one of the three modes described under
   [How a Step Prompts](#how-a-step-prompts) — one composed prompt (the default, sent once per
   content item, or once for `start`/`end`/`none` targets without step-filter iteration), one
   prompt per action (`sendSeparatePrompts`), or a chat conversation (`useChatCompletions`, which
   takes precedence). Actions that compose no prompt (`deduplicate`, `fetch-content`, and "Always
   run" actions) are excluded from the composition; when none are left the LLM call is skipped.
   LLM requests use a configurable timeout (`LLMRequestTimeoutSeconds`) and retry transient
   failures — timeouts, throttling (429), connection failures, and server errors — up to
   `LLMRequestAttempts`. A step instance that still fails is counted in the step's `failures` and
   the run continues with the next item.
5. The response is parsed with each action's confirmation statement; confirmations and executions are
   counted per action across the run, and `maxCalls` caps executions.
6. Confirmed actions accumulate their effects, and **content updates are applied once per step** (a
   single content update request per step per item, sent with `index=true` so Elasticsearch and
   publish/unpublish transitions stay in sync) rather than per action.
7. The run is completed (`Completed` or `Failed`) with a JSON **summary** recording per-step/per-action
   confirmation, execution, skip, abort, and failure counts, the list of content changes, and every
   LLM response (step, content id, and — for deduplication comparisons — the action name). The run
   diff endpoint serves this summary to the editor's run review modal ("LLM Responses" and "Action
   Outcomes" sections).

Run statuses: `Draft` (queued), `Running` (picked up by a service instance), `Completed`, `Failed`.
There is no approval workflow for automation runs.

### LLM Configuration

Each step may optionally select its own LLM (`llmId`), overriding the profile's LLM for that
step's prompts (including deduplication comparisons) — for example a faster model for scoring
steps and a stronger model for selection. When a step sends separate prompts per action (or
runs in chat-conversation mode), each action may additionally select its own LLM (`llmId` on
the action) for its individual prompt. Overrides are fetched once per run and fall back
action -> step -> profile. Chat-conversation steps require a deployment-based (API key) LLM —
agent-mode LLMs are single-turn only.

The LLM record determines how the service authenticates, matching the reporting engine:

- **API key mode** — `deploymentName` + `projectEndpoint` + `apiKey` on the LLM record: a direct
  chat-completions request authenticated with the LLM's own API key. No service credentials required.
  The LLM's `systemPrompt` is used when present.
- **Agent mode** — `agentName` + `projectEndpoint`: an Azure AI Foundry agent call, which requires the
  automation service's `AzureAI__TenantId/ClientId/ClientSecret` configuration (Entra service
  principal).

### Service Configuration

`services/net/automation` options (`Service` section / `Service__*` environment variables):
`RunRetentionDays` (7), `AbandonedRunMinutes` (240),
`DefaultTonePoolId` (1), `Topics` ("automation"), `MaxParallelContentItems` (8),
`IncludeLLMPromptsInSummary` (false - when true, each recorded LLM response in the run summary
also includes the prompt that was sent; useful for debugging, but prompts embed the full content
payload so summaries grow large),
`LLMRequestTimeoutSeconds` (300), `LLMRequestAttempts` (3),
`StalePendingRunMinutes` (5). The service also requires Keycloak service-account credentials,
`Service__ApiUrl`, Elasticsearch connection settings, and Kafka consumer settings
(`Kafka:Consumer`, consumer group `Automation`).

## Runs and Retention

Each execution of a profile — manual or scheduled — records an **Automation Run** (`automation_run`)
capturing the profile, status, trigger, timing, and the JSON summary (counts, content changes, and
LLM responses). Run history is retained for a configurable number of days (`RunRetentionDays`,
default 7) and older runs are pruned by the automation service on a periodic sweep.

## Determinism

- Steps execute by configured order (`priority` ascending).
- Actions execute by configured array order within a step.
- `maxCalls` limits total executions of an action across a single profile run.
