# MMI Automation Planning

This folder contains implementation stories for AI-assisted automation in MMI.

For a conceptual overview of what an automation profile is and how it executes, see
[00-concept.md](00-concept.md).

## Scope

- An automation profile is a scheduled (or manually run) process that uses an LLM to inspect and act on content.
- A profile is configured with an LLM, an optional Elasticsearch filter used to fetch content items to iterate over, and zero or more schedules (event schedules fired by the scheduler service).
- When a profile has a filter, it iterates over each returned content item and executes its steps in order. Filters return **all** matches — the engine pages through the full result set (the filter's stored `size` is a UI page size, not a cap).
- Profiles use ordered steps (not rules) and ordered actions within each step.
- Step targets support `start` ("Run once at start"), `content`, and `end` ("Run once at end") execution timing (with a profile filter), or `none` (without one).
- A `content` step can run its own Elasticsearch query (results injected at `{results}`), or apply the profile filter's results as a gate (`applyToAutomationFilter`). A `start`/`end` step can instead iterate over its step filter's results (`iterateStepFilter`) — each hit becomes the step's content item.
- Step prompts support templating with runtime tokens: `{content}`, `{content.<field>}`, `{results}`, `{results[i].<field>}`, `{actions}`, and `{candidates:<objective>}`; action prompts/statements also support `{value}`, `{field}`, `{objective}`, and (deduplicate) `{previous}`.
- Each action requires its configured confirmation statement in the LLM response before the coded action (by action type) is performed; per-action `maxCalls` bounds executions. The `deduplicate` action type instead runs its own LLM comparisons against a prior action's processed items and aborts the step for duplicates.
- Score/select objectives support choosing the best X stories (e.g. Top Stories, Featured, Commentary) via per-item scoring and a candidate-selection prompt over all scored items.
- Runs are queued by the API and executed by the automation service (`services/net/automation`), which atomically claims each run (safe for horizontal scaling) and records a JSON outcome summary per run, including every LLM response.

## Story Files

- [00-concept.md](00-concept.md)
- [01-foundation-and-shared-ai.md](01-foundation-and-shared-ai.md)
- [02-data-and-api.md](02-data-and-api.md)
- [03-automation-service.md](03-automation-service.md)
- [04-editor-configuration.md](04-editor-configuration.md)
- [05-editor-operations-and-review.md](05-editor-operations-and-review.md)
- [06-scheduling-observability-rollout.md](06-scheduling-observability-rollout.md)
- [07-prioritized-delivery-plan.md](07-prioritized-delivery-plan.md)
- [08-dependency-map-and-checklist.md](08-dependency-map-and-checklist.md)

## Current Configuration Standard

- Profile fields: `name`, `description`, `isEnabled`, `schemaVersion`, optional `filterId`, optional `llmId`, `schedules[]` (each an `event_schedule` fired by the scheduler service: `name`, `isEnabled`, `startAt` run-at time, `runOnWeekDays`), `steps[]`.
- Step fields: `name`, `description`, `prompt`, `priority`, `target` (`none|content|start|end`), optional `filterId`, `applyToAutomationFilter` (`content` targets only), `iterateStepFilter` (`start`/`end` targets only), optional `llmId` (overrides the profile LLM for this step), `sendSeparatePrompts` (one prompt per action; an abort stops later actions before their prompts are sent), `useChatCompletions` (conversation mode: the step prompt is the system prompt and each action is its own user message that builds on earlier responses; requires a deployment-based LLM), `isEnabled`, `actions[]`.
- Action fields: `id` (round-tripped so saves keep action identities stable), `name`, `prompt`, `actionType`, `maxCalls`, `confirmationStatement`, optional `contentField` (update-content-field), optional `contentActionId` (add-action, select-top), optional `reportId` (run-report), optional `notificationId` (run-notification), optional `priorActionId` (deduplicate), optional `objective` (score-content, select-top), optional `llmId` (used for the action's own prompt when the step sends separate prompts), `autoExecute` ("Always run": executes unconditionally with no LLM confirmation; value-less action types only; a step of only always-run actions skips the LLM call), `isEnabled`.
- Run fields: `profileId`, `status` (`Draft|Running|Completed|Failed`), `trigger` (`manual|schedule`), `note`, `startedOn`, `completedOn`, `summary` (JSON outcome).

## Persistence

- Profiles, steps, actions, and runs are stored in the database (`automation_profile`, `automation_step`, `automation_action`, `automation_run`).
- Profile schedules are stored as `event_schedule` rows (type `Automation`, FK `automation_profile_id`) with child `schedule` rows, reconciled on profile save and fired by the scheduler service.
- Run history is retained for a configurable number of days (`RunRetentionDays`, default 7); older runs are pruned by the automation service.
- The run `summary` records per-step/per-action confirmation, execution, skip, abort, and failure counts, the list of content changes, and every LLM response; the run diff endpoint serves it to the editor's run review modal.

## Implementation Status (2026-07-24)

- **Done:** shared AI library (`libs/net/ai`) with reporting backward compatibility; DB schema + admin CRUD/run APIs; Kafka-driven run execution with an atomic run claim (scale-safe), Draft-run reconciliation, and abandoned-run failure; scheduler-service integration (`EventScheduleType.Automation` — the automation service performs no schedule evaluation); the execution engine (full filter paging, parallel per-item processing within each step (`MaxParallelContentItems`) with synchronized run state and atomic `maxCalls` reservations, three prompt modes per step — combined, separate prompt per action, and chat-completions conversation (step prompt as system prompt, one user message per action sharing context) — confirmation parsing with `{value}`/`{field}`/`{objective}` tokens, per-step and per-action LLM overrides, "Always run" actions with LLM-call skipping, per-step batched content updates with indexing, score/select objectives, deduplication against a prior action's items, run-report/run-notification, LLM request timeout/retry with per-item failure tolerance, and step start/end/filter-count logging); editor profile/step/action configuration UI (collapsible steps grid with a show/hide-all toggle, schedules grid with add/edit/delete, LLM/prior-action/report/notification selects, chat-aware default prompts), manual Run button with live status, and run history grid with an outcome modal showing LLM responses and action outcomes.
- **Not started:** run-item/field-diff persistence (MMI-AUTO-008/009 detail tables), step test harness UI (MMI-AUTO-028), granular authorization, metrics, automated test coverage, UAT, and the rollout runbook (MMI-AUTO-032..038).

## Ordering Standard

- Steps execute by configured order (`priority ASC`).
- Actions execute by configured array order within each step.
- `maxCalls` limits total action executions in a single profile run.
