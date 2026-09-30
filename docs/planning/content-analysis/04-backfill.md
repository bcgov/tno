# Phase 4 — Manual Historical Backfill

Let an administrator analyze existing content in a chosen date range. Backfill improves the input
reports receive for older stories; reports never depend on it, never wait for it, and never create
it.

## Administration

An admin page and API with:

- required start and end;
- date field: publication date (default) or creation date;
- mode: missing or stale analysis only (default), or force reanalysis;
- a preview count before submission, including content excluded because it has no publication
  date;
- status, counts, errors, cancellation, and resume.

The start is inclusive and the end exclusive. The page shows the selected timezone and normalizes
boundaries to UTC. Excluded media types and sources ([Phase 2](02-content-analysis.md#eligibility))
apply to backfills too.

## Processing

At submission:

1. Persist the criteria and a creation high-water mark.
2. Enumerate matching content with stable keyset pagination, checkpointing progress.
3. Create `analysis_job` rows with reason `backfill`, backfill priority, and the current input
   hash — the same queue and service as lifecycle work.
4. Track analyzed, already current, superseded, deleted, failed, and indexed counts.

- Lifecycle work always outranks backfill, and backfill uses at most 20% of provider throughput
  (configurable).
- Content changed during a backfill gets ordinary lifecycle work; the backfill job for it is
  superseded.
- Cancellation stops further scheduling; claimed work may finish.
- Resume continues from the last checkpoint without duplicating accepted analysis.
- A backfill is complete when every target item is resolved and successful results are searchable.
  Unresolved failures stay visible for replay.

## Tests

- Date boundaries, timezones, missing publication dates, and both date fields.
- Missing/stale versus force modes.
- Durable, idempotent cancellation and resume.
- Content updated or deleted mid-backfill reconciles correctly.
- Lifecycle work keeps priority and the throughput share holds.
- Reporting never creates a backfill or analysis job.
