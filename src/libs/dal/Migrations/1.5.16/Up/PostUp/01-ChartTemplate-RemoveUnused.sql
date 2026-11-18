-- Remove the chart templates no report template offers. Deleting a template cascades to its section
-- links, so a template an environment still links is kept and no saved report loses a chart.
DELETE FROM public."chart_template" ct
WHERE ct."name" IN (
  'Count',
  'Stacked Published On',
  'Average Sentiment',
  'Earned Media',
  'Sentiment Count',
  'Coverage Count',
  'Pre-grouped by Report Section - Over time'
)
  AND NOT EXISTS (SELECT 1 FROM public."report_template_chart_template" l WHERE l."chart_template_id" = ct."id")
  AND NOT EXISTS (SELECT 1 FROM public."report_section_chart_template" l WHERE l."chart_template_id" = ct."id");
