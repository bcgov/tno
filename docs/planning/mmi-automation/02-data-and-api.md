# Epic Group 2: Data and API

## MMI-AUTO-007 - Migration: Automation Profile, Step, and Action Schema

**Story**: As engineering, we need database support for automation profiles, ordered steps, and ordered actions.

**Acceptance Criteria**

- New migration adds profile/step/action schema.
- Profile supports optional `filterId`, optional `llmId`, `timeZone`, and optional `scheduleCron`.
- Step supports `name`, `description`, `prompt`, optional `filterId`, `applyToAutomationFilter`, `target`, `isEnabled`, `priority`.
- Action supports `prompt`, `actionType`, `maxCalls`, `target`, `confirmationStatement`, `isEnabled`.
- Schema maps to EF entities and services.

## MMI-AUTO-008 - Migration: Run Tracking and Audit

**Story**: As editors and auditors, we need run-level and item-level traceability.

**Acceptance Criteria**

- New migration adds run, run-item, and item-change tables.
- Action evaluation and action execution records are persisted.
- Action execution counts are persisted for `maxCalls` enforcement.

## MMI-AUTO-009 - Migration: Prompt and Response Contract Metadata

**Story**: As admins, we need persisted prompt/response metadata to trace automation decisions.

**Acceptance Criteria**

- Run metadata stores rendered prompt(s), template substitutions, and response confirmation statements.
- Metadata links each response to the step/action that interpreted it.
- Version/effective fields support future prompt contract evolution.

## MMI-AUTO-010 - Admin CRUD APIs for Profiles and Steps

**Story**: As admins, we need APIs to manage automation profiles and structured steps/actions.

**Acceptance Criteria**

- CRUD endpoints exist for profiles and step/action graphs.
- Server-side validation enforces required fields for steps/actions and valid `target` values.
- API returns actionable validation errors.

## MMI-AUTO-011 - Run Control APIs

**Story**: As editors, we need to manually run automation multiple times to tune outcomes.

**Acceptance Criteria**

- APIs exist for `run`, `validate`, and current compatibility endpoints where required.
- Run requests support date window and dry-run flags.
- Multiple manual runs are supported and auditable.

## MMI-AUTO-012 - Approval APIs

**Story**: As reviewers, we need optional review controls for run approvals where operational policy requires it.

**Acceptance Criteria**

- Approve/reject endpoints exist.
- Approver, time, and notes are persisted.
- Run gating policy reads approval state correctly when enabled.

## MMI-AUTO-013 - Run History and Diff APIs

**Story**: As editors, we need visibility into what changed and why for each run.

**Acceptance Criteria**

- History endpoints support filtering by profile/date/stage/status.
- Diff endpoint returns field-level before/after + step/action hit metadata.
- Response includes rendered prompt snippets and confirmation statement matches where applicable.
