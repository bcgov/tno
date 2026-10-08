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

A backfill is a work order (`WorkOrderType.ContentAnalysisBackfill`) that the Event Handler runs; the
API never runs it in the background.

At submission the API:

1. records the criteria, a creation high-water mark, and the eligible total in the work order's
   configuration;
2. sends a `work-order` message.

The Event Handler consumes `work-order` with its own consumer, so a backfill never delays event
schedules. Each message is one page (`AnalysisBackfillPageSize`, default 500):

1. read the next page of eligible content after the checkpoint (stable keyset pagination by
   content ID), leaving out content whose analysis is current unless the backfill is forced;
2. send each item to `analysis-backfill` with reason `backfill` and the work order ID;
3. save the checkpoint and counts on the work order (a cancellation is never overwritten);
4. send a message to continue, carrying the work order's new version, or mark it completed.

A message for an older version is ignored, so only one chain of messages works on a backfill. A page
that fails is received again; after `RetryLimit` failures the work order is marked failed for an
administrator to resume. Sending a page twice is harmless: Content-Analysis skips content whose
analysis is already current.

- Content-Analysis consumes `analysis-backfill` apart from new content, so a backfill never delays
  it, and backfill uses at most 20% of provider throughput (configurable).
- Content changed during a backfill gets an ordinary lifecycle request; the backfill request for the
  old input is skipped.
- Cancellation stops further pages, and Content-Analysis skips the cancelled backfill's requests.
- Resume continues from the last checkpoint without duplicating accepted analysis.
- Progress shows the total, the content sent, already current, and failed (content whose newest
  run, from this backfill, failed). The administration page shows the requests waiting in
  `analysis-backfill`.

## Tests

- Date boundaries, timezones, missing publication dates, and both date fields.
- Missing/stale versus force modes.
- Durable, idempotent cancellation and resume.
- Content updated or deleted mid-backfill reconciles correctly.
- New content is never delayed by a backfill, and the throughput share holds.
- Reporting never creates a backfill or analysis request.
