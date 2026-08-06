# Epic Group 1: Foundation and Shared AI

## MMI-AUTO-001 - Finalize Automation Architecture ADR

**Story**: As engineering, we need an approved architecture decision record for profile/step/action automation behavior so implementation is aligned.

**Acceptance Criteria**

- ADR defines `automation` service, shared AI library, API, and Editor responsibilities.
- ADR defines profile filter + step filter behavior and step targets (`start`, `content`, `end`).
- ADR defines deterministic ordering for steps and step actions.
- ADR is reviewed and approved by technical and product stakeholders.

## MMI-AUTO-002 - Freeze Canonical Data Contracts

**Story**: As engineering, we need stable DTO contracts for profile/steps/actions/runs so API and UI implementation can proceed safely.

**Acceptance Criteria**

- Contracts include profile, step, action, run, run item change, and action result models.
- Contracts include optional profile filter and optional step filter semantics.
- Contract includes schema versioning for step/action extensibility.
- Contract docs are published and referenced by implementation stories.

## MMI-AUTO-003 - Extract Generic AI Client to Shared Library

**Story**: As engineering, we need AI agent logic moved from template-specific code into a shared library so reporting and automation can reuse it.

**Acceptance Criteria**

- Shared AI project created under `libs/net/`.
- Foundry/OpenAPI call path and MCP approval flow are preserved.
- Existing reporting path compiles against shared abstractions.

## MMI-AUTO-004 - Prompt Composition Abstraction

**Story**: As admins, we need configurable prompt composition so step/action prompt changes are made in Editor and applied next run.

**Acceptance Criteria**

- Prompt composer supports template variables including `{content.*}`, `{results}`, and selected result fields.
- System/user prompt composition is configuration-driven.
- Prompt hash/version is recorded in run metadata.

## MMI-AUTO-005 - Payload Shaping Abstraction

**Story**: As admins, we need control over what content data is sent to the model without requiring profile-level payload schema configuration.

**Acceptance Criteria**

- Payload construction supports profile iterated item data and optional step filter result payload.
- Step filter behavior supports either profile-item gate or separate Elasticsearch result enrichment.
- Payload hash/version persisted per run.

## MMI-AUTO-006 - Reporting Backward Compatibility

**Story**: As product, we need no regression in existing reporting AI functionality after extraction.

**Acceptance Criteria**

- Reporting AI path uses shared library.
- Existing report generation behavior remains functional.
- Regression tests pass for reporting AI scenarios.
