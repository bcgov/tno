# Epic Group 5: Editor Operations and Review

## MMI-AUTO-025 - Manual Run Controls

**Story**: As editors, we need to manually run automation repeatedly while tuning steps.

**Acceptance Criteria**

- UI provides manual run controls for selected profile.
- Run modal supports date window and dry-run/apply modes.
- Multiple runs are supported and visible in history.

## MMI-AUTO-026 - Run Review UI

**Story**: As reviewers, we need to inspect run outputs and action decisions before promoting operational changes.

**Acceptance Criteria**

- Review screen shows proposed updates per content item.
- Review shows step/action evaluation outcomes and confirmation matches.
- Optional approve/reject actions are available with notes when enabled by policy.

## MMI-AUTO-027 - Run History and Diff Viewer UI

**Story**: As editors, we need a complete audit trail for each run.

**Acceptance Criteria**

- History view supports filters for profile/date/stage/status.
- Diff view shows before/after values with step/action references.
- Publish/unpublish transition sequence is displayed.

## MMI-AUTO-028 - Step Test Harness UI

**Story**: As admins, we need to test steps on selected stories without executing live side effects.

**Acceptance Criteria**

- Tester accepts selected content items or search criteria.
- Results show step/action outcomes and final resolved action intents.
- Harness output includes order of step evaluation and action execution.
