DO $$
BEGIN

INSERT INTO public."report_template_chart_template" (
  "report_template_id"
  , "chart_template_id"
  , "created_by"
  , "updated_by"
) VALUES (
  (SELECT "id" FROM public."report_template" WHERE "name" = 'Custom Report' LIMIT 1) -- report_template_id
  , (SELECT "id" FROM public."chart_template" WHERE "name" = 'Custom' LIMIT 1) -- chart_template_id
  , '' -- created_by
  , '' -- updated_by
) ON CONFLICT DO NOTHING;

END $$;
