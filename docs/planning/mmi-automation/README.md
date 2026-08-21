# MMI Automation

Automation profiles are scheduled (or manually run) processes that use an LLM to inspect and act
on content: triage wire stories, set metadata, detect duplicates, score and select top stories,
create digest items, and publish reports and notifications.

For the execution model in detail — run context, collections, phases, analyses, actions, the save
model, and observability — see [01-engine.md](01-engine.md).

> History: this folder previously held the v1 delivery stories and the engine-v2 design proposal.
> The v2 design shipped, v1 was removed entirely (code, endpoints, and tables), and the planning
> documents were retired with it. Git history has them if needed.

## Shape of a profile

- A profile is a **definition document** (`automation_profile.definition`, jsonb): a prompt
  library plus ordered steps. There are no step/action tables — the document is the
  configuration, validated against the action catalog on save.
- Profile fields: `name`, `description`, `isEnabled`, `schemaVersion` (2), `definition`, optional
  `llmId` (the default LLM), `schedules[]` (event schedules fired by the scheduler service).
- Steps run by **phase**: `init` runs once (typically `search` actions loading content into named
  collections), `process` runs once per item of its source collection, `complete` runs once after
  all items.
- Steps declare **analyses** (named LLM prompts with declared result shapes, sent lazily) and
  ordered **actions** gated by conditions or LLM confirmations.
- Nothing is written to the database until an explicit **Save Collection** or **Save Content Now**
  action runs; unwritten changes are reported at the end of the run.

## Where things live

| Path | Role |
| ---- | ---- |
| `services/net/automation/` | The service: Kafka listener, run claim, watchdog, pruning |
| `services/net/automation/Engine/` | The execution engine (`AutomationEngine`, `RunContext`, `PromptBuilder`, `RunLogger`) |
| `libs/net/models/Areas/Admin/Automation/` | Definition model, validator, `ActionCatalog` (the single source of truth for action types) |
| `api/net/Areas/Admin/Controllers/AutomationController.cs` | Profile CRUD, runs, run logs, descriptors, validation, debug assistant |
| `app/editor/src/features/admin/automation/` | The admin page: designer, run history, live log, debugging |
| `libs/net/dal/Migrations/1.5.3` + `20260821171416_1.5.3` | The consolidated schema migration (adds run log/definition columns, drops the v1 tables) |

## Operations

- Runs are queued by the API (manual button or scheduler) and executed by the automation service,
  which atomically claims each run — safe for horizontal scaling.
- A startup sweep fails runs orphaned by a service restart; a watchdog fails runs whose decision
  log goes quiet. **Do not restart the automation service or the API while a run is executing**
  (`select count(*) from automation_run where status = 1`).
- Run history is pruned after `RunRetentionDays` (default 7); the decision log keeps the current
  date only.
- Dry runs execute everything and write nothing, producing the full decision log and intended
  change set; comparison runs execute a candidate definition beside the saved one.

## Status (2026-08-21)

Shipped and in daily use: the definition engine (all of [01-engine.md](01-engine.md)), the
designer UI, run history/outcome/decision-log views, live log, the debugging assistant,
export/import (filters and LLMs are bundled and remapped by name), dedupe memory via
`content_link`, and report/notification publishing from runs.

Known gaps:

- **Import does not remap report/notification ids** — an imported profile keeps the source
  environment's `report`/`notification` ids in `report.run`/`notification.run` actions, saves
  without complaint, and at run time either fails (id missing) or silently publishes the wrong
  target (id exists). Until import bundles names for these, review those actions after importing.
- No run-cancel: a running automation can only be stopped by deleting the run (the log writer
  aborts) — there is no cooperative cancel button.
