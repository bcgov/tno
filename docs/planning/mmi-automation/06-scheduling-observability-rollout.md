# Epic Group 6: Scheduling, Observability, and Rollout

## MMI-AUTO-029 - Profile Schedule Integration

**Story**: As operations, we need automation profiles to run automatically when schedule cron is configured.

**Acceptance Criteria**

- Scheduler triggers runs for enabled profiles with `scheduleCron` configured.
- Profiles without schedule cron remain manual-only.
- Scheduled runs are marked distinctly from manual runs.

## MMI-AUTO-030 - Step Target Orchestration in Scheduled Runs

**Story**: As operations, we need scheduled runs to execute `start`, `content`, and `end` steps in deterministic order.

**Acceptance Criteria**

- `start` steps execute once before content iteration.
- `content` steps execute per iterated profile item.
- `end` steps execute once after iteration completes.

## MMI-AUTO-031 - Safe Schedule Transition Controls

**Story**: As administrators, we need controlled transitions between manual-only and scheduled profile behavior.

**Acceptance Criteria**

- Schedule enable/disable requires explicit confirmation.
- Schedule changes are audited with actor and timestamp.
- Invalid cron updates are blocked with clear messaging.

## MMI-AUTO-032 - Logging and Metrics

**Story**: As support/ops, we need visibility into automation health and quality.

**Acceptance Criteria**

- Metrics include step hit rates, action execution counts, `maxCalls` saturation, and run outcomes.
- Structured logs include run ids, profile ids, step ids, and action ids.
- Dashboard-friendly telemetry is emitted.

## MMI-AUTO-033 - Authorization and Access Control

**Story**: As security/governance, we need role-based access for config, run, and approval actions.

**Acceptance Criteria**

- Separate permissions exist for config edit, run execution, and approval.
- Unauthorized requests are rejected and logged.
- UI hides restricted operations appropriately.

## MMI-AUTO-034 - Failure Handling and Retry Policies

**Story**: As operations, we need predictable failure behavior with safe retries.

**Acceptance Criteria**

- Retry policies implemented for transient errors.
- Failed and blocked statuses carry actionable reason details.
- No silent partial failures.

## MMI-AUTO-035 - Automated Test Coverage

**Story**: As engineering, we need test coverage for core automation step/action behavior.

**Acceptance Criteria**

- Unit tests cover step ordering, action ordering, and `maxCalls` enforcement.
- Integration tests cover run lifecycle and filter behavior (`applyToAutomationFilter` true/false).
- CI executes and passes required test suites.

## MMI-AUTO-036 - End-to-End UAT Scenarios

**Story**: As product, we need validated business scenarios before rollout.

**Acceptance Criteria**

- UAT pack includes step target and step-filter behavior scenarios.
- UAT includes manual and scheduled profile runs.
- Stakeholder sign-off is captured.

## MMI-AUTO-037 - Rollout Runbook

**Story**: As operations, we need profile scheduling rollout instructions and rollback guidance.

**Acceptance Criteria**

- Runbook covers prechecks, cutover, and rollback for manual/scheduled profile operation.
- Monitoring thresholds and go/no-go criteria are defined.
- Runbook is published in docs and reviewed.

## MMI-AUTO-038 - SOP and Training Enablement

**Story**: As editorial operations, we need process documentation and training.

**Acceptance Criteria**

- SOP updated for step/action authoring, prompt contracts, and run review.
- Training materials delivered to admin/editor users.
- Post-training feedback captured and actioned.
