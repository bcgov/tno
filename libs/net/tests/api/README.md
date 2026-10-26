# Orphaned content tests

Run `dotnet test libs/net/tests/api/TNO.Test.API.csproj` from the repository root.
Email tests use mocked CHES and in-memory Elasticsearch responses; no email is sent.

To include the database tests, set `TNO_ORPHAN_TEST_POSTGRES` to a **local test PostgreSQL**
admin connection string. Each test creates and drops its own uniquely named database.
The account needs permission to create databases. These tests apply migration 1.5.13's
operations and exercise foreign keys, restoration, sequence advancement, and committed
index requests against PostgreSQL. Background scan tests also exercise migration 1.5.14,
replica locking, durable checkpoints, bounded result paging, cancellation, grace-period rechecks,
posted-date/eligibility filters, Pacific daylight-saving boundaries, date cursor continuation, PIT/search-after requests, and fail-closed partial responses. Without the environment variable they are skipped.

The Editor interaction tests run with Node 22 (the installed Vitest/Vite toolchain requires it):

```sh
cd app/editor
yarn vitest run src/test-vitest/components/orphaned-content/OrphanedContent.test.tsx
```

The Editor production build continues to use Node 18 and `yarn build`.
