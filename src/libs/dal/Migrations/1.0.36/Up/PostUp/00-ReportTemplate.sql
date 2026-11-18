DO $$
BEGIN

-- Initializes the report templates.
INSERT INTO public."report_template" (
  "name"
  , "description"
  , "subject"
  , "body"
  , "is_enabled"
  , "sort_order"
  , "enable_sections"
  , "enable_section_summary"
  , "enable_summary"
  , "enable_charts"
  , "enable_charts_over_time"
  , "created_on"
  , "created_by"
  , "updated_on"
  , "updated_by"
) SELECT
  r."name"
  , r."description"
  , r."settings" ->> 'subject' subject -- subject
  , r."template" -- body
  , r."is_enabled"
  , r."sort_order"
  , false -- enable_sections
  , false -- enable_section_summary
  , false -- enable_summary
  , false -- enable_charts
  , false -- enable_charts_over_time
  , r."created_on"
  , r."created_by"
  , r."updated_on"
  , r."updated_by"
FROM _report r;

END $$;
