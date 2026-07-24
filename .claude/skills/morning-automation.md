# Skill: Morning Automation Implementation

Use this skill when working on MMI automation profile features (the "morning process" automation).

## Architecture (current)

- **Profile → Steps → Actions** model persisted in the database (`automation_profile`,
  `automation_step`, `automation_action`, `automation_run`). No separate curate/dispatch stages or
  execution modes — one profile executes ordered steps.
- **API** (`api/net/Areas/Admin/Controllers/AutomationController.cs`): profile CRUD (including
  schedule reconciliation), run queueing (`POST profiles/{id}/run` creates a `Draft` run and
  publishes the Kafka work item), atomic run claim (`POST runs/{id}/claim`, Draft -> Running),
  run update (`PUT runs/{id}`, used by the service), history, diff (serves the run summary),
  retention prune. Run statuses: Draft (queued), Running, Completed, Failed - no approval workflow.
- **Scheduling**: profiles hold zero or more schedules (`event_schedule` rows, type `Automation`,
  FK `automation_profile_id`), managed in a grid on the profile form. The **scheduler service**
  (`services/net/scheduler`) fires them and requests runs through the API — the automation service
  performs no schedule evaluation, so it can be horizontally scaled.
- **Automation service** (`services/net/automation`): a Kafka consumer (topic `automation`,
  group `Automation`, long-running-job mode so runs may exceed the Kafka poll interval) — one
  instance atomically claims and executes each queued run. Execution: Elasticsearch filter
  iteration (full paging — a filter's stored `size` is never a cap), prompt composition, LLM calls
  (configurable timeout + transient-failure retries, per-item failure tolerance), confirmation
  parsing, action execution, run summary reporting (including every LLM response). The service
  also reconciles stale `Draft` runs (lost messages), fails abandoned `Running` runs, and prunes
  run history.
- **Editor UI** (`app/editor/src/features/admin/automation`): profile form with schedules grid,
  collapsible step/action builder (drag+drop), Run button with live status, Runs tab with an
  outcome modal (LLM responses + action outcomes).
- Conceptual reference: `docs/planning/mmi-automation/00-concept.md` (tokens, action types,
  confirmation statement contract, engine flow).

## Key Contracts

- Step targets: `content`/`start`/`end` require a profile filter; `none` is the only target without
  one. The UI validates this on Done/Save. A step filter acts as a gate (`applyToAutomationFilter`,
  `content` targets only), as prompt enrichment at `{results}`, or — `start`/`end` targets only —
  as the step's own iteration source (`iterateStepFilter`).
- Three prompt modes per step: **combined** (default — one prompt per step instance, action
  prompts at `{actions}`), **separate** (`sendSeparatePrompts` — one prompt per action at its
  sequence position; an abort stops later actions before their prompts are sent), and
  **chat conversation** (`useChatCompletions`, takes precedence — step prompt is the system
  prompt, each action a user message in a shared client-side history; deployment-based LLM
  required). `deduplicate` actions always send their own per-comparison prompts (`{previous}`).
- LLM selection falls back action (`llmId`, separate/chat modes) -> step (`llmId`) -> profile;
  overrides are cached per run.
- Steps run sequentially; content items within a step run in parallel
  (`MaxParallelContentItems`, default 8) with synchronized run state and atomic `maxCalls`
  reservations.
- `autoExecute` ("Always run") actions execute without LLM confirmation (value-less action types
  only); their prompts are never sent, and a step of only always-run/dedupe actions skips the
  LLM call entirely. An earlier abort still stops them.
- Confirmation statements: literal substring match, or pattern with `{value}` capture;
  `{field}`/`{objective}` tokens substituted first; multiline values bounded by start/end markers.
- `maxCalls` caps action executions across a run; counts are tracked per action.
- `abort-step` ("Stop Remaining Actions") is position sensitive: prior confirmed actions still apply.
- Content updates are batched: one content update request per step per item.
- Score/select: `score-content` records run-scoped scores per `objective`; `select-top` (end step)
  receives top candidates at `{candidates:<objective>}` and applies its content action to the
  selected ids.
- LLM auth: API-key mode (LLM record's `apiKey` + `deploymentName`) needs no service credentials;
  agent mode (`agentName`) requires `AzureAI__*` service configuration.
- Publish state is non-monotonic: later steps/actions may unpublish.

## Implementation Priorities

1. Maintain deterministic execution (step `priority`, action order).
2. Preserve auditability: every run records a JSON summary (step/action counts, content changes).
3. Keep prompt and confirmation contracts configuration-driven (tokens, not code changes).
4. Preserve existing report/notification pipelines; `run-report` queues the configured report via
   the Kafka report request and `run-notification` publishes via the editor notifications publish
   endpoint.
5. Action ids are round-tripped by the editor so saves update actions in place — required for
   `deduplicate` prior-action references (`priorActionId`).
