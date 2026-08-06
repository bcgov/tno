# Prioritized Delivery Plan

This plan sequences stories from highest-value risk reduction to full production automation.

## Phase 0: Architecture and Contract Baseline

**Goal**: freeze boundaries and contracts before implementation starts.

- MMI-AUTO-001
- MMI-AUTO-002

**Exit Criteria**

- ADR approved.
- Canonical DTO contracts finalized and published.

## Phase 1: Technical Enabler Foundation

**Goal**: extract shared AI implementation and ensure no reporting regression.

- MMI-AUTO-003
- MMI-AUTO-004
- MMI-AUTO-005
- MMI-AUTO-006

**Exit Criteria**

- Reporting uses shared AI library.
- Prompt composition and response confirmation parsing are configuration-ready.

## Phase 2: Persistence and Core API

**Goal**: enable profile/step/action storage and operational run control APIs.

- MMI-AUTO-007
- MMI-AUTO-008
- MMI-AUTO-009
- MMI-AUTO-010
- MMI-AUTO-011
- MMI-AUTO-012
- MMI-AUTO-013

**Exit Criteria**

- End-to-end CRUD and run lifecycle API surfaces are available.
- Audit persistence for step/action execution is operational.

## Phase 3: Automation Service MVP

**Goal**: deterministic profile run with profile filter iteration and ordered steps/actions.

- MMI-AUTO-014
- MMI-AUTO-015
- MMI-AUTO-016
- MMI-AUTO-017
- MMI-AUTO-018
- MMI-AUTO-019

**Exit Criteria**

- Service supports manual profile runs with deterministic step/action processing.
- Step target orchestration and filter behavior semantics verified.
- Idempotency and rerun safety implemented.

## Phase 4: Editor Configuration MVP

**Goal**: manage all required automation config in structured Editor forms.

- MMI-AUTO-020
- MMI-AUTO-021
- MMI-AUTO-022
- MMI-AUTO-023
- MMI-AUTO-024

**Exit Criteria**

- Admins can configure profiles, steps, prompts, filters, and actions in UI.

## Phase 5: Review and Manual Operations

**Goal**: complete manual run/review loop for iterative tuning.

- MMI-AUTO-025
- MMI-AUTO-026
- MMI-AUTO-027
- MMI-AUTO-028

**Exit Criteria**

- Editors can run profiles repeatedly, review diffs, and inspect step/action outcomes.

## Phase 6: Scheduled Profile Runs

**Goal**: enable scheduled profile runs using `scheduleCron`.

- MMI-AUTO-029
- MMI-AUTO-031

**Exit Criteria**

- Scheduled profile runs work in production configuration.
- Schedule transition controls/audit active.

## Phase 7: Orchestration and Governance Hardening

**Goal**: harden step target orchestration and governance controls in scheduled operation.

- MMI-AUTO-030
- MMI-AUTO-031 (if not completed in Phase 6)

**Exit Criteria**

- Step target orchestration is validated in scheduled runs.
- Governance controls are validated against operational policy.

## Phase 8: Full Automation Readiness

**Goal**: complete observability, reliability, and rollout readiness for broad profile adoption.

- MMI-AUTO-032
- MMI-AUTO-033
- MMI-AUTO-034
- MMI-AUTO-035
- MMI-AUTO-036
- MMI-AUTO-037
- MMI-AUTO-038

**Exit Criteria**

- Monitoring, security, retries, and test/UAT evidence complete.
- Rollout runbook approved.
- Operations training completed.

## Suggested MVP Cut

For first production release, target completion through **Phase 5**.

- Delivers value with human control.
- Enables iterative step/action and prompt tuning.
- De-risks scheduling before full automation.
