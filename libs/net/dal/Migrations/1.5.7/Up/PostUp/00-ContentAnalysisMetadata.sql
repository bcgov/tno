-- Content whose newest analysis request failed, for the Content-Analysis administration page.
-- The predicate must match the queries in ContentAnalysisService (FindFailures, CountFailures).
CREATE INDEX IF NOT EXISTS "IX_content_analysis_failed"
  ON public.content (id)
  WHERE metadata -> 'analysis' ->> 'status' = 'Failed';
