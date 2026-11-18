DO $$
BEGIN

-- Create two templates for AV Overview.
INSERT INTO public."report_template" (
  "name"
  , "description"
  , "report_type"
  , "is_enabled"
  , "sort_order"
  , "subject"
  , "body"
  , "settings"
  , "created_by"
  , "updated_by"
) values (
  'AV Overview - Weekday' -- name
  , 'Razor template for the AV evening overview weekday report.' -- description
  , 1 -- report_type
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- subject
  , '' -- body
  , '{}' -- settings
  , '' -- created_by
  , '' -- updated_by
), (
  'AV Overview - Weekend' -- name
  , 'Razor template for the AV evening overview weekend report.' -- description
  , 1 -- report_type
  , true -- is_enabled
  , 0 -- sort_order
  , '' -- subject
  , '' -- body
  , '{}' -- settings
  , '' -- created_by
  , '' -- updated_by
) ON CONFLICT DO NOTHING;

-- The template model has been renamed.
UPDATE public."report_template"
SET
  "subject" = REPLACE("subject", 'ReportTemplateModel', 'ReportEngineContentModel')
  , "body" = REPLACE("subject", 'ReportTemplateModel', 'ReportEngineContentModel');

UPDATE public."chart_template"
SET
  "template" = REPLACE("template", 'ChartTemplateModel', 'ChartEngineContentModel');

END $$;
