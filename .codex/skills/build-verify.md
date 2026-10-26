# Skill: Build Verify

Use this skill when validating TNO changes before a commit, push, or PR, or investigating GitHub build/lint annotations.

## Local verification

- Read the affected GitHub workflow and use its SDK, configuration, lint command, and build command. Run relevant tests after the final edit.
- Fix lint, build, and test failures and rerun affected checks before committing. For database coverage, provide `TNO_ORPHAN_TEST_POSTGRES` pointing to a disposable test server; do not count skipped integration tests as passed.
- The stop hook (`bash .claude/scripts/build-verify.sh`) runs builds only. It does not replace lint, tests, or compiler-warning review, and may detect no changes after a commit.
- For API changes, run with .NET 9:

  ```bash
  dotnet format api/net/TNO.API.csproj --verify-no-changes
  dotnet build api/net/TNO.API.csproj --configuration Release --no-incremental
  dotnet test libs/net/tests/api/TNO.Test.API.csproj --configuration Release
  ```

- Keep documentation generation enabled. The API project treats missing documentation, malformed XML, unmatched/missing parameter tags, invalid `cref` references, and misplaced comments as errors (CS1570, CS1572, CS1573, CS1574, CS1584, CS1587, CS1591). Do not suppress these diagnostics or remove documentation generation to pass.
- Read the compiler output, not only its exit code. `dotnet format` does not replace compiler XML checks; an incremental build may skip compilation and hide warnings. Fix new warnings before committing. When asked to clear XML warnings, fix all reported XML warnings even if they predate the branch.
- For Editor changes, run `yarn lint`, relevant tests, and `yarn build`. Verify layout in a real browser when CSS changes; jsdom cannot validate alignment.

## GitHub verification

- Compare the PR head SHA with each check's head SHA. Fetch all check runs/annotations using pagination (`gh api --paginate` or `per_page=100`); the first default page can omit validation checks.
- Inspect annotations from the current API validation job as well as its success/failure status. A green check can still carry compiler warnings.
- Check CodeQL separately from compiler/lint results. For security review comments, inspect the linked alert's instance for `refs/pull/<number>/merge` and confirm the analysis matches the current PR merge commit. Do not classify an open security alert as stale merely because the original comment is old. After fixing it, verify a new analysis and the alert instance's state; a successful CodeQL execution alone does not prove there are no findings.
- Trace a warning shown in the Files changed view to its originating run. Report stale annotations separately from current failures, with the matching commit/run link. Do not treat stale annotations as proof that the current compiler is failing, or dismiss current warnings merely because the screenshot is old.
- A warning can remain beside the latest diff after its code is fixed. Verify the annotation's `blob_href` commit and its check run's `head_sha`; compare the reported method signature with the current declaration. Matching line numbers or seeing corrected comments behind the warning does not establish that the annotation belongs to the current commit.
- When annotations persist, explain this explicitly in the completion report and include links to both the originating run and the clean validation run. If already authorized to update the PR, record that evidence in its validation notes. Say "the current API check has no XML diagnostics" rather than claiming that all warnings disappeared from the PR. Do not make redundant code edits or rerun an old commit to try to clear historical annotations.
- After pushing, inspect the new run. State explicitly if verification is pending; do not call pending checks passed. Do not delete check runs to hide annotations.
