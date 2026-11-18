DO $$
BEGIN

-- Create two templates for AV Overview.
INSERT INTO public."av_overview_template" (
  "template_type"
  , "report_template_id"
  , "created_by"
  , "updated_by"
) VALUES (
  0 -- template_type
  , (SELECT id FROM public."report_template" WHERE "name" = 'AV Overview - Weekday' LIMIT 1) -- report_template_id
  , '' -- created_by
  , '' -- updated_by
), (
  1 -- template_type
  , (SELECT id FROM public."report_template" WHERE "name" = 'AV Overview - Weekend' LIMIT 1) -- report_template_id
  , '' -- created_by
  , '' -- updated_by
) ON CONFLICT DO NOTHING;

END $$;
