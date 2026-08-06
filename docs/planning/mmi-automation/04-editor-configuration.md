# Epic Group 4: Editor Configuration UX

## MMI-AUTO-020 - Automation Profile Management UI

**Story**: As admins, we need to create and edit automation profiles in Editor.

**Acceptance Criteria**

- Admin pages support profile list/detail CRUD.
- Form includes optional profile filter, optional LLM, timezone, and a single optional schedule field.
- Form does not include execution modes or fixed max-item caps.

## MMI-AUTO-021 - Structured Step Builder UI

**Story**: As admins, we need a structured form-based step builder with no code editing.

**Acceptance Criteria**

- Step grid supports add/edit/delete and drag+drop ordering.
- Step modal supports `name`, `description`, `prompt`, optional `filterId`, `applyToAutomationFilter`, `target`, and `isEnabled`.
- Step modal includes ordered action builder controls.

## MMI-AUTO-022 - Step Filter Behavior Configuration UI

**Story**: As admins, we need to configure whether step filters gate iterated content or fetch result sets.

**Acceptance Criteria**

- Step filter can be linked to each step.
- UI supports toggle for `applyToAutomationFilter` behavior.
- Validation prevents unsupported combinations for `target` and filter behavior.

## MMI-AUTO-023 - Prompt Template Variables Guidance UI

**Story**: As admins, we need clear step/action prompt authoring guidance for runtime substitutions.

**Acceptance Criteria**

- UI documents supported substitutions (`{content.*}`, `{results}`, selected result fields).
- Step prompt and action prompt editors support multi-line templated input.
- Preview/test utility shows resolved prompt examples.

## MMI-AUTO-024 - Step Action Builder UI

**Story**: As admins, we need to configure ordered actions with explicit execution contracts.

**Acceptance Criteria**

- Action editor supports `prompt`, `actionType`, `maxCalls`, `target`, `confirmationStatement`, and `isEnabled`.
- Action list supports add/remove/reorder operations.
- Validation enforces required `actionType` and non-negative `maxCalls`.
