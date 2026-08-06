# Epic Group 3: Automation Service

## MMI-AUTO-014 - Scaffold Generic Automation Microservice

**Story**: As engineering, we need a reusable automation microservice for current and future automation tasks.

**Acceptance Criteria**

- New service exists at `services/net/automation`.
- Service follows existing manager/options patterns.
- Service starts with health checks and configuration wiring.

## MMI-AUTO-015 - Deterministic Step Engine

**Story**: As admins, we need steps executed predictably to trust automated outcomes.

**Acceptance Criteria**

- Engine executes ordered steps by `priority`.
- Steps execute by `target` timing: `start`, `content`, `end`.
- Action arrays execute in configured order for each step.

## MMI-AUTO-016 - Profile Filter and Iteration Processor

**Story**: As editors, we need profile filter iteration so content-targeted steps process each returned item.

**Acceptance Criteria**

- Processor loads candidate content items from profile `filterId` when configured.
- For `content` steps, each iterated item is evaluated and processed.
- Produces reviewable run output with item-level diffs and action execution metadata.

## MMI-AUTO-017 - Step Filter Evaluation Processor

**Story**: As admins, we need each step filter to support both gating and enrichment behavior.

**Acceptance Criteria**

- When `applyToAutomationFilter=true`, step filter evaluates current iterated content item and aborts step if no match.
- When `applyToAutomationFilter=false`, step filter runs as separate Elasticsearch query and returns `results` payload.
- Step prompt receives either item content, filter results, or both according to configuration.

## MMI-AUTO-018 - Prompt Rendering, Confirmation Parsing, and Action Execution

**Story**: As admins, we need reliable prompt output parsing so actions run only when explicit confirmations are present.

**Acceptance Criteria**

- Prompt rendering supports substitutions such as `{content.headline}` and `{results}`.
- Action execution requires matching the configured `confirmationStatement` in LLM response.
- Action types are executed through registered handlers in configured order.
- `maxCalls` is enforced per action definition across a single profile run.
- Confirmation parsing results are recorded for audit.

## MMI-AUTO-019 - Idempotency and Rerun Safety

**Story**: As editors, we need safe reruns for iterative tuning without duplicate sends.

**Acceptance Criteria**

- Run-level idempotency keys implemented.
- Repeated profile runs are safe and traceable.
- Duplicate action side effects are prevented across reruns.
