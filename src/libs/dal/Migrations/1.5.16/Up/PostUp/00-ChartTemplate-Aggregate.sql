-- The 'Custom' chart template only groups with the shared helpers, so the aggregation service
-- produces the same output without any stories. The other seeded templates group by hand.
UPDATE public."chart_template"
SET "settings" = "settings" || '{"aggregate": true}'::jsonb
WHERE "name" = 'Custom';
